# Skywright M1.5 â€” Checkpoint A
Date: 2026-10-18
Scope: representative art sample and replaceable kit workflow.
Status: Checkpoint A implemented and technically verified; awaiting Jonathan's visual review.
**Checkpoint B is not authorized or started. Visual approval remains Jonathan's decision.**
M1 passed Jonathan's initial playtest. No airships, flight, new game systems or catalog expansion were added.

## Launch
On MashPC, open **Skywright Art Preview** on the desktop and choose **Explore art sample**.
Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
Scene: Assets/Scenes/FirstHome.unity
Preview: Builds/M1_5_Preview/Skywright.exe
Shortcut: C:\Users\jsayl\Desktop\Skywright Art Preview.lnk
Direct launch: Skywright.exe -artpreview -screen-fullscreen 0 -screen-width 1600 -screen-height 900

The shortcut uses art-preview.json, separate from your personal home.json.
The original M1 executable at Builds/Windows/Skywright.exe and its desktop shortcut remain available.
No personal save was used as a test destination. Preservation copies and verification evidence are in Evidence/M1_5_A.

## Changes
- Nine functional sample pieces: deck, wall, window, doorway, roof slope, gable, railing, lantern, planter.
- Pale timber, softly rounded pearl frames, teal roofing, aqua reveals, blue-green glass, warm lamps and original modeled foliage.
- A separate 23-piece art-review layout, small meadow/limestone garden, leafy trees, decorative pool and static blue/cyan cloud sky.
- Wheel rotation in 45-degree increments; Shift+wheel reach; no Q/E rotation. Alt and Tab retained.
- Saturated blue valid placement preview, red invalid preview, distinct from lightly tinted glass.
- Prefab-derived palette thumbnails and catalog-provided names/hints.
- FirstHome stays the launch scene. The preview build does not regenerate it.
- Final code review also closed the existing F9 menu-opening frame's construction-input path.

The review layout was composed from a copy of the input-built M1 demonstration. It was not built entirely through player input. Separate input tests added a deck, wall, roof and railing to this layout.

## Replaceable art workflow
Resources/SkywrightCatalog.asset holds explicit stable type IDs, names/hints, nominal dimensions, origin descriptions, prefab references and local snap connections.
Each prefab has a functional BuiltPiece root, an invisible Collision branch retaining M1's exact colliders, and a separate Visuals branch with meshes/materials.
Swap or edit Visuals in Prefab Mode without moving the root, changing Collision or editing snap connections. Saved IDs and transforms do not depend on asset filenames or hierarchy names.
The meshes/materials/prefabs are ordinary Unity assets. The editor-only authoring utility produced the first originals; converted geometry is instantiated from assets at runtime.
Do not run the explicit authoring/regeneration command over later manual art edits.
Stairs, post, beam and bench deliberately retain the M1 fallback until approval of Checkpoint B.
See MAINTENANCE.md for exact authoring and build commands.

## Verification evidence
All input-based checks used OS keyboard/mouse input in the Windows standalone player. Read-only diagnostic snapshots and logs recorded outcomes; the game does not read commands from these files.

| Check | Observed result |
|---|---|
| Original M1 load | Unchanged verification fixture loaded 29 pieces; all 29 IDs, type IDs, parent IDs and transforms match exactly |
| Collider compatibility | All nine converted prefab collider sets match M1 counts, types, positions, rotations, scales and shape data |
| M1 traversal | Walked through doorway; climbed to Y=3.232 on the original elevated deck; descended and walked dock to Z=-24.037 |
| New construction | Placed deck, wall, roof and railing through normal selection/aim/click, increasing sample from 23 to 27 |
| Wheel directions | +120 notch produced 45 degrees; -120 returned to 0 |
| Reach modifier | Shift+wheel changed reach from 8.0 to 8.5 m with rotation still 0 |
| Removed bindings | Q/E input left rotation at 0 |
| Free placement | Held Alt changed snapped preview to free surface placement; overlap remained rejected |
| Snap cycling | Tab changed wall preview from (-4,0.2,-10), yaw 180 to (-2,0.2,-12), yaw 270 |
| Palette focus | Wheel and background click while palette open left count 23 and rotation 0 |
| Roof inspection | Exterior roof/gable and interior ceiling/window joins inspected at player scale |
| Genuine reload | Saved the 27-piece input construction, closed the application, relaunched and loaded it; all IDs and transforms match exactly |
| Final release smoke test | F9 menu blocked scroll/click changes; damaged verification save rejected with all 23 current pieces retained; roof removal affected only one piece; replacement saved and survived another full restart |
| User work | Personal home.json and original verification.json remain byte-for-byte identical to their preservation copies |

Key evidence:
- collider-compatibility.txt
- save-verification.json
- loaded-m1-audit.json, input-build-before-restart.json, input-build-after-restart.json
- wheel-positive.json, wheel-negative.json, shift-wheel.json, alt-free.json, tab-wall.json, menu-scroll.json
- stairs-top.json, dock-traversal.json
- build-b.log, play-a.log, play-b.log, play-restart.log, play-final.log
All paths above are under Evidence/M1_5_A.
Final build: build-final.log — SKYWRIGHT_ART_BUILD Succeeded errors=0.
Final runtime: play-release.log and play-release-restart.log; no managed exceptions or shader errors. The deliberate damaged-save warning and existing D3D12 startup diagnostic are documented exceptions.
Final release verification: release-verification.json, release-before-restart.json, release-after-restart.json, f9-menu-guard.json, failed-load-kept-world.json.
The replaced roof's deleted ID is absent after restart; the other 22 pieces remain.
Local Git checkpoint a54d7b5 contains the initial sample; the following handoff commit contains final focus/metadata fixes and this report.
No repository publishing occurred.

## Responsiveness
Same PC, 1600x900 window, 120 FPS target, same unchanged 29-piece fixture and camera position (0,0.23484,-10), yaw 180/pitch 20.
Separate 15-second stationary samples, with no editor build running:

| Metric | Original M1 | Art preview |
|---|---:|---:|
| Process CPU seconds | 8.375 | 7.578 |
| Average CPU core equivalents | 0.557 | 0.504 |
| Working set MiB | 537.2 | 567.3 |

Preview diagnostics recorded steady ten-second windows at approximately 8.33 ms/frame, near the 120 FPS cap.
These short samples show no obvious CPU responsiveness regression in this scene. They are not a statistically rigorous benchmark or an uncapped GPU measurement; original M1 did not expose comparable frame-time telemetry.
Initial loading/first-use windows can hitch; an early preview recorded a 218.6 ms worst frame. Warm-window averages do not hide that limitation.
No claim is made about large settlements, other PCs or all possible build combinations.
Sources: performance-before.json, performance-after.json and PERF entries in play logs.

## Screenshots â€” normal gameplay
![Exterior](Evidence/M1_5_A/exterior.png)
![Interior and material detail](Evidence/M1_5_A/interior.png)
![Placement preview](Evidence/M1_5_A/placement.png)
![Prefab-derived palette](Evidence/M1_5_A/palette.png)

## Asset provenance
All added geometry, material textures, shader code, plants, thumbnails and sample layout were originally created for Skywright during this milestone. No third-party art packs, purchases, new packages, paid APIs or runtime services.
The three approved generated concept images were available and inspected as references. They were not imported as fake 3D game assets.
Unity/URP/Input System retain their existing package licenses.

## Provisional presentation and limits
- The sample establishes palette, modular shape language and replacement workflow; it is substantially simpler than the offline concepts.
- Plant leaves, tree trunks, limestone and water need further art judgment. The decorative pool is a static bounded visual surface; it has no clear-water refraction, swimming or fluid simulation.
- Existing distant islands/pines, most ground and the four unconverted pieces remain M1 art. The whole island was not restyled.
- Roof pitch and doorway dimensions remain M1's. Larger arched openings and gentler roof geometry would need a separately considered compatibility decision.
- Window/railing glass is decorative. M1 window openings and railing gaps retain their original collision behavior, including potential passage through visually glazed openings.
- Garden decoration has no collision and can intersect later player construction. No foliage-clearing or terrain-editing system was introduced.
- Some board seams remain visible. No baked interior GI, reflection-probe system, advanced glass reflections or bloom pass was added.
- Preview save isolation depends on launching with -artpreview (the provided shortcut does this). Directly launching the executable without that argument uses normal home.json behavior.
- Unity's existing D3D12 info-queue startup diagnostic (0x80004002) remains a known non-blocking issue on this PC.

## Review procedure
1. Open Skywright Art Preview, then Explore art sample.
2. Walk around the home, inspect the roof and step inside.
3. Look closely at white frames, wood, glass, roof seams and planted corners.
4. Press B; select a deck, wall, roof or railing. Try wheel rotation, Shift+wheel reach, Alt and Tab.
5. F5 saves only to the preview slot through this shortcut. Quit/reopen and Continue saved preview.
6. Judge style, first-person scale, join readability, whether it feels like creative construction and whether this asset workflow is worth extending.

Stop after this handoff. Do not interpret successful verification as permission for Checkpoint B.

## M1.5 playtest follow-up â€” snapping, selection and surface clarity (2026-10-18)
Completed within Checkpoint A. Checkpoint B kit conversion and M2 remain unstarted.

### Changes
- Replaced opaque candidate cycling with named local anchors. Tab cycles Auto and piece-specific points: deck edges 1â€“4, stair landings, beam ends, roof eave/ridge/sides. The HUD shows the selected source point, target point and contact marker.
- Rotation is applied before solving root position from the contact. Beam ends remain on the post at 45-degree increments.
- Ground stair previews use their low foot, retaining the M1 root beneath the high end. Auto chooses a usable lower/upper landing; explicit underground landings are rejected with an explanation.
- Added roof ridge and lateral joins, plus explicit eave-to-ridge slope extension. Targeting prioritizes the piece under the crosshair.
- Shift + middle click selects the looked-at built piece's type, including with construction put away. It creates no object and copies no saved instance ID.
- Mouse wheel rotation, Shift + wheel reach, Alt free placement and individual removal remain. UI middle/right clicks no longer activate IMGUI buttons; construction input remains blocked in menus.
- Removed coplanar internal surfaces in sample wall/door/window/roof/railing visuals and redundant glass backface rendering. Colliders and root contracts are unchanged.
- Disabled the noisy ScreenSpaceAmbientOcclusion feature in PC_Renderer. Matched interior screenshots showed its mottling on white frames; disabling it removed the observed noise, including after strafing. Ordinary lights and shadows remain. A shadow-bias experiment did not resolve the noise and was reverted.
- Tightened same-location duplicate detection, including symmetric floor/beam/wall rotations. Existing constructions are not rearranged or resnapped.

### Playable build
Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
Scene: Assets/Scenes/FirstHome.unity
Executable: Builds/M1_5_Preview/Skywright.exe
Launch: desktop Skywright Art Preview shortcut, or the executable with -artpreview.
Choose Continue saved preview to resume the preserved 119-piece build.
The original Builds/Windows M1 executable remains unchanged.

### Verification actually performed
All interaction tests below used normal keyboard/mouse input in the Windows standalone player. Read-only QA telemetry recorded results; it did not command gameplay.
The four-object connection fixture and interior camera fixture were authored test setups, not claimed as player-built worlds.

| Check | Result |
| --- | --- |
| Compilation and Windows build | Passed; build-handoff.log reports Succeeded, errors=0 |
| Original collision contracts | All nine converted prefab collider layouts match M1 |
| Ground stairs | Placed through input with foot at terrain; no underground default preview |
| Upper stair landing | Placed a deck, climbed onto it (player y=3.035), saved, restarted, loaded and descended |
| Stair/deck modes | Auto chose Bottom on a low deck; explicit Bottom valid; explicit Top rejected burial; Tab returned to Auto |
| Beam/post rotation | 0, +45, +90 and reverse to 45 degrees stayed attached; a 90-degree beam was placed; measured anchor error zero |
| Roof joins | Placed opposite ridge half and lateral extension; explicit Left edge reached the opposite target side and placed another section |
| Tab feedback | Roof eave/ridge/left/right/Auto and stair Bottom/Top/Auto verified; deck Edge 1â€“4 label cycling/wrap verified |
| Pick piece | Shift+middle selected post and roof; worked with tool put away; middle alone did not select; subsequent placement used a new ID |
| Reach and free placement | Shift+wheel changed 8 to 8.5 m without rotation; Alt removed snapping while held |
| UI input ownership | Final palette and Escape-menu checks retained piece, rotation, reach, mode and object count under wheel/Shift+middle/Tab/outside clicks |
| Duplicate and removal | Duplicate roof click rejected; one right click removed exactly one fixture roof (8 to 7), retaining all other IDs |
| Reloads | Input-built stair/deck (2) and connection construction (7) reloaded across player restarts with identical saved piece data |
| Unchanged M1 compatibility | Backed-up 29-piece M1 fixture loaded with identical IDs/types/parents/transforms; deck and doorway traversal passed |
| Current-build compatibility | 119 pieces retained across load, save and later process reload; IDs/types/parents identical, transform component drift at most 5.96e-8 |
| Surface clarity | Compared same interior camera before/after AO change and inspected post-movement screenshots; visible frame mottling removed |

The initial palette test exposed IMGUI accepting middle clicks and invalidated the subsequent queued tests while the palette stayed open. It was corrected; final-palette-block/final-menu-block and the affected stair/roof cases were rerun. Earlier failed or blocked attempts remain in the evidence rather than being counted as passes.

### Preservation and evidence
Before rebuilding, the live session was saved through its menu and backed up as Evidence/SnapFix/user-live-build.json (119 pieces).
The personal art-preview.json SHA256 remains 92B4CA11CC13BC3402302909B426C0DEAC0F957E444F7E016C810C6D3A1BDD21.
Compatibility work used verification-snapping.json under -snapqa. Original fixtures and personal slots were not overwritten by test constructions.
Evidence/SnapFix is ignored by Git because it includes personal-save preservation copies.
Evidence includes input-evidence.jsonl, compatibility.txt, collider-compatibility.txt, build/play logs, stairs-ground.png, roof-ridge.png, interior-after.png, interior-final.png and interior-final-moving.png.
No new third-party assets, services or dependencies were introduced.

### Performance and remaining limits
Observed gameplay windows in the input tests averaged approximately 8.33â€“8.35 ms (about 120 FPS at the existing cap); no matched before/after benchmark is claimed.
Existing floating or intersecting pieces keep their saved transforms. Remove and re-place them to use the corrected snapping; there is no automatic repair migration.
Tab selects the moving piece's attachment point; aiming selects the target. An unavailable or occupied connection can still show invalid placement. Alt bypasses snapping, not collision checks.
Not every piece-pair/rotation combination was exercised. Deck mode labels were cycled, but all four deck-edge attachment orientations were not individually placed in this pass.
Existing D3D12 info-queue startup diagnostic remains non-blocking. Rendering has been checked at the 1600x900 preview settings; other GPU/display configurations are untested.
No full kit art conversion, airships or new progression systems were added.
