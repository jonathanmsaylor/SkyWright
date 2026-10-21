# M1 — First Home
Date: 2026-10-18
Status: **Historical M1 verification record. Jonathan subsequently reported that M1 functions as intended.**
Current controls and M1.5 preview instructions are in MAINTENANCE.md; Q/E and wheel-reach references below describe the historical M1 executable.
Milestone 2 has not been started.

## Play
Use the **Play Skywright** shortcut on the MashPC desktop.
Choose **Begin building** for an empty island, or **Explore demonstration** for the input-built example.
The personal home.json save slot was verified absent at handoff; verification used a separate slot.

Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
Unity: 6000.4.7f1 / URP 17.4.0
Scene: Assets/Scenes/FirstHome.unity
Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Windows\Skywright.exe
Desktop shortcut: C:\Users\jsayl\Desktop\Play Skywright.lnk

## Implemented
- First-person mouse look, walk, sprint, jump, adjustable sensitivity, cursor/menu ownership.
- Safe fall recovery and menu recovery without losing construction.
- One fixed, handcrafted island, distant scenery, sky, clouds, stylized trees, grass, warm lighting and shadows.
- 13 compatible pieces with illustrated selection cards: deck, wall, window, doorway, roof, gable, stairs, railing, post, beam, planter, lantern and bench.
- Snap previews, free placement, 45-degree rotation, adjustable reach, cycling nearby connections, individual removal and explanatory rejection messages.
- Permissive dock extension, unlimited materials, no chain destruction.
- Local saves with stable IDs, validated staging, safe replacement and backups.
- Standalone release build, bundled demonstration and local Git repository.
- Original geometry, generated wood texture, geometry-derived icons and synthesized placement sound. No imported third-party artwork or paid assets.

## Checkpoint A — passed
Built the initial house using OS mouse/keyboard input in the standalone game:
one foundation, doorway, two window walls, rear wall, two roof slopes and two gables.
Walked through the doorway and across the constructed floor.
Extended adjacent decks beyond the island and walked onto the projecting dock.

No editor/script command created the demonstration pieces. Their PLACE events came from the game's normal construction input.

## Checkpoint B — passed
The completed demonstration contains **29 pieces with 29 unique IDs**, covering all **13 types**:
8 decks, 2 windows, 1 wall, 2 roofs, 2 gables, 1 bench, 1 lantern, 1 planter, 6 railings, 1 stairway, 1 doorway, 2 posts and 1 beam.

| Check | Observed result |
|---|---|
| Home and roof closure | Constructed and entered the home; both roof slopes and gable ends present |
| Stairs and elevated deck | Climbed from approximately Y=0.235 to Y=3.232, then descended through movement input |
| Dock traversal | Walked to approximately Z=-24.29 beyond the island surface |
| Rotation and free placement | Rotated the bench through Q/E; placed the stairway using held Alt |
| Reach | Mouse wheel changed reach from 8.0 to 8.5 metres |
| Competing snap connections | Final build Tab cycle changed target from Z=-16 to Z=-12; identical candidates are deduplicated |
| Player obstruction | Attempt to place a wall through the player rejected |
| Piece overlap | Overlapping free-placed floor rejected with a reason |
| Duplicate placement | Occupied connection rejected without increasing piece count |
| Removal and replacement | Removed/replaced deck, doorway, gable and roof; adjacent construction remained |
| Support fit | Placed two posts and a crossbeam beneath the raised deck |
| Menus | Palette selection, pause, controls and recovery exercised without accidental placement |
| Sensitivity and movement | Sensitivity slider adjusted and restored near 0.1; sprint and jump exercised |
| Fall recovery | Walked off the island; recovered to spawn with piece count unchanged |
| Save/restart | Closed and relaunched multiple standalone sessions; construction reconstructed from disk |
| Modification persistence | Replacements persisted, and deleted object IDs remained absent |
| Corrupt save | Replaced only the backed-up verification slot with {}; Load displayed an error and kept all 26 then-current pieces; restored the valid file |
| Demo packaging | Final release loaded the bundled 29-piece demonstration |
| Final restart | Final release saved 29 pieces, exited, reopened and loaded 29 pieces with saved player position/view |

Verification used actual OS inputs plus screenshots, application logs, and read-only diagnostic state. The game never consumes commands from the diagnostic file.

## Fixes found during verification
- Retained explicit Lit/sky material assets so standalone shader stripping could not remove required shaders.
- Excluded the player's collider from construction targeting.
- Tightened free-placement overlap checks and added gable collision checks.
- Preserved intended stair/deck joints.
- Corrected post height to account for deck thickness.
- Removed duplicate snap candidates.
- Replaced the first cloud/tree placeholders with more coherent silhouettes.
- Improved UI button styling and kept menu notifications clear of the title.

## Build and evidence
Final build: Evidence/build-final.log — SKYWRIGHT_BUILD Succeeded errors=0.
Final runtime: Evidence/play-final.log and Evidence/play-final-restart.log.
No managed exceptions or gameplay errors were found in those final runtime logs.
Unity logs a D3D12 startup diagnostic: failed to query info queue interface (0x80004002). Rendering and input tests completed successfully; no graphics-driver changes were made.
The deliberate damaged-save warning is in the earlier play-e.log.

Additional evidence:
- Evidence/play-b.log — initial input construction and stair/dock traversal.
- Evidence/play-d.log — overlap rejection/removal.
- Evidence/play-e.log — recovery, replacements and damaged-save protection.
- Evidence/play-f.log — posts/crossbeam and 29-piece save.
- Evidence/save-validation.json — counts, unique IDs, all types and removed-ID checks.
- Assets/StreamingAssets/demonstration.json — the input-built demonstration.

![Final home and raised platform](Evidence/first-home.png)
![Illustrated construction kit](Evidence/building-kit.png)
![Walkable dock over the clouds](Evidence/dock-view.png)

## Playtest route
1. Explore the demonstration briefly: enter the house, inspect the roof, climb the stairs and walk to the dock.
2. Start a fresh island.
3. Press B, select a deck and build your own small home.
4. Try Q/E rotation, Alt free placement, Tab snap cycling and removal.
5. Save with F5, quit, reopen and Continue saved home.
6. Assess whether selecting, aiming, snapping and correcting mistakes feel comfortable, and whether you want to keep building.

Movement: WASD / mouse / Shift / Space.
Construction: B kit; F equip; LMB place; RMB remove; Q/E rotate; Alt free; wheel reach; Tab next snap.
Escape: menu, controls/sensitivity, save/load and recovery.

## Limits and pending decisions
- This is a compact construction prototype with simple original art, not final production presentation.
- Stairs rise three metres. Upward construction currently starts with free-placing the stair base, then snapping a deck to its top; this deserves particular UX feedback.
- Doorway is an open frame, with no swinging door.
- Saving is explicit; there is no autosave on quit.
- Tested on MashPC with keyboard/mouse and a compact 29-piece build. Large settlements, other hardware and every possible piece combination have not been stress-tested.
- No ships, flight, resource economy, procedural worlds, survival, NPCs or multiplayer were added.
- Whether construction is enjoyable remains Jonathan's decision. Stop here for that playtest.
