using Godot;

namespace FootballProto;

/// <summary>
/// Drives one goalkeeper. GKs are always AI-controlled (player switching
/// excludes them). Behaviours: arc positioning between goal and ball,
/// shot detection with intercept-or-dive response, claiming slow balls in
/// the box, and timed distribution to an open defender after a catch.
/// </summary>
public class GoalkeeperBrain
{
    public Player Gk;

    private MatchController _m;
    private GameplayTuning T => App.I.Tuning;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _holdTimer;

    private float GoalX => Gk.Team == 0 ? -MatchController.HalfLen : MatchController.HalfLen;
    private Vector3 GoalCenter => new Vector3(GoalX, 0, 0);

    public GoalkeeperBrain(MatchController m, Player gk)
    {
        _m = m;
        Gk = gk;
        _rng.Randomize();
    }

    public void Process(float dt)
    {
        if (_m.Phase != MatchPhase.InPlay)
        {
            Gk.CmdMove = Vector3.Zero;
            return;
        }

        // Holding the ball after a catch: wait then distribute
        if (_m.GkHolder == Gk)
        {
            Gk.CmdMove = Vector3.Zero;
            _holdTimer += dt;
            _m.Ball.Hold(Gk.GlobalPosition + Gk.Facing * 0.5f + Vector3.Up * 1.0f);
            if (_holdTimer > 1.6f) Distribute();
            return;
        }
        _holdTimer = 0f;

        Ball ball = _m.Ball;
        Vector3 ballPos = ball.GlobalPosition;
        Vector3 ballVel = ball.LinearVelocity;
        float ballSpeed = U.Flat(ballVel).Length();

        // ---- Shot detection: fast ball travelling toward our goal line ----
        bool towardGoal = Gk.Team == 0 ? ballVel.X < -1f : ballVel.X > 1f;
        if (ballSpeed > 9f && towardGoal)
        {
            float vx = Mathf.Abs(ballVel.X);
            float timeToLine = Mathf.Abs(ballPos.X - GoalX) / Mathf.Max(vx, 0.1f);
            if (timeToLine < 1.3f)
            {
                // Predicted crossing point on the goal line
                float crossZ = ballPos.Z + ballVel.Z * timeToLine;
                Vector3 intercept = new Vector3(GoalX + (Gk.Team == 0 ? 1.2f : -1.2f), 0,
                    Mathf.Clamp(crossZ, -4.2f, 4.2f));
                float lateral = U.FlatDist(Gk.GlobalPosition, intercept);

                if (lateral < 3.5f && timeToLine < 0.55f)
                {
                    Gk.StartDive(intercept - Gk.GlobalPosition);
                    TryStop(ball);
                }
                else
                {
                    MoveTo(intercept, sprint: true);
                    TryStop(ball);
                }
                return;
            }
        }

        // ---- Claim slow balls in our box ----
        bool inBox = Mathf.Abs(ballPos.X - GoalX) < 16.5f && Mathf.Abs(ballPos.Z) < 20.16f;
        if (inBox && ballSpeed < 6f
            && U.FlatDist(Gk.GlobalPosition, ballPos) < 7f
            && (_m.Carrier == null || _m.Carrier.Team == Gk.Team))
        {
            MoveTo(ballPos, sprint: true);
            if (U.FlatDist(Gk.GlobalPosition, ballPos) < 1.0f && ballSpeed < 6f)
                Catch(ball);
            return;
        }

        // ---- Default arc positioning between goal centre and the ball ----
        Vector3 toBall = U.Flat(ballPos - GoalCenter);
        float off = Mathf.Clamp(toBall.Length() * 0.08f, 0.6f, 4.5f);
        Vector3 home = GoalCenter + (toBall.LengthSquared() > 0.01f
            ? toBall.Normalized() * off : new Vector3(Gk.Team == 0 ? 1 : -1, 0, 0) * off);
        home.Z = Mathf.Clamp(home.Z, -5.5f, 5.5f);
        MoveTo(home, sprint: false);
    }

    /// <summary>Catch slow shots, parry fast ones.</summary>
    private void TryStop(Ball ball)
    {
        if (U.FlatDist(Gk.GlobalPosition, ball.GlobalPosition) > 1.6f) return;
        if (ball.GlobalPosition.Y > 2.3f) return;

        float speed = ball.LinearVelocity.Length();
        if (speed < 23f)
        {
            Catch(ball);
        }
        else
        {
            // Parry: reflect outward and upfield
            Vector3 reflect = U.Flat(ball.LinearVelocity) * -0.25f
                + new Vector3((Gk.Team == 0 ? 1 : -1) * 4f, 2.5f,
                    _rng.RandfRange(-5f, 5f));
            ball.KickBall(reflect, Vector3.Zero, Gk);
        }
    }

    private void Catch(Ball ball)
    {
        ball.Hold(Gk.GlobalPosition + Gk.Facing * 0.5f + Vector3.Up * 1.0f);
        ball.LastTouch = Gk;
        _m.GkCatch(Gk);
        _holdTimer = 0f;
    }

    private void Distribute()
    {
        // Find the most open defender (fall back to any mate)
        Player best = null;
        float bestOpen = -1f;
        foreach (Player p in _m.Players)
        {
            if (p.Team != Gk.Team || p == Gk) continue;
            float open = 99f;
            Player opp = _m.NearestOpponent(p);
            if (opp != null) open = U.FlatDist(p.GlobalPosition, opp.GlobalPosition);
            if (p.Role == PRole.DEF) open += 3f; // prefer defenders
            if (open > bestOpen) { bestOpen = open; best = p; }
        }
        if (best == null) return;

        _m.GkRelease(Gk);
        Vector3 from = _m.Ball.GlobalPosition;
        float d = U.FlatDist(from, best.GlobalPosition);
        Vector3 vel = d < 25f
            ? KickMath.GroundPass(from, best.GlobalPosition, 0.55f, 0.7f, T, _rng)
            : KickMath.Lob(from, best.GlobalPosition, 0.7f, 0.7f, T, _rng);
        _m.Ball.KickBall(vel, Vector3.Zero, Gk);
    }

    private void MoveTo(Vector3 target, bool sprint)
    {
        Vector3 to = U.Flat(target - Gk.GlobalPosition);
        float d = to.Length();
        if (d < 0.35f) { Gk.CmdMove = Vector3.Zero; return; }
        Gk.CmdMove = to / d * Mathf.Clamp(d / 1.5f, 0.4f, 1f);
        Gk.CmdSprint = sprint;
    }
}
