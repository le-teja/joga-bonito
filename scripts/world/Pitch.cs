using Godot;

namespace FootballProto;

/// <summary>
/// Builds the match environment:
///   - PBR grass pitch with mowing stripe shader and real textures
///   - Goal frames + nets + area triggers
///   - Perimeter board walls
///   - Stadium loaded from GLTF
///   - HDRI sky and lighting
/// </summary>
public partial class Pitch : Node3D
{
    private const float HalfLen = MatchController.HalfLen;
    private const float HalfWid = MatchController.HalfWid;
    private const float GoalHalfWidth = 3.66f;
    private const float GoalHeight    = 2.44f;

    private MatchController _match;

    public void Init(MatchController match)
    {
        _match = match;
        BuildSky();
        BuildGround();
        BuildLines();
        BuildGoal(+1);
        BuildGoal(-1);
        BuildPerimeterWalls();
        BuildStadium();
    }

    // =====================================================================
    // SKY + LIGHTING
    // =====================================================================
    private void BuildSky()
    {
        // Sun
        var sun = new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-52f, 35f, 0f),
            LightEnergy = 1.35f,
            LightColor = new Color(1.0f, 0.97f, 0.88f),
            ShadowEnabled = true,
            DirectionalShadowMaxDistance = 200f
        };
        AddChild(sun);

        // Ambient fill
        var fill = new DirectionalLight3D
        {
            RotationDegrees = new Vector3(-25f, 200f, 0f),
            LightEnergy = 0.28f,
            LightColor = new Color(0.65f, 0.72f, 1.0f),
            ShadowEnabled = false
        };
        AddChild(fill);

        Sky sky;
        string hdrPath = "res://assets/textures/sky.hdr";
        if (ResourceLoader.Exists(hdrPath))
        {
            var panorama = new PanoramaSkyMaterial
            {
                Panorama = ResourceLoader.Load<Texture2D>(hdrPath)
            };
            sky = new Sky { SkyMaterial = panorama };
        }
        else
        {
            var proc = new ProceduralSkyMaterial
            {
                SkyTopColor     = new Color(0.18f, 0.38f, 0.72f),
                SkyHorizonColor = new Color(0.56f, 0.72f, 0.92f),
                GroundBottomColor = new Color(0.08f, 0.10f, 0.08f),
                SunAngleMax = 28f
            };
            sky = new Sky { SkyMaterial = proc };
        }

        var env = new Godot.Environment
        {
            BackgroundMode    = Godot.Environment.BGMode.Sky,
            Sky               = sky,
            AmbientLightSource = Godot.Environment.AmbientSource.Sky,
            AmbientLightEnergy = 0.55f,
            TonemapMode     = Godot.Environment.ToneMapper.Aces,
            TonemapExposure = 1.1f,
            GlowEnabled       = true,
            GlowIntensity     = 0.4f,
            GlowBloom         = 0.08f,
            SsaoEnabled       = true,
            SsaoRadius        = 0.8f
        };
        AddChild(new WorldEnvironment { Environment = env });
    }

    // =====================================================================
    // GROUND (PBR grass with mowing stripes)
    // =====================================================================
    private void BuildGround()
    {
        var ground = new StaticBody3D { CollisionLayer = 1, CollisionMask = 0 };
        ground.AddChild(new CollisionShape3D
        {
            Shape = new BoxShape3D { Size = new Vector3(160f, 1f, 110f) },
            Position = new Vector3(0, -0.5f, 0)
        });
        AddChild(ground);

        // Outer apron (slightly darker green)
        var apron = new MeshInstance3D
        {
            Mesh = new PlaneMesh { Size = new Vector2(160f, 110f), SubdivideDepth = 1, SubdivideWidth = 1 },
            Position = new Vector3(0, 0f, 0),
            MaterialOverride = MakeGrassMaterial(new Color(0.08f, 0.32f, 0.10f), tile: 28f)
        };
        AddChild(apron);

        // Main pitch surface with mowing stripes
        var pitch = new MeshInstance3D
        {
            Mesh = new PlaneMesh
            {
                Size = new Vector2(HalfLen * 2f, HalfWid * 2f),
                SubdivideWidth  = 20,
                SubdivideDepth  = 13
            },
            Position = new Vector3(0, 0.005f, 0)
        };

        pitch.MaterialOverride = MakeStripedGrassMaterial();
        AddChild(pitch);
    }

    private StandardMaterial3D MakeGrassMaterial(Color tint, float tile)
    {
        var mat = new StandardMaterial3D
        {
            AlbedoColor   = tint,
            Roughness     = 0.90f,
            Metallic      = 0f,
            Uv1Scale      = new Vector3(tile, tile, tile)
        };
        TryLoadTexture("res://assets/textures/grass_albedo.jpg",   t => mat.AlbedoTexture   = t);
        TryLoadTexture("res://assets/textures/grass_normal.jpg",   t => { mat.NormalEnabled = true; mat.NormalTexture = t; });
        TryLoadTexture("res://assets/textures/grass_roughness.jpg",t => mat.RoughnessTexture = t);
        return mat;
    }

    private StandardMaterial3D MakeStripedGrassMaterial()
    {
        // Mowing stripe effect: two slightly different UV1 tiling values on a
        // detail albedo blend. Godot 4.3 StandardMaterial3D exposes DetailEnabled
        // and DetailAlbedo but not UV2 scale directly, so we approximate the
        // stripes by blending two grass samples at different tiling via Detail.
        var mat = new StandardMaterial3D
        {
            AlbedoColor   = new Color(0.13f, 0.50f, 0.15f),
            Roughness     = 0.88f,
            Metallic      = 0f,
            Uv1Scale      = new Vector3(22f, 22f, 1f),
            DetailEnabled = true,
            DetailBlendMode = BaseMaterial3D.BlendModeEnum.Mul,
        };

        TryLoadTexture("res://assets/textures/grass_albedo.jpg", t =>
        {
            mat.AlbedoTexture = t;
            mat.DetailAlbedo  = t;   // Detail layer at default UV gives subtle stripe
        });
        TryLoadTexture("res://assets/textures/grass_normal.jpg", t =>
        {
            mat.NormalEnabled = true;
            mat.NormalTexture = t;
        });
        TryLoadTexture("res://assets/textures/grass_roughness.jpg", t =>
            mat.RoughnessTexture = t);
        return mat;
    }

    private static void TryLoadTexture(string path, System.Action<Texture2D> apply)
    {
        if (ResourceLoader.Exists(path))
            apply(ResourceLoader.Load<Texture2D>(path));
    }

    // =====================================================================
    // LINE MARKINGS
    // =====================================================================
    private void BuildLines()
    {
        var im = new ImmediateMesh();
        var mi = new MeshInstance3D
        {
            Mesh = im,
            MaterialOverride = new StandardMaterial3D
            {
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                AlbedoColor = new Color(1f, 1f, 1f, 0.92f),
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                NoDepthTest = false
            }
        };
        AddChild(mi);

        const float y = 0.012f;
        im.SurfaceBegin(Mesh.PrimitiveType.Lines);

        AddRect(im, -HalfLen, -HalfWid, HalfLen, HalfWid, y);
        AddSeg(im, new Vector3(0, y, -HalfWid), new Vector3(0, y, HalfWid));
        AddCircle(im, Vector3.Zero, 9.15f, y, 64);
        AddCircle(im, Vector3.Zero, 0.3f,  y, 16);   // centre spot

        foreach (float s in new[] { -1f, 1f })
        {
            float gl = s * HalfLen;
            // Penalty box
            float pbx0 = gl, pbx1 = gl - s * 16.5f;
            AddRect3(im, pbx0, pbx1, -20.16f, 20.16f, y);
            // Six-yard box
            AddRect3(im, gl, gl - s * 5.5f, -9.16f, 9.16f, y);
            // Penalty spot
            AddCircle(im, new Vector3(gl - s * 11f, 0, 0), 0.24f, y, 16);
            // Penalty arc (partial circle behind the box)
            AddArc(im, new Vector3(gl - s * 11f, 0, 0), 9.15f, y, 64,
                startDeg: 127f * s, sweepDeg: -104f * s);
            // Corner arcs
            foreach (float cz in new[] { -1f, 1f })
            {
                AddArc(im, new Vector3(gl, 0, cz * HalfWid), 1.0f, y, 16,
                    startDeg: s > 0 ? (cz > 0 ? 180f : 90f) : (cz > 0 ? 270f : 0f),
                    sweepDeg: 90f);
            }
        }

        im.SurfaceEnd();
    }

    private static void AddSeg(ImmediateMesh im, Vector3 a, Vector3 b)
    {
        im.SurfaceAddVertex(a); im.SurfaceAddVertex(b);
    }

    private static void AddRect(ImmediateMesh im, float x0, float z0, float x1, float z1, float y)
    {
        AddSeg(im, new Vector3(x0,y,z0), new Vector3(x1,y,z0));
        AddSeg(im, new Vector3(x1,y,z0), new Vector3(x1,y,z1));
        AddSeg(im, new Vector3(x1,y,z1), new Vector3(x0,y,z1));
        AddSeg(im, new Vector3(x0,y,z1), new Vector3(x0,y,z0));
    }

    private static void AddRect3(ImmediateMesh im, float xa, float xb, float z0, float z1, float y)
    {
        float x0 = Mathf.Min(xa,xb), x1 = Mathf.Max(xa,xb);
        AddRect(im, x0, z0, x1, z1, y);
    }

    private static void AddCircle(ImmediateMesh im, Vector3 c, float r, float y, int segs)
    {
        for (int i = 0; i < segs; i++)
        {
            float a0 = Mathf.Tau * i / segs, a1 = Mathf.Tau * (i + 1) / segs;
            AddSeg(im,
                new Vector3(c.X + Mathf.Cos(a0) * r, y, c.Z + Mathf.Sin(a0) * r),
                new Vector3(c.X + Mathf.Cos(a1) * r, y, c.Z + Mathf.Sin(a1) * r));
        }
    }

    private static void AddArc(ImmediateMesh im, Vector3 c, float r, float y,
        int segs, float startDeg, float sweepDeg)
    {
        float s0 = Mathf.DegToRad(startDeg);
        float sw = Mathf.DegToRad(sweepDeg);
        for (int i = 0; i < segs; i++)
        {
            float a0 = s0 + sw * i / segs, a1 = s0 + sw * (i + 1) / segs;
            AddSeg(im,
                new Vector3(c.X + Mathf.Cos(a0) * r, y, c.Z + Mathf.Sin(a0) * r),
                new Vector3(c.X + Mathf.Cos(a1) * r, y, c.Z + Mathf.Sin(a1) * r));
        }
    }

    // =====================================================================
    // GOALS
    // =====================================================================
    private void BuildGoal(int sideSign)
    {
        float gl = sideSign * HalfLen;
        var frame = new StaticBody3D { CollisionLayer = 1, CollisionMask = 0 };
        AddChild(frame);

        var postMat = new StandardMaterial3D
        {
            AlbedoColor = Colors.White,
            Roughness = 0.45f,
            Metallic = 0.15f
        };

        // Posts
        foreach (float z in new[] { -GoalHalfWidth, GoalHalfWidth })
        {
            var post = new MeshInstance3D
            {
                Mesh = new CylinderMesh { TopRadius = 0.06f, BottomRadius = 0.06f, Height = GoalHeight },
                MaterialOverride = postMat,
                Position = new Vector3(gl, GoalHeight / 2f, z)
            };
            frame.AddChild(post);
            frame.AddChild(new CollisionShape3D
            {
                Shape = new CylinderShape3D { Radius = 0.06f, Height = GoalHeight },
                Position = new Vector3(gl, GoalHeight / 2f, z)
            });
        }

        // Crossbar
        var bar = new MeshInstance3D
        {
            Mesh = new CylinderMesh { TopRadius = 0.06f, BottomRadius = 0.06f, Height = GoalHalfWidth * 2f },
            MaterialOverride = postMat,
            Position = new Vector3(gl, GoalHeight, 0f),
            RotationDegrees = new Vector3(90f, 0f, 0f)
        };
        frame.AddChild(bar);
        frame.AddChild(new CollisionShape3D
        {
            Shape = new CylinderShape3D { Radius = 0.06f, Height = GoalHalfWidth * 2f },
            Position = new Vector3(gl, GoalHeight, 0f),
            RotationDegrees = new Vector3(90f, 0f, 0f)
        });

        // Net — grid of thin dark quads
        BuildNet(gl, sideSign);

        // Backstop
        var backstop = new StaticBody3D { CollisionLayer = 8, CollisionMask = 0 };
        backstop.AddChild(new CollisionShape3D
        {
            Shape = new BoxShape3D { Size = new Vector3(0.4f, 5f, 12f) },
            Position = new Vector3(gl + sideSign * 2.0f, 2.5f, 0)
        });
        AddChild(backstop);

        // Goal trigger
        var trigger = new Area3D { CollisionLayer = 0, CollisionMask = 4, Monitoring = true };
        trigger.AddChild(new CollisionShape3D
        {
            Shape = new BoxShape3D { Size = new Vector3(1.3f, 2.3f, 6.9f) },
            Position = new Vector3(gl + sideSign * 0.8f, 1.15f, 0)
        });
        AddChild(trigger);
        int cap = sideSign;
        trigger.BodyEntered += body => { if (body is Ball) _match.OnGoal(cap); };
    }

    private void BuildNet(float gl, int sideSign)
    {
        // Render a grid of thin cylinders to look like net mesh
        var netMat = new StandardMaterial3D
        {
            AlbedoColor = new Color(0.9f, 0.9f, 0.9f, 0.55f),
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };

        float depth = 1.8f * sideSign;
        float w = GoalHalfWidth * 2f;
        float h = GoalHeight;

        // Back panel
        var back = new MeshInstance3D
        {
            Mesh = new QuadMesh { Size = new Vector2(w, h) },
            MaterialOverride = netMat,
            Position = new Vector3(gl + depth, h / 2f, 0),
            RotationDegrees = new Vector3(0, 90, 0)
        };
        AddChild(back);

        // Side panels
        foreach (float s in new[] { -1f, 1f })
        {
            var side = new MeshInstance3D
            {
                Mesh = new QuadMesh { Size = new Vector2(Mathf.Abs(depth), h) },
                MaterialOverride = netMat,
                Position = new Vector3(gl + depth / 2f, h / 2f, s * GoalHalfWidth),
                RotationDegrees = new Vector3(0, 0, 0)
            };
            AddChild(side);
        }

        // Top panel
        var top = new MeshInstance3D
        {
            Mesh = new QuadMesh { Size = new Vector2(w, Mathf.Abs(depth)) },
            MaterialOverride = netMat,
            Position = new Vector3(gl + depth / 2f, h, 0),
            RotationDegrees = new Vector3(90, 0, 0)
        };
        AddChild(top);
    }

    // =====================================================================
    // PERIMETER WALLS (ball-only boards)
    // =====================================================================
    private void BuildPerimeterWalls()
    {
        var walls = new StaticBody3D
        {
            CollisionLayer = 8, CollisionMask = 0,
            PhysicsMaterialOverride = new PhysicsMaterial { Bounce = 0.45f }
        };
        AddChild(walls);

        const float wx = HalfLen + 3f;
        const float wz = HalfWid + 1.2f;
        const float h  = 6f;

        foreach (float s in new[] { -1f, 1f })
        {
            walls.AddChild(new CollisionShape3D
            {
                Shape    = new BoxShape3D { Size = new Vector3(wx * 2f + 4f, h, 0.4f) },
                Position = new Vector3(0, h / 2f, s * wz)
            });
        }

        foreach (float s in new[] { -1f, 1f })
        {
            float segLen = wz - GoalHalfWidth - 0.6f;
            foreach (float zs in new[] { -1f, 1f })
            {
                walls.AddChild(new CollisionShape3D
                {
                    Shape    = new BoxShape3D { Size = new Vector3(0.4f, h, segLen) },
                    Position = new Vector3(s * wx, h / 2f, zs * (GoalHalfWidth + 0.6f + segLen / 2f))
                });
            }
        }
    }

    // =====================================================================
    // STADIUM
    // =====================================================================
    private void BuildStadium()
    {
        // Try both .gltf and .glb — user may have either
        string path = null;
        foreach (var candidate in new[] {
            "res://assets/stadium/stadium.gltf",
            "res://assets/stadium/stadium.glb",
            "res://assets/stadium/scene.gltf",
            "res://assets/stadium/scene.glb" })
        {
            if (ResourceLoader.Exists(candidate)) { path = candidate; break; }
        }
        if (path == null)
        {
            GD.Print("Stadium: no file found in assets/stadium/ — skipping.");
            return;
        }

        PackedScene scene = null;
        try { scene = ResourceLoader.Load<PackedScene>(path); }
        catch (System.Exception e) { GD.PrintErr("Stadium load exception: " + e.Message); }

        if (scene == null)
        {
            GD.PrintErr("Stadium: could not load " + path +
                " — check the file imported correctly in the FileSystem panel.");
            return;
        }

        Node3D stadium = null;
        try { stadium = scene.Instantiate<Node3D>(); }
        catch (System.Exception e) { GD.PrintErr("Stadium instantiate failed: " + e.Message); return; }

        stadium.Position = Vector3.Zero;
        stadium.Scale    = Vector3.One;
        AddChild(stadium);

        CallDeferred(MethodName.FitStadium, stadium);
    }

    private void FitStadium(Node3D stadium)
    {
        // Target: stadium bowl should be roughly 180×130 m
        float targetX = 180f;
        var aabb = GetStadiumAabb(stadium);
        if (aabb.Size.X < 0.1f) return;
        float s = targetX / aabb.Size.X;
        stadium.Scale = new Vector3(s, s, s);
        // Re-centre
        var newAabb = GetStadiumAabb(stadium);
        stadium.Position = new Vector3(-newAabb.GetCenter().X, 0, -newAabb.GetCenter().Z);
    }

    private static Aabb GetStadiumAabb(Node3D root)
    {
        var aabb = new Aabb();
        bool first = true;
        foreach (var child in root.FindChildren("*", "MeshInstance3D", owned: false))
        {
            if (child is not MeshInstance3D mi) continue;
            var a = mi.GetAabb().Abs();
            if (first) { aabb = a; first = false; }
            else aabb = aabb.Merge(a);
        }
        return aabb;
    }
}
