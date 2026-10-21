# M1.8.1 — Mountain Island and Visual Rebuild

Status: representative environment implemented; awaiting player visual review. Technical verification does not constitute visual approval. No M2, full island detailing or full kit conversion was started.

## Launch and playtest

Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Visual_Preview\Skywright.exe
Scene: Assets/Scenes/FirstHome.unity
Desktop shortcut: Skywright Visual Preview. Choose Explore visual sample.
Previous visual executable: Builds/Visual_M1_8_Backup/Skywright.exe.
Existing Elemental preview remains available and was not rebuilt.

Walk around and through the cottage, inspect the deck and roof, then follow the rising meadow/rock corridor northwest toward the foothill overlook. The route starts near (-15,0,10) and ends near (-110,36,98). High mountain traversal is not a finished playtest route. Escape offers safe position recovery.

Controls: WASD/mouse, Shift sprint, Space jump; F build, B kit, RMB opens/closes the kit into construction; LMB place, MMB remove, Shift+MMB copy type, wheel rotate, Shift+wheel reach, Shift+Tab snap mode, Alt free placement. Tab wand, Q element. Earth LMB summon/launch/strike, RMB recall. Air LMB lift/release, held RMB gust. R hover, Space rise, Ctrl descend. F5 explicit save.

## Implemented environment

The old small home terrain is replaced only in visual mode. The fixed authored outline is approximately 1150 x 780 m (about 50 m shorter than the provisional 1.2 km target), with small irregular boundary variation. Maximum sampled elevation is 239.55 m. Three dominant connected peaks plus lower ridges, saddles and spurs occupy the actual island mesh. The footprint is shaped around the existing home and the foothill route rather than uniformly scaling the old island.

The home plateau preserves y=0 within a 38 m radius, exceeding the original roughly 23 m home footprint. A 131.24 m authored route rises to a foothill viewpoint around 36 m. There are 107 collidable top-surface chunks sampled at 3 m spacing, a closed layered cliff underside, and three neighboring scenery islands with larger silhouettes. Visible top meshes and terrain collision share geometry. Boundary projection removes the initial grid-step outline.

The representative composition includes a revised 30-piece, eight-metre-wide cottage/deck, new broadleaf tree visuals, limestone boulders, ferns and restrained grass groups. All ten existing harvesting trees receive the replacement visual model without replacing their harvesting identities, crown pivots or deliberate colliders. Additional woodland trees are scenery and do not introduce new harvesting nodes.

Terrain uses grass/rock blending by slope, height and path, triplanar limestone and broad color variation. A dedicated URP preview pipeline adds restrained SSAO, soft sunlight, balanced ambient light, light bloom, neutral tone mapping and atmospheric separation. The existing lightweight cloud sky avoids a terrestrial HDRI horizon. No waterfall or pool was added; those optional treatments were deferred to finish terrain and verification.

The existing nine-piece natural construction catalog remains. Functional dimensions, pivots, anchors, colliders and stable IDs are unchanged. This pass improves the environment and sample composition; it does not convert every construction piece.

## Engineering changes and corrections

MountainAeroAuthor produces ordinary editable meshes, materials, prefabs and a bundled demonstration. MountainEnvironment instantiates those assets only in visual mode. There is no runtime procedural world framework. Visual camera far plane is 3000 m.

Construction ground checks now use the existing EarthSurface component instead of the literal object name Home island. This preserves floor/post ground allowances and stair burial checks across terrain chunks. Actual stair placement and climbing were tested after this correction.

Corrections during implementation included the Forward+ lighting shader variant, cliff winding, coarse boundary stair steps, tree LOD canopy loss, and preview SSAO build-resource stripping. The build wrapper temporarily selects the preview renderer while building and restores project settings afterward. The initial direct build with missing SSAO resources was rejected and replaced.

Build current assets with Skywright.Editor.MountainAeroAuthor.Build. Explicit AuthorAndBuild regenerates the island and sample and can overwrite manual changes to those generated assets. Do not use the historical direct BuildNaturalPreview entry point for this preview.

Earth tuning remains 18 m/s, 150 degrees/s steering, 20 m player separation, 40 m accumulated path and 3 s active flight; five tree impacts and eight limestone impacts. Recall, hover, construction reach, collection radius, resource costs and save schemas were not retuned.

## Actual input verification

Tests used native keyboard and mouse events in the built executable. Read-only QA snapshots supported observation; gameplay state was not injected to substitute for input.

| Flow | Result / evidence |
| --- | --- |
| Spawn, movement, jumping and cottage/deck traversal | Completed through ordinary movement, including both open entrances and roof interior inspection. |
| Foothill route | Walked the full route repeatedly; final position approximately (-109.8,35.5,97.8). No tested path gaps or blockers. |
| Cliff fall / safe recovery | Walked off the edge, captured below the terrain, and recovered to the established spawn. Menu recovery also tested. |
| Copy, rotate, reach and snap | Shift+MMB copied a floor, wheel rotated both directions, Shift+wheel changed reach without rotation, Shift+Tab changed named snap mode, Alt produced free placement. |
| Place and remove | Snapped floor placed, wood deducted, individual floor removed and wood refunded. An initial aim through a board gap missed; corrected aim selected and removed the intended floor. |
| Stairs on new terrain | Valid ground placement and climb tested after EarthSurface correction; player reached approximately y=2.58 on the stair. Underground guard also remained active. |
| Palette input | RMB opened/closed the kit into construction without accidental placement. |
| Earth harvesting | Zero-inventory summon; initial hit plus four distinct strikes depleted tree 0. Automatic return restored ready state. Wood stayed at zero at a distance and became 12 only after entering the local collection radius. |
| Earth recall | Launch and RMB recall after restart returned to ready state. |
| Air | Crate lift/release, continuous held-RMB wind, anchor hover ascent and Ctrl descent exercised; cancelling hover returned to ground. |
| Genuine save/restart | Exited and relaunched executable; construction, ready Earthstone, inventory and resolved harvesting survived. A newly placed floor also survived a separate restart. |
| Older-save compatibility | Protected pre-pass 25-piece visual fixture loaded and explicitly saved without changing piece records or Earth state. |
| Runtime/build logs | Final build succeeded; no introduced compilation, shader or gameplay exceptions found in final logs. Startup D3D12 info-queue query diagnostic also appeared in the baseline. |

Contracts in Evidence/MountainAero/contracts.txt compare all nine converted piece definitions and 70 colliders, plus save and tuning gates.

Persistence comparisons:
- Earlier fixture: 25/25 piece records identical, Earth identical. Air crate settling changed a Y coordinate by roughly 3.7e-9 m; no identity or resource change.
- First new restart: 30/30 piece records and Earth/Air records identical.
- Placed-floor restart: 31/31 piece records and Earth identical. Air crate position/safe Y changed by 1.68e-8 m from physics settling; no semantic loss.
- Original personal home and Elemental constructions lie within the preserved flat plateau and were not relocated.
- The older 119-piece art-preview save has 42 pieces outside the preserved plateau. Its file remains untouched; compatibility of that entire larger construction with the new landscape was not established. Do not automatically import it.

## Performance

Measured in the Windows executable on MashPC's RTX 4070 SUPER at 1920 x 1080, existing PC quality and 120 FPS cap, before video recording. Camera location was reproduced within 0.29 m; view directions were matched. The environment and preview rendering necessarily differ between versions.

| View / build | Steady average | Worst frame in recorded steady windows |
| --- | --- | --- |
| Rejected preview, home/mountain baseline | 8.33 ms | 8.49–8.68 ms |
| New mountain view, two 1200-frame windows | 8.33 ms | 8.52 / 8.57 ms |
| New home view, two 1200-frame windows | 8.33 ms | 8.48 / 8.51 ms |

An earlier baseline transition window included a 23.27 ms worst frame. Capped results do not measure unused GPU capacity or establish performance on weaker hardware. No uncapped GPU profile or full-island worst-case stress test was performed. New near tree meshes are relatively expensive, so further widespread scattering needs profiling.

## Assets and editable sources

New models are CC0 Poly Haven assets:
- Tree Small 02, Rico Cilliers: https://polyhaven.com/a/tree_small_02
- Coast Rocks 05, Rob Tuytel: https://polyhaven.com/a/coast_rocks_05
- License: https://polyhaven.com/license

Reused approved material sources: ambientCG Wood091A, Grass004, Rock030 and Poly Haven Fern 02. Detailed source/license/modification records: Assets/MountainAero/ASSET_PROVENANCE.md and Assets/NaturalAero/ASSET_PROVENANCE.md.

Blender 5.1.2 was used on MashPC for source preparation, LOD exports and packed editable sources:
- ArtSource/MountainIsland/Broadleaf_prepared.blend
- ArtSource/MountainIsland/Limestone_prepared.blend
- prepare_models.py, prepare_textures.py, pack_sources.py and model-budgets.json in the same folder.

Tree LOD triangle counts: 379640 / 159848 / 64938. Rock: 13998 / 3998 / 900. These are actual exported counts, not estimated budgets. Two-kilopixel maps, shared materials and LODs limit cost; richer leaf topology was retained after aggressive simplification made the canopy visibly thin.

No purchases, account creation, paid service or online gameplay dependency. Recording uses local imageio-ffmpeg tooling in ignored evidence, not the shipped game.

## Evidence and visual limitations

All evidence is local in Evidence/MountainAero; it is excluded from Git because it includes private save fixtures and large captures. Review home-exterior-final.png, interior-final.png, deck-detail-final.png, foothill-route-final.png, home-from-foothill.png, construction-preview.png, stairs-on-terrain.png and cliff-underside.png. Baseline views and measured JSON records are in the same folder.

The screenshot called floating-island-silhouette.png did not produce a useful underside composition and is not presented as successful evidence. cliff-underside.png is a close below-edge gameplay view; a full remote flyaround of the underside was not performed because normal recovery limits the fall. No cinematic-only camera or altered screenshot was used.

This is not photorealistic and does not yet reproduce the references' vegetation density or fine rock shaping. Grass blades remain simple and sparse; the single broadleaf species limits variety; distant terrain has improved large forms but close mountain slopes still reveal simple geometry and material repetition. Some existing planters and unconverted pieces retain prototype shapes. Detailed high-peak routes, neighboring-island traversal, broad compatibility outside the preserved home plateau, other hardware and every possible construction junction remain untested.

The appropriate next decision is visual approval or specific art feedback on this actual build. Do not expand the world or kit automatically.

## Final handoff checks

Desktop shortcut was launched and its actual 1600 x 900 Mountain Island menu inspected (shortcut-menu.png), then closed without starting or saving a personal session.

gameplay-walkthrough.mp4 is a 105-second, silent, 1600 x 900 ordinary gameplay capture covering cottage/deck movement and the complete foothill walk. Extracted frames at 20 and 90 seconds were visually inspected and show distinct positions and views. The first HWND-based capture froze and is retained only as rejected-frozen-window-capture.mp4; it is not the delivered walkthrough. Final recording uses a desktop crop of the game client.

Before-change save files are backed up in Evidence/MountainAero/saves-before, with SHA-256 manifest preservation-before.json. Final preservation-after.json reports personal_saves_unchanged=true and personal_visual_slot_exists=false. Only the isolated visual QA slot and QA telemetry changed. Legacy QA load-audit and original player logs were restored after preserving the new evidence separately. Prior visual build is backed up, and the Elemental build remains untouched.

The tracked-code diff whitespace check passed using the repository's Windows line-ending configuration. The full staged-asset check also reported Unity-generated serialization whitespace (including empty metadata values); this was not treated as a compile or runtime failure, and generated assets were not mass-reformatted. Local checkpoint includes code, ordinary assets, packed Blender sources and documentation; no repository publication. Use git log -1 for the final checkpoint hash.
