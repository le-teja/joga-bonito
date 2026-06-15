using Godot;

namespace FootballProto;

/// <summary>
/// Main menu, built in code: preset, team size and match length selectors,
/// Start and Quit, plus a controls hint.
/// </summary>
public partial class MainMenu : Control
{
    private OptionButton _preset;
    private OptionButton _size;
    private OptionButton _length;

    public override void _Ready()
    {
        var bg = new ColorRect
        {
            Color = new Color(0.07f, 0.12f, 0.08f),
            AnchorRight = 1, AnchorBottom = 1
        };
        AddChild(bg);

        var v = new VBoxContainer
        {
            AnchorLeft = 0.5f, AnchorRight = 0.5f, AnchorTop = 0.5f, AnchorBottom = 0.5f,
            OffsetLeft = -220, OffsetRight = 220, OffsetTop = -230, OffsetBottom = 230
        };
        v.AddThemeConstantOverride("separation", 16);
        AddChild(v);

        var title = new Label { Text = "FOOTY PROTO", HorizontalAlignment = HorizontalAlignment.Center };
        title.AddThemeFontSizeOverride("font_size", 52);
        v.AddChild(title);

        var sub = new Label
        {
            Text = "Local football prototype — Godot 4 + C#",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        sub.AddThemeFontSizeOverride("font_size", 16);
        v.AddChild(sub);

        v.AddChild(new HSeparator());

        _preset = MakeOption(v, "Gameplay preset",
            new[] { "Assisted (recommended)", "Sim" }, 0);
        _size = MakeOption(v, "Team size", new[] { "11 v 11", "7 v 7" }, 0);
        _length = MakeOption(v, "Match length", new[] { "3 min", "5 min", "8 min" }, 1);

        v.AddChild(new HSeparator());

        var start = new Button { Text = "START MATCH" };
        start.AddThemeFontSizeOverride("font_size", 26);
        start.Pressed += OnStart;
        v.AddChild(start);

        var quit = new Button { Text = "Quit" };
        quit.Pressed += () => GetTree().Quit();
        v.AddChild(quit);

        var hint = new Label
        {
            Text = "Pad: A pass · B shoot/tackle · X lob/slide · Y through · RT sprint · LT shield/jockey · LB switch · RB finesse/press\n" +
                   "Keys: WASD move · Space pass · E shoot/tackle · Q lob/slide · R through · Shift sprint · Ctrl shield · Tab switch · F finesse",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        hint.AddThemeFontSizeOverride("font_size", 13);
        v.AddChild(hint);
    }

    private static OptionButton MakeOption(VBoxContainer parent, string label,
        string[] items, int selected)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 12);
        parent.AddChild(row);

        var l = new Label { Text = label, CustomMinimumSize = new Vector2(160, 0) };
        l.AddThemeFontSizeOverride("font_size", 18);
        row.AddChild(l);

        var ob = new OptionButton { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        foreach (var item in items) ob.AddItem(item);
        ob.Selected = selected;
        row.AddChild(ob);
        return ob;
    }

    private void OnStart()
    {
        App.I.Preset = _preset.Selected == 1 ? GamePreset.Sim : GamePreset.Assisted;
        App.I.TeamSize = _size.Selected == 1 ? 7 : 11;
        App.I.MatchMinutes = _length.Selected switch { 0 => 3, 2 => 8, _ => 5 };
        App.I.LoadTuning();
        GetTree().ChangeSceneToFile("res://scenes/Match.tscn");
    }
}
