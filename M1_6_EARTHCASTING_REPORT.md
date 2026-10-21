# Skywright M1.6 — Earthcasting harvesting experiment
Date: 2026-10-18
Status: Implemented; verification and limitations below. Stop for Jonathan's playtest. No air abilities, airships, M2, survival meters or remaining M1.5 kit conversion.

## Launch and short playtest
Double-click **Skywright Earthcasting Preview** on the MashPC desktop. Choose **Begin harvesting trial**.
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Scene: Assets/Scenes/FirstHome.unity (the existing runtime bootstrap)
- Executable: Builds/Earthcasting_Preview/Skywright.exe
- Shortcut: C:\Users\jsayl\Desktop\Skywright Earthcasting Preview.lnk
- Normal save: %USERPROFILE%\AppData\LocalLow\Skywright\Skywright\earthcasting-trial.json
The sibling earthcasting.preview marker also makes direct executable launches use the isolated trial. The shortcut passes -earthcasting explicitly. Existing Windows and M1_5_Preview builds and shortcuts are preserved.

1. Walk toward one of the small loose-stone piles, aim at it and press E. Each supplies 18 charges.
2. Press G. Aim at an ash trunk within 8 m and left-click. Watch the ground lift, suspension, launch, impact and exposed fracture.
3. Strike near the same fracture to fell it in two hits, or different areas for three ordinary hits.
4. Stay back: its wood waits near the stump. Approach within 3.5 m to draw the bundle toward you and receive 12 wood.
5. B opens the familiar building kit. A deck costs 6 wood in this trial; other pieces are explicitly free. Extend the small dock behind the starting position.
6. Try removal/refund, a limestone outcrop (two charges for 12 stone), and F5 save. Quit, reopen and Continue harvesting trial.
7. Judge repeated casts, sound, fall timing, reward collection and whether harvesting makes building more enjoyable.

Controls: WASD/mouse/Shift/Space as before; G Earthcasting, E loose-stone pickup, LMB cast when equipped. F or building selection returns to construction. Wheel rotates 45 degrees; Shift+wheel adjusts reach; Alt free placement; Tab named snap points; Shift+MMB selects a built piece and returns to construction. RMB removes only in construction. Menus block gameplay input. Save explicitly with F5; there is no autosave.

## Implemented
- One authored ash-tree prefab, ten rotated placements, deliberate trunk colliders, local fracture memory, visible bark scars, damage reaction, controlled directional falls, stump and wood yield.
- A first-person pearl/timber/aqua casting focus; eased emergence, brief suspension, accelerating stone, dirt shedding, short trail, impact fragments/ripple, tree response and recovery. Default cast phases: .42/.16/.22/.28 seconds (1.08 total); tree fall 1.65 seconds.
- Seven original synthesized audio layers for preparation, lift, release, impact, crack, fall and collection, with pitch variation and bounded voices.
- Three loose-stone piles, two net-positive outcrops, a starter dock and two static cargo crates for future air tests.
- A 3.5 m collection radius plus a clear-path check. Waiting yield survives loading. Once collection starts, its short animation finishes even if the player backs away.
- A minimal stone/wood HUD, target/rejection feedback, resource cost display, exact per-instance wood refunds and a trial-only reset.
- Authored Unity prefab/material/mesh/audio assets and an EarthConfig tuning asset. Ordinary builds retain authored assets; explicit regeneration is a separate editor command.

No terrain deformation, full-tree rigidbody simulation, log hauling, mana, hunger, rest, durability, combat, paid assets or services were introduced. Existing creative construction remains unlimited.

## Future air direction retained
PROJECT_BRIEF.md records the approved ideas: controlled building hover, local wind anchors, short lifts/slow descent away from anchors, local updrafts, optional cargo movement, aimed gusts, and later sails/docking/wind-powered equipment. Preserve an airship's long-distance travel role. Use the ground crates for the first levitation test. Do not levitate trees or replace automatic wood collection with mandatory hauling. Air mechanics remain a separate instruction.

## Persistence and preservation
Existing piece IDs, type IDs, local transforms, catalog anchors and collision geometry remain unchanged. The build checks all nine authored kit collider contracts against M1.
Schema v1 adds an explicit harvestingTrial marker and optional per-instance paidWood. An absent/false marker identifies legacy/creative data. This is necessary because JsonUtility materializes default objects for missing/null inline class fields. The initial implementation missed that behavior; the unchanged M1 compatibility test caught it, and it was fixed before handoff.
Trial state stores explicit layout IDs, local fracture position, damage/depletion/collection, inventory and payments. Saving settles a committed cast/fall/started collection once; untouched waiting loot remains waiting. Invalid data is rejected before replacing the active build or valid save file.
Tests used verification-earthcasting.json and backed-up verification-snapping.json, never the personal slots. home.json, art-preview.json (119 pieces), original verification.json and verification-m15.json remain byte-for-byte unchanged; hashes are in Evidence/Earthcasting/preservation.json. The user's art-preview SHA256 is still 92B4CA11CC13BC3402302909B426C0DEAC0F957E444F7E016C810C6D3A1BDD21.
During development only the new QA trial fixture was marked with the explicit trial discriminator. No personal or original M1 save was migrated or overwritten.

## Verification performed
Player-flow tests used OS keyboard/mouse input in the standalone Windows player. qa-state.json and log output are read-only observation; they do not issue gameplay commands. Initial resource layouts are authored test content. Harvesting, paid construction and removal were performed through input.

| Check | Observed result |
| --- | --- |
| Final compilation/build | build-final.log: EARTH_SAVE_CONTRACTS passed; SKYWRIGHT_EARTH_BUILD Succeeded errors=0 |
| Stone pickup and charges | E pickup supplied 18; each committed cast consumed exactly one; zero-ammo rejection consumed none |
| Range and obstruction | Confirmed out-of-range and deck-covered launch-ground rejections retained 15 charges; invalid targets rejected |
| Damage timing | Captured emergence/release frames show tree intact before impact; damage logs occur at impact, except explicit save settlement |
| Precision and ordinary hits | Two near-fracture hits felled one tree; three separated-height hits felled another |
| Directional fall | Final capture shows the controlled fall; logged direction (-.72,0,.69). Clearance check was narrowed after an overly conservative first sample collapsed in place |
| Radius | No wood at 3.87 m; collection began at 3.47 m and awarded 12 once |
| Waiting loot reload | Full process restart retained depleted-but-uncollected tree and zero awarded wood |
| Stone replenishment | Two casts spent 2 stone; collection returned 12 (net +10) |
| Paid construction | Two dock extensions cost 6 wood each through normal placement |
| Removal | Paid deck refunded 6; free starter deck refunded 0; paid refund also survived a process restart |
| Insufficient wood | Fresh trial rejected an otherwise valid deck with zero wood |
| Input ownership | Escape menu/palette ignored wheel, G, E, Tab, middle-click and outside clicks; RMB did not remove during Earthcasting |
| Construction controls | Shift+MMB returned from spell to a looked-at deck; wheel +45/reverse 0, Shift+wheel reach 8 to 8.5, Tab Edge 1, Alt free placement observed |
| Transient saves | Timed F5 during cast, collection and fall produced consistent state; subsequent full-process reloads retained spent charge, damage, collected status or waiting loot |
| Final release format | release-before.json and release-after.json have identical resources, node state, payments and pieces across process restart |
| Damaged save | Invalid version was rejected with all 5 current pieces and resource state retained |
| Legacy compatibility | Unchanged 29-piece M1 copy loaded; all IDs/types/parents/transforms match exactly, legacy refunds zero. Doorway traversal passed. Re-saved creative format loaded after another process restart |
| Build contracts | All nine converted kit collision contracts match M1; snap/root dimensions were not edited |

The automated serializer checks additionally verify creative/trial round trips, paid amounts, negative-resource and duplicate-node rejection, and preservation of the last valid file after failed writes. These supplement player tests.
Earlier failed/invalid attempts remain in the evidence: the first baseline launch stayed in the menu; a range test initially aimed at an in-range tree; one helper's console encoding failed after moving the mouse; initial legacy load failed before the discriminator fix. They are not counted as successful checks. The legacy stair-route attempts stopped at existing fixture geometry before reaching the stairs; full stair traversal was not reverified in M1.6. Prior M1.5 evidence and unchanged collision contracts remain available.

## Responsiveness and runtime checks
Same PC, 1600x900, 120 FPS cap, initial spawn (0,.035,-10), yaw 0/pitch 12; ten-second stationary samples:
- Prior art preview, fresh clearing: 1,200 frames, 8.33 ms mean, 8.62 ms worst.
- Earthcasting preview, fresh trial: 1,200 frames, 8.33 ms mean, 8.98 ms worst.
The trial includes ten trees/resources and three starter pieces; the old fresh scene has zero built pieces. Both maintained the cap in this small test. This is not an uncapped GPU benchmark or a large-settlement performance claim. Source logs: baseline-final.log and play-b.log. The final serializer-only correction does not alter rendering.
Final release trial/legacy logs contain no managed exceptions or shader errors. Existing D3D12 startup diagnostics and intentional damaged-save warnings remain distinguishable from gameplay failures.

## Evidence and review media
All paths below are relative to Evidence/Earthcasting:
- trial-overview.png; spell-lift.png; spell-impact.png; tree-fall.png; waiting-wood.png; future-air-crates.png.
- earthcasting-gameplay.gif: silent 800x450 capture of actual keyboard/mouse gameplay, approximately 3.8 seconds. Original full-resolution frames are in cast-frames/.
- input-evidence.jsonl; persistence-results.json; legacy-results.json; preservation.json.
- final-before/after.json; release-before/after.json; waiting-loot-before/after.json; midcast-before/after.json; collection-save-before.json.
- build-final.log; play-a.log; play-reload.log; play-final.log; release-trial.log; release-reload.log; legacy-final.log; legacy-reload.log.
Evidence contains private preservation copies and is ignored by Git.

## Asset provenance and remaining limits
All new geometry, textures, materials, sound waveforms, layout and UI were originally created for this project. No third-party art/audio packs, purchases, package dependencies or online runtime services were added. Existing Unity packages retain their existing licenses.
This is a focused stylized prototype. It uses one tree mesh with rotated placement, a compact focus rather than rigged hands, cosmetic fragments and controlled whole-tree motion. If every fall corridor is obstructed, the tree collapses compactly rather than damaging a build; there is no physical log simulation. Leaves/canopies may overlap scenery. Resource reset is trial-wide, not a regrowth system.
Sound files and playback integration are present, but live audio mix/audibility was not assessed through this remote connection. Jonathan should judge loudness, repetition and satisfaction during the playtest. The GIF is silent.
No exhaustive test of every building arrangement, rapidly changing projectile obstruction, every resource-node combination, other GPUs/resolutions or long sessions was performed. Visual and gameplay polish remain subject to Jonathan's judgment.

## Stopping point
M1.6 is ready for the harvesting playtest. Do not begin air abilities, M1.5 Checkpoint B or M2 automatically. The next action is Jonathan's feedback on spell feel, collection radius, material use and presentation.
