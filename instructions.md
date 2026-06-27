You are a principal gameplay engineer, physics programmer, technical game director, and tools architect.

Build a playable PC football game prototype as a vertical slice, not a full commercial game. The design goal is:
- physics feel inspired by eFootball: weighty, realistic, momentum-based, believable collisions, more natural ball behavior
- user experience and presentation inspired by EA Sports FC / FIFA: polished menus, responsive controls, readable HUD, clear camera behavior, smooth match flow

IMPORTANT SCOPE:
I do NOT want a huge fake project with placeholder architecture and broken files.
I want a genuinely working, small-but-playable prototype that I can run locally, plug in a controller, and play a match between 2 teams.
Prioritize correctness, playability, and code quality over feature count.

DELIVERABLES:
1. A working executable build target with clear run/build instructions
2. A full source repository with clean architecture that I can edit and extend
3. Controller support for at least one common gamepad
4. One playable local exhibition match between 2 teams
5. Basic menus to select teams and start a match
6. AI-controlled players for unselected players
7. A physics-focused gameplay core

TECH STACK:
Choose the best stack for actually delivering a working prototype fast.
Default preference:
- Godot 4.x with GDScript or C#
If you strongly believe another stack is better for a one-shot working prototype, justify it briefly in README and use that instead.
Do not choose a stack that is likely to fail to run locally without heavy setup.

CORE GAMEPLAY REQUIREMENTS:
- Top-down or broadcast-style football camera
- 2 teams on a pitch
- One full playable match loop with kickoff, play, goals, restart, clock, and simple end-of-match screen
- Controller input for movement, pass, through ball, shoot, sprint, tackle / pressure, player switch
- Keyboard fallback controls too
- Simple but functional team selection menu
- Basic HUD: score, timer, team names, player indicator, power bar for pass/shoot if implemented

PHYSICS REQUIREMENTS:
This is the most important part.
I want realistic-feeling football physics, not arcade pinball behavior.

Implement:
- ball velocity, angular velocity / spin approximation, friction, damping, restitution, ground rolling vs bouncing
- pass, shot, lob, and deflection behavior with different force profiles
- player momentum with acceleration/deceleration curves
- turning radius and inertia so players cannot pivot unrealistically
- collision responses influenced by relative speed, angle, body mass proxy, and balance
- shielding / body positioning effect on ball protection
- first touch quality influenced by player movement and incoming ball speed
- simple stamina effect on acceleration and recovery
- goalkeeper simplified but believable enough to test shots

GAMEPLAY DESIGN REQUIREMENTS:
I want the game to feel closer to simulation than pure arcade.
Create 2 tunable presets:
- Sim preset = heavier movement, more realistic traps, looser ball, more mistakes
- Assisted preset = slightly more responsive and forgiving
Expose these gameplay parameters in config files so I can tweak them later.

PLAYER MODELING REQUIREMENTS:
Do not attempt impossible full biomechanics.
Instead create a practical simulation layer with attributes such as:
- top speed
- acceleration
- agility
- balance
- strength
- ball control
- passing
- shooting
- defensive reach
- stamina
Use these to influence movement, first touch, duels, and action quality.

REAL-WORLD MECHANICS DIRECTION:
I eventually want each player to emulate their real-life tendencies on the pitch.
For this prototype, design the code architecture so this can later be extended with:
- playstyle traits
- preferred foot
- dribble aggressiveness
- passing risk appetite
- positioning discipline
- pressing intensity
- shooting tendencies
- weak foot / skill modifiers
Do not fully implement a database of real players now.
Just make the architecture ready for it.

AI REQUIREMENTS:
Implement simple but real gameplay AI:
- attacking support positioning
- defending shape
- nearest-player pressure
- basic passing decisions
- shooting in reasonable ranges
No fake “advanced ML AI”.
Use deterministic rule-based AI that actually works.

CODEBASE REQUIREMENTS:
- clean folder structure
- comments only where useful
- no huge monolithic files unless truly justified
- README with setup, controls, architecture, known limitations, and extension points
- gameplay constants isolated in config/data files
- avoid placeholder TODO spam
- if something is not implemented, say so clearly

BUILD REQUIREMENTS:
- provide a runnable local project
- provide build/export instructions for a desktop executable
- if possible include an export preset / packaging script
- ensure the project starts from the main scene / entry point without manual wiring

QUALITY BAR:
- better a small working football prototype than a fake AAA repo
- all included files must be consistent with each other
- no invented systems that are not wired into gameplay
- no broken imports
- no pseudo-code unless placed in a clearly marked docs/next_steps file

REPO STRUCTURE:
Please output the full repository contents file by file.
Start with:
1. repository tree
2. README
3. all source files
4. config/data files
5. build/export instructions
6. short explanation of gameplay tuning

VALIDATION:
Before finalizing, self-check that:
- the project can boot
- a match can start
- controls are mapped
- goals can be scored
- restart after goal works
- match timer works
- there are 2 playable teams
- the code matches the README

NON-GOALS:
- online multiplayer
- licenses for real clubs/players
- advanced mocap animation
- hyper-real graphics
- full career mode
- card collection modes
- gigantic content pipeline

If any requested feature would reduce the odds of a genuinely working prototype, cut scope intelligently and favor a robust vertical slice

Do not compress or summarize the repository. I want complete files with real code, even if the output is long.


-----

You are a principal game engineer, gameplay programmer, simulation designer, AI gameplay designer, and technical architect.

Build a genuinely playable football game prototype as a local-first vertical slice using Godot 4 + C#.
This is phase 1 of a much bigger ambition: I eventually want to turn this into a AAA-quality football game with realistic physics, deeper player behavior, and future online multiplayer.
But for this phase, do NOT build a huge fake AAA framework.
I want a small, honest, working prototype that I can play locally on my Windows machine with a controller, then export/share with friends for testing.

PRIMARY GOAL:
Create the highest-probability playable prototype, not the most ambitious architecture.
Optimize for:
- working local gameplay
- good football feel
- controller support
- clean and editable code
- Windows exportability
- future extensibility without overengineering

ENGINE + LANGUAGE:
- Use Godot 4.x
- Use C# for core gameplay and systems
- You may use GDScript only for lightweight editor glue or UI helpers if absolutely necessary, but prefer C# for important logic
- The project must open and run as a proper Godot project

TARGET PLATFORM:
- Windows desktop first
- The repository must include clear instructions to run in the editor and export a Windows executable
- Assume I want to share the exported build with friends for local testing

IMPORTANT SCOPE RULE:
Do NOT attempt a full commercial football game.
Do NOT build career mode, online multiplayer, licenses, card modes, advanced cinematics, or huge content pipelines.
Build one polished vertical slice that proves the gameplay direction.

DELIVERABLES:
1. Full Godot 4 project repository with all needed source files
2. Clear folder structure and complete code files
3. A playable local exhibition match between 2 teams
4. Controller support for at least one standard gamepad
5. Keyboard fallback controls
6. Match flow with kickoff, open play, goals, restart after goal, timer, and final whistle/end screen
7. Simple menu flow: title screen, team select, start match
8. Windows export instructions in README
9. Gameplay tuning/config files that I can edit later
10. A short architecture note explaining how this prototype could later evolve toward online multiplayer

QUALITY BAR:
- Better a small but truly working football prototype than a large but broken project
- All files must be consistent and wired together
- No fake placeholder systems unless clearly marked in a docs/next_steps section
- No pseudo-code in production files
- No missing scripts or broken scene references
- No giant monolithic file unless truly necessary

CORE GAMEPLAY REQUIREMENTS:
Build a playable football match prototype with:
- one pitch
- two teams
- a camera suitable for football gameplay (broadcast-style or smart elevated view)
- player movement
- player switching
- passing
- through pass or driven pass variant if feasible
- shooting
- sprinting
- tackle / pressure
- simple goalkeeper behavior
- goals and restarts
- visible scoreboard and match timer
- simple player indicator for the controlled player

PHYSICS / FEEL REQUIREMENTS:
This is the most important part.
I want the prototype to lean toward realistic football feel, inspired by eFootball-style weight and ball behavior more than arcade gameplay.

Implement a practical football simulation layer with:
- ball velocity and bounce behavior
- rolling friction and damping
- shot force vs pass force differences
- simple spin / curve approximation if feasible
- first touch quality affected by incoming speed and player movement
- player acceleration and deceleration
- turning inertia / turning radius so players cannot pivot unrealistically
- collision outcomes influenced by movement speed and body contact angle
- shielding / body positioning effect if feasible
- stamina or fatigue-lite effect if simple enough
Do NOT rely on chaotic raw physics for everything.
Tune for controllable but believable gameplay.

GAMEPLAY PRESETS:
Create at least 2 gameplay presets in editable config/data files:
1. Sim preset = heavier, more realistic, more loose-ball behavior, more mistakes
2. Assisted preset = slightly more responsive and forgiving

All important gameplay constants should be exposed in editable config/data files, not buried in code.

PLAYER MODEL REQUIREMENTS:
Use a practical player attribute model such as:
- max_speed
- acceleration
- agility
- balance
- strength
- ball_control
- pass_accuracy
- shot_power
- defensive_reach
- stamina
Use these attributes to influence gameplay outcomes where appropriate.

IMPORTANT:
Do NOT attempt a full real-player database or full biomechanics simulation.
However, design the architecture so I can later extend it with:
- preferred foot
- playstyle traits
- dribble tendency
- passing risk
- positioning discipline
- pressing intensity
- shooting tendency
- weak foot modifiers

AI REQUIREMENTS:
Implement simple but functional rule-based football AI:
- attacking support movement
- defending shape
- nearest-player pressure
- basic pass choice
- basic shot decision near goal
- simple restart positioning
Do NOT fake “advanced machine learning AI”.
Use deterministic and understandable game AI that actually works.

INPUT REQUIREMENTS:
- Support controller input for movement and actions
- Support keyboard fallback
- Input actions should be clearly named and easy to remap
- Show controls in README

UI / UX REQUIREMENTS:
I want the user experience to feel clear and usable.
Implement:
- title menu
- team selection
- match HUD with score and timer
- pause/restart option if feasible
- simple end-of-match screen
Keep UI clean and responsive, but do not waste time on fancy cosmetics.

GRAPHICS / ASSET REQUIREMENTS:
- Use simple placeholder-friendly visuals if needed, but keep the game readable
- Prioritize gameplay readability over realism
- Use simple field, players, and ball visuals if necessary
- Avoid dependency on licensed club/player assets
- If using placeholders, keep them organized and replaceable

ARCHITECTURE REQUIREMENTS:
Design the codebase to be clean and extendable without overengineering.
Separate concerns roughly into:
- match flow / game state
- player control
- ball simulation
- player simulation
- AI logic
- UI/HUD
- data/config
- input
Do NOT create a fake multi-engine abstraction framework.
Do NOT try to support Unreal or another engine in this repo.
This repo should be a strong Godot-first prototype.

MULTIPLAYER-READY DIRECTION:
Do NOT implement full multiplayer now unless it can be done without risking local play quality.
Instead, make the architecture multiplayer-aware where reasonable:
- keep match state explicit
- avoid hard-coding logic only in UI scenes
- keep player and ball state organized and serializable where practical
- include a README section called “Future Online Path” explaining how this could later move toward:
  - client/server architecture
  - authoritative match state
  - input replication
  - prediction/reconciliation
  - dedicated server approach in a later phase

REPOSITORY STRUCTURE:
Please output:
1. Repository tree
2. README.md
3. Project settings notes
4. Scene files and code files
5. Config/data files
6. Any export-related notes
7. A short explanation of tuning parameters

README REQUIREMENTS:
The README must include:
- project overview
- engine/version assumptions
- how to open/run in Godot
- how to test locally
- controls
- project structure
- known limitations
- Windows export steps
- future online path
- ideas for phase 2

EXPORT / SHARING REQUIREMENTS:
The prototype must be organized so I can export a Windows build and share it with friends.
Document:
- any Godot export template assumptions
- the Windows export flow
- any files/folders needed to distribute the build

IMPLEMENTATION PREFERENCE:
Favor robust simplicity over breadth.
If a feature risks breaking playability, reduce scope.
For example:
- simple goalkeeper is better than broken advanced keeper AI
- simple readable camera is better than cinematic camera
- simple team select is better than a complex front-end
- simple but tuned collision responses are better than fancy broken physics

VALIDATION CHECKLIST:
Before finalizing, self-check that:
- the Godot project boots
- scenes are wired correctly
- the match can start from menu flow
- controller input exists
- keyboard fallback exists
- a goal can be scored
- restart after goal works
- timer works
- game can end
- the README matches the code/project
- important tuning values are editable
- code is understandable and extendable

NON-GOALS FOR THIS PHASE:
- online multiplayer implementation
- licenses
- career mode
- ultimate-team style systems
- motion capture pipeline
- realistic broadcast package
- advanced commentary
- huge animation system
- advanced procedural biomechanics
- photoreal graphics

FINAL INSTRUCTION:
I care most about getting a prototype that is genuinely playable and editable.
Please be disciplined about scope.
Do not impress me with size.
Impress me with a football prototype that actually runs, feels coherent, and gives me a real base to build on.
Also make sure the repository is complete enough that I can edit gameplay dynamics later in Godot and export/share a Windows build with friends.

------










----------------------------------------------------------------------------

Final Prompt
___________________________________________________________________________

You are a principal game engineer, football gameplay designer, physics programmer, animation systems designer, AI gameplay engineer, and tools architect.

Build a genuinely playable football game prototype as a LOCAL-FIRST vertical slice using Godot 4.x + C# only.

This is phase 1 of a long-term ambition to eventually become a AAA-quality football game, but for this first deliverable I care most about one thing:
A working, locally playable, fun, believable football prototype that I can run on my Windows machine, plug in a controller, play properly, and then share the full repository and Windows executable with friends for testing.

Do NOT generate a fake AAA project.
Do NOT create a bloated architecture with unfinished systems.
Do NOT over-prioritize graphics over gameplay feel.
Do NOT build multiplayer in v1 unless it is extremely lightweight and does not reduce the quality of the local prototype.
Do not compress, summarize, or truncate the repository. I want the full Godot project with all files, even if the output is long.

My goals for v1:
- local play on Windows
- exported executable build target
- full editable source repository
- controller support
- football feel that is as human-like and real-world-like as possible within a small prototype
- ball motion and player movement that feel grounded, weighty, and believable
- architecture that can later evolve toward online multiplayer and a more advanced future game

Additional v1 goals:
- The game must be buildable and playable using only free resources, free tools, and redistributable assets.
- Do not depend on paid assets, paid plugins, subscriptions, or licensed football content.
- Anyone I share the project with should be able to test it either by opening the repository in Godot or by running the exported Windows executable.
- Prefer a self-contained Windows export with clear packaging instructions and any required license notices for redistribution.
- Target full 11v11 if it can be implemented cleanly without harming prototype reliability, football feel, or code quality.
- If full 11v11 would reduce the quality of the prototype, fall back to the largest reliable player count that still preserves realistic spacing and football behavior, and document that decision clearly.

ENGINE AND LANGUAGE:
- Use Godot 4.x
- Use C# for gameplay systems
- You may use lightweight GDScript only if absolutely necessary for editor/tooling convenience, but gameplay systems should stay in C#
- Structure the project so it opens cleanly in Godot and can be exported to Windows Desktop

DELIVERABLES:
1. Full Godot project repository
2. Clean folder structure
3. All source code files
4. Project settings and input mappings
5. README with:
   - setup instructions
   - how to open and run in Godot
   - how to export a Windows executable
   - controls for keyboard and controller
   - architecture overview
   - known limitations
   - extension roadmap for future online multiplayer
6. Export instructions for Windows Desktop executable
7. A playable match prototype that starts and runs locally

CORE PHASE-1 REQUIREMENTS:
Create one playable local exhibition match between 2 teams on one pitch.
The prototype must support:
- kickoff
- live play
- goals
- restart after goal
- match timer
- score display
- end-of-match state or simple final whistle flow
- controller input for one main player
- AI control for other players
- player switching
- keyboard fallback controls

MATCH SCOPE:
Keep scope tight and robust:
- 2 teams
- small roster per team if needed for reliability
- 1 stadium / pitch
- 1 match mode only
- no career mode
- no online play in v1
- no commentary system
- no licensing / real clubs / real players
- no advanced cinematic systems unless already very easy

GAMEPLAY FEEL:
The intended feel is:
- ball and body physics inspired by more realistic football sims
- UX clarity and responsiveness inspired by polished mainstream football games
- not arcade pinball
- not overly floaty
- not twitchy or robotic

The game must feel like footballers have weight, momentum, recovery time, balance, and limits.
The ball must feel like a separate physical object, not glued to the player.

PHYSICS REQUIREMENTS:
This is the highest priority area.

Implement a practical football simulation layer with believable behavior:
- ball velocity
- spin approximation / angular influence where practical
- rolling friction
- bounce / restitution tuning
- damping
- different force profiles for pass, through pass, lofted pass, and shot
- first touch behavior affected by incoming speed and player movement
- collisions and deflections
- loose-ball behavior
- player acceleration and deceleration
- turning inertia / turning radius
- momentum so players cannot snap-turn unrealistically
- body contact outcomes influenced by direction, speed, balance, and strength proxies
- simple shielding behavior
- stamina or fatigue effect on repeated sprinting, recovery, and action sharpness

The implementation does NOT need perfect real-world rigid-body scientific simulation.
But it MUST produce gameplay that feels believable, weighty, and human-like.

PLAYER MOVEMENT REQUIREMENTS:
Players should look and behave as human as possible within prototype scope.
Avoid robotic movement.

Implement:
- locomotion states such as idle, jog, run, sprint, turn, receive, kick, recover
- acceleration curves
- turning delays
- action commitment windows so shots/passes/tackles are not instant teleport actions
- recovery windows after strong actions
- foot-to-ball interaction timing abstraction
- movement penalties when receiving awkward balls or turning under momentum
- context-sensitive first touch quality

If full advanced animation is not possible, simulate human-like behavior through state timing, speed curves, directional inertia, and action lock windows.

BALL INTERACTION REQUIREMENTS:
- grounded pass behavior
- longer driven pass behavior
- lob pass behavior
- through ball behavior
- shot behavior with stronger force and shot preparation
- finesse shot modifier if feasible
- reasonable ball slowdown across grass
- believable ricochet / interception outcomes
- controllable but imperfect first touch
- ball separation from player when touch quality is poor
- no magnetized dribbling

AI REQUIREMENTS:
Use deterministic, practical AI that actually works.
No fake ML systems.

Implement simple but useful AI:
- support runs
- basic attacking spacing
- simple defensive shape
- nearest-player pressure
- pass selection
- shot selection in sensible ranges
- goalkeeper positioning and save attempts simplified but believable

CONTROLS AND INPUT SCHEME:
Use a football-game style controller layout inspired by mainstream console football games.
Implement this as named Godot input actions so the bindings are easy to change later.
Support Xbox and PlayStation-equivalent mappings under the same action scheme.
Use keyboard fallback controls too.

RIGHT FACE BUTTONS / PRIMARY ACTIONS

ATTACK:
- Shoot = B on Xbox / Circle on PlayStation
- Ground Pass = A on Xbox / Cross on PlayStation
- Lob Pass = X on Xbox / Square on PlayStation
- Through Ball = Y on Xbox / Triangle on PlayStation

DEFENCE:
- Standing Tackle = B on Xbox / Circle on PlayStation
- Cover Ball / Follow Ball / Pressure = A on Xbox / Cross on PlayStation
- Slide Tackle = X on Xbox / Square on PlayStation
- Y / Triangle on defence can remain unused in v1 unless a good defensive action is easy to implement cleanly

TRIGGERS / SHOULDERS / OTHER CONTROLS

ATTACK:
- Sprint = RT / R2
- Hold Ball / Shield Ball = LT / L2
- Finesse modifier = RB / R1
- LB / L1 acts as an advanced contextual modifier
  - LB/L1 + Through Ball = lobbed through pass if implemented
  - LB/L1 + Ground Pass = pass-and-run only if it can be implemented cleanly
  - if multiple attack behaviors conflict, prioritize the simplest working version and document it clearly

DEFENCE:
- Sprint = RT / R2
- Player Change = LB / L1
- Jockey = LT / L2
- Call Second Player = RB / R1

MOVEMENT:
- Left Stick = player movement
- Right Stick is optional in v1 and should not be required unless used for simple camera support or future skill controls

INPUT DESIGN RULES:
- Use Godot Input Map actions, not hardcoded per-device checks
- Make the same gameplay actions work for both Xbox and PlayStation-style controllers
- Include keyboard fallback controls
- Put all input action names in a clean, readable convention
- Document all bindings in the README
- Use sensible deadzones for sticks and triggers
- Keep the control scheme responsive and readable for testing

GAMEPLAY INTERPRETATION OF CONTROLS:
- Tap pass buttons for normal actions
- Hold where useful for power accumulation or stronger versions if implemented
- Finesse modifier should affect shot behavior and optionally controlled passing if simple to support
- Hold Ball should prioritize shielding and body orientation
- Cover Ball / Follow Ball / Pressure on defence should help the controlled defender track the ball carrier or close space
- Call Second Player should trigger basic teammate pressure support if feasible
- Player Change should switch to the most sensible nearby defender or closest useful player
- If some advanced combinations are too risky for v1, simplify them while preserving the overall control philosophy and document the exact implemented behavior

CAMERA AND UX:
Implement a readable football gameplay camera:
- broadcast-style or slightly elevated tactical angle
- camera follows the flow of play smoothly
- score and timer HUD
- simple start menu or match start screen
- simple team select if feasible
- visible selected-player indicator
- shot/pass power bar if practical

VISUAL / ANIMATION APPROACH:
Prioritize functional clarity and believable movement over flashy visuals.
Simple visuals are acceptable if the game is playable and readable.

Use:
- readable player indicators
- visible team differentiation
- simple HUD
- clean camera
- basic animation/state transitions if available

Do not spend too much of the implementation budget on high-end art polish.

ARCHITECTURE REQUIREMENTS:
Design the repository so it is clean and extendable.

Use clear separation between:
- domain/game rules
- player logic
- ball logic
- AI logic
- match flow/state
- input
- presentation/UI
- config/tuning data

Keep gameplay constants data-driven where possible.
Place tunable parameters in clearly named config/resources/data files so I can adjust gameplay feel later.

FUTURE-PROOFING REQUIREMENTS:
Even though v1 is local only, design it so it can later be extended toward online multiplayer.
Do NOT implement full online multiplayer now.
Instead:
- avoid local-only hacks that would block future sync
- keep match state explicit
- keep player and ball state structured and serializable where practical
- include a short README section describing a future path toward ENet-based or server-authoritative multiplayer architecture in later phases

CONFIGURABLE GAMEPLAY PRESETS:
Create at least 2 gameplay presets exposed in config:
1. Sim preset:
   - heavier movement
   - more momentum
   - more imperfect control
   - looser ball
   - stronger punishment for poor balance / awkward turns
2. Assisted preset:
   - slightly more responsive control
   - cleaner trapping
   - slightly easier passing/shooting behavior

These presets should be easy to tune later.

PLAYER ATTRIBUTE MODEL:
Implement a small, data-driven player attribute system using values such as:
- top speed
- acceleration
- agility
- balance
- strength
- ball control
- passing
- shooting
- defensive reach
- stamina

These attributes should influence gameplay outcomes.

WINDOWS SHARING REQUIREMENTS:
The project should be structured so I can:
- run it locally in Godot
- export a Windows Desktop executable
- share the executable and repository with friends for testing

Please include:
- explicit Windows export steps in the README
- any assumptions about export templates
- any limitations related to sharing the executable

CODE QUALITY REQUIREMENTS:
- no giant monolithic script unless absolutely necessary
- no placeholder pseudo-code passed off as implementation
- no broken references
- no fake systems that are not wired into gameplay
- comments only where helpful
- consistent naming
- buildable / runnable project structure
- all code and files should agree with the README

OUTPUT FORMAT:
Output the repository in this order:
1. repository tree
2. README.md
3. project.godot and relevant project config
4. input mappings
5. gameplay source files
6. scenes/resources/data files
7. UI/HUD files
8. export notes
9. short explanation of how to tune gameplay feel

VALIDATION BEFORE FINALIZING:
Before final output, self-check that:
- the project boots
- the match can start
- one player is controllable
- AI players move and participate
- the ball can be passed and shot
- goals can be scored
- score updates correctly
- play restarts after goals
- the timer works
- keyboard and controller mappings exist
- the codebase is coherent
- the README matches the project

NON-GOALS FOR V1:
- real player likenesses
- full biomechanics simulation
- online multiplayer implementation
- live service systems
- ultimate-team-style modes
- career mode
- advanced replay suite
- photoreal graphics
- giant asset pipelines

Target platform:
- Windows 11 desktop
- should run comfortably on a machine with an 8 GB VRAM GPU

Most important instruction:
I want the highest-probability playable football prototype, not the most ambitious fake architecture.
Favor a smaller, coherent, working game over a broad but broken project.
If scope must be reduced, preserve:
1. playability
2. believable football feel
3. clean code
4. Windows local testing and sharing

------------------------------------------------------------------

Claude reasoning
__________________________________________________________________

You are a principal gameplay engineer, physics programmer, technical game director, and tools architect.

Build a playable PC football game prototype as a vertical slice, not a full commercial game. The design goal is:
- physics feel inspired by eFootball: weighty, realistic, momentum-based, believable collisions, more natural ball behavior
- user experience and presentation inspired by EA Sports FC / FIFA: polished menus, responsive controls, readable HUD, clear camera behavior, smooth match flow

IMPORTANT SCOPE:
I do NOT want a huge fake project with placeholder architecture and broken files.
I want a genuinely working, small-but-playable prototype that I can run locally, plug in a controller, and play a match between 2 teams.
Prioritize correctness, playability, and code quality over feature count.

DELIVERABLES:
1. A working executable build target with clear run/build instructions
2. A full source repository with clean architecture that I can edit and extend
3. Controller support for at least one common gamepad
4. One playable local exhibition match between 2 teams
5. Basic menus to select teams and start a match
6. AI-controlled players for unselected players
7. A physics-focused gameplay core

TECH STACK:
Choose the best stack for actually delivering a working prototype fast.
Default preference:
- Godot 4.x with GDScript or C#
If you strongly believe another stack is better for a one-shot working prototype, justify it briefly in README and use that instead.
Do not choose a stack that is likely to fail to run locally without heavy setup.

CORE GAMEPLAY REQUIREMENTS:
- Top-down or broadcast-style football camera
- 2 teams on a pitch
- One full playable match loop with kickoff, play, goals, restart, clock, and simple end-of-match screen
- Controller input for movement, pass, through ball, shoot, sprint, tackle / pressure, player switch
- Keyboard fallback controls too
- Simple but functional team selection menu
- Basic HUD: score, timer, team names, player indicator, power bar for pass/shoot if implemented

PHYSICS REQUIREMENTS:
This is the most important part.
I want realistic-feeling football physics, not arcade pinball behavior.

Implement:
- ball velocity, angular velocity / spin approximation, friction, damping, restitution, ground rolling vs bouncing
- pass, shot, lob, and deflection behavior with different force profiles
- player momentum with acceleration/deceleration curves
- turning radius and inertia so players cannot pivot unrealistically
- collision responses influenced by relative speed, angle, body mass proxy, and balance
- shielding / body positioning effect on ball protection
- first touch quality influenced by player movement and incoming ball speed
- simple stamina effect on acceleration and recovery
- goalkeeper simplified but believable enough to test shots

GAMEPLAY DESIGN REQUIREMENTS:
I want the game to feel closer to simulation than pure arcade.
Create 2 tunable presets:
- Sim preset = heavier movement, more realistic traps, looser ball, more mistakes
- Assisted preset = slightly more responsive and forgiving
Expose these gameplay parameters in config files so I can tweak them later.

PLAYER MODELING REQUIREMENTS:
Do not attempt impossible full biomechanics.
Instead create a practical simulation layer with attributes such as:
- top speed
- acceleration
- agility
- balance
- strength
- ball control
- passing
- shooting
- defensive reach
- stamina
Use these to influence movement, first touch, duels, and action quality.

REAL-WORLD MECHANICS DIRECTION:
I eventually want each player to emulate their real-life tendencies on the pitch.
For this prototype, design the code architecture so this can later be extended with:
- playstyle traits
- preferred foot
- dribble aggressiveness
- passing risk appetite
- positioning discipline
- pressing intensity
- shooting tendencies
- weak foot / skill modifiers
Do not fully implement a database of real players now.
Just make the architecture ready for it.

AI REQUIREMENTS:
Implement simple but real gameplay AI:
- attacking support positioning
- defending shape
- nearest-player pressure
- basic passing decisions
- shooting in reasonable ranges
No fake “advanced ML AI”.
Use deterministic rule-based AI that actually works.

CODEBASE REQUIREMENTS:
- clean folder structure
- comments only where useful
- no huge monolithic files unless truly justified
- README with setup, controls, architecture, known limitations, and extension points
- gameplay constants isolated in config/data files
- avoid placeholder TODO spam
- if something is not implemented, say so clearly

BUILD REQUIREMENTS:
- provide a runnable local project
- provide build/export instructions for a desktop executable
- if possible include an export preset / packaging script
- ensure the project starts from the main scene / entry point without manual wiring

QUALITY BAR:
- better a small working football prototype than a fake AAA repo
- all included files must be consistent with each other
- no invented systems that are not wired into gameplay
- no broken imports
- no pseudo-code unless placed in a clearly marked docs/next_steps file

REPO STRUCTURE:
Please output the full repository contents file by file.
Start with:
1. repository tree
2. README
3. all source files
4. config/data files
5. build/export instructions
6. short explanation of gameplay tuning

VALIDATION:
Before finalizing, self-check that:
- the project can boot
- a match can start
- controls are mapped
- goals can be scored
- restart after goal works
- match timer works
- there are 2 playable teams
- the code matches the README

NON-GOALS:
- online multiplayer
- licenses for real clubs/players
- advanced mocap animation
- hyper-real graphics
- full career mode
- card collection modes
- gigantic content pipeline

If any requested feature would reduce the odds of a genuinely working prototype, cut scope intelligently and favor a robust vertical slice

Do not compress or summarize the repository. I want complete files with real code, even if the output is long.


-----

You are a principal game engineer, gameplay programmer, simulation designer, AI gameplay designer, and technical architect.

Build a genuinely playable football game prototype as a local-first vertical slice using Godot 4 + C#.
This is phase 1 of a much bigger ambition: I eventually want to turn this into a AAA-quality football game with realistic physics, deeper player behavior, and future online multiplayer.
But for this phase, do NOT build a huge fake AAA framework.
I want a small, honest, working prototype that I can play locally on my Windows machine with a controller, then export/share with friends for testing.

PRIMARY GOAL:
Create the highest-probability playable prototype, not the most ambitious architecture.
Optimize for:
- working local gameplay
- good football feel
- controller support
- clean and editable code
- Windows exportability
- future extensibility without overengineering

ENGINE + LANGUAGE:
- Use Godot 4.x
- Use C# for core gameplay and systems
- You may use GDScript only for lightweight editor glue or UI helpers if absolutely necessary, but prefer C# for important logic
- The project must open and run as a proper Godot project

TARGET PLATFORM:
- Windows desktop first
- The repository must include clear instructions to run in the editor and export a Windows executable
- Assume I want to share the exported build with friends for local testing

IMPORTANT SCOPE RULE:
Do NOT attempt a full commercial football game.
Do NOT build career mode, online multiplayer, licenses, card modes, advanced cinematics, or huge content pipelines.
Build one polished vertical slice that proves the gameplay direction.

DELIVERABLES:
1. Full Godot 4 project repository with all needed source files
2. Clear folder structure and complete code files
3. A playable local exhibition match between 2 teams
4. Controller support for at least one standard gamepad
5. Keyboard fallback controls
6. Match flow with kickoff, open play, goals, restart after goal, timer, and final whistle/end screen
7. Simple menu flow: title screen, team select, start match
8. Windows export instructions in README
9. Gameplay tuning/config files that I can edit later
10. A short architecture note explaining how this prototype could later evolve toward online multiplayer

QUALITY BAR:
- Better a small but truly working football prototype than a large but broken project
- All files must be consistent and wired together
- No fake placeholder systems unless clearly marked in a docs/next_steps section
- No pseudo-code in production files
- No missing scripts or broken scene references
- No giant monolithic file unless truly necessary

CORE GAMEPLAY REQUIREMENTS:
Build a playable football match prototype with:
- one pitch
- two teams
- a camera suitable for football gameplay (broadcast-style or smart elevated view)
- player movement
- player switching
- passing
- through pass or driven pass variant if feasible
- shooting
- sprinting
- tackle / pressure
- simple goalkeeper behavior
- goals and restarts
- visible scoreboard and match timer
- simple player indicator for the controlled player

PHYSICS / FEEL REQUIREMENTS:
This is the most important part.
I want the prototype to lean toward realistic football feel, inspired by eFootball-style weight and ball behavior more than arcade gameplay.

Implement a practical football simulation layer with:
- ball velocity and bounce behavior
- rolling friction and damping
- shot force vs pass force differences
- simple spin / curve approximation if feasible
- first touch quality affected by incoming speed and player movement
- player acceleration and deceleration
- turning inertia / turning radius so players cannot pivot unrealistically
- collision outcomes influenced by movement speed and body contact angle
- shielding / body positioning effect if feasible
- stamina or fatigue-lite effect if simple enough
Do NOT rely on chaotic raw physics for everything.
Tune for controllable but believable gameplay.

GAMEPLAY PRESETS:
Create at least 2 gameplay presets in editable config/data files:
1. Sim preset = heavier, more realistic, more loose-ball behavior, more mistakes
2. Assisted preset = slightly more responsive and forgiving

All important gameplay constants should be exposed in editable config/data files, not buried in code.

PLAYER MODEL REQUIREMENTS:
Use a practical player attribute model such as:
- max_speed
- acceleration
- agility
- balance
- strength
- ball_control
- pass_accuracy
- shot_power
- defensive_reach
- stamina
Use these attributes to influence gameplay outcomes where appropriate.

IMPORTANT:
Do NOT attempt a full real-player database or full biomechanics simulation.
However, design the architecture so I can later extend it with:
- preferred foot
- playstyle traits
- dribble tendency
- passing risk
- positioning discipline
- pressing intensity
- shooting tendency
- weak foot modifiers

AI REQUIREMENTS:
Implement simple but functional rule-based football AI:
- attacking support movement
- defending shape
- nearest-player pressure
- basic pass choice
- basic shot decision near goal
- simple restart positioning
Do NOT fake “advanced machine learning AI”.
Use deterministic and understandable game AI that actually works.

INPUT REQUIREMENTS:
- Support controller input for movement and actions
- Support keyboard fallback
- Input actions should be clearly named and easy to remap
- Show controls in README

UI / UX REQUIREMENTS:
I want the user experience to feel clear and usable.
Implement:
- title menu
- team selection
- match HUD with score and timer
- pause/restart option if feasible
- simple end-of-match screen
Keep UI clean and responsive, but do not waste time on fancy cosmetics.

GRAPHICS / ASSET REQUIREMENTS:
- Use simple placeholder-friendly visuals if needed, but keep the game readable
- Prioritize gameplay readability over realism
- Use simple field, players, and ball visuals if necessary
- Avoid dependency on licensed club/player assets
- If using placeholders, keep them organized and replaceable

ARCHITECTURE REQUIREMENTS:
Design the codebase to be clean and extendable without overengineering.
Separate concerns roughly into:
- match flow / game state
- player control
- ball simulation
- player simulation
- AI logic
- UI/HUD
- data/config
- input
Do NOT create a fake multi-engine abstraction framework.
Do NOT try to support Unreal or another engine in this repo.
This repo should be a strong Godot-first prototype.

MULTIPLAYER-READY DIRECTION:
Do NOT implement full multiplayer now unless it can be done without risking local play quality.
Instead, make the architecture multiplayer-aware where reasonable:
- keep match state explicit
- avoid hard-coding logic only in UI scenes
- keep player and ball state organized and serializable where practical
- include a README section called “Future Online Path” explaining how this could later move toward:
  - client/server architecture
  - authoritative match state
  - input replication
  - prediction/reconciliation
  - dedicated server approach in a later phase

REPOSITORY STRUCTURE:
Please output:
1. Repository tree
2. README.md
3. Project settings notes
4. Scene files and code files
5. Config/data files
6. Any export-related notes
7. A short explanation of tuning parameters

README REQUIREMENTS:
The README must include:
- project overview
- engine/version assumptions
- how to open/run in Godot
- how to test locally
- controls
- project structure
- known limitations
- Windows export steps
- future online path
- ideas for phase 2

EXPORT / SHARING REQUIREMENTS:
The prototype must be organized so I can export a Windows build and share it with friends.
Document:
- any Godot export template assumptions
- the Windows export flow
- any files/folders needed to distribute the build

IMPLEMENTATION PREFERENCE:
Favor robust simplicity over breadth.
If a feature risks breaking playability, reduce scope.
For example:
- simple goalkeeper is better than broken advanced keeper AI
- simple readable camera is better than cinematic camera
- simple team select is better than a complex front-end
- simple but tuned collision responses are better than fancy broken physics

VALIDATION CHECKLIST:
Before finalizing, self-check that:
- the Godot project boots
- scenes are wired correctly
- the match can start from menu flow
- controller input exists
- keyboard fallback exists
- a goal can be scored
- restart after goal works
- timer works
- game can end
- the README matches the code/project
- important tuning values are editable
- code is understandable and extendable

NON-GOALS FOR THIS PHASE:
- online multiplayer implementation
- licenses
- career mode
- ultimate-team style systems
- motion capture pipeline
- realistic broadcast package
- advanced commentary
- huge animation system
- advanced procedural biomechanics
- photoreal graphics

FINAL INSTRUCTION:
I care most about getting a prototype that is genuinely playable and editable.
Please be disciplined about scope.
Do not impress me with size.
Impress me with a football prototype that actually runs, feels coherent, and gives me a real base to build on.
Also make sure the repository is complete enough that I can edit gameplay dynamics later in Godot and export/share a Windows build with friends.

------










----------------------------------------------------------------------------

Final Prompt
___________________________________________________________________________

You are a principal game engineer, football gameplay designer, physics programmer, animation systems designer, AI gameplay engineer, and tools architect.

Build a genuinely playable football game prototype as a LOCAL-FIRST vertical slice using Godot 4.x + C# only.

This is phase 1 of a long-term ambition to eventually become a AAA-quality football game, but for this first deliverable I care most about one thing:
A working, locally playable, fun, believable football prototype that I can run on my Windows machine, plug in a controller, play properly, and then share the full repository and Windows executable with friends for testing.

Do NOT generate a fake AAA project.
Do NOT create a bloated architecture with unfinished systems.
Do NOT over-prioritize graphics over gameplay feel.
Do NOT build multiplayer in v1 unless it is extremely lightweight and does not reduce the quality of the local prototype.
Do not compress, summarize, or truncate the repository. I want the full Godot project with all files, even if the output is long.

My goals for v1:
- local play on Windows
- exported executable build target
- full editable source repository
- controller support
- football feel that is as human-like and real-world-like as possible within a small prototype
- ball motion and player movement that feel grounded, weighty, and believable
- architecture that can later evolve toward online multiplayer and a more advanced future game

Additional v1 goals:
- The game must be buildable and playable using only free resources, free tools, and redistributable assets.
- Do not depend on paid assets, paid plugins, subscriptions, or licensed football content.
- Anyone I share the project with should be able to test it either by opening the repository in Godot or by running the exported Windows executable.
- Prefer a self-contained Windows export with clear packaging instructions and any required license notices for redistribution.
- Target full 11v11 if it can be implemented cleanly without harming prototype reliability, football feel, or code quality.
- If full 11v11 would reduce the quality of the prototype, fall back to the largest reliable player count that still preserves realistic spacing and football behavior, and document that decision clearly.

ENGINE AND LANGUAGE:
- Use Godot 4.x
- Use C# for gameplay systems
- You may use lightweight GDScript only if absolutely necessary for editor/tooling convenience, but gameplay systems should stay in C#
- Structure the project so it opens cleanly in Godot and can be exported to Windows Desktop

DELIVERABLES:
1. Full Godot project repository
2. Clean folder structure
3. All source code files
4. Project settings and input mappings
5. README with:
   - setup instructions
   - how to open and run in Godot
   - how to export a Windows executable
   - controls for keyboard and controller
   - architecture overview
   - known limitations
   - extension roadmap for future online multiplayer
6. Export instructions for Windows Desktop executable
7. A playable match prototype that starts and runs locally

CORE PHASE-1 REQUIREMENTS:
Create one playable local exhibition match between 2 teams on one pitch.
The prototype must support:
- kickoff
- live play
- goals
- restart after goal
- match timer
- score display
- end-of-match state or simple final whistle flow
- controller input for one main player
- AI control for other players
- player switching
- keyboard fallback controls

MATCH SCOPE:
Keep scope tight and robust:
- 2 teams
- small roster per team if needed for reliability
- 1 stadium / pitch
- 1 match mode only
- no career mode
- no online play in v1
- no commentary system
- no licensing / real clubs / real players
- no advanced cinematic systems unless already very easy

GAMEPLAY FEEL:
The intended feel is:
- ball and body physics inspired by more realistic football sims
- UX clarity and responsiveness inspired by polished mainstream football games
- not arcade pinball
- not overly floaty
- not twitchy or robotic

The game must feel like footballers have weight, momentum, recovery time, balance, and limits.
The ball must feel like a separate physical object, not glued to the player.

PHYSICS REQUIREMENTS:
This is the highest priority area.

Implement a practical football simulation layer with believable behavior:
- ball velocity
- spin approximation / angular influence where practical
- rolling friction
- bounce / restitution tuning
- damping
- different force profiles for pass, through pass, lofted pass, and shot
- first touch behavior affected by incoming speed and player movement
- collisions and deflections
- loose-ball behavior
- player acceleration and deceleration
- turning inertia / turning radius
- momentum so players cannot snap-turn unrealistically
- body contact outcomes influenced by direction, speed, balance, and strength proxies
- simple shielding behavior
- stamina or fatigue effect on repeated sprinting, recovery, and action sharpness

The implementation does NOT need perfect real-world rigid-body scientific simulation.
But it MUST produce gameplay that feels believable, weighty, and human-like.

PLAYER MOVEMENT REQUIREMENTS:
Players should look and behave as human as possible within prototype scope.
Avoid robotic movement.

Implement:
- locomotion states such as idle, jog, run, sprint, turn, receive, kick, recover
- acceleration curves
- turning delays
- action commitment windows so shots/passes/tackles are not instant teleport actions
- recovery windows after strong actions
- foot-to-ball interaction timing abstraction
- movement penalties when receiving awkward balls or turning under momentum
- context-sensitive first touch quality

If full advanced animation is not possible, simulate human-like behavior through state timing, speed curves, directional inertia, and action lock windows.

BALL INTERACTION REQUIREMENTS:
- grounded pass behavior
- longer driven pass behavior
- lob pass behavior
- through ball behavior
- shot behavior with stronger force and shot preparation
- finesse shot modifier if feasible
- reasonable ball slowdown across grass
- believable ricochet / interception outcomes
- controllable but imperfect first touch
- ball separation from player when touch quality is poor
- no magnetized dribbling

AI REQUIREMENTS:
Use deterministic, practical AI that actually works.
No fake ML systems.

Implement simple but useful AI:
- support runs
- basic attacking spacing
- simple defensive shape
- nearest-player pressure
- pass selection
- shot selection in sensible ranges
- goalkeeper positioning and save attempts simplified but believable

CONTROLS AND INPUT SCHEME:
Use a football-game style controller layout inspired by mainstream console football games.
Implement this as named Godot input actions so the bindings are easy to change later.
Support Xbox and PlayStation-equivalent mappings under the same action scheme.
Use keyboard fallback controls too.

RIGHT FACE BUTTONS / PRIMARY ACTIONS

ATTACK:
- Shoot = B on Xbox / Circle on PlayStation
- Ground Pass = A on Xbox / Cross on PlayStation
- Lob Pass = X on Xbox / Square on PlayStation
- Through Ball = Y on Xbox / Triangle on PlayStation

DEFENCE:
- Standing Tackle = B on Xbox / Circle on PlayStation
- Cover Ball / Follow Ball / Pressure = A on Xbox / Cross on PlayStation
- Slide Tackle = X on Xbox / Square on PlayStation
- Y / Triangle on defence can remain unused in v1 unless a good defensive action is easy to implement cleanly

TRIGGERS / SHOULDERS / OTHER CONTROLS

ATTACK:
- Sprint = RT / R2
- Hold Ball / Shield Ball = LT / L2
- Finesse modifier = RB / R1
- LB / L1 acts as an advanced contextual modifier
  - LB/L1 + Through Ball = lobbed through pass if implemented
  - LB/L1 + Ground Pass = pass-and-run only if it can be implemented cleanly
  - if multiple attack behaviors conflict, prioritize the simplest working version and document it clearly

DEFENCE:
- Sprint = RT / R2
- Player Change = LB / L1
- Jockey = LT / L2
- Call Second Player = RB / R1

MOVEMENT:
- Left Stick = player movement
- Right Stick is optional in v1 and should not be required unless used for simple camera support or future skill controls

INPUT DESIGN RULES:
- Use Godot Input Map actions, not hardcoded per-device checks
- Make the same gameplay actions work for both Xbox and PlayStation-style controllers
- Include keyboard fallback controls
- Put all input action names in a clean, readable convention
- Document all bindings in the README
- Use sensible deadzones for sticks and triggers
- Keep the control scheme responsive and readable for testing

GAMEPLAY INTERPRETATION OF CONTROLS:
- Tap pass buttons for normal actions
- Hold where useful for power accumulation or stronger versions if implemented
- Finesse modifier should affect shot behavior and optionally controlled passing if simple to support
- Hold Ball should prioritize shielding and body orientation
- Cover Ball / Follow Ball / Pressure on defence should help the controlled defender track the ball carrier or close space
- Call Second Player should trigger basic teammate pressure support if feasible
- Player Change should switch to the most sensible nearby defender or closest useful player
- If some advanced combinations are too risky for v1, simplify them while preserving the overall control philosophy and document the exact implemented behavior

CAMERA AND UX:
Implement a readable football gameplay camera:
- broadcast-style or slightly elevated tactical angle
- camera follows the flow of play smoothly
- score and timer HUD
- simple start menu or match start screen
- simple team select if feasible
- visible selected-player indicator
- shot/pass power bar if practical

VISUAL / ANIMATION APPROACH:
Prioritize functional clarity and believable movement over flashy visuals.
Simple visuals are acceptable if the game is playable and readable.

Use:
- readable player indicators
- visible team differentiation
- simple HUD
- clean camera
- basic animation/state transitions if available

Do not spend too much of the implementation budget on high-end art polish.

ARCHITECTURE REQUIREMENTS:
Design the repository so it is clean and extendable.

Use clear separation between:
- domain/game rules
- player logic
- ball logic
- AI logic
- match flow/state
- input
- presentation/UI
- config/tuning data

Keep gameplay constants data-driven where possible.
Place tunable parameters in clearly named config/resources/data files so I can adjust gameplay feel later.

FUTURE-PROOFING REQUIREMENTS:
Even though v1 is local only, design it so it can later be extended toward online multiplayer.
Do NOT implement full online multiplayer now.
Instead:
- avoid local-only hacks that would block future sync
- keep match state explicit
- keep player and ball state structured and serializable where practical
- include a short README section describing a future path toward ENet-based or server-authoritative multiplayer architecture in later phases

CONFIGURABLE GAMEPLAY PRESETS:
Create at least 2 gameplay presets exposed in config:
1. Sim preset:
   - heavier movement
   - more momentum
   - more imperfect control
   - looser ball
   - stronger punishment for poor balance / awkward turns
2. Assisted preset:
   - slightly more responsive control
   - cleaner trapping
   - slightly easier passing/shooting behavior

These presets should be easy to tune later.

PLAYER ATTRIBUTE MODEL:
Implement a small, data-driven player attribute system using values such as:
- top speed
- acceleration
- agility
- balance
- strength
- ball control
- passing
- shooting
- defensive reach
- stamina

These attributes should influence gameplay outcomes.

WINDOWS SHARING REQUIREMENTS:
The project should be structured so I can:
- run it locally in Godot
- export a Windows Desktop executable
- share the executable and repository with friends for testing

Please include:
- explicit Windows export steps in the README
- any assumptions about export templates
- any limitations related to sharing the executable

CODE QUALITY REQUIREMENTS:
- no giant monolithic script unless absolutely necessary
- no placeholder pseudo-code passed off as implementation
- no broken references
- no fake systems that are not wired into gameplay
- comments only where helpful
- consistent naming
- buildable / runnable project structure
- all code and files should agree with the README

OUTPUT FORMAT:
Output the repository in this order:
1. repository tree
2. README.md
3. project.godot and relevant project config
4. input mappings
5. gameplay source files
6. scenes/resources/data files
7. UI/HUD files
8. export notes
9. short explanation of how to tune gameplay feel

VALIDATION BEFORE FINALIZING:
Before final output, self-check that:
- the project boots
- the match can start
- one player is controllable
- AI players move and participate
- the ball can be passed and shot
- goals can be scored
- score updates correctly
- play restarts after goals
- the timer works
- keyboard and controller mappings exist
- the codebase is coherent
- the README matches the project

NON-GOALS FOR V1:
- real player likenesses
- full biomechanics simulation
- online multiplayer implementation
- live service systems
- ultimate-team-style modes
- career mode
- advanced replay suite
- photoreal graphics
- giant asset pipelines

Target platform:
- Windows 11 desktop
- should run comfortably on a machine with an 8 GB VRAM GPU

Most important instruction:
I want the highest-probability playable football prototype, not the most ambitious fake architecture.
Favor a smaller, coherent, working game over a broad but broken project.
If scope must be reduced, preserve:
1. playability
2. believable football feel
3. clean code
4. Windows local testing and sharing

------------------------------------------------------------------

Claude reasoning
__________________________________________________________________

The user wants a complete Godot 4.x C# football game prototype, and they've explicitly requested the full project without compression or truncation. Rather than dumping everything as text, I should build the actual project files in the container and deliver them as a structured repository with a README.Since no existing skills cover Godot C# game development, I'll proceed directly with creating a coherent, complete project structure including the project configuration, C# solution setup, and all necessary game systems.I'm planning a configurable team size (defaulting to 7v7 with support for 11v11) to balance complexity and reliability, and I'll document the limitations honestly since I can't actually run Godot to validate the code in the container.Now I'm mapping out the project structure with core gameplay systems, ball physics, player logic, and configuration resources that can be tuned via presets. I'm thinking through the scene structure—spawning 14-22 players procedurally from MatchController rather than embedding them in the .tscn file keeps things cleaner and avoids scene-file bloat. Match.tscn would just contain the pitch geometry, ball, camera, and HUD, with players instantiated from Player.tscn at runtime. For the pitch itself, I'm considering a StaticBody3D with collision for the ground, and I'm debating whether to use invisible perimeter walls to keep the ball in play or implement proper out-of-bounds logic—the wall approach is simpler for a prototype but I should document it as a limitation since it skips throw-ins and corners.Goals are Area3D zones behind the goal line between the posts, detecting when the ball center crosses. For ball physics, I'm using a RigidBody3D with custom forces to handle rolling friction on the ground, air drag, Magnus effect from spin, and restitution through PhysicsMaterial—kicks will set both linear velocity and spin. Players are CharacterBody3D capsules with their own movement model.For player control, I'm handling desired direction from input or AI with acceleration that respects turn-rate limits based on speed and agility, plus accel/decel curves and stamina-based sprinting. The action system cycles through states like Idle, Move, Kicking, Receiving, Tackling, Slide, Recovering, Jockey, and Shielding.Ball interaction works through dribbling touches when the ball is near the player's feet—periodic nudges in the movement direction scaled by control and speed, with the ball remaining a free body. First touch quality depends on the player's control attribute, incoming ball speed, and movement direction, applying an error-adjusted push. Kicks start with a windup phase that builds power.For kicks, I'm handling pass selection toward the best teammate in the input direction, through balls that lead runners, lobs with vertical lift, shots aimed at goal with stick-based aiming, and finesse shots that trade power for curve and accuracy. The AI system assigns formation-based roles—like 4-3-3 for eleven players or 2-3-1 for seven—with each AI player anchoring to a home position relative to the ball. Possession carriers decide between dribbling, passing, or shooting through simple scoring logic, while supporting players make runs into attacking space. The goalkeeper stays positioned on the goal line.The goalkeeper dives or saves when detecting a fast shot within range, using reach to attempt interception. Player switching works manually by selecting the defender closest to the ball or automatically when receiving a pass during attack. Match flow runs from menu through kickoff with reset positions and ball at center, then progresses through goals with celebration delays and kickoff resets to the conceding team, all timed by a configurable match length that leads to a result screen. The HUD displays score, timer, and power bar, while the camera follows the midpoint between ball and player position.Now I'm setting up the input mapping in the project configuration, mapping joypad buttons and triggers to their Godot 4 indices so controls respond correctly during gameplay. Writing input map config... Now I'm mapping out the keyboard fallback controls—WASD for movement, Shift for sprinting, and then assigning the action buttons to accessible keys like Space for pass/pressure and E for shoot/tackle, keeping the semantic naming consistent with the controller layout.Setting up the Godot 4 .NET project configuration...For the scenes, I'm creating a Player with a CharacterBody3D, collision capsule, mesh, and a flat torus ring as a selection indicator that toggles visibility, with team colors applied through material overrides in the script. The Ball is a RigidBody3D with a sphere collider and mesh, physics material for bounce and friction, and continuous collision detection enabled. Now laying out the Match scene as the root Node3D container. I'll generate the pitch markings procedurally using an ImmediateMesh to draw white line strips for the center line and circle, keeping everything unshaded and slightly above the ground. The goal structure—posts, crossbar, and trigger area—will also be built in code rather than the scene file, so Pitch.cs handles all the geometry and colliders, leaving Match.tscn minimal.For the .tscn files, I'm avoiding hand-written scene XML to sidestep parse errors; code-generated geometry is safer. Player.tscn and Ball.tscn stay small with just their scripts attached. On the uid and script reference front, Godot 4.3 doesn't strictly require uid attributes in ext_resource tags—path-only references work fine, and the editor will fill in uids if needed. C# script classes need to match their filenames and can use namespaces with [GlobalClass] if needed for scene attachment.For the GameplayTuning resource, I'm defining it as a [GlobalClass] Resource with [Export] fields, then creating a .tres file that references the C# script and sets the tuning parameters like player acceleration.Property names in the .tres file need to match the C# exported property names exactly — Godot serializes them using their C# names in PascalCase. I could also simplify this by using static factory presets in C# selected via enum, which avoids .tres file pitfalls, but since the user wants config files I'll stick with the .tres approach and be careful about property matching.I'll set code defaults equal to the simple preset as a safety net so the game remains playable even if a property fails to load. For export presets, I'm including the Windows Desktop configuration.On team size, the user prefers 11v11 if it's clean — my formation-based AI handles that well performance-wise with 22 CharacterBody3D nodes, so I'll default to 11v11 with a configurable TeamSize option and document both 7v7 and 11v11 tunings. For match length, I'm going with 4 minutes total, and I need to decide whether to split it into halves with a kickoff direction swap or keep it simple.Simplifying the match structure: single continuous period with kickoff at the start and after goals, final whistle at the end — no halftime complexity. I'm skipping offside and fouls entirely since tackles are always legal, and I'll document these omissions.Now I'm designing the core classes: InputActions.cs will hold constant strings, and GameplayTuning.cs as a Resource will contain all the movement and stamina parameters like max speed multipliers, acceleration, deceleration, turn rates at different speeds, sprint multiplier, stamina drain and recovery rates, fatigue penalties, and first touch error values.Writing tuning parameters... Defining player mechanics... The MatchController drives each player every physics frame—the human driver reads input and moves the controlled player relative to camera orientation, while the team AI controllers handle the other players.For input, I'm mapping button presses to actions: tap versus hold determines charge time for shooting or passing, with modifiers like LB for lobbed through balls. Defense switches context based on possession state, offering contain, tackle, slide, and player-switching options. The team AI evaluates the game phase (attacking, defending, or loose ball), positions players in formation relative to the ball with some elasticity, and makes decisions for the ball carrier like evaluating shot opportunities. ff (waiting for first touch), InPlay, GoalScored, FullTime, and Paused states. The MatchController handles score tracking, timer management, and restart logic, plus player switching—I'm going with manual switching via LB button and auto-switching when a teammate receives a pass from the controlled player to keep it intuitive. For out-of-bounds situations, tall walls contain the ball, but if it somehow escapes vertically or horizontally, it resets to the nearest valid point.The MatchCamera follows the ball with a velocity lead blended toward the controlled player's position, adjusting height and FOV based to distance while staying clamped to the pitch bounds. The HUD displays the score, timer, power bar, controlled player indicator, and goal/full-time banners with rematch and menu options. The MainMenu is minimal—just title, Play buttons for simulation and assisted modes, team size selection between 7 and 11 players, and quit. I'm using a GameSettings singleton that autoloads and stores the chosen configuration and team size, which the Match scene reads on startup. The whole project is structured across roughly 20 moderately-sized scripts.Now I'm breaking down the line counts: Player around 400 lines, TeamAI 300, MatchController 350, Ball 150, HumanDriver 200, Goalkeeper 120, Camera 80, HUD 120, Menu 80, Tuning 120, Attributes 80, Pitch 200, with miscellaneous utilities filling out the rest—aiming for around 4000 lines total while keeping implementations compact but functional. I'm also setting up the export_presets.cfg with minimal valid configuration for Windows, letting Godot fill in the defaults. Then I'll write a thorough README and add an MIT license.I'll generate all files through the container and deliver them as a complete zip, along with a repo tree and key explanations in chat rather than dumping raw text everywhere. Since I can't actually run Godot here, I'll do a static consistency pass to verify script paths in scenes match, action names align, property names in .tres files correspond to C# exports, and node paths used in code exist. I'll note in the README that this is authored for Godot 4.3 .NET and should open cleanly, though it's prototype-level polish and any parse issues will surface in the editor.Now I'm mapping out the input actions: directional movement via four separate actions tied to keyboard and joypad axes, sprint on the right trigger and Shift, action buttons for pass, shoot/tackle, lob/slide, and through ball, modifier buttons for finesse, left bumper switch, shield/jockey, pause, plus the built-in UI actions. The stick input will use GetVector across the four directional actions with joypad axes 0 and 1 mapped to left/right and up/down respectively.For the Godot 4 C# implementation, I need to account for how GetVector returns a Vector2 where positive Y corresponds to the down action, but in world space forward is negative Y, so I'll need to flip that. I'm looking at using CharacterBody3D with its Velocity property and MoveAndSlide method for player movement, while the ball will likely use RigidBody3D with LinearVelocity and either ApplyCentralImpulse or direct velocity modification in _IntegrateForces for more precise physics control.For collision detection and ball deflection, I'll set up collision layers where the world is layer 1, players are layer 2, and the ball is layer 3. Players should collide with everything, but I need to distinguish between intentional dribble nudges applied through code versus physical deflections from body contact—the ball will detect when it hits a player's capsule and apply an impulse based on the player's movement direction, while dribble inputs are handled separately through code-based velocity adjustments.To avoid the ball bouncing off the player's own capsule during dribbling, I'll keep the ball positioned slightly ahead (around 0.5m) and use a smaller collision volume for ball-to-player interactions. For possession tracking and visual feedback, the MatchController will monitor which player has the ball, and I'll use a ring color indicator to show the currently controlled player.For the kick mechanic, I'm implementing a charge system where holding builds power from 0 to 1, then releasing executes the kick if the ball is within range (about 1.3m and roughly in front). If the ball isn't in range when released, there's a small whiff animation, but I'll add a 0.25-second buffer so if the ball enters range during that window, the kick still executes for better feel. Pass selection will pick the teammate that minimizes the angle relative to input direction (or facing direction if no input), weighted by distance, with through balls leading the runner. Shots aim at the goal mouth with an offset based on input direction.For finesse shots, I'm reducing power while increasing accuracy and adding side spin for curve. The goalkeeper catches the ball when it's within reach and moving below a speed threshold or if the dive succeeds, then freezes the ball at the GK's hands using a state timer before distributing it via pass or punt. Player-to-player collisions trigger stumbles when the relative speed exceeds a threshold, with the weaker player getting knocked back based on momentum and strength, capped to prevent constant stumbling.Now I'm setting up match parameters: default 5-minute length with halftime skip. For 11v11, I'm positioning players in a 4-3-3 formation using normalized coordinates relative to field dimensions—goalkeeper deep, defenders across the back line, midfielders in the center, forwards pushing up. Each player has a home position that shifts dynamically based on ball location with elasticity factors to create natural movement toward the play. For 7v7, I'm using a simpler structure with goalkeeper, three defenders, two midfielders, and one forward.I'm thinking through the scene files I need to create: Ball, Player, Match, and Menu as minimal Godot scenes. For Player.tscn specifically, I'm defining a capsule-based character with collision shape, mesh geometry, and material properties...Finishing the Player scene structure with collision and mesh nodes positioned appropriately, adding a small nose indicator to show facing direction, and including a selection ring that toggles visibility.Now I'm setting up the Ball scene as a physics object with a RigidBody3D, configuring its mass, bounce, and friction properties, and giving it a sphere mesh and white material. Then the Match scene brings everything together with a root controller script, environmental lighting, a camera, and a HUD canvas layer with UI elements like labels and progress bars for the game interface.I'm going to build the HUD and menu UIs directly in code rather than laying them out in scene files to avoid complexity and potential bugs. The Match scene becomes minimal—just the root controller, camera, light, environment, HUD canvas layer, and instances for the ball and pitch geometry. The Menu scene similarly has a Control root with the menu UI built programmatically. I'll set the project to start with Menu.tscn, configure physics to tick at 60 Hz, and use an autoload for GameSettings to manage global configuration.Now I need to write out all the code, keeping in mind the C# Godot API conventions like Input action checks, scene transitions, physics configuration, vector math, signals, and UI components.The pitch dimensions are 105 by 68 meters with goals positioned at x=±52.5, width 7.32, height 2.44. Team 0 attacks toward positive X, team 1 toward negative X. Players have a top sprint speed of 8.6 m/s, run speed 6.5, jog 4, with acceleration around 8 m/s² and turn rates that vary from 720 degrees per second when stationary down to 140 at sprint speed. Ball kick mechanics include ground passes at 12-18 m/s, driven shots at 20, lobs at 14 with vertical velocity 7, through balls at 16, and shots at 22 m/s.Rolling friction decelerates the ball at roughly 4.5 m/s² on the simulated surface, allowing a 15 m/s pass to travel about 25 meters. I'm implementing Magnus force using a spin vector in radians per second, where the acceleration is calculated as the Magnus factor times the cross product of spin and velocity—finesse shots apply a spin of ±8 around the y-axis. Control radius is 1.6 meters with a kick range of 1.4 meters, and first touch mechanics factor in incoming speed to determine touch error angle and resulting velocity direction based on player movement.For dribbling, when the player and ball are within 1.2 meters and moving slowly, I apply velocity to the ball every 0.45 seconds while jogging or 0.6 seconds while sprinting, scaled by control skill—if the ball gets within 0.45 meters, a soft constraint gently matches it to the player without creating magnetic behavior. Stamina drains at 6 per second during sprints and recovers at 4 per second below jogging speed, with fatigue reducing acceleration and top speed to 85% while increasing touch error. Action timings include a 0.12-second windup plus 0.15-second lock for passes, 0.25 seconds for shots with optional charging and 0.35-second recovery, and standing tackles with 0.2 seconds to commit, 0.2 seconds active, and 0.5 seconds to recover.For slide tackles, there's a 0.5-second active window and 1.2-second recovery, with an 80% miss chance. The AI shoots when within 22 meters and the angle is favorable, passes when pressured or a better option exists, otherwise dribbles forward. The MatchController runs the human driver for the controlled player, and on defense the "contain" action auto-moves toward the ball carrier while blending with manual input. Player switching happens manually via LB, automatically when receiving a pass, and once when the opposing team gains possession to select the nearest defender. The pause menu toggles the game state with a simple overlay, and the HUD uses a CanvasLayer set to always process so it remains visible. When full-time is reached, play stops.After a goal, there's a 2-second banner, then teams reposition for kickoff to the team that conceded. At kickoff, the ball starts center with teams in their own halves at formation, and the nearest AI player walks to the ball and passes backward to start play. The goalkeeper holds the ball for 1.5 seconds before passing to the nearest open defender, using a ground pass if close or a lofted pass upfield otherwise. For out-of-bounds safety, if the ball goes beyond the field boundaries, it's repositioned appropriately.For walls, I'm setting up invisible collision boundaries just beyond the field lines to keep the ball in play—placing them 2 meters behind the goal line to avoid weird pinball effects at the goal. I'm using separate collision layers so walls only interact with the ball, not players, allowing players to step slightly over the touchline without getting stuck.Now I'm working through the collision masks: the ball needs to collide with the world, players, and walls; players should collide with the world and other players but not the ball itself, since that would stop them mid-movement. The key insight is that in Godot, a RigidBody will bounce off a CharacterBody if the rigid body's mask includes the character's layer, regardless of whether the character's mask includes the rigid body—so I can make the ball collide with players while keeping players unaffected by the ball.For the layer setup: ground and posts go on layer 1, walls on layer 8 (only the ball needs to see walls), players on layer 2, and the ball on layer 4. The ball's mask includes 1, 2, and 8; players' mask includes only 1 and 2. This way the ball bounces off players and the environment while players walk through the ball's space, and we handle separation via code distance checks. When players collide with each other, we'll add stumble logic to make contact feel physical.For goalkeeper mechanics, I'm handling catching and parrying based on ball speed and proximity—if the ball's within reach and the keeper is diving or set, they catch it if it's slow enough, otherwise deflect it to the side with dampening. The dive itself is a quick lunge toward the intercept point at 9 m/s for 0.4 seconds, then a 1-second recovery. Shot detection watches for the ball's velocity crossing the goal plane within 5 meters of center in under 1.2 seconds.Building the Pitch.cs file now...Setting up the ground with a green mesh for the playing surface and a darker box beneath it. I'm using an immediate mesh to draw the field lines in white—the outline, halfway line, center circle with 48 segments, penalty boxes, 6-yard boxes, and penalty spots. The goals themselves are cylinders for the posts at the corners with a crossbar at regulation height, plus colliders and a trigger area for detecting when the ball enters the goal.I'm adding walls around the pitch perimeter and considering a ceiling to prevent the ball from escaping upward. For goal detection, I'll rely on the trigger area as the primary method with a fallback positional check in the match controller. The MatchController will reference the ball, camera, HUD, and pitch through node paths—either using exported NodePaths configured in the scene file or fixed child names retrieved at runtime.I'm loading the player scene dynamically from code rather than exporting it in the editor, and I'll set up a GameSettings autoload to store preset selection, team size, and match duration that the menu can write to and the match can read from. The HUD power bar will display the human player's charge state pulled from the driver, and I'll assign jersey numbers to identify players.The HUD will show "You: #7" for the player and display goal banners like "GOAL! Blue 1 - 0 Red" with team colors. I'm going to skip sound generation for now since there are no audio assets, but I'll document that as a future enhancement.Now I'm planning the file structure systematically — starting with directories, then project configuration, C# project files, all the scripts, scene files, data resources, and finally export settings and documentation. Let me start drafting the input actions class...Writing input actions...Next I'll create a GameplayTuning resource class that holds about forty exported tuning parameters plus a name field, with static factory methods to generate preset configurations for simulation and assisted modes.For PlayerAttributes, I'm deciding between making it a Resource with .tres templates or keeping it as a plain code class with role-based factories and randomization. The requirement asks for a data-driven system, so I could ship a few .tres attribute templates, but that adds complexity with resource management. I'll compromise by defining the attribute templates in code using a dictionary keyed by role, which gives me the data-driven feel without the overhead of managing multiple .tres files.Actually, a cleaner approach: I'll load role templates from a single JSON file at runtime—attributes.json with role definitions like GK, DEF, MID, FWD, each with stat ranges from 0-100. This keeps the data visible and editable while staying simple, and I can use System.Text.Json to parse it with sensible defaults as a fallback.Now moving to Ball.cs, which handles the physics object for the ball itself—tracking spin, which player last touched it, and ground state detection.Writing Ball physics code... Finishing the Hold method to zero out spin, then adding a SafetyClamp function that keeps the ball within bounds and resets it if it falls too far below the ground. Now I'm setting up a static Game class to hold references to tuning, match controller, and other global settings—using it as a simple service locator rather than scattering statics everywhere. GameSettings will autoload on startup and populate the Game class with the selected tuning preset.Now moving into the Player class, which is the big one. I'm defining enums for player roles (goalkeeper, defender, midfielder, forward) and states (normal movement, wind-up, kick recovery, tackle, slide, receive, stumble, get up, goalkeeper dive, goalkeeper hold). The Player class itself will track team index, role, number, attributes, home position, and whether it's human-controlled, plus command inputs each frame like movement direction, sprint, shield, and jockey actions.Defining Player class structure... Adding speed and ball interaction logic... RequestKick takes a type, power, and aim direction, transitioning to WindUp state and checking if the ball is in range when the windup completes—if so, ExecuteKick fires, otherwise it's a whiff with recovery. I'm reconsidering the API design so drivers pass the desired ball velocity and spin directly rather than computing it inside ExecuteKick, making the kick request cleaner and more flexible.For tackles, StartTackle commits with a small lunge toward the ball and pokes it away if contact happens during the active window, with success reduced by shields and missed tackles causing long recovery—strong contact with the carrier can trigger a stumble. Slide works similarly but with greater range and longer recovery. Stamina drains during sprinting and affects top speed, acceleration, and touch accuracy through a fatigue factor. Contact resolution in the movement loop applies momentum transfer when colliding with other players above a speed threshold, factoring in strength.For kicks, I check if the facing direction aligns with the ball within a generous angle. The HumanDriver node reads input each frame to handle movement, sprinting, and shield commands—shielding locks facing between the ball and nearest opponent while reducing speed. Charging works by tracking which action button is held, initiating charge on press and computing the kick on release.All four kick types support charging with different max durations: pass caps at 0.7 seconds, lob at 0.9, through at 0.7 (or instant tap with minimal charge), and shot at 1.1 seconds. The aim direction comes from the move input if it's strong enough, otherwise I use the current facing. When finding a pass target, I query for available teammates in the aimed direction, then use KickMath helpers to compute the ball velocity—ground passes blend toward an ideal speed that accounts for friction and distance, with aim assist optionally steering the direction toward the target. Lobs follow a ballistic arc instead.For through balls, I calculate the target point ahead of the receiver based on their velocity and a secondary lead factor toward goal, then pass to that point with extra pace. Shots aim at a goal target point with a vertical offset from input, using a speed range of 16-30 based on power and shooting stat, and finesse shots reduce speed slightly while improving accuracy and adding side spin to curve toward the far post. Error is applied as angle noise scaled by skill, power, and fatigue.When the team loses possession, defensive controls take over: holding A triggers contain mode to steer toward the predicted ball carrier, B initiates a tackle, X performs a slide, LB switches the controlled player, RB calls a second presser, and LT activates jockey mode for slow, strafe-based defense. In attack, LB modifies combo inputs like lofted through balls and pass-and-run actions.For the pass-and-run mechanic, I'm simplifying it so after an LB+A pass, the passer gets a brief forward-run directive for 1.5 seconds via a timer. The pause system is handled through the HUD and Match controller.On the AI side, each team runs a drive loop that determines the possession phase, selects pressers based on proximity to the ball or carrier, identifies chasers for loose balls, and gives the AI-controlled carrier a decision timer to evaluate shooting, passing, or dribbling options while other players move toward their formation positions.For player movement, I'm setting up support positioning within 18 meters of the carrier during attacks—finding open spots while keeping defenders goal-side—then issuing move commands with arrival slowdown and sprint logic based on distance or urgency. Passes and shots go through the same kick system, and the goalkeeper gets its own positioning logic along the goal line.The possession model tracks who owns the ball by checking if someone touched it within the last 0.7 seconds, is within 1.5 meters, and the ball is moving slowly enough, making that player the carrier and determining which team has possession.For pass selection, I'm scoring potential targets based on how well they align with the intended direction, penalizing distance and opponents blocking the passing lane, then either passing to the best option or into open space if none qualify. Player switching picks the nearest teammate to the ball with a bias toward goal-side positioning, and when pressing, the nearest available teammate gets assigned to harass the carrier. The match controller cycles through phases like kickoff, active play, goal celebrations, and full-time using an enum system.During active play, timers tick down and goals trigger callbacks that transition to a celebration phase before resetting for a kickoff. For kickoffs, the ball sits at center and the kicking team's striker approaches—once in range, play unfreezes and they can pass. The defending team stays restricted to their half until the first touch, keeping things organized during the restart.When the match timer expires, the whistle blows immediately regardless of ball position, and a full-time panel appears. The HUD displays the score at the top, a power meter in the bottom-left while charging, and large center labels for goal and match-end announcements, with pause and full-time menus handling navigation. The camera blends between the ball's predicted position and the controlled player, smoothly tracking from a fixed offset that looks down at the action.For the broadcast-style camera, I'm positioning it side-on with the camera at a fixed Z depth around +26, X following the ball's X position clamped to the field bounds, and Y at a base height that adjusts slightly with ball velocity. The camera looks toward the ball with a 35-degree FOV, and I'm using exponential smoothing to blend position changes naturally. Input handling needs to account for the camera orientation—since it's looking roughly down the -Z axis from the positive side, screen directions map to world space accordingly. Setting up the scene geometry in Pitch.cs: drawing grid lines with unshaded white material at ground level, creating a ground plane with a box collider and mesh (top surface at y=0), adding darker grass around the edges, and positioning cylindrical posts as static obstacles with collision shapes.Now I'm defining the goal areas as trigger zones that detect when the ball enters and signals a score, making sure the ball's layer is included in the detection mask. I'm also considering adding a translucent box behind each goal for visual clarity, and implementing logic so that once a goal is scored, the ball freezes in place until the match resets.For the trigger placement, I need to position it just beyond the goal line so the ball fully crosses before registering, accounting for the ball's radius. The goalkeeper starts at a fixed position near their own goal line, and I'm setting up the team formation with arrays that define each player's fractional position across the field—goalkeeper at the back, defenders spread across the width, midfielders in the center, and forwards pushing toward the opponent's goal. Defining formation variations and dynamic positioning logic... Dribble scoring factors in available space ahead. For execution, the AI either requests a kick with computed velocity, passes the ball to a teammate, or dribbles by moving toward goal while favoring open space and sprinting when safe. Pressure is measured as the nearest opponent within 3 units. The kick system stores pending parameters at request time to avoid recomputation, and the goalkeeper logic handles claiming loose balls in the box or computing intercept points for incoming shots.For shot defense, the keeper either dives toward the intercept point if needed speed exceeds a threshold, or catches the ball if it comes within reach—holding it if the incoming speed is manageable, otherwise parrying it to the side with dampening. Positioning uses a normalized vector from goal center scaled by distance factors, clamped to keep the keeper near the goal line. After holding the ball for a set duration, the keeper distributes it by passing to an open defender. Tackle success is probabilistic, with the base chance reduced by shield bonuses when the ball carrier is shielded and facing away, plus adjustments for balance differences.Now I'm ready to start writing the implementation. I need to be careful with the C# 12 and Godot 4.3 API details—particularly how Input vectors work with positive Y pointing downward, how CharacterBody3D handles velocity and collision detection, and how Area3D signals are structured in C#. I'll define the environment and lighting setup in the scene file with a blue sky background, ambient lighting at 60% intensity, and a directional light with adjusted rotation. Setting up the HUD controls: I'll set the canvas layer to always process even when paused, handle pause/unpause and scene transitions through the tree, wire up button signals to their handlers, customize label fonts and progress bar ranges through code overrides, and position UI elements using anchor presets.For physics, I'm configuring RigidBody3D with continuous collision detection enabled, applying physics material properties like bounce and friction through the scene resource, and managing the freeze state for kinematic bodies. I'm also using the random number generators available in GD and Mathf for gameplay logic, plus setting up an autoload script in the project configuration.I'm restructuring the autoload to use an App class that holds game settings like preset, team size, and match duration, with a static instance accessor and a shorthand App.T for accessing the tuning configuration. For serialization, C# exported properties will use their declared PascalCase names directly in the scene files rather than converting to snake_case. Now I'm laying out the project directory structure with the csproj file and standard documentation files.Organizing the folder hierarchy with data files for tuning presets and attributes, scene files for the main UI and gameplay, and scripts split into core systems, AI logic, and player-related classes.Setting up the C# project structure with a root namespace and establishing the App singleton that provides access to gameplay tuning parameters, then starting to write the App class with a static instance and tuning accessor.Continuing with App.cs...Now I'm setting up the GameplayTuning class to hold all the gameplay parameters with default values for simulation mode, and the MakeAssisted factory method will override those defaults for assisted mode. I'm defining movement-related fields like walk, jog, run, and sprint speeds that scale based on attributes, along with acceleration, deceleration, turning rates that vary by speed, and sprint penalties. There's also shield and jockey movement speeds. For stamina, I'm starting to lay out those parameters but the list got cut off.Continuing with the remaining tuning parameters: stamina drain during sprints, recovery rates, and fatigue factors; ball physics like friction, air drag, and Magnus effect for curved shots; touch mechanics including control radius, dribble distance and timing, speed thresholds, and error margins that increase with speed; and kick parameters starting with minimum and maximum pass speeds.Now looking at the rest of the kick settings—through ball bonuses, lob trajectories with time windows, shot speeds and lift limits, finesse modifiers for power and spin, accuracy bonuses, and error ranges for passes and shots that vary between simulation and gameplay modes, plus aim assist blending toward the target direction.Continuing with the timing windows for various actions like pass windup and recovery, lob setup, shot mechanics, through ball timing, tackle phases, slide duration, receive lock timing, and charge times for passes versus shots, along with kick range and stumble thresholds.Then there's the AI configuration reusing pass error values and setting pressure distance, plus assisted mode overrides that tighten error margins significantly and boost aim assist while adjusting ball friction.Now I'm defining the PlayerAttributes class to hold the core stats like speed, acceleration, agility, balance, strength, ball control, passing, shooting, defensive reach, and stamina all normalized to a 0-1 range, loading role templates from a data file and generating attributes based on selected roles.I'm setting up the attributes data file with position-specific ranges for each stat—goalkeepers get high defensive reach and stamina, defenders get strength and balance, midfielders get balanced passing and control, and forwards get shooting and speed—then parsing it safely using Godot's built-in JSON parser to avoid export trimming issues.Now I'm building the KickMath utility with static methods for calculating ball physics: a flattening function for 2D projections, a ground pass velocity calculator that determines the ideal speed to reach a target with friction applied and blends between min/max speeds based on power and assist levels, and a lob velocity function that factors in distance and time constraints. Now I'm calculating the time to reach the goal and adjusting vertical velocity so the ball arrives at the target height, which varies based on power—higher power aims for around 1.6 meters while lower power targets 0.4 meters. I'm also adding directional error by rotating the shot direction around the Y-axis using a Gaussian distribution, clamped to stay within reasonable bounds. With gravity set to the default 3D value of 9.8, I'm ready to start writing the main Player class.