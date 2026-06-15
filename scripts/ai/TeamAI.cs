using Godot;
using System.Collections.Generic;

namespace FootballProto;

/// <summary>
/// Deterministic, practical team AI. Drives every AI-controlled outfielder
/// each tick by writing their command fields:
/// - formation home positions elastically shifted by ball position
/// - primary presser with hysteresis + optional called second presser
/// - loose-ball chasing by the two closest players
/// - on-ball decisions: shoot / pass / dribble, evaluated on a slow tick
/// - support runs ahead and wide of the carrier
/// </summary>
public class TeamAI
{
    public int Team;
    public float PressTimer; // second presser active while > 0

    private MatchController _m;
    private GameplayTuning T => App.I.Tuning;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();

    private Player _presser;
    private float _decideTimer;
    private float _tackleTick;

    public TeamAI(MatchController m, int team)
    {
        _m = m;
        Team = team;
        _rng.Randomize();
    }

    private float AttackSign => Team == 0 ? 1f : -1f;

    public void Process(float dt)
    {
        PressTimer -= dt;
        _decideTimer -= dt;
        _tackleTick -= dt;

        if (_m.Phase == MatchPhase.Kickoff) { ProcessKickoff(dt); return; }
        if (_m.Phase != MatchPhase.InPlay) { HoldAll(); return; }

        bool wePossess = _m.TeamInPossession == Team;
        bool looseBall = _m.TeamInPossession == -1;

        UpdatePresser();

        var chasers = looseBall ? PickLooseBallChasers() : null;

        foreach (Player p in _m.Players)
        {
            if (p.Team != Team) continue;
            if (p == _m.HumanControlled(Team)) continue;
            if (p.Role == PRole.GK) continue; // GoalkeeperBrain owns the GK

            p.CmdSprint = false;
            p.CmdShield = false;
            p.CmdJockey = false;

            if (wePossess)
            {
                if (_m.Carrier == p) DriveCarrier(p);
                else DriveSupport(p);
            }
            else if (looseBall && chasers != null && chasers.Contains(p))
            {
                DriveChase(p);
            }
            else
            {
                DriveDefend(p);
            }
        }
    }

    // =====================================================================
    private void ProcessKickoff(float dt)
    {
        foreach (Player p in _m.Players)
        {
            if (p.Team != Team || p.Role == PRole.GK) continue;
            if (p == _m.HumanControlled(Team)) continue;

            if (_m.KickoffTeam == Team && p == _m.KickoffTaker)
            {
                // Walk to the ball and play a short pass to the nearest mid
                Vector3 toBall = U.Flat(_m.Ball.GlobalPosition - p.GlobalPosition);
                if (toBall.Length() > 1.0f)
                {
                    p.CmdMove = toBall.Normalized() * 0.6f;
                }
                else
                {
                    p.CmdMove = Vector3.Zero;
                    Player mate = NearestMate(p);
                    if (mate != null && p.CanAct)
                    {
                        Vector3 vel = KickMath.GroundPass(_m.Ball.GlobalPosition,
                            mate.GlobalPosition, 0.4f, p.Attr.Passing, T, _rng);
                        p.RequestKick(vel, Vector3.Zero, T.PassWindup, T.PassRecover);
                    }
                }
            }
            else
            {
                MoveToward(p, _m.HomePosition(p), sprint: false);
            }
        }
    }

    private void HoldAll()
    {
        foreach (Player p in _m.Players)
        {
            if (p.Team != Team) continue;
            p.CmdMove = Vector3.Zero;
            p.CmdSprint = false;
        }
    }

    // =====================================================================
    //  DEFENCE
    // =====================================================================
    private void UpdatePresser()
    {
        if (_m.TeamInPossession == Team) { _presser = null; return; }

        Player best = null;
        float bestD = float.MaxValue;
        Vector3 ballPos = _m.Ball.GlobalPosition;
        foreach (Player p in _m.Players)
        {
            if (p.Team != Team || p.Role == PRole.GK) continue;
            if (p == _m.HumanControlled(Team)) continue;
            float d = U.FlatDist(p.GlobalPosition, ballPos);
            if (d < bestD) { bestD = d; best = p; }
        }
        // Hysteresis: keep the current presser unless someone is clearly closer
        if (_presser != null && GodotObject.IsInstanceValid(_presser)
            && _presser != _m.HumanControlled(Team))
        {
            float curD = U.FlatDist(_presser.GlobalPosition, ballPos);
            if (curD < bestD + 2.5f) return;
        }
        _presser = best;
    }

    private void DriveDefend(Player p)
    {
        bool isPresser = p == _presser;
        bool isSecond = PressTimer > 0f && p == SecondPresser();

        if (isPresser || isSecond)
        {
            Player carrier = _m.Carrier;
            Vector3 target = carrier != null && carrier.Team != Team
                ? carrier.GlobalPosition + U.Flat(carrier.Velocity) * 0.25f
                : _m.Ball.GlobalPosition;
            MoveToward(p, target, sprint: true);

            // Attempt a tackle when close, on a slow randomised tick
            if (_tackleTick <= 0f)
            {
                _tackleTick = 0.2f;
                float d = U.FlatDist(p.GlobalPosition, _m.Ball.GlobalPosition);
                if (d < T.AiPressTackleDist && _rng.Randf() < 0.45f)
                    p.StartTackle();
            }
        }
        else
        {
            MoveToward(p, _m.HomePosition(p), sprint: false);
        }
    }

    private Player SecondPresser()
    {
        Player best = null;
        float bestD = float.MaxValue;
        foreach (Player p in _m.Players)
        {
            if (p.Team != Team || p.Role == PRole.GK || p == _presser) continue;
            if (p == _m.HumanControlled(Team)) continue;
            float d = U.FlatDist(p.GlobalPosition, _m.Ball.GlobalPosition);
            if (d < bestD) { bestD = d; best = p; }
        }
        return best;
    }

    private List<Player> PickLooseBallChasers()
    {
        // Predict where the rolling ball will stop: d = v^2 / (2a)
        Vector3 v = U.Flat(_m.Ball.LinearVelocity);
        float sp = v.Length();
        Vector3 stop = _m.Ball.GlobalPosition
            + (sp > 0.5f ? v.Normalized() * (sp * sp / (2f * T.BallRollFriction)) : Vector3.Zero);

        var sorted = new List<Player>();
        foreach (Player p in _m.Players)
        {
            if (p.Team != Team || p.Role == PRole.GK) continue;
            if (p == _m.HumanControlled(Team)) continue;
            sorted.Add(p);
        }
        sorted.Sort((a, b) =>
            U.FlatDist(a.GlobalPosition, stop).CompareTo(U.FlatDist(b.GlobalPosition, stop)));
        if (sorted.Count > 2) sorted.RemoveRange(2, sorted.Count - 2);
        _chaseTarget = stop;
        return sorted;
    }

    private Vector3 _chaseTarget;

    private void DriveChase(Player p)
    {
        MoveToward(p, _chaseTarget, sprint: true);
    }

    // =====================================================================
    //  ATTACK
    // =====================================================================
    private void DriveCarrier(Player p)
    {
        Vector3 goal = new Vector3(AttackSign * MatchController.HalfLen, 0, 0);
        float distGoal = U.FlatDist(p.GlobalPosition, goal);

        if (_decideTimer <= 0f)
        {
            _decideTimer = 0.3f;

            // Shoot?
            if (distGoal < T.AiShootRange && p.CanAct)
            {
                float angleOk = Mathf.Abs(p.GlobalPosition.Z) < 18f ? 1f : 0.3f;
                if (_rng.Randf() < 0.55f * angleOk)
                {
                    Vector3 target = _m.GoalTarget(Team, p.GlobalPosition, Vector3.Zero);
                    float power = Mathf.Clamp(distGoal / 26f + 0.35f, 0.4f, 1f);
                    var (vel, spin) = KickMath.Shot(_m.Ball.GlobalPosition, target,
                        power, p.Attr.Shooting, finesse: false, T, _rng);
                    p.RequestKick(vel, spin,
                        Mathf.Lerp(T.ShotWindupMin, T.ShotWindupMax, power), T.ShotRecover);
                    return;
                }
            }

            // Pass?
            float pressure = NearestOpponentDist(p);
            Player best = BestPassOption(p, out float bestScore);
            bool mustRelease = pressure < T.AiPassPressureDist;
            if (p.CanAct && best != null && (mustRelease || bestScore > 1.4f))
            {
                Vector3 tpos = best.GlobalPosition + U.Flat(best.Velocity) * 0.4f;
                Vector3 vel = KickMath.GroundPass(_m.Ball.GlobalPosition, tpos,
                    0.5f, p.Attr.Passing, T, _rng);
                p.RequestKick(vel, Vector3.Zero, T.PassWindup, T.PassRecover);
                return;
            }
        }

        // Dribble toward goal, biased away from the nearest opponent
        Vector3 dir = U.Flat(goal - p.GlobalPosition).Normalized();
        Player opp = _m.NearestOpponent(p);
        if (opp != null)
        {
            Vector3 away = U.Flat(p.GlobalPosition - opp.GlobalPosition);
            float d = away.Length();
            if (d < 4f && d > 0.05f)
                dir = (dir + away.Normalized() * (1.2f - d / 4f)).Normalized();
        }
        p.CmdMove = dir;
        p.CmdSprint = NearestOpponentDist(p) > 3.5f;
    }

    private void DriveSupport(Player p)
    {
        Player carrier = _m.Carrier;
        Vector3 home = _m.HomePosition(p);
        if (carrier == null) { MoveToward(p, home, false); return; }

        // Support point: ahead of carrier, offset laterally by side of pitch
        float lateral = Mathf.Sign(p.GlobalPosition.Z - carrier.GlobalPosition.Z);
        if (lateral == 0) lateral = p.Number % 2 == 0 ? 1 : -1;
        Vector3 support = carrier.GlobalPosition
            + new Vector3(AttackSign * 7f, 0, lateral * 7f);

        Vector3 target = home.Lerp(support, RoleSupportWeight(p.Role));
        MoveToward(p, target, sprint: p.Role == PRole.FWD);
    }

    private static float RoleSupportWeight(PRole r) => r switch
    {
        PRole.FWD => 0.75f,
        PRole.MID => 0.55f,
        PRole.DEF => 0.2f,
        _ => 0f
    };

    private Player BestPassOption(Player from, out float bestScore)
    {
        Player best = null;
        bestScore = 0f;
        foreach (Player mate in _m.Players)
        {
            if (mate.Team != Team || mate == from || mate.Role == PRole.GK) continue;
            float d = U.FlatDist(from.GlobalPosition, mate.GlobalPosition);
            if (d < 4f || d > 30f) continue;

            float progress = (mate.GlobalPosition.X - from.GlobalPosition.X) * AttackSign / 20f;
            float openness = Mathf.Clamp(NearestOpponentDist(mate) / 6f, 0f, 1f);
            float laneBlock = LaneBlocked(from.GlobalPosition, mate.GlobalPosition) ? 0.8f : 0f;
            float score = 1f + progress + openness - laneBlock - d / 60f;
            if (score > bestScore) { bestScore = score; best = mate; }
        }
        return best;
    }

    private bool LaneBlocked(Vector3 a, Vector3 b)
    {
        Vector3 ab = U.Flat(b - a);
        float len = ab.Length();
        if (len < 0.5f) return false;
        Vector3 dir = ab / len;
        foreach (Player opp in _m.Players)
        {
            if (opp.Team == Team) continue;
            Vector3 ap = U.Flat(opp.GlobalPosition - a);
            float t = ap.Dot(dir);
            if (t < 1f || t > len - 1f) continue;
            float perp = (ap - dir * t).Length();
            if (perp < 1.2f) return true;
        }
        return false;
    }

    // =====================================================================
    private float NearestOpponentDist(Player p)
    {
        Player o = _m.NearestOpponent(p);
        return o == null ? 99f : U.FlatDist(p.GlobalPosition, o.GlobalPosition);
    }

    private Player NearestMate(Player p)
    {
        Player best = null;
        float bestD = float.MaxValue;
        foreach (Player mate in _m.Players)
        {
            if (mate.Team != Team || mate == p || mate.Role == PRole.GK) continue;
            float d = U.FlatDist(p.GlobalPosition, mate.GlobalPosition);
            if (d < bestD) { bestD = d; best = mate; }
        }
        return best;
    }

    private void MoveToward(Player p, Vector3 target, bool sprint)
    {
        Vector3 to = U.Flat(target - p.GlobalPosition);
        float d = to.Length();
        if (d < 0.6f)
        {
            p.CmdMove = Vector3.Zero;
            return;
        }
        p.CmdMove = to / d * Mathf.Clamp(d / 2.5f, 0.4f, 1f);
        p.CmdSprint = sprint && d > 4f && p.Stamina > 15f;
    }
}
