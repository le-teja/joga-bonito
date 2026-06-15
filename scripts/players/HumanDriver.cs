using Godot;

namespace FootballProto;

/// <summary>
/// Translates input actions into commands on the currently controlled player.
/// Context-sensitive: the same buttons mean attack actions when our team has
/// the ball and defensive actions otherwise.
/// Charging: pass/lob/through/shot are hold-to-charge, fire on release.
/// </summary>
public class HumanDriver
{
    public int Team;
    public Player Controlled;

    // Exposed for the HUD power bar
    public float Charge01 { get; private set; }
    public bool Charging { get; private set; }

    private MatchController _match;
    private GameplayTuning T => App.I.Tuning;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private enum ChargeAction { None, Pass, Lob, Through, Shot }
    private ChargeAction _charge = ChargeAction.None;
    private float _chargeTime;

    public HumanDriver(MatchController match, int team)
    {
        _match = match;
        Team = team;
        _rng.Randomize();
    }

    public void Process(float dt)
    {
        if (Controlled == null || !IsInstanceValid(Controlled)) return;
        Player p = Controlled;

        // ----- Movement -----
        // Camera convention (see MatchCamera): camera sits on the +Z side looking
        // toward -Z, so screen-right = world +X and screen-up = world -Z.
        // Input.GetVector returns stick.Y = -1 when pressing up, which maps
        // directly to -Z, so the raw (x, 0, y) mapping is screen-correct.
        Vector2 stick = Input.GetVector(
            InputActions.MoveLeft, InputActions.MoveRight,
            InputActions.MoveUp, InputActions.MoveDown);
        p.CmdMove = new Vector3(stick.X, 0, stick.Y);

        p.CmdSprint = Input.IsActionPressed(InputActions.Sprint);

        bool weAttack = _match.TeamInPossession == Team || _match.TeamInPossession == -1;

        if (weAttack) ProcessAttack(p, dt);
        else ProcessDefense(p, dt);
    }

    // =====================================================================
    private void ProcessAttack(Player p, float dt)
    {
        p.CmdJockey = false;
        p.CmdShield = Input.IsActionPressed(InputActions.ShieldJockey)
                      && _match.Carrier == p;

        bool nearBall = U.FlatDist(p.GlobalPosition, _match.Ball.GlobalPosition) < 3.5f;

        // --- Start charges ---
        if (Input.IsActionJustPressed(InputActions.PassPressure)) Begin(ChargeAction.Pass);
        if (Input.IsActionJustPressed(InputActions.LobSlide)) Begin(ChargeAction.Lob);
        if (Input.IsActionJustPressed(InputActions.Through)) Begin(ChargeAction.Through);
        if (Input.IsActionJustPressed(InputActions.ShootTackle)) Begin(ChargeAction.Shot);

        if (_charge != ChargeAction.None)
        {
            _chargeTime += dt;
            float max = _charge == ChargeAction.Shot ? T.ShotChargeTime : T.PassChargeTime;
            Charge01 = Mathf.Clamp(_chargeTime / max, 0f, 1f);
            Charging = true;
        }
        else { Charging = false; Charge01 = 0f; }

        // --- Release => execute ---
        if (_charge == ChargeAction.Pass && Input.IsActionJustReleased(InputActions.PassPressure))
            Fire(p, nearBall, ChargeAction.Pass);
        else if (_charge == ChargeAction.Lob && Input.IsActionJustReleased(InputActions.LobSlide))
            Fire(p, nearBall, ChargeAction.Lob);
        else if (_charge == ChargeAction.Through && Input.IsActionJustReleased(InputActions.Through))
            Fire(p, nearBall, ChargeAction.Through);
        else if (_charge == ChargeAction.Shot && Input.IsActionJustReleased(InputActions.ShootTackle))
            Fire(p, nearBall, ChargeAction.Shot);
    }

    private void Begin(ChargeAction a)
    {
        if (_charge != ChargeAction.None) return;
        _charge = a;
        _chargeTime = 0f;
    }

    private void Fire(Player p, bool nearBall, ChargeAction a)
    {
        float power = Charge01;
        _charge = ChargeAction.None;
        Charging = false; Charge01 = 0f;
        if (!nearBall || !p.CanAct) return;

        Vector3 ballPos = _match.Ball.GlobalPosition;
        Vector3 aimDir = p.CmdMove.LengthSquared() > 0.04f
            ? U.Flat(p.CmdMove).Normalized() : p.Facing;
        bool lbHeld = Input.IsActionPressed(InputActions.LbSwitch);
        bool finesse = Input.IsActionPressed(InputActions.FinesseCall);

        switch (a)
        {
            case ChargeAction.Pass:
            {
                Player target = _match.FindPassTarget(p, aimDir, longRange: false);
                Vector3 tpos = target != null
                    ? target.GlobalPosition + U.Flat(target.Velocity) * 0.35f
                    : ballPos + aimDir * Mathf.Lerp(8f, 20f, power);
                Vector3 vel = KickMath.GroundPass(ballPos, tpos, Mathf.Max(power, 0.25f),
                    p.Attr.Passing, T, _rng);
                if (p.RequestKick(vel, Vector3.Zero, T.PassWindup, T.PassRecover))
                {
                    if (lbHeld) { p.ForcedRunDir = p.Facing; p.ForcedRunTimer = 1.6f; } // pass-and-run
                    if (target != null) _match.QueueSwitchTo(this, target);
                }
                break;
            }
            case ChargeAction.Lob:
            {
                Player target = _match.FindPassTarget(p, aimDir, longRange: true);
                Vector3 tpos = target != null
                    ? target.GlobalPosition + U.Flat(target.Velocity) * 0.6f
                    : ballPos + aimDir * Mathf.Lerp(14f, 32f, power);
                Vector3 vel = KickMath.Lob(ballPos, tpos, Mathf.Max(power, 0.3f),
                    p.Attr.Passing, T, _rng);
                if (p.RequestKick(vel, Vector3.Zero, T.PassWindup * 1.4f, T.PassRecover * 1.2f)
                    && target != null)
                    _match.QueueSwitchTo(this, target);
                break;
            }
            case ChargeAction.Through:
            {
                Player target = _match.FindPassTarget(p, aimDir, longRange: true, preferRunners: true);
                Vector3 attackDir = Team == 0 ? Vector3.Right : Vector3.Left;
                Vector3 tpos = target != null
                    ? KickMath.ThroughTarget(target, attackDir)
                    : ballPos + aimDir * Mathf.Lerp(12f, 28f, power);
                Vector3 vel;
                if (lbHeld) // lobbed through
                    vel = KickMath.Lob(ballPos, tpos, Mathf.Max(power, 0.35f), p.Attr.Passing, T, _rng);
                else
                {
                    vel = KickMath.GroundPass(ballPos, tpos, Mathf.Max(power, 0.35f),
                        p.Attr.Passing, T, _rng) * T.ThroughSpeedBonus;
                }
                if (p.RequestKick(vel, Vector3.Zero, T.PassWindup, T.PassRecover)
                    && target != null)
                    _match.QueueSwitchTo(this, target);
                break;
            }
            case ChargeAction.Shot:
            {
                Vector3 goal = _match.GoalTarget(Team, ballPos, aimDir);
                float windup = Mathf.Lerp(T.ShotWindupMin, T.ShotWindupMax, power);
                var (vel, spin) = KickMath.Shot(ballPos, goal, Mathf.Max(power, 0.3f),
                    p.Attr.Shooting, finesse, T, _rng);
                p.RequestKick(vel, spin, windup, T.ShotRecover);
                break;
            }
        }
    }

    // =====================================================================
    private void ProcessDefense(Player p, float dt)
    {
        _charge = ChargeAction.None;
        Charging = false; Charge01 = 0f;
        p.CmdShield = false;
        p.CmdJockey = Input.IsActionPressed(InputActions.ShieldJockey);

        // Contain: hold A to blend steering toward the carrier
        if (Input.IsActionPressed(InputActions.PassPressure))
        {
            Player carrier = _match.Carrier;
            Vector3 target = carrier != null
                ? carrier.GlobalPosition : _match.Ball.GlobalPosition;
            Vector3 toT = U.Flat(target - p.GlobalPosition);
            if (toT.LengthSquared() > 0.3f)
            {
                Vector3 auto = toT.Normalized();
                p.CmdMove = p.CmdMove.LengthSquared() > 0.04f
                    ? (p.CmdMove * 0.45f + auto * 0.55f).Normalized()
                    : auto;
            }
        }

        if (Input.IsActionJustPressed(InputActions.ShootTackle)) p.StartTackle();
        if (Input.IsActionJustPressed(InputActions.LobSlide)) p.StartSlide();
        if (Input.IsActionJustPressed(InputActions.LbSwitch)) _match.SwitchPlayer(this);
        if (Input.IsActionJustPressed(InputActions.FinesseCall))
            _match.CallSecondPresser(Team);
    }

    private static bool IsInstanceValid(GodotObject o) => GodotObject.IsInstanceValid(o);
}
