using Godot;

namespace FootballProto;

/// <summary>
/// Per-player 0..1 attributes. Generated from role ranges in
/// res://data/attributes.json with random variation, so squads feel
/// slightly different each match. Falls back to defaults if JSON missing.
/// </summary>
public class PlayerAttributes
{
    public float TopSpeed = 0.6f;
    public float Acceleration = 0.6f;
    public float Agility = 0.6f;
    public float Balance = 0.6f;
    public float Strength = 0.6f;
    public float BallControl = 0.6f;
    public float Passing = 0.6f;
    public float Shooting = 0.55f;
    public float DefensiveReach = 0.55f;
    public float Stamina = 0.7f;

    private static Godot.Collections.Dictionary _data;

    public static PlayerAttributes ForRole(PRole role, RandomNumberGenerator rng)
    {
        LoadData();
        var a = new PlayerAttributes();
        if (_data == null) return a;

        string key = role.ToString();
        if (!_data.ContainsKey(key)) return a;
        var roleDict = _data[key].AsGodotDictionary();

        a.TopSpeed = Sample(roleDict, "top_speed", rng, a.TopSpeed);
        a.Acceleration = Sample(roleDict, "acceleration", rng, a.Acceleration);
        a.Agility = Sample(roleDict, "agility", rng, a.Agility);
        a.Balance = Sample(roleDict, "balance", rng, a.Balance);
        a.Strength = Sample(roleDict, "strength", rng, a.Strength);
        a.BallControl = Sample(roleDict, "ball_control", rng, a.BallControl);
        a.Passing = Sample(roleDict, "passing", rng, a.Passing);
        a.Shooting = Sample(roleDict, "shooting", rng, a.Shooting);
        a.DefensiveReach = Sample(roleDict, "defensive_reach", rng, a.DefensiveReach);
        a.Stamina = Sample(roleDict, "stamina", rng, a.Stamina);
        return a;
    }

    private static float Sample(Godot.Collections.Dictionary roleDict, string key,
        RandomNumberGenerator rng, float fallback)
    {
        if (!roleDict.ContainsKey(key)) return fallback;
        var range = roleDict[key].AsGodotArray();
        if (range.Count < 2) return fallback;
        float lo = (float)range[0].AsDouble() / 100f;
        float hi = (float)range[1].AsDouble() / 100f;
        return Mathf.Clamp(rng.RandfRange(lo, hi), 0f, 1f);
    }

    private static void LoadData()
    {
        if (_data != null) return;
        const string path = "res://data/attributes.json";
        if (!FileAccess.FileExists(path)) return;
        using var f = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var parsed = Json.ParseString(f.GetAsText());
        if (parsed.VariantType == Variant.Type.Dictionary)
            _data = parsed.AsGodotDictionary();
    }
}
