using Godot;

namespace FootballProto;

/// <summary>
/// Drives the AnimationTree on the Mixamo Soccer Player character.
/// Sits as a child of the Player node, holds a reference to the AnimationTree
/// and translates Player.State + velocity into animation states each frame.
///
/// Animation file → tree node name mapping (all your exact Mixamo filenames):
///
/// LOCOMOTION BLEND (BlendSpace2D, axes: speed -1..1 forward, -1..1 strafe):
///   idle              → "offensive idle.fbx"   (standing still)
///   jog_fwd           → "jog forward.fbx"
///   jog_bwd           → "jog backward.fbx"
///   jog_left          → "jog strafe left.fbx"
///   jog_right         → "jog strafe right.fbx"
///   jog_fwd_diag_l    → "jog forward diagonal.fbx"
///   jog_fwd_diag_r    → "jog forward diagonal (2).fbx"
///   jog_bwd_diag_l    → "jog backward diagonal.fbx"
///   jog_bwd_diag_r    → "jog backward diagonal (2).fbx"
///   strike_fwd        → "strike foward jog.fbx"  (full sprint)
///
/// ACTION STATES (one-shots triggered by PState):
///   kick              → "kick soccerball.fbx"
///   kick2             → "kick soccerball (2).fbx"
///   kick_power        → "soccer penalty kick.fbx"
///   kick_scissor      → "scissor kick.fbx"
///   kick_up           → "kick up soccerball.fbx"
///   tackle            → "soccer tackle.fbx"
///   tackle2           → "soccer tackle (2).fbx"
///   tackle3           → "soccer tackle (3).fbx"
///   slide             → "soccer trip.fbx"
///   stumble           → "standing up.fbx"
///   receive           → "receive soccerball.fbx"
///   header            → "header soccerball.fbx"
///   stall             → "stall soccerball.fbx"
///   transition        → "transition.fbx"
///
/// GK STATES:
///   gk_idle           → "goalkeeper idle.fbx"
///   gk_dive_l         → "goalkeeper sidestep.fbx"
///   gk_dive_r         → "goalkeeper sidestep (2).fbx"
///   gk_catch          → "goalkeeper scoop.fbx"
///   gk_throw          → "goalkeeper overhand throw.fbx"
///   gk_pass           → "goalkeeper pass.fbx"
///   gk_kick           → "goalkeeper drop kick.fbx"
///   gk_place          → "goalkeeper placing ball.fbx"
///   gk_miss           → "goalkeeper miss.fbx"
/// </summary>
public partial class PlayerAnimator : Node
{
    private Player _player;
    private AnimationTree _tree;
    private AnimationNodeStateMachinePlayback _sm;

    // Locomotion blend parameters
    private const string ParamBlendPos = "parameters/Locomotion/blend_position";
    private const string ParamStateMachine = "parameters/playback";

    // State machine node names
    private const string StateLocomotion = "Locomotion";
    private const string StateKick = "Kick";
    private const string StateKick2 = "Kick2";
    private const string StateKickPower = "KickPower";
    private const string StateKickScissor = "KickScissor";
    private const string StateTackle = "Tackle";
    private const string StateTackle2 = "Tackle2";
    private const string StateSlide = "Slide";
    private const string StateStumble = "Stumble";
    private const string StateReceive = "Receive";
    private const string StateHeader = "Header";
    private const string StateGkIdle = "GkIdle";
    private const string StateGkDiveL = "GkDiveL";
    private const string StateGkDiveR = "GkDiveR";
    private const string StateGkCatch = "GkCatch";
    private const string StateGkThrow = "GkThrow";

    private PState _lastState = PState.Normal;
    private float _kickVariantTimer;
    private int _kickVariant;

    public override void _Ready()
    {
        _player = GetParent<Player>();
        _tree = GetNodeOrNull<AnimationTree>("../Visual/AnimationTree");
        if (_tree == null) return;

        _tree.Active = true;
        _sm = (AnimationNodeStateMachinePlayback)_tree.Get(ParamStateMachine);
    }

    public override void _Process(double delta)
    {
        if (_tree == null || _sm == null || _player == null) return;
        _kickVariantTimer -= (float)delta;

        UpdateState();
        UpdateLocomotionBlend();
    }

    private void UpdateState()
    {
        PState s = _player.State;
        if (s == _lastState) return;
        _lastState = s;

        switch (s)
        {
            case PState.Normal:
            case PState.KickRecover:
                _sm.Travel(StateLocomotion);
                break;

            case PState.WindUp:
                // Rotate through kick variants for visual variety
                _kickVariant = _kickVariantTimer > 0f ? _kickVariant : GD.RandRange(0, 2);
                _kickVariantTimer = 0.8f;
                string kickState = _kickVariant switch
                {
                    0 => StateKick,
                    1 => StateKick2,
                    _ => _player.CmdSprint ? StateKickScissor : StateKickPower
                };
                _sm.Travel(kickState);
                break;

            case PState.TackleWind:
            case PState.TackleActive:
            case PState.TackleRecover:
                _sm.Travel(_kickVariant % 2 == 0 ? StateTackle : StateTackle2);
                break;

            case PState.SlideActive:
            case PState.SlideRecover:
                _sm.Travel(StateSlide);
                break;

            case PState.Stumble:
                _sm.Travel(StateStumble);
                break;

            case PState.Receive:
                _sm.Travel(StateReceive);
                break;

            case PState.GkDive:
                // Pick dive direction based on player facing vs ball
                bool diveLeft = _player.Facing.Z > 0;
                _sm.Travel(diveLeft ? StateGkDiveL : StateGkDiveR);
                break;

            case PState.GkGetUp:
                _sm.Travel(StateGkCatch);
                break;
        }
    }

    private void UpdateLocomotionBlend()
    {
        if (_sm.GetCurrentNode() != StateLocomotion) return;

        Vector3 vel = U.Flat(_player.Velocity);
        float speed = vel.Length();
        float maxSpeed = _player.TopSpeedMs;

        if (speed < 0.4f)
        {
            // Idle: use GK idle for goalkeeper
            _tree.Set(ParamBlendPos, Vector2.Zero);
            return;
        }

        // Project velocity onto player's local axes for directional blending
        Vector3 facing = _player.Facing.LengthSquared() > 0.01f
            ? _player.Facing : Vector3.Forward;
        Vector3 right = facing.Cross(Vector3.Up).Normalized();

        float fwd = vel.Dot(facing) / maxSpeed;
        float strafe = vel.Dot(right) / maxSpeed;

        // Sprint: blend toward forward max at high speed
        float sprintBlend = Mathf.Clamp(speed / maxSpeed, 0f, 1f);
        fwd = Mathf.Lerp(fwd, 1f, sprintBlend * (_player.CmdSprint ? 0.6f : 0f));

        _tree.Set(ParamBlendPos, new Vector2(strafe, fwd));
    }
}
