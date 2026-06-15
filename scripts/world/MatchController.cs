using Godot;
using System.Collections.Generic;

namespace FootballProto;

public enum MatchPhase { Kickoff, InPlay, GoalScored, FullTime }

/// <summary>
/// Owns the match: spawning, phases, score/clock, the possession model,
/// player switching, pass-target selection and per-tick driver updates.
///
/// Possession model (purely spatial, no parenting):
/// - Carrier = LastTouch if within 2 m of a ball moving under 8 m/s,
///   else the nearest player within ControlRadius of a slow ball.
/// - GkHolder overrides everything while the GK has the ball in hand.
/// - TeamInPossession = Carrier's team, else -1 (loose ball).
/// </summary>
public partial class MatchController : Node3D
{
    public const float HalfLen = 52.5f;  // X half-length (105 m pitch)
    public const float HalfWid = 34f;    // Z half-width  (68 m pitch)

    public Ball Ball { get; private set; }
    public List<Player> Players { get; } = new List<Player>();
    public Player Carrier { get; private set; }
    public Player GkHolder { get; private set; }
    public int TeamInPossession { get; private set; } = -1;

    public MatchPhase Phase { get; private set; } = MatchPhase.Kickoff;
    public int ScoreBlue { get; private set; }
    public int ScoreRed { get; private set; }
    public float TimeLeft { get; private set; }

    public int KickoffTeam { get; private set; }
    public Player KickoffTaker { get; private set; }

    private HumanDriver _human;          // human controls team 0 (BLUE)
    private TeamAI[] _teamAi = new TeamAI[2];
    private List<GoalkeeperBrain> _gkBrains = new List<GoalkeeperBrain>();
    private Hud _hud;

    private float _autoControlTimer;
    private Player _queuedSwitch;
    private HumanDriver _queuedSwitchDriver;
    private float _goalTimer;
    private float _ballDeadTimer;
    private float _touchCooldown;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private static readonly PackedScene PlayerScene =
        GD.Load<PackedScene>("res://scenes/Player.tscn");

    // Formation fractions: x in [-1..0] own half depth, z in [-1..1] width.
    // Defined for the team attacking +X; mirrored for the other team.
    private static readonly (Vector2 frac, PRole role)[] Formation11 =
    {
        (new Vector2(-0.95f,  0.00f), PRole.GK),
        (new Vector2(-0.70f, -0.60f), PRole.DEF),
        (new Vector2(-0.72f, -0.22f), PRole.DEF),
        (new Vector2(-0.72f,  0.22f), PRole.DEF),
        (new Vector2(-0.70f,  0.60f), PRole.DEF),
        (new Vector2(-0.45f, -0.35f), PRole.MID),
        (new Vector2(-0.50f,  0.00f), PRole.MID),
        (new Vector2(-0.45f,  0.35f), PRole.MID),
        (new Vector2(-0.18f, -0.55f), PRole.FWD),
        (new Vector2(-0.12f,  0.00f), PRole.FWD),
        (new Vector2(-0.18f,  0.55f), PRole.FWD),
    };

    private static readonly (Vector2 frac, PRole role)[] Formation7 =
    {
        (new Vector2(-0.95f,  0.00f), PRole.GK),
        (new Vector2(-0.65f, -0.45f), PRole.DEF),
        (new Vector2(-0.70f,  0.00f), PRole.DEF),
        (new Vector2(-0.65f,  0.45f), PRole.DEF),
        (new Vector2(-0.40f, -0.30f), PRole.MID),
        (new Vector2(-0.40f,  0.30f), PRole.MID),
        (new Vector2(-0.15f,  0.00f), PRole.FWD),
    };

    private GameplayTuning T => App.I.Tuning;

    // =====================================================================
    public override void _Ready()
    {
        _rng.Randomize();
        TimeLeft = App.I.MatchMinutes * 60f;

        Ball = GetNode<Ball>("Ball");
        _hud = GetNode<Hud>("HUD");

        // Pitch and camera init are non-fatal — asset errors must not abort the match
        try { GetNode<Pitch>("Pitch").Init(this); }
        catch (System.Exception e) { GD.PrintErr("Pitch.Init error (non-fatal): " + e.Message); }

        try { GetNode<MatchCamera>("GameCamera").Init(this); }
        catch (System.Exception e) { GD.PrintErr("Camera.Init error (non-fatal): " + e.Message); }

        _hud.Init(this);
        BuildLighting();
        SpawnTeams();

        _human    = new HumanDriver(this, 0);
        _teamAi[0] = new TeamAI(this, 0);
        _teamAi[1] = new TeamAI(this, 1);

        SetupKickoff(kickoffTeam: 0);
    }

    private void BuildLighting()
    {
        var sun = new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-55, 30, 0),
            LightEnergy = 1.2f,
            ShadowEnabled = true
        };
        AddChild(sun);

        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Sky,
            Sky = new Sky { SkyMaterial = new ProceduralSkyMaterial() },
            AmbientLightSource = Godot.Environment.AmbientSource.Sky,
            AmbientLightEnergy = 0.7f
        };
        AddChild(new WorldEnvironment { Environment = env });
    }

    private void SpawnTeams()
    {
        var formation = App.I.TeamSize == 7 ? Formation7 : Formation11;
        for (int team = 0; team < 2; team++)
        {
            for (int i = 0; i < formation.Length; i++)
            {
                var p = PlayerScene.Instantiate<Player>();
                AddChild(p);
                p.Init(this, team, i + 1, formation[i].role,
                    seed: (ulong)(_rng.Randi() + i + team * 100));
                p.GlobalPosition = FormationWorld(team, formation[i].frac);
                Players.Add(p);
                if (formation[i].role == PRole.GK)
                    _gkBrains.Add(new GoalkeeperBrain(this, p));
            }
        }
    }

    private Vector3 FormationWorld(int team, Vector2 frac)
    {
        // frac.X in [-1..0]: -1 = own goal line, 0 = halfway line.
        // Team 0 (BLUE) attacks +X, own goal at -HalfLen -> X = frac.X * HalfLen.
        // Team 1 (RED) is mirrored across the halfway line.
        float sign = team == 0 ? 1f : -1f;
        return new Vector3(sign * frac.X * HalfLen, 0, frac.Y * HalfWid * 0.85f);
    }

    /// <summary>Elastic formation home: base slot shifted toward the ball.</summary>
    public Vector3 HomePosition(Player p)
    {
        var formation = App.I.TeamSize == 7 ? Formation7 : Formation11;
        int idx = (p.Number - 1) % formation.Length;
        Vector3 baseHome = FormationWorld(p.Team, formation[idx].frac);

        bool defending = TeamInPossession != -1 && TeamInPossession != p.Team;
        float compress = defending ? 0.82f : 1f;
        baseHome.Z *= compress;

        // Elastic shift toward the ball along X, smaller along Z
        Vector3 ballPos = Ball != null ? Ball.GlobalPosition : Vector3.Zero;
        float shiftX = ballPos.X * 0.4f;
        float shiftZ = ballPos.Z * 0.22f;
        Vector3 home = baseHome + new Vector3(shiftX, 0, shiftZ);

        home.X = Mathf.Clamp(home.X, -HalfLen + 2f, HalfLen - 2f);
        home.Z = Mathf.Clamp(home.Z, -HalfWid + 2f, HalfWid - 2f);
        return home;
    }

    // =====================================================================
    //  MAIN LOOP
    // =====================================================================
    public override void _PhysicsProcess(double delta)
    {
        // Guard: if _Ready() was interrupted by an asset error, skip until safe
        if (Ball == null || _human == null || _teamAi[0] == null) return;

        float dt = (float)delta;
        _touchCooldown -= dt;

        switch (Phase)
        {
            case MatchPhase.Kickoff:
                if (!Ball.IsHeld && Ball.LinearVelocity.Length() > 1.5f)
                    Phase = MatchPhase.InPlay;
                break;

            case MatchPhase.InPlay:
                TimeLeft -= dt;
                if (TimeLeft <= 0f) { EndMatch(); return; }
                break;

            case MatchPhase.GoalScored:
                _goalTimer -= dt;
                if (_goalTimer <= 0f)
                    SetupKickoff(KickoffTeam);
                return;

            case MatchPhase.FullTime:
                return;
        }

        UpdatePossession(dt);
        DetectFirstTouch();
        BallSafety(dt);

        _human.Process(dt);
        _teamAi[0].Process(dt);
        _teamAi[1].Process(dt);
        foreach (var gk in _gkBrains) gk.Process(dt);

        UpdateSwitching(dt);
    }

    // =====================================================================
    //  POSSESSION
    // =====================================================================
    private void UpdatePossession(float dt)
    {
        Player newCarrier = null;

        if (GkHolder != null)
        {
            newCarrier = GkHolder;
        }
        else
        {
            float ballSpeed = U.Flat(Ball.LinearVelocity).Length();

            // Last toucher keeps "carrier" status while close to a controllable ball
            if (Ball.LastTouch != null && IsInstanceValid(Ball.LastTouch)
                && ballSpeed < 8f
                && U.FlatDist(Ball.LastTouch.GlobalPosition, Ball.GlobalPosition) < 2f)
            {
                newCarrier = Ball.LastTouch;
            }
            else if (ballSpeed < 4f)
            {
                float bestD = T.ControlRadius;
                foreach (Player p in Players)
                {
                    float d = U.FlatDist(p.GlobalPosition, Ball.GlobalPosition);
                    if (d < bestD) { bestD = d; newCarrier = p; }
                }
            }
        }

        bool lost = Carrier != null && newCarrier != null && newCarrier.Team != Carrier.Team;
        Carrier = newCarrier;
        TeamInPossession = Carrier?.Team ?? -1;

        // Auto-control: take over the human-team carrier shortly after they win it
        if (Carrier != null && Carrier.Team == 0 && Carrier.Role != PRole.GK
            && _human.Controlled != Carrier)
        {
            _autoControlTimer += dt;
            if (_autoControlTimer > 0.35f)
                SetControlled(Carrier);
        }
        else
        {
            _autoControlTimer = 0f;
        }

        // On possession loss, switch the human to the nearest defender if their
        // current player is far from the action
        if (lost && Carrier.Team == 1 && _human.Controlled != null
            && U.FlatDist(_human.Controlled.GlobalPosition, Ball.GlobalPosition) > 18f)
        {
            SwitchPlayer(_human);
        }
    }

    private void DetectFirstTouch()
    {
        if (_touchCooldown > 0f || Ball.IsHeld) return;
        float ballSpeed = U.Flat(Ball.LinearVelocity).Length();
        if (ballSpeed < T.FirstTouchSpeedThreshold) return;
        if (Ball.GlobalPosition.Y > 1.4f) return;

        foreach (Player p in Players)
        {
            if (p == Ball.LastTouch) continue;
            if (p.IsBusy && p.State != PState.Receive) continue;
            if (U.FlatDist(p.GlobalPosition, Ball.GlobalPosition) < 0.9f)
            {
                p.FirstTouch(Ball);
                _touchCooldown = 0.3f;
                break;
            }
        }
    }

    /// <summary>Drop-ball reset if the ball settles in an unreachable dead zone.</summary>
    private void BallSafety(float dt)
    {
        Vector3 p = Ball.GlobalPosition;
        bool outOfArena = Mathf.Abs(p.X) > HalfLen + 4f || Mathf.Abs(p.Z) > HalfWid + 3f
                          || p.Y < -2f;
        bool stuck = U.Flat(Ball.LinearVelocity).Length() < 0.2f
                     && (Mathf.Abs(p.X) > HalfLen + 0.5f || Mathf.Abs(p.Z) > HalfWid + 0.5f);

        if (outOfArena) { DropBall(); return; }
        if (stuck)
        {
            _ballDeadTimer += dt;
            if (_ballDeadTimer > 1.5f) DropBall();
        }
        else _ballDeadTimer = 0f;
    }

    private void DropBall()
    {
        _ballDeadTimer = 0f;
        Vector3 p = Ball.GlobalPosition;
        Vector3 safe = new Vector3(
            Mathf.Clamp(p.X, -HalfLen + 3f, HalfLen - 3f), Ball.Radius + 0.4f,
            Mathf.Clamp(p.Z, -HalfWid + 3f, HalfWid - 3f));
        Ball.Release();
        Ball.GlobalPosition = safe;
        Ball.LinearVelocity = Vector3.Zero;
        Ball.Spin = Vector3.Zero;
        Ball.LastTouch = null;
    }

    // =====================================================================
    //  SWITCHING
    // =====================================================================
    public Player HumanControlled(int team)
        => team == 0 ? _human.Controlled : null;

    private void SetControlled(Player p)
    {
        if (_human.Controlled != null && IsInstanceValid(_human.Controlled))
            _human.Controlled.SetSelected(false);
        _human.Controlled = p;
        _autoControlTimer = 0f;
        if (p != null) p.SetSelected(true);
    }

    /// <summary>LB switch: nearest non-GK teammate to the ball (excluding current).</summary>
    public void SwitchPlayer(HumanDriver driver)
    {
        Player best = null;
        float bestD = float.MaxValue;
        foreach (Player p in Players)
        {
            if (p.Team != driver.Team || p.Role == PRole.GK) continue;
            if (p == driver.Controlled) continue;
            float d = U.FlatDist(p.GlobalPosition, Ball.GlobalPosition);
            if (d < bestD) { bestD = d; best = p; }
        }
        if (best != null) SetControlled(best);
    }

    /// <summary>Queue an auto-switch to the intended pass receiver.</summary>
    public void QueueSwitchTo(HumanDriver driver, Player receiver)
    {
        _queuedSwitch = receiver;
        _queuedSwitchDriver = driver;
    }

    private void UpdateSwitching(float dt)
    {
        if (_queuedSwitch == null || !IsInstanceValid(_queuedSwitch)) return;
        // Switch when the ball gets near the receiver, or give up if it stalls
        float d = U.FlatDist(_queuedSwitch.GlobalPosition, Ball.GlobalPosition);
        if (d < 4f)
        {
            SetControlled(_queuedSwitch);
            _queuedSwitch = null;
        }
        else if (U.Flat(Ball.LinearVelocity).Length() < 1.5f && d > 8f)
        {
            _queuedSwitch = null;
        }
    }

    public void CallSecondPresser(int team) => _teamAi[team].PressTimer = 1.3f;

    // =====================================================================
    //  QUERIES used by drivers / AI
    // =====================================================================
    public Player NearestOpponent(Player p)
    {
        Player best = null;
        float bestD = float.MaxValue;
        foreach (Player o in Players)
        {
            if (o.Team == p.Team) continue;
            float d = U.FlatDist(o.GlobalPosition, p.GlobalPosition);
            if (d < bestD) { bestD = d; best = o; }
        }
        return best;
    }

    /// <summary>
    /// Pick the best pass target in the aimed direction: scores aim alignment,
    /// openness and sensible distance. preferRunners biases toward players
    /// moving toward the opponent goal (through balls).
    /// </summary>
    public Player FindPassTarget(Player from, Vector3 aimDir, bool longRange,
        bool preferRunners = false)
    {
        float minD = longRange ? 8f : 3f;
        float maxD = longRange ? 38f : 24f;
        Vector3 attackDir = from.Team == 0 ? Vector3.Right : Vector3.Left;

        Player best = null;
        float bestScore = 0.25f; // threshold: bad options return null
        foreach (Player mate in Players)
        {
            if (mate.Team != from.Team || mate == from || mate.Role == PRole.GK) continue;
            Vector3 to = U.Flat(mate.GlobalPosition - from.GlobalPosition);
            float d = to.Length();
            if (d < minD || d > maxD) continue;

            float aim = aimDir.LengthSquared() > 0.01f
                ? Mathf.Max(to.Normalized().Dot(aimDir.Normalized()), 0f) : 0.5f;
            if (aim < 0.25f) continue;

            float open = 1f;
            Player opp = NearestOpponent(mate);
            if (opp != null)
                open = Mathf.Clamp(U.FlatDist(mate.GlobalPosition, opp.GlobalPosition) / 6f,
                    0.15f, 1f);

            float runner = preferRunners
                ? Mathf.Max(U.Flat(mate.Velocity).Normalized().Dot(attackDir), 0f) * 0.6f : 0f;

            float score = aim * 1.4f + open * 0.6f + runner - d / 80f;
            if (score > bestScore) { bestScore = score; best = mate; }
        }
        return best;
    }

    /// <summary>Aim point inside the opponent goal, biased by stick input.</summary>
    public Vector3 GoalTarget(int team, Vector3 from, Vector3 aimDir)
    {
        float goalX = team == 0 ? HalfLen : -HalfLen;
        float z = 0f;
        if (aimDir.LengthSquared() > 0.04f)
            z = Mathf.Clamp(aimDir.Z * 6f, -3f, 3f);
        return new Vector3(goalX, 0.6f, z);
    }

    // =====================================================================
    //  GOALS / KICKOFF / FULL TIME
    // =====================================================================
    /// <summary>Called by goal Area3D triggers. sideSign = +1 for the +X goal.</summary>
    public void OnGoal(int sideSign)
    {
        if (Phase != MatchPhase.InPlay) return;

        // Ball entering the +X goal = BLUE (attacks +X) scored
        if (sideSign > 0) { ScoreBlue++; KickoffTeam = 1; }
        else { ScoreRed++; KickoffTeam = 0; }

        Phase = MatchPhase.GoalScored;
        _goalTimer = 2.6f;
        Ball.Hold(new Vector3(sideSign * (HalfLen + 1.5f), Ball.Radius, 0));
        _hud.ShowBanner("GOAL!");
    }

    public void SetupKickoff(int kickoffTeam)
    {
        KickoffTeam = kickoffTeam;
        Phase = MatchPhase.Kickoff;
        GkHolder = null;
        Carrier = null;
        TeamInPossession = -1;
        _queuedSwitch = null;

        Ball.Release();
        Ball.GlobalPosition = new Vector3(0, Ball.Radius, 0);
        Ball.LinearVelocity = Vector3.Zero;
        Ball.Spin = Vector3.Zero;
        Ball.LastTouch = null;

        var formation = App.I.TeamSize == 7 ? Formation7 : Formation11;
        foreach (Player p in Players)
        {
            int idx = (p.Number - 1) % formation.Length;
            Vector3 home = FormationWorld(p.Team, formation[idx].frac);
            // Kicking-off team pushes its centre FWD to the spot
            p.GlobalPosition = home;
            p.Velocity = Vector3.Zero;
        }

        // Designate the kickoff taker: central-most FWD of the kicking team
        KickoffTaker = null;
        float bestZ = float.MaxValue;
        foreach (Player p in Players)
        {
            if (p.Team != kickoffTeam || p.Role != PRole.FWD) continue;
            float az = Mathf.Abs(p.GlobalPosition.Z);
            if (az < bestZ) { bestZ = az; KickoffTaker = p; }
        }
        if (KickoffTaker != null)
            KickoffTaker.GlobalPosition = new Vector3(
                (kickoffTeam == 0 ? -2.5f : 2.5f), 0, 0.5f);

        // Human starts on a central midfielder
        Player start = null;
        foreach (Player p in Players)
        {
            if (p.Team != 0 || p.Role != PRole.MID) continue;
            if (start == null
                || Mathf.Abs(p.GlobalPosition.Z) < Mathf.Abs(start.GlobalPosition.Z))
                start = p;
        }
        if (kickoffTeam == 0 && KickoffTaker != null) start = KickoffTaker;
        SetControlled(start);
    }

    public void GkCatch(Player gk)
    {
        GkHolder = gk;
        Carrier = gk;
        TeamInPossession = gk.Team;
    }

    public void GkRelease(Player gk)
    {
        if (GkHolder == gk) GkHolder = null;
        Ball.Release();
    }

    private void EndMatch()
    {
        Phase = MatchPhase.FullTime;
        TimeLeft = 0f;
        Ball.Hold(Ball.GlobalPosition);
        _hud.ShowFullTime(ScoreBlue, ScoreRed);
    }

    public HumanDriver Human => _human;
}
