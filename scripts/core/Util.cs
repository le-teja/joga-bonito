using Godot;

namespace FootballProto;

public static class U
{
    /// <summary>Flatten a vector onto the pitch plane (y = 0).</summary>
    public static Vector3 Flat(Vector3 v) => new Vector3(v.X, 0f, v.Z);

    /// <summary>Flat distance between two points, ignoring height.</summary>
    public static float FlatDist(Vector3 a, Vector3 b) => Flat(a - b).Length();

    /// <summary>Seconds since engine start, as float.</summary>
    public static float Now() => Time.GetTicksMsec() / 1000f;
}
