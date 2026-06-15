using Godot;

namespace FootballProto;

/// <summary>
/// All in-match UI, built in code: score + clock (top centre), power bar
/// while charging a kick (bottom centre), stamina + player info (bottom left),
/// GOAL / FULL TIME banner, pause menu and full-time panel.
/// </summary>
public partial class Hud : CanvasLayer
{
    private MatchController _match;

    private Label _score;
    private Label _clock;
    private Label _banner;
    private ProgressBar _power;
    private ProgressBar _stamina;
    private Label _playerInfo;
    private PanelContainer _pausePanel;
    private PanelContainer _ftPanel;
    private Label _ftLabel;
    private float _bannerTimer;

    public void Init(MatchController match)
    {
        _match = match;
        ProcessMode = ProcessModeEnum.Always;
        BuildUi();
    }

    private void BuildUi()
    {
        // ----- Top centre: score + clock -----
        var top = new VBoxContainer
        {
            AnchorLeft = 0.5f, AnchorRight = 0.5f, AnchorTop = 0f,
            OffsetLeft = -130, OffsetRight = 130, OffsetTop = 8,
            Alignment = BoxContainer.AlignmentMode.Begin
        };
        AddChild(top);

        _score = MakeLabel("BLUE 0 - 0 RED", 26);
        _score.HorizontalAlignment = HorizontalAlignment.Center;
        top.AddChild(_score);

        _clock = MakeLabel("05:00", 20);
        _clock.HorizontalAlignment = HorizontalAlignment.Center;
        top.AddChild(_clock);

        // ----- Bottom centre: power bar -----
        _power = new ProgressBar
        {
            MinValue = 0, MaxValue = 1, Value = 0, ShowPercentage = false,
            AnchorLeft = 0.5f, AnchorRight = 0.5f, AnchorTop = 1f, AnchorBottom = 1f,
            OffsetLeft = -120, OffsetRight = 120, OffsetTop = -46, OffsetBottom = -30,
            Visible = false
        };
        AddChild(_power);

        // ----- Bottom left: stamina + player info -----
        var bl = new VBoxContainer
        {
            AnchorLeft = 0f, AnchorTop = 1f, AnchorBottom = 1f,
            OffsetLeft = 16, OffsetTop = -78, OffsetRight = 240, OffsetBottom = -16
        };
        AddChild(bl);

        _playerInfo = MakeLabel("#8 MID", 16);
        bl.AddChild(_playerInfo);

        _stamina = new ProgressBar
        {
            MinValue = 0, MaxValue = 100, Value = 100, ShowPercentage = false,
            CustomMinimumSize = new Vector2(200, 14)
        };
        bl.AddChild(_stamina);

        // ----- Centre banner -----
        _banner = MakeLabel("", 56);
        _banner.HorizontalAlignment = HorizontalAlignment.Center;
        _banner.AnchorLeft = 0f; _banner.AnchorRight = 1f;
        _banner.AnchorTop = 0.35f; _banner.AnchorBottom = 0.35f;
        _banner.Visible = false;
        AddChild(_banner);

        // ----- Pause panel -----
        _pausePanel = BuildPanel("PAUSED");
        var pv = (VBoxContainer)_pausePanel.GetChild(0);
        pv.AddChild(MakeButton("Resume", () => TogglePause(false)));
        pv.AddChild(MakeButton("Restart Match", () =>
        {
            GetTree().Paused = false;
            GetTree().ReloadCurrentScene();
        }));
        pv.AddChild(MakeButton("Quit to Menu", () =>
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://scenes/Menu.tscn");
        }));

        // ----- Full-time panel -----
        _ftPanel = BuildPanel("FULL TIME");
        var fv = (VBoxContainer)_ftPanel.GetChild(0);
        _ftLabel = MakeLabel("", 24);
        _ftLabel.HorizontalAlignment = HorizontalAlignment.Center;
        fv.AddChild(_ftLabel);
        fv.AddChild(MakeButton("Rematch", () => GetTree().ReloadCurrentScene()));
        fv.AddChild(MakeButton("Menu", () =>
            GetTree().ChangeSceneToFile("res://scenes/Menu.tscn")));
    }

    private PanelContainer BuildPanel(string title)
    {
        var panel = new PanelContainer
        {
            AnchorLeft = 0.5f, AnchorRight = 0.5f, AnchorTop = 0.5f, AnchorBottom = 0.5f,
            OffsetLeft = -160, OffsetRight = 160, OffsetTop = -130, OffsetBottom = 130,
            Visible = false
        };
        AddChild(panel);
        var v = new VBoxContainer();
        v.AddThemeConstantOverride("separation", 14);
        panel.AddChild(v);
        var t = MakeLabel(title, 30);
        t.HorizontalAlignment = HorizontalAlignment.Center;
        v.AddChild(t);
        return panel;
    }

    private static Label MakeLabel(string text, int size)
    {
        var l = new Label { Text = text };
        l.AddThemeFontSizeOverride("font_size", size);
        l.AddThemeColorOverride("font_outline_color", Colors.Black);
        l.AddThemeConstantOverride("outline_size", 6);
        return l;
    }

    private static Button MakeButton(string text, System.Action onPress)
    {
        var b = new Button { Text = text };
        b.AddThemeFontSizeOverride("font_size", 20);
        b.Pressed += () => onPress();
        return b;
    }

    // =====================================================================
    public override void _Process(double delta)
    {
        if (_match == null) return;

        if (Input.IsActionJustPressed(InputActions.Pause)
            && _match.Phase != MatchPhase.FullTime)
            TogglePause(!GetTree().Paused);

        _score.Text = $"BLUE {_match.ScoreBlue} - {_match.ScoreRed} RED";
        int t = Mathf.Max(Mathf.CeilToInt(_match.TimeLeft), 0);
        _clock.Text = $"{t / 60:00}:{t % 60:00}";

        var human = _match.Human;
        if (human != null)
        {
            _power.Visible = human.Charging;
            _power.Value = human.Charge01;

            Player p = human.Controlled;
            if (p != null && GodotObject.IsInstanceValid(p))
            {
                _stamina.Value = p.Stamina;
                _playerInfo.Text = $"#{p.Number} {p.Role} · {App.I.Preset}";
            }
        }

        if (_bannerTimer > 0f)
        {
            _bannerTimer -= (float)delta;
            if (_bannerTimer <= 0f) _banner.Visible = false;
        }
    }

    private void TogglePause(bool pause)
    {
        GetTree().Paused = pause;
        _pausePanel.Visible = pause;
    }

    public void ShowBanner(string text)
    {
        _banner.Text = text;
        _banner.Visible = true;
        _bannerTimer = 2.2f;
    }

    public void ShowFullTime(int blue, int red)
    {
        _banner.Visible = false;
        _ftLabel.Text = $"BLUE {blue} - {red} RED";
        _ftPanel.Visible = true;
    }
}
