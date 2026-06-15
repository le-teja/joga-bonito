namespace FootballProto;

/// <summary>
/// Central list of input action names. These must match the [input] section
/// of project.godot. Actions are semantic and dual-context:
/// the same physical button means different things in attack vs defence.
/// </summary>
public static class InputActions
{
    public const string MoveLeft = "move_left";
    public const string MoveRight = "move_right";
    public const string MoveUp = "move_up";
    public const string MoveDown = "move_down";

    public const string Sprint = "sprint";                  // RT / R2 / Shift

    // Attack: ground pass | Defence: pressure / contain
    public const string PassPressure = "act_pass_pressure"; // A / Cross / Space
    // Attack: shoot | Defence: standing tackle
    public const string ShootTackle = "act_shoot_tackle";   // B / Circle / E
    // Attack: lob pass | Defence: slide tackle
    public const string LobSlide = "act_lob_slide";         // X / Square / Q
    // Attack: through ball
    public const string Through = "act_through";            // Y / Triangle / R

    // Attack: finesse modifier | Defence: call second presser
    public const string FinesseCall = "mod_finesse_call";   // RB / R1 / F
    // Attack: advanced modifier (pass-and-run, lobbed through) | Defence: switch player
    public const string LbSwitch = "mod_lb_switch";         // LB / L1 / Tab
    // Attack: shield ball | Defence: jockey
    public const string ShieldJockey = "mod_shield_jockey"; // LT / L2 / Ctrl

    public const string Pause = "pause";                    // Start / Esc
}
