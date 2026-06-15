using Godot;

namespace FootballProto;

/// <summary>
/// Every tunable gameplay constant lives here. Two presets ship as .tres files
/// in res://data (tuning_sim.tres, tuning_assisted.tres). If a resource fails
/// to load, the code falls back to MakeSim() so the game stays playable.
/// </summary>
[GlobalClass]
public partial class GameplayTuning : Resource
{
    // ---------- Player movement ----------
    [Export] public float Accel = 7.5f;                // m/s^2 toward desired velocity
    [Export] public float Decel = 11f;                 // m/s^2 when stopping
    [Export] public float TurnDegLowSpeed = 680f;      // deg/s turn rate when slow
    [Export] public float TurnDegHighSpeed = 150f;     // deg/s turn rate at top speed
    [Export] public float ShieldSpeed = 1.6f;          // m/s while shielding
    [Export] public float JockeySpeed = 3.4f;          // m/s while jockeying

    // ---------- Stamina ----------
    [Export] public float StaminaDrainSprint = 6.5f;   // per second sprinting
    [Export] public float StaminaRecover = 4.5f;       // per second not sprinting
    [Export] public float FatigueMinFactor = 0.82f;    // speed/sharpness floor at 0 stamina

    // ---------- Ball physics ----------
    [Export] public float BallRollFriction = 4.6f;     // m/s^2 rolling decel on grass
    [Export] public float BallAirDrag = 0.12f;         // proportional air drag
    [Export] public float MagnusFactor = 0.05f;        // spin curl strength

    // ---------- Possession / touch ----------
    [Export] public float ControlRadius = 1.7f;        // m, loose-ball claim radius
    [Export] public float DribbleTouchDist = 1.05f;    // m, nudge target ahead of foot
    [Export] public float DribbleTouchInterval = 0.55f;// s between dribble touches
    [Export] public float DribbleSpeedFactor = 1.12f;  // touch speed vs player speed
    [Export] public float FirstTouchSpeedThreshold = 5.5f; // incoming speed where touches degrade
    [Export] public float TouchErrorBase = 0.34f;      // base first-touch scatter
    [Export] public float ReceiveLockBase = 0.22f;     // s control lock on awkward touch

    // ---------- Kicking ----------
    [Export] public float KickRange = 1.55f;           // m, max foot-to-ball distance
    [Export] public float PassSpeedMin = 9f;
    [Export] public float PassSpeedMax = 21f;
    [Export] public float LobSpeedMin = 11f;
    [Export] public float LobSpeedMax = 24f;
    [Export] public float ThroughSpeedBonus = 1.18f;   // multiplier over ground pass
    [Export] public float ShotSpeedMin = 16f;
    [Export] public float ShotSpeedMax = 31f;
    [Export] public float FinessePowerScale = 0.8f;
    [Export] public float FinesseSideSpin = 7.5f;
    [Export] public float PassErrorDeg = 7f;           // aim error stddev at 0 skill
    [Export] public float ShotErrorDeg = 6f;
    [Export] public float AimAssist = 0.45f;           // 0 raw stick, 1 fully solved

    // ---------- Action timing ----------
    [Export] public float PassWindup = 0.12f;
    [Export] public float PassRecover = 0.18f;
    [Export] public float ShotWindupMin = 0.16f;
    [Export] public float ShotWindupMax = 0.30f;
    [Export] public float ShotRecover = 0.34f;
    [Export] public float PassChargeTime = 0.7f;       // s to full pass power
    [Export] public float ShotChargeTime = 1.1f;       // s to full shot power

    // ---------- Tackling ----------
    [Export] public float TackleWindup = 0.10f;
    [Export] public float TackleActive = 0.22f;
    [Export] public float TackleRecover = 0.45f;
    [Export] public float SlideActive = 0.55f;
    [Export] public float SlideRecover = 0.95f;
    [Export] public float SlideSpeed = 8.5f;
    [Export] public float TackleBaseChance = 0.65f;

    // ---------- Body contact ----------
    [Export] public float StumbleSpeedThreshold = 4.5f;
    [Export] public float StumbleDuration = 0.55f;

    // ---------- AI ----------
    [Export] public float AiPressTackleDist = 1.9f;
    [Export] public float AiShootRange = 23f;
    [Export] public float AiPassPressureDist = 2.8f;

    public static GameplayTuning MakeSim() => new GameplayTuning();

    public static GameplayTuning MakeAssisted()
    {
        var t = new GameplayTuning
        {
            Accel = 9.0f,
            Decel = 13f,
            TurnDegLowSpeed = 760f,
            TurnDegHighSpeed = 210f,
            TouchErrorBase = 0.18f,
            ReceiveLockBase = 0.14f,
            FirstTouchSpeedThreshold = 7.0f,
            PassErrorDeg = 4f,
            ShotErrorDeg = 3.5f,
            AimAssist = 0.8f,
            TackleBaseChance = 0.72f,
            StumbleSpeedThreshold = 5.5f
        };
        return t;
    }
}
