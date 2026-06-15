using Godot;
using System;

namespace FootballProto;

public enum PRole { GK, DEF, MID, FWD }

public enum PState
{
    Normal,
    WindUp,        // committed to a kick
    KickRecover,
    TackleWind,
    TackleActive,
    TackleRecover,
    SlideActive,
    SlideRecover,
    Receive,       // awkward first touch lock
    Stumble,       // lost balance after contact
    GkDive,
    GkGetUp
}

/// <summary>
/// One footballer. CharacterBody3D driven each physics tick by command fields
/// (CmdMove, CmdSprint, ...) which are written by either HumanDriver or the
/// team AI. All "human-like" feel comes from: speed-dependent turn-rate caps,
/// accel/decel curves, action commitment windows, recovery windows, first-touch
/// quality, stamina fatigue, and balance-based contact outcomes.
/// The ball is NEVER parented to a player — dribbling is periodic real kicks.
/// </summary>
public partial class Player : CharacterBody3D
{
    [Export] public int Team;             // 0 = BLUE (+X attack), 1 = RED (-X attack)
    [Export] public int Number = 1;

    public PRole Role = PRole.MID;
    public PlayerAttributes Attr = new PlayerAttributes();
    public PState State { get; private set; } = PState.Normal;

    // ----- Commands (written by drivers each frame) -----
    public Vector3 CmdMove = Vector3.Zero;  // desired direction, length <= 1
    public bool CmdSprint;
    public bool CmdShield;
    public bool CmdJockey;

    // ----- Runtime -----
    public float Stamina = 100f;
    public Vector3 Facing = Vector3.Forward;
    public float ForcedRunTimer;           // pass-and-run impulse
    public Vector3 ForcedRunDir;

    private MatchController _match;
    private GameplayTuning T => App.I.Tuning;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private float _stateTimer;
    private float _dribbleTimer;
    private float _stumbleCooldown;
    private float _whiffTimer;             // committed kick waiting for ball in range

    // Pending kick payload (committed during WindUp)
    private Vector3 _pendingVel;
    private Vector3 _pendingSpin;
    private float _pendingRecover;

    private MeshInstance3D _selectRing;
    private Node3D _nose;

    public float TopSpeedMs => Mathf.Lerp(7.0f, 9.4f, Attr.TopSpeed) * FatigueFactor();
    public bool IsBusy => State != PState.Normal && State != PState.Receive;
    public bool CanAct => State == PState.Normal;

    public void Init(MatchController match, int team, int number, PRole role, ulong seed)
    {
        _match = match;
        Team = team;
        Number = number;
        Role = role;
        _rng.Seed = seed;
        Attr = PlayerAttributes.ForRole(role, _rng);
        Facing = team == 0 ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0);
    }

    public override void _Ready()
    {
        _selectRing = GetNodeOrNull<MeshInstance3D>("SelectRing");
        _nose = null; // facing handled by PlayerAnimator via Visual node
        PlayerSceneBuilder.BuildVisuals(this);
        // Visual node was just added — get reference for facing rotation
        _nose = GetNodeOrNull<Node3D>("Visual");
    }

    public void SetSelected(bool sel)
    {
        if (_selectRing != null) _selectRing.Visible = sel;
    }

    public float FatigueFactor()
        => Mathf.Lerp(T.FatigueMinFactor, 1f, Stamina / 100f);

    // =====================================================================
    //  PHYSICS TICK
    // =====================================================================
    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        _stateTimer -= dt;
        _stumbleCooldown -= dt;
        _dribbleTimer -= dt;

        TickState(dt);
        TickStamina(dt);
        TickMove(dt);
        TickDribble();
        TickContacts();
        ClampToArena();
        UpdateVisualFacing();
    }

    private void TickState(float dt)
    {
        switch (State)
        {
            case PState.WindUp:
                if (_stateTimer <= 0f)
                {
                    // Fire the committed kick if ball is in reach, else whiff-buffer
                    if (TryExecuteKick())
                        Enter(PState.KickRecover, _pendingRecover);
                    else
                    {
                        _whiffTimer += dt;
                        if (_whiffTimer > 0.18f)
                            Enter(PState.KickRecover, _pendingRecover * 0.6f);
                        else
                            _stateTimer = 0f; // keep trying briefly
                    }
                }
                break;

            case PState.TackleWind:
                if (_stateTimer <= 0f) Enter(PState.TackleActive, T.TackleActive);
                break;

            case PState.TackleActive:
                TryPoke(reach: 1.5f * Mathf.Lerp(0.8f, 1.25f, Attr.DefensiveReach));
                if (_stateTimer <= 0f) Enter(PState.TackleRecover, T.TackleRecover);
                break;

            case PState.SlideActive:
                TryPoke(reach: 2.1f * Mathf.Lerp(0.8f, 1.25f, Attr.DefensiveReach));
                if (_stateTimer <= 0f) Enter(PState.SlideRecover, T.SlideRecover);
                break;

            case PState.KickRecover:
            case PState.TackleRecover:
            case PState.SlideRecover:
            case PState.Receive:
            case PState.Stumble:
            case PState.GkGetUp:
                if (_stateTimer <= 0f) Enter(PState.Normal, 0f);
                break;

            case PState.GkDive:
                if (_stateTimer <= 0f) Enter(PState.GkGetUp, 0.5f);
                break;
        }
    }

    private void Enter(PState s, float duration)
    {
        State = s;
        _stateTimer = duration;
        if (s == PState.WindUp) _whiffTimer = 0f;
    }

    private void TickStamina(float dt)
    {
        bool sprinting = CmdSprint && U.Flat(Velocity).Length() > 5.5f;
        if (sprinting)
            Stamina -= T.StaminaDrainSprint * dt / Mathf.Lerp(0.7f, 1.3f, Attr.Stamina);
        else
            Stamina += T.StaminaRecover * dt;
        Stamina = Mathf.Clamp(Stamina, 0f, 100f);
    }

    // =====================================================================
    //  LOCOMOTION: accel/decel + speed-dependent turn rate (no snap turns)
    // =====================================================================
    private void TickMove(float dt)
    {
        Vector3 vel = Velocity;
        Vector3 flatVel = U.Flat(vel);
        float speed = flatVel.Length();

        Vector3 desiredDir = U.Flat(CmdMove);
        if (ForcedRunTimer > 0f)
        {
            ForcedRunTimer -= dt;
            desiredDir = ForcedRunDir;
        }
        if (desiredDir.LengthSquared() > 1f) desiredDir = desiredDir.Normalized();

        float maxSpeed = TopSpeedMs;
        if (!CmdSprint) maxSpeed *= 0.74f;
        if (CmdShield) maxSpeed = T.ShieldSpeed;
        if (CmdJockey) maxSpeed = Mathf.Min(maxSpeed, T.JockeySpeed);

        // State-driven movement overrides
        switch (State)
        {
            case PState.WindUp:
            case PState.KickRecover:
                maxSpeed *= 0.35f; desiredDir *= 0.4f; break;
            case PState.TackleWind:
            case PState.TackleActive:
                maxSpeed *= 0.5f; break;
            case PState.SlideActive:
                // Slide is ballistic: keep current direction at slide speed
                desiredDir = Facing; maxSpeed = T.SlideSpeed; break;
            case PState.SlideRecover:
            case PState.TackleRecover:
            case PState.Stumble:
            case PState.GkGetUp:
                maxSpeed *= 0.15f; desiredDir = Vector3.Zero; break;
            case PState.Receive:
                maxSpeed *= 0.45f; break;
            case PState.GkDive:
                desiredDir = Facing; maxSpeed = 7.5f; break;
        }

        Vector3 targetVel = desiredDir * maxSpeed;

        // Turn-rate cap: fast players cannot snap-turn
        if (speed > 0.5f && targetVel.LengthSquared() > 0.01f)
        {
            float speed01 = Mathf.Clamp(speed / 9.4f, 0f, 1f);
            float turnDeg = Mathf.Lerp(T.TurnDegLowSpeed, T.TurnDegHighSpeed, speed01)
                            * Mathf.Lerp(0.75f, 1.25f, Attr.Agility) * FatigueFactor();
            float maxRad = Mathf.DegToRad(turnDeg) * dt;
            Vector3 curDir = flatVel.Normalized();
            Vector3 wantDir = targetVel.Normalized();
            float ang = curDir.SignedAngleTo(wantDir, Vector3.Up);
            float clamped = Mathf.Clamp(ang, -maxRad, maxRad);
            Vector3 newDir = curDir.Rotated(Vector3.Up, clamped);
            targetVel = newDir * targetVel.Length();
            // Hard turns shed speed
            if (Mathf.Abs(ang) > maxRad * 1.5f)
                targetVel *= 0.9f;
        }

        float accel = (targetVel.LengthSquared() > flatVel.LengthSquared() ? T.Accel : T.Decel)
                      * Mathf.Lerp(0.75f, 1.25f, Attr.Acceleration) * FatigueFactor();
        Vector3 newFlat = flatVel.MoveToward(targetVel, accel * dt);

        // Facing: follow velocity; while shielding face AWAY from nearest opponent
        if (CmdShield && _match != null)
        {
            var opp = _match.NearestOpponent(this);
            if (opp != null)
            {
                Vector3 away = U.Flat(GlobalPosition - opp.GlobalPosition);
                if (away.LengthSquared() > 0.01f) Facing = away.Normalized();
            }
        }
        else if (newFlat.LengthSquared() > 0.4f)
        {
            Facing = newFlat.Normalized();
        }
        else if (desiredDir.LengthSquared() > 0.04f)
        {
            Facing = Facing.Slerp(desiredDir.Normalized(), 6f * dt).Normalized();
        }

        Velocity = new Vector3(newFlat.X, vel.Y - 9.81f * dt, newFlat.Z);
        MoveAndSlide();
        if (IsOnFloor() && Velocity.Y < 0)
            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
    }

    // =====================================================================
    //  DRIBBLING: periodic real kicks, no magnetism
    // =====================================================================
    private void TickDribble()
    {
        if (_match == null || State != PState.Normal) return;
        if (_match.Carrier != this) return;
        if (_dribbleTimer > 0f) return;

        Ball ball = _match.Ball;
        if (ball.IsHeld) return;
        float dist = U.FlatDist(GlobalPosition, ball.GlobalPosition);
        if (dist > T.ControlRadius) return;

        float mySpeed = U.Flat(Velocity).Length();
        if (mySpeed < 0.8f && CmdMove.LengthSquared() < 0.04f) return; // standing still: leave ball

        // Nudge the ball ahead of the run
        Vector3 dir = mySpeed > 0.8f ? U.Flat(Velocity).Normalized() : Facing;
        float touchSpeed = Mathf.Max(mySpeed * T.DribbleSpeedFactor, 2.2f);
        // Control skill keeps the touch closer; sprinting pushes it further
        float ctrl = Mathf.Lerp(1.25f, 0.85f, Attr.BallControl);
        if (CmdSprint) ctrl *= 1.3f;
        touchSpeed *= ctrl;

        float errDeg = T.TouchErrorBase * 14f * (1f - Attr.BallControl * 0.7f);
        dir = dir.Rotated(Vector3.Up, Mathf.DegToRad(_rng.Randfn(0f, errDeg)));

        ball.KickBall(dir * touchSpeed, Vector3.Zero, this);
        _dribbleTimer = T.DribbleTouchInterval * Mathf.Lerp(1.15f, 0.85f, Attr.BallControl);
    }

    /// <summary>
    /// Called by MatchController when a moving ball arrives at this player.
    /// Damps the ball based on control skill, incoming speed and own movement.
    /// A bad touch scatters the ball and locks the player briefly.
    /// </summary>
    public void FirstTouch(Ball ball)
    {
        Vector3 inVel = ball.LinearVelocity;
        float inSpeed = U.Flat(inVel).Length();
        float mySpeed = U.Flat(Velocity).Length();

        float difficulty = Mathf.Clamp(
            (inSpeed - T.FirstTouchSpeedThreshold) / 10f, 0f, 1f)
            + Mathf.Clamp(mySpeed / 9f, 0f, 0.5f);
        float quality = Mathf.Clamp(Attr.BallControl - difficulty * 0.7f, 0f, 1f);

        // Cushion: kill most velocity, keep a touch in movement direction
        Vector3 keepDir = CmdMove.LengthSquared() > 0.04f
            ? U.Flat(CmdMove).Normalized() : Facing;
        float keepSpeed = Mathf.Lerp(3.5f, 1.4f, quality);

        float scatter = T.TouchErrorBase * (1f - quality);
        Vector3 dir = keepDir.Rotated(Vector3.Up,
            _rng.Randfn(0f, scatter * 2.2f));

        ball.KickBall(dir * keepSpeed + Vector3.Up * scatter * 1.5f, Vector3.Zero, this);

        if (quality < 0.45f)
            Enter(PState.Receive, T.ReceiveLockBase * Mathf.Lerp(2f, 1f, quality / 0.45f));

        _dribbleTimer = 0.25f;
    }

    // =====================================================================
    //  KICKING: windup commitment -> execute -> recover
    // =====================================================================
    /// <summary>Request a committed kick. Velocity/spin computed by the caller.</summary>
    public bool RequestKick(Vector3 ballVel, Vector3 spin, float windup, float recover)
    {
        if (!CanAct) return false;
        _pendingVel = ballVel;
        _pendingSpin = spin;
        _pendingRecover = recover;
        Enter(PState.WindUp, windup);
        // Face the kick
        Vector3 flat = U.Flat(ballVel);
        if (flat.LengthSquared() > 0.01f) Facing = flat.Normalized();
        return true;
    }

    private bool TryExecuteKick()
    {
        Ball ball = _match.Ball;
        if (ball.IsHeld) return false;
        if (U.FlatDist(GlobalPosition, ball.GlobalPosition) > T.KickRange * 1.25f)
            return false;
        ball.KickBall(_pendingVel, _pendingSpin, this);
        return true;
    }

    // =====================================================================
    //  TACKLING
    // =====================================================================
    public bool StartTackle()
    {
        if (!CanAct) return false;
        Enter(PState.TackleWind, T.TackleWindup);
        return true;
    }

    public bool StartSlide()
    {
        if (!CanAct) return false;
        Enter(PState.SlideActive, T.SlideActive);
        return true;
    }

    private void TryPoke(float reach)
    {
        Ball ball = _match.Ball;
        if (ball.IsHeld) return;
        if (U.FlatDist(GlobalPosition, ball.GlobalPosition) > reach) return;

        Player carrier = _match.Carrier;
        float chance = T.TackleBaseChance;
        if (carrier != null && carrier != this && carrier.Team != Team)
        {
            if (carrier.CmdShield) chance *= 0.55f;
            chance *= Mathf.Lerp(1.2f, 0.8f, carrier.Attr.Balance);
            chance *= Mathf.Lerp(0.85f, 1.15f, Attr.DefensiveReach);
        }
        if (_rng.Randf() > chance) return;

        // Poke the ball away from the carrier, roughly forward of the tackler
        Vector3 dir = (Facing + new Vector3(_rng.RandfRange(-0.4f, 0.4f), 0,
            _rng.RandfRange(-0.4f, 0.4f))).Normalized();
        ball.KickBall(dir * _rng.RandfRange(4f, 7f), Vector3.Zero, this);

        // Carrier may stumble from the challenge
        if (carrier != null && carrier.Team != Team)
            carrier.MaybeStumbleFromChallenge(this);
    }

    public void MaybeStumbleFromChallenge(Player tackler)
    {
        if (_stumbleCooldown > 0f) return;
        float resist = Attr.Balance * 0.6f + Attr.Strength * 0.4f;
        float force = tackler.Attr.Strength * 0.7f
            + Mathf.Clamp(U.Flat(tackler.Velocity).Length() / 9f, 0f, 1f) * 0.5f;
        if (force > resist + 0.15f)
        {
            Enter(PState.Stumble, T.StumbleDuration);
            _stumbleCooldown = 1.5f;
        }
    }

    // =====================================================================
    //  BODY CONTACT: shoulder-to-shoulder outcomes
    // =====================================================================
    private void TickContacts()
    {
        if (_stumbleCooldown > 0f || State == PState.Stumble) return;
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            var col = GetSlideCollision(i);
            if (col.GetCollider() is Player other && other.Team != Team)
            {
                float mySpeed = U.Flat(Velocity).Length();
                float otherSpeed = U.Flat(other.Velocity).Length();
                float myPower = mySpeed * (0.5f + Attr.Strength) * (0.6f + Attr.Balance * 0.4f);
                float otherPower = otherSpeed * (0.5f + other.Attr.Strength)
                    * (0.6f + other.Attr.Balance * 0.4f);

                if (CmdShield) myPower *= 1.35f;
                if (other.CmdShield) otherPower *= 1.35f;

                if (otherPower > myPower + T.StumbleSpeedThreshold)
                {
                    Enter(PState.Stumble, T.StumbleDuration);
                    _stumbleCooldown = 1.5f;
                }
                break;
            }
        }
    }

    // =====================================================================
    //  GOALKEEPER
    // =====================================================================
    public void StartDive(Vector3 dir)
    {
        if (IsBusy) return;
        Facing = U.Flat(dir).Normalized();
        Enter(PState.GkDive, 0.45f);
    }

    // =====================================================================
    //  HOUSEKEEPING
    // =====================================================================
    private void ClampToArena()
    {
        Vector3 p = GlobalPosition;
        p.X = Mathf.Clamp(p.X, -MatchController.HalfLen - 2.5f, MatchController.HalfLen + 2.5f);
        p.Z = Mathf.Clamp(p.Z, -MatchController.HalfWid - 2.5f, MatchController.HalfWid + 2.5f);
        if (p != GlobalPosition) GlobalPosition = p;
    }

    private void UpdateVisualFacing()
    {
        // Rotate only the Visual node so the collision capsule stays upright.
        if (_nose == null || Facing.LengthSquared() < 0.01f) return;
        var visual = _nose as Node3D;
        if (visual == null) return;
        // Yaw only: atan2 of facing X/Z
        visual.Rotation = new Vector3(0, Mathf.Atan2(-Facing.X, -Facing.Z), 0);
    }
}
