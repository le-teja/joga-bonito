using Godot;

namespace FootballProto;

/// <summary>
/// The match ball. RigidBody3D with custom rolling friction, air drag and a
/// simple Magnus (spin curl) approximation. The ball is never parented to a
/// player — possession is purely spatial.
/// </summary>
public partial class Ball : RigidBody3D
{
    public const float Radius = 0.22f;

    /// <summary>Angular spin vector used for Magnus curl (not visual rotation).</summary>
    public Vector3 Spin = Vector3.Zero;

    /// <summary>Last player to deliberately touch the ball.</summary>
    public Player LastTouch;

    private bool _held;
    private Vector3 _holdPos;
    private GameplayTuning T => App.I.Tuning;

    public bool IsHeld => _held;

    private MeshInstance3D _mesh;

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 4;
        CanSleep = false;
        _mesh = GetNodeOrNull<MeshInstance3D>("Mesh");
        ApplyLeatherMaterial();
    }

    private void ApplyLeatherMaterial()
    {
        if (_mesh == null) return;
        var mat = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.92f, 0.88f, 0.78f),
            Roughness   = 0.68f,
            Metallic    = 0f
        };
        if (ResourceLoader.Exists("res://assets/textures/ball_albedo.jpg"))
            mat.AlbedoTexture = ResourceLoader.Load<Texture2D>("res://assets/textures/ball_albedo.jpg");
        if (ResourceLoader.Exists("res://assets/textures/ball_normal.jpg"))
        {
            mat.NormalEnabled = true;
            mat.NormalTexture = ResourceLoader.Load<Texture2D>("res://assets/textures/ball_normal.jpg");
            mat.NormalScale   = 1.4f;
        }
        if (ResourceLoader.Exists("res://assets/textures/ball_roughness.jpg"))
            mat.RoughnessTexture = ResourceLoader.Load<Texture2D>("res://assets/textures/ball_roughness.jpg");
        _mesh.MaterialOverride = mat;
    }

    public override void _Process(double delta)
    {
        // Visually rotate the mesh to show spin — cosmetic only, physics spin is separate
        if (_mesh == null || _held) return;
        Vector3 vel = LinearVelocity;
        float speed = vel.Length();
        if (speed > 0.5f)
        {
            // Rotate around the axis perpendicular to velocity (rolling appearance)
            Vector3 axis = vel.Cross(Vector3.Up).Normalized();
            if (axis.LengthSquared() > 0.01f)
                _mesh.RotateObjectLocal(axis, speed * (float)delta / Radius);
        }
        // Side spin from Magnus
        if (Spin.LengthSquared() > 0.1f)
            _mesh.RotateObjectLocal(Spin.Normalized(), Spin.Length() * (float)delta * 0.8f);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        if (_held)
        {
            state.Transform = new Transform3D(Basis.Identity, _holdPos);
            state.LinearVelocity = Vector3.Zero;
            state.AngularVelocity = Vector3.Zero;
            return;
        }

        Vector3 v = state.LinearVelocity;
        bool grounded = GlobalPosition.Y <= Radius + 0.05f && Mathf.Abs(v.Y) < 1.2f;

        if (grounded)
        {
            // Constant-deceleration rolling friction on grass
            Vector3 flat = U.Flat(v);
            float sp = flat.Length();
            if (sp > 0.05f)
            {
                float drop = T.BallRollFriction * (float)state.Step;
                float ns = Mathf.Max(sp - drop, 0f);
                Vector3 nf = flat.Normalized() * ns;
                state.LinearVelocity = new Vector3(nf.X, v.Y, nf.Z);
            }
            else
            {
                state.LinearVelocity = new Vector3(0f, v.Y, 0f);
            }
            Spin = Spin.Lerp(Vector3.Zero, 4f * (float)state.Step);
        }
        else
        {
            // Air drag + Magnus curl
            state.LinearVelocity -= v * T.BallAirDrag * (float)state.Step;
            if (Spin.LengthSquared() > 0.01f)
            {
                Vector3 magnus = Spin.Cross(v) * T.MagnusFactor;
                state.LinearVelocity += magnus * (float)state.Step;
                Spin = Spin.Lerp(Vector3.Zero, 0.8f * (float)state.Step);
            }
        }
    }

    /// <summary>Apply a deliberate kick: sets velocity, spin and touch credit.</summary>
    public void KickBall(Vector3 velocity, Vector3 spin, Player kicker)
    {
        Release();
        LinearVelocity = velocity;
        Spin = spin;
        LastTouch = kicker;
    }

    /// <summary>Freeze the ball at a position (kickoff, goal reset, GK catch).</summary>
    public void Hold(Vector3 pos)
    {
        _held = true;
        _holdPos = pos;
    }

    public void Release() => _held = false;
}
