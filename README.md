# Footy Proto

A complete, locally playable 11v11 football prototype built with **Godot 4.3 (.NET)** and **C#**. Phase 1 of a longer journey: physics-first ball, weighty player movement, dual-context FIFA-style controls, deterministic practical team AI, and a clean architecture aimed at a future server-authoritative multiplayer rewrite.

No third-party assets — every mesh, line and UI element is generated in code or built from Godot primitives, so the whole repo is freely redistributable (MIT).

---

## 1. Requirements

| Tool | Version |
|---|---|
| Godot Engine **.NET edition** | 4.3.x (4.2+ should work; tested target is 4.3) |
| .NET SDK | 8.0 |
| OS | Windows 11 (primary target); runs on Linux/macOS in-editor too |

Download Godot .NET from https://godotengine.org/download (make sure it is the **.NET / C#** build, not the standard one).

## 2. Run it

1. Open Godot → **Import** → select this folder's `project.godot`.
2. First open: Godot generates the `.godot/` folder and triggers a C# build. If it doesn't, press **Build** (hammer icon, top-right) or run `dotnet build` in the repo root.
3. Press **F5** (Play). The menu lets you pick preset (Assisted/Sim), team size (11v11 / 7v7) and match length (3/5/8 min).

> Honest note: this repo was authored in an environment without a Godot runtime, so it has been reviewed statically but not compiled by the author. If the first build surfaces a trivial issue (a renamed enum, a signature change between 4.x minors), it should be a one-line fix — the architecture is deliberately boring and engine-API-light.

## 3. Export a Windows .exe

1. **Editor → Manage Export Templates** → download templates for your Godot version (one-time).
2. **Project → Export…** — a "Windows Desktop" preset is already in `export_presets.cfg` (embedded PCK, x86_64, output `build/FootyProto.exe`).
3. Click **Export Project**, keep "Export With Debug" off for a release build.
4. Ship the resulting `FootyProto.exe` (plus the `data_FootballProto_*` folder Godot places next to it for .NET exports — zip both together).

## 4. Controls

Context-sensitive: the same buttons attack when your team has the ball and defend when it doesn't. You control the **BLUE** team (attacks left → right). Goalkeepers are always AI.

### Gamepad (Xbox / PS)

| Input | In possession | Out of possession |
|---|---|---|
| Left stick | Move / aim | Move |
| RT / R2 | Sprint | Sprint |
| A / Cross | Ground pass (hold = power) | Pressure / contain (hold) |
| B / Circle | Shoot (hold = power) | Standing tackle |
| X / Square | Lob pass | Slide tackle |
| Y / Triangle | Through ball | — |
| RB / R1 | Finesse modifier (with shot) | Call second presser |
| LB / L1 | + pass = pass-and-run; + through = lobbed through | Switch player |
| LT / L2 | Shield ball | Jockey |
| Start | Pause | Pause |

### Keyboard

| Key | Action |
|---|---|
| WASD | Move / aim |
| Shift | Sprint |
| Space | Pass / pressure |
| E | Shoot / tackle |
| Q | Lob / slide |
| R | Through ball |
| F | Finesse / call presser |
| Tab | Advanced modifier / switch |
| Ctrl | Shield / jockey |
| Esc | Pause |

Pass, lob, through and shot are **hold-to-charge**: a power bar appears at the bottom of the screen; release to play the ball.

## 5. Repository layout

```
footy-proto/
├── project.godot            # Engine config + full input map
├── FootballProto.csproj     # .NET 8, Godot.NET.Sdk/4.3.0
├── export_presets.cfg       # Windows Desktop export preset
├── scenes/
│   ├── Menu.tscn            # Entry scene (UI built in MainMenu.cs)
│   ├── Match.tscn           # MatchController + Pitch + Ball + Camera + HUD
│   ├── Player.tscn          # CharacterBody3D capsule + facing nose + select ring
│   └── Ball.tscn            # RigidBody3D, continuous CD, physics material
├── data/
│   ├── tuning_sim.tres      # Full-physics preset
│   ├── tuning_assisted.tres # Forgiving preset (default)
│   └── attributes.json      # Per-role attribute ranges (0–100)
└── scripts/
    ├── core/                # App autoload, tuning resource, kick math, input names
    ├── ball/                # Ball.cs (rolling friction, drag, Magnus)
    ├── players/             # Player.cs (states/locomotion), HumanDriver, attributes
    ├── ai/                  # TeamAI, GoalkeeperBrain
    ├── world/               # MatchController (match flow), Pitch (built in code)
    └── presentation/        # MatchCamera, Hud, MainMenu
```

## 6. Architecture in 60 seconds

- **Commands, not control flow.** `Player.cs` only reads command fields (`CmdMove`, `CmdSprint`, …). `HumanDriver` and `TeamAI`/`GoalkeeperBrain` write those fields. Swapping who drives a player is just changing who writes — this is also the seam where a future network layer injects remote inputs.
- **The ball is never parented to anyone.** Dribbling = periodic real kicks ahead of the runner; first touches damp and scatter the ball based on skill and incoming speed. Possession is a spatial query in `MatchController.UpdatePossession`.
- **Physics-first feel** comes from a small set of mechanisms: speed-dependent turn-rate caps (no snap turns at pace), accel/decel curves, committed wind-up → execute → recover windows on every kick/tackle, stamina fatigue scaling speed and sharpness, and balance/strength-based body-contact outcomes.
- **All gameplay numbers** live in one `GameplayTuning` resource (~50 exported floats) with two shipped presets. The code falls back to hard-coded defaults if a `.tres` fails to load.
- **AI is deterministic and practical**: elastic formation homes shifted by ball position, a single pressing defender with hysteresis (plus an on-demand second presser), two-player loose-ball chasing toward the predicted stop point, and a scored shoot/pass/dribble decision on a slow tick for the AI carrier.

## 7. Tuning guide

Open `data/tuning_assisted.tres` (or `_sim`) in the Godot inspector or any text editor. The high-leverage dials:

| Feel complaint | Dial |
|---|---|
| Players feel sluggish / skatey | `Accel`, `Decel`, `TurnDegHighSpeed` |
| Dribbling too loose / too sticky | `DribbleTouchInterval`, `DribbleSpeedFactor`, `ControlRadius` |
| First touches too punishing | `TouchErrorBase`, `FirstTouchSpeedThreshold`, `ReceiveLockBase` |
| Passes feel laser-guided / wayward | `AimAssist`, `PassErrorDeg` |
| Shots too easy / too wild | `ShotErrorDeg`, `ShotSpeedMax`, `FinessePowerScale` |
| Ball rolls forever / dies | `BallRollFriction` |
| Tackles win too often | `TackleBaseChance`, `TackleActive` |
| Defence too passive | `AiPressTackleDist`, `AiPassPressureDist` |

Changes to `.tres` files take effect on the next match start (the menu reloads tuning).

## 8. Known limitations (deliberate v1 scope)

- **No throw-ins, corners or goal kicks** — invisible perimeter "boards" rebound the ball into play; a drop-ball reset fires if the ball settles somewhere dead.
- **No offside, no fouls/cards** — slide tackles are free; stumbles are the only consequence of contact.
- **Single period** — no halftime or side swap.
- **GK is always AI** — switching cycles outfielders only.
- **Placeholder visuals** — capsules with a facing "nose"; no animation rig.
- Kickoff is simplified: teams reset to formation, the designated taker plays a short pass, play goes live on first touch.

## 9. Multiplayer roadmap (not implemented, designed for)

The v1 architecture keeps the future ENet path cheap:

1. **State**: match-relevant state (ball pos/vel/spin, per-player pos/vel/state/stamina, score/clock/phase) is already concentrated in `MatchController`, `Ball`, `Player` — serialize it into a snapshot struct.
2. **Inputs**: `HumanDriver` already reduces all input to a small command set per tick. Networking = sending those commands to a server-authoritative simulation instead of writing them locally.
3. **Determinism**: all gameplay randomness flows through seeded `RandomNumberGenerator` instances — promote seeds to match state and the sim is replayable/verifiable.
4. Suggested order: extract a headless `MatchSim` (no nodes) → host it behind Godot's `ENetMultiplayerPeer` → client-side interpolation + input prediction.

## 10. License

MIT — see `LICENSE`. All assets are original/procedural; nothing here references real leagues, clubs or players.
