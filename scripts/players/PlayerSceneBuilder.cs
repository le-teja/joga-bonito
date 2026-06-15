using Godot;
using System.Collections.Generic;

namespace FootballProto;

/// <summary>
/// Builds the visual subtree of a Player at runtime:
///   1. Loads the Mixamo Soccer Player FBX as the Visual node.
///   2. Creates an AnimationLibrary from all individual FBX animation files.
///   3. Constructs a full AnimationTree with:
///        - A BlendSpace2D for 8-directional locomotion (idle/jog/sprint)
///        - A StateMachine wrapping Locomotion + all one-shot action states
///   4. Applies team-coloured material to the character mesh.
///
/// Call BuildVisuals() from Player._Ready() after Init().
/// </summary>
public static class PlayerSceneBuilder
{
    // ---- Paths ----
    private const string CharacterPath = "res://assets/characters/player.fbx";
    private const string AnimDir = "res://assets/animations/";

    // Animation file → library name mapping (exact filenames you downloaded)
    private static readonly Dictionary<string, string> AnimFiles = new()
    {
        // Locomotion
        ["idle"]          = "offensive idle.fbx",
        ["jog_fwd"]       = "jog forward.fbx",
        ["jog_bwd"]       = "jog backward.fbx",
        ["jog_left"]      = "jog strafe left.fbx",
        ["jog_right"]     = "jog strafe right.fbx",
        ["jog_fwd_dL"]    = "jog forward diagonal.fbx",
        ["jog_fwd_dR"]    = "jog forward diagonal (2).fbx",
        ["jog_bwd_dL"]    = "jog backward diagonal.fbx",
        ["jog_bwd_dR"]    = "jog backward diagonal (2).fbx",
        ["sprint"]        = "strike foward jog.fbx",

        // Kicks
        ["kick"]          = "kick soccerball.fbx",
        ["kick2"]         = "kick soccerball (2).fbx",
        ["kick_power"]    = "soccer penalty kick.fbx",
        ["kick_scissor"]  = "scissor kick.fbx",
        ["kick_up"]       = "kick up soccerball.fbx",
        ["kneeing"]       = "kneeing soccerball.fbx",

        // Tackles / physical
        ["tackle"]        = "soccer tackle.fbx",
        ["tackle2"]       = "soccer tackle (2).fbx",
        ["tackle3"]       = "soccer tackle (3).fbx",
        ["slide"]         = "soccer trip.fbx",
        ["stumble"]       = "standing up.fbx",
        ["transition"]    = "transition.fbx",

        // Ball skills
        ["receive"]       = "receive soccerball.fbx",
        ["header"]        = "header soccerball.fbx",
        ["stall"]         = "stall soccerball.fbx",
        ["stall2"]        = "stall soccerball (2).fbx",
        ["stall3"]        = "stall soccerball (3).fbx",
        ["stall4"]        = "stall soccerball (4).fbx",
        ["throw_in"]      = "throw in.fbx",

        // Goalkeeper
        ["gk_idle"]       = "goalkeeper idle.fbx",
        ["gk_idle2"]      = "goalkeeper idle (2).fbx",
        ["gk_dive_l"]     = "goalkeeper sidestep.fbx",
        ["gk_dive_r"]     = "goalkeeper sidestep (2).fbx",
        ["gk_catch"]      = "goalkeeper scoop.fbx",
        ["gk_throw"]      = "goalkeeper overhand throw.fbx",
        ["gk_pass"]       = "goalkeeper pass.fbx",
        ["gk_kick"]       = "goalkeeper drop kick.fbx",
        ["gk_place"]      = "goalkeeper placing ball.fbx",
        ["gk_place2"]     = "goalkeeper placing ball (2).fbx",
        ["gk_miss"]       = "goalkeeper miss.fbx",
    };

    public static void BuildVisuals(Player player)
    {
        // Remove old procedural visual if present
        var old = player.GetNodeOrNull<Node3D>("Visual");
        old?.QueueFree();

        // Load the Mixamo character scene
        if (!ResourceLoader.Exists(CharacterPath))
        {
            GD.PrintErr("PlayerSceneBuilder: player.fbx not found at " + CharacterPath);
            BuildFallbackCapsule(player);
            return;
        }

        var charScene = ResourceLoader.Load<PackedScene>(CharacterPath);
        var visual = charScene.Instantiate<Node3D>();
        visual.Name = "Visual";

        // Scale: Mixamo characters import at 100x — bring to ~1.8 m
        visual.Scale = new Vector3(0.01f, 0.01f, 0.01f);
        player.AddChild(visual);

        // Build animation library from individual FBX files
        var animPlayer = FindAnimationPlayer(visual);
        if (animPlayer == null)
        {
            GD.PrintErr("PlayerSceneBuilder: no AnimationPlayer found in character scene");
            ApplyTeamMaterial(visual, player);
            return;
        }

        LoadAnimations(animPlayer, player.Role);
        var animTree = BuildAnimationTree(animPlayer, player.Role);
        visual.AddChild(animTree);

        // Add animator driver node
        var animator = new PlayerAnimator();
        animator.Name = "Animator";
        player.AddChild(animator);

        ApplyTeamMaterial(visual, player);
    }

    // =====================================================================
    // ANIMATION LIBRARY
    // =====================================================================
    private static void LoadAnimations(AnimationPlayer ap, PRole role)
    {
        foreach (var kv in AnimFiles)
        {
            // Skip GK animations for outfielders and vice versa
            bool isGkAnim = kv.Key.StartsWith("gk_");
            if (isGkAnim && role != PRole.GK) continue;
            if (!isGkAnim && role == PRole.GK && kv.Key != "idle" && kv.Key != "stumble") continue;

            string path = AnimDir + kv.Value;
            if (!ResourceLoader.Exists(path)) continue;

            var fbxScene = ResourceLoader.Load<PackedScene>(path);
            var inst = fbxScene.Instantiate();
            var subAp = FindAnimationPlayerInNode(inst);
            if (subAp == null) { inst.QueueFree(); continue; }

            // Copy each animation into the main player's AnimationPlayer
            foreach (var animName in subAp.GetAnimationList())
            {
                var anim = subAp.GetAnimation(animName);
                string libKey = kv.Key;
                if (!ap.HasAnimationLibrary(""))
                    ap.AddAnimationLibrary("", new AnimationLibrary());
                var lib = ap.GetAnimationLibrary("");
                if (!lib.HasAnimation(libKey))
                    lib.AddAnimation(libKey, anim);
            }
            inst.QueueFree();
        }
    }

    // =====================================================================
    // ANIMATION TREE CONSTRUCTION
    // =====================================================================
    private static AnimationTree BuildAnimationTree(AnimationPlayer ap, PRole role)
    {
        var tree = new AnimationTree
        {
            Name      = "AnimationTree",
            AnimPlayer = ap.GetPath(),
            Active    = true
        };

        // Root: StateMachine
        var sm = new AnimationNodeStateMachine();
        tree.TreeRoot = sm;

        // ---- Locomotion BlendSpace2D ----
        // Axes: X = strafe (-1 left, +1 right), Y = forward (-1 back, +1 fwd)
        var bs = new AnimationNodeBlendSpace2D
        {
            BlendMode = AnimationNodeBlendSpace2D.BlendModeEnum.Discrete
        };
        AddBlend(bs, "idle",       0f,    0f);
        AddBlend(bs, "jog_fwd",    0f,    0.5f);
        AddBlend(bs, "sprint",     0f,    1f);
        AddBlend(bs, "jog_bwd",    0f,   -0.5f);
        AddBlend(bs, "jog_left",  -0.6f,  0f);
        AddBlend(bs, "jog_right",  0.6f,  0f);
        AddBlend(bs, "jog_fwd_dL",-0.5f,  0.5f);
        AddBlend(bs, "jog_fwd_dR", 0.5f,  0.5f);
        AddBlend(bs, "jog_bwd_dL",-0.5f, -0.5f);
        AddBlend(bs, "jog_bwd_dR", 0.5f, -0.5f);

        sm.AddNode("Locomotion", bs);

        // ---- One-shot action nodes ----
        AddOneShot(sm, "Kick",        "kick");
        AddOneShot(sm, "Kick2",       "kick2");
        AddOneShot(sm, "KickPower",   "kick_power");
        AddOneShot(sm, "KickScissor", "kick_scissor");
        AddOneShot(sm, "Tackle",      "tackle");
        AddOneShot(sm, "Tackle2",     "tackle2");
        AddOneShot(sm, "Slide",       "slide");
        AddOneShot(sm, "Stumble",     "stumble");
        AddOneShot(sm, "Receive",     "receive");
        AddOneShot(sm, "Header",      "header");

        if (role == PRole.GK)
        {
            AddOneShot(sm, "GkIdle",  "gk_idle");
            AddOneShot(sm, "GkDiveL", "gk_dive_l");
            AddOneShot(sm, "GkDiveR", "gk_dive_r");
            AddOneShot(sm, "GkCatch", "gk_catch");
            AddOneShot(sm, "GkThrow", "gk_throw");
        }

        // ---- Wire Start → Locomotion ----
        var startTrans = new AnimationNodeStateMachineTransition { XfadeTime = 0f };
        sm.AddTransition("Start", "Locomotion", startTrans);

        // ---- All action nodes → return to Locomotion automatically ----
        var actionNodes = new[]
        {
            "Kick","Kick2","KickPower","KickScissor","Tackle","Tackle2",
            "Slide","Stumble","Receive","Header","GkIdle","GkDiveL",
            "GkDiveR","GkCatch","GkThrow"
        };
        foreach (var n in actionNodes)
        {
            if (!sm.HasNode(n)) continue;
            // Auto-return after one-shot finishes
            sm.AddTransition(n, "Locomotion",
                new AnimationNodeStateMachineTransition
                {
                    AdvanceMode = AnimationNodeStateMachineTransition.AdvanceModeEnum.Auto,
                    XfadeTime   = 0.15f
                });
            // Allow travel from Locomotion to this node
            sm.AddTransition("Locomotion", n,
                new AnimationNodeStateMachineTransition
                {
                    AdvanceMode = AnimationNodeStateMachineTransition.AdvanceModeEnum.Enabled,
                    XfadeTime   = 0.12f
                });
        }

        return tree;
    }

    private static void AddBlend(AnimationNodeBlendSpace2D bs, string animKey,
        float x, float y)
    {
        var node = new AnimationNodeAnimation { Animation = animKey };
        bs.AddBlendPoint(node, new Vector2(x, y));
    }

    private static void AddOneShot(AnimationNodeStateMachine sm, string nodeName, string animKey)
    {
        sm.AddNode(nodeName, new AnimationNodeAnimation { Animation = animKey });
    }

    // =====================================================================
    // TEAM MATERIALS
    // =====================================================================
    private static void ApplyTeamMaterial(Node3D visual, Player player)
    {
        Color shirt, shorts, skin, socks;
        skin  = new Color(0.87f, 0.71f, 0.56f);
        socks = new Color(0.92f, 0.92f, 0.92f);

        if (player.Role == PRole.GK)
        {
            shirt  = player.Team == 0
                ? new Color(0.95f, 0.82f, 0.10f)
                : new Color(0.10f, 0.80f, 0.35f);
            shorts = new Color(0.12f, 0.12f, 0.12f);
        }
        else
        {
            shirt  = player.Team == 0
                ? new Color(0.08f, 0.22f, 0.88f)
                : new Color(0.88f, 0.10f, 0.10f);
            shorts = player.Team == 0
                ? new Color(0.05f, 0.12f, 0.55f)
                : new Color(0.55f, 0.06f, 0.06f);
        }

        // Walk all MeshInstance3D children and recolour by material slot index
        // Mixamo Soccer Player material order: 0=skin, 1=shirt, 2=shorts, 3=socks/boots
        int slotIdx = 0;
        foreach (var child in visual.FindChildren("*", "MeshInstance3D", owned: false))
        {
            if (child is not MeshInstance3D mi) continue;
            for (int i = 0; i < mi.GetSurfaceOverrideMaterialCount(); i++)
            {
                var mat = new StandardMaterial3D { Roughness = 0.72f };
                mat.AlbedoColor = slotIdx switch
                {
                    0 => skin,
                    1 => shirt,
                    2 => shorts,
                    _ => socks
                };
                mi.SetSurfaceOverrideMaterial(i, mat);
                slotIdx++;
            }
        }
    }

    // =====================================================================
    // FALLBACK (if player.fbx missing)
    // =====================================================================
    private static void BuildFallbackCapsule(Player player)
    {
        var visual = new Node3D { Name = "Visual" };
        player.AddChild(visual);
        Color c = player.Team == 0
            ? new Color(0.1f, 0.25f, 0.9f)
            : new Color(0.9f, 0.1f, 0.1f);
        var mi = new MeshInstance3D
        {
            Mesh = new CapsuleMesh { Radius = 0.32f, Height = 1.8f },
            MaterialOverride = new StandardMaterial3D { AlbedoColor = c }
        };
        mi.Position = new Vector3(0, 0.9f, 0);
        visual.AddChild(mi);
    }

    // =====================================================================
    // HELPERS
    // =====================================================================
    private static AnimationPlayer FindAnimationPlayer(Node root)
        => FindAnimationPlayerInNode(root);

    private static AnimationPlayer FindAnimationPlayerInNode(Node root)
    {
        if (root is AnimationPlayer ap) return ap;
        foreach (Node child in root.GetChildren())
        {
            var found = FindAnimationPlayerInNode(child);
            if (found != null) return found;
        }
        return null;
    }

}
