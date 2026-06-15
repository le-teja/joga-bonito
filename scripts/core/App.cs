using Godot;

namespace FootballProto;

public enum GamePreset { Sim, Assisted }

/// <summary>
/// Autoload singleton. Holds menu selections and the active tuning resource.
/// Registered in project.godot as "App".
/// </summary>
public partial class App : Node
{
    public static App I { get; private set; }

    public GamePreset Preset = GamePreset.Assisted;
    public int TeamSize = 11;        // 11 or 7
    public int MatchMinutes = 5;     // real-time minutes per match
    public GameplayTuning Tuning { get; private set; }

    public override void _Ready()
    {
        I = this;
        ProcessMode = ProcessModeEnum.Always;
        LoadTuning();
    }

    public void LoadTuning()
    {
        string path = Preset == GamePreset.Sim
            ? "res://data/tuning_sim.tres"
            : "res://data/tuning_assisted.tres";

        GameplayTuning t = null;
        if (ResourceLoader.Exists(path))
            t = ResourceLoader.Load<GameplayTuning>(path);

        Tuning = t ?? (Preset == GamePreset.Sim
            ? GameplayTuning.MakeSim()
            : GameplayTuning.MakeAssisted());
    }
}
