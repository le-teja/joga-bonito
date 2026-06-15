using Godot;
using System;

namespace FootballProto;

/// <summary>
/// Pure functions that turn an intent (target, power, skill) into a ball
/// velocity + spin. Keeping this stateless makes future server-side reuse trivial.
/// </summary>
public static class KickMath
{
    /// <summary>
    /// Ground pass. Solves the speed needed for the ball to roll to the target
    /// under constant rolling friction, blends with raw power by AimAssist,
    /// then applies gaussian aim error scaled by (1 - skill).
    /// </summary>
    public static Vector3 GroundPass(Vector3 from, Vector3 target, float power01,
        float skill01, GameplayTuning t, RandomNumberGenerator rng)
    {
        Vector3 flat = U.Flat(target - from);
        float d = Mathf.Max(flat.Length(), 0.5f);
        Vector3 dir = flat / d;

        // v^2 = 2 * a * d  -> speed that just reaches the target
        float ideal = Mathf.Sqrt(2f * t.BallRollFriction * d) * 1.04f;
        float raw = Mathf.Lerp(t.PassSpeedMin, t.PassSpeedMax, power01);
        float speed = Mathf.Lerp(raw, ideal, t.AimAssist);
        speed = Mathf.Clamp(speed, t.PassSpeedMin * 0.8f, t.PassSpeedMax);

        float errDeg = t.PassErrorDeg * (1f - skill01 * 0.8f);
        float err = Mathf.DegToRad(rng.Randfn(0f, errDeg));
        dir = dir.Rotated(Vector3.Up, err);

        return dir * speed;
    }

    /// <summary>
    /// Lofted pass: ballistic solve for a flight time proportional to distance.
    /// </summary>
    public static Vector3 Lob(Vector3 from, Vector3 target, float power01,
        float skill01, GameplayTuning t, RandomNumberGenerator rng)
    {
        Vector3 flat = U.Flat(target - from);
        float d = Mathf.Max(flat.Length(), 2f);
        Vector3 dir = flat / d;

        float flight = Mathf.Clamp(d / 13f, 0.7f, 2.4f);
        float vHoriz = d / flight;
        vHoriz = Mathf.Clamp(vHoriz * Mathf.Lerp(0.85f, 1.15f, power01),
            t.LobSpeedMin * 0.5f, t.LobSpeedMax);
        float vy = 0.5f * 9.81f * flight; // reach apex at half flight

        float errDeg = t.PassErrorDeg * 1.3f * (1f - skill01 * 0.8f);
        dir = dir.Rotated(Vector3.Up, Mathf.DegToRad(rng.Randfn(0f, errDeg)));

        return dir * vHoriz + Vector3.Up * vy;
    }

    /// <summary>
    /// Shot. Power controls speed and a modest rising trajectory. Finesse trades
    /// power for accuracy and adds side spin for curl.
    /// </summary>
    public static (Vector3 vel, Vector3 spin) Shot(Vector3 from, Vector3 target,
        float power01, float skill01, bool finesse, GameplayTuning t,
        RandomNumberGenerator rng)
    {
        Vector3 flat = U.Flat(target - from);
        float d = Mathf.Max(flat.Length(), 1f);
        Vector3 dir = flat / d;

        float speed = Mathf.Lerp(t.ShotSpeedMin, t.ShotSpeedMax, power01);
        float errDeg = t.ShotErrorDeg * (1f - skill01 * 0.75f);
        if (finesse)
        {
            speed *= t.FinessePowerScale;
            errDeg *= 0.55f;
        }
        // High power costs accuracy
        errDeg *= Mathf.Lerp(0.8f, 1.5f, power01);

        dir = dir.Rotated(Vector3.Up, Mathf.DegToRad(rng.Randfn(0f, errDeg)));

        // Target height: low drilled shots at low power, rising at high power
        float targetH = Mathf.Lerp(0.3f, 1.9f, power01 * power01);
        float time = d / speed;
        float vy = (targetH / Mathf.Max(time, 0.1f)) + 0.5f * 9.81f * time * 0.35f;
        vy = Mathf.Clamp(vy, 0.5f, 8f);

        Vector3 vel = dir * speed + Vector3.Up * vy;

        Vector3 spin = Vector3.Zero;
        if (finesse)
        {
            // Curl toward goal centre: spin axis up, sign from shot side
            float side = Mathf.Sign(from.Z - target.Z);
            if (side == 0) side = 1;
            spin = Vector3.Up * t.FinesseSideSpin * side;
        }
        return (vel, spin);
    }

    /// <summary>
    /// Through-ball target: lead the receiver by their velocity plus a bias
    /// toward the attacking direction.
    /// </summary>
    public static Vector3 ThroughTarget(Player receiver, Vector3 attackDir)
    {
        return receiver.GlobalPosition
            + U.Flat(receiver.Velocity) * 0.9f
            + attackDir * 3f;
    }
}
