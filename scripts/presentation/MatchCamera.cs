using Godot;

namespace FootballProto;

/// <summary>
/// Broadcast-style camera on the +Z side looking toward -Z (identity yaw, so
/// screen-right = world +X and stick mapping in HumanDriver stays trivial).
/// Tracks a blend of ball (with velocity lead) and the controlled player,
/// pulls back and widens FOV as play speeds up.
/// </summary>
public partial class MatchCamera : Camera3D
{
    private MatchController _match;
    private Vector3 _smoothTarget;

    public void Init(MatchController match)
    {
        _match = match;
        Current = true;
        _smoothTarget = Vector3.Zero;
    }

    public override void _Process(double delta)
    {
        if (_match == null || _match.Ball == null) return;
        float dt = (float)delta;

        Ball ball = _match.Ball;
        Vector3 ballPos = ball.GlobalPosition;
        float ballSpeed = U.Flat(ball.LinearVelocity).Length();

        // Lead the ball slightly in its direction of travel
        Vector3 lead = U.Flat(ball.LinearVelocity) * 0.35f;
        Vector3 focus = ballPos + lead;

        Player ctl = _match.Human?.Controlled;
        if (ctl != null && GodotObject.IsInstanceValid(ctl))
            focus = focus * 0.7f + ctl.GlobalPosition * 0.3f;

        // Keep the frame inside the pitch-ish bounds
        focus.X = Mathf.Clamp(focus.X, -MatchController.HalfLen + 6f, MatchController.HalfLen - 6f);
        focus.Z = Mathf.Clamp(focus.Z, -MatchController.HalfWid + 4f, MatchController.HalfWid - 4f);

        // Exponential smoothing
        float k = 1f - Mathf.Exp(-4.5f * dt);
        _smoothTarget = _smoothTarget.Lerp(focus, k);

        float speed01 = Mathf.Clamp(ballSpeed / 25f, 0f, 1f);
        float height = Mathf.Lerp(24f, 29f, speed01);
        float dist = Mathf.Lerp(24f, 28f, speed01);

        GlobalPosition = _smoothTarget + new Vector3(0, height, dist);
        LookAt(_smoothTarget, Vector3.Up);
        Fov = Mathf.Lerp(35f, 40f, speed01);
    }
}
