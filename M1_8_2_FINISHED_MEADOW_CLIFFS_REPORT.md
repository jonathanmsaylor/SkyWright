# M1.8.2 — Finished Meadow, Natural Cliffs, and Owned Asset Integration

Implemented 2026-10-19 on MashPC. Visual approval remains the player's decision. Scope includes the subsequent request to harvest placed scenery trees/rocks and replace the handmade limestone resource visuals.

## Launch and review
- Desktop: **Skywright Visual Preview**.
- Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Visual_Preview\Skywright.exe
- Playable scene: Assets/Scenes/FirstHome.unity.
- Choose **Explore the island** for the bundled fresh sample, or continue your own Visual Preview save. The review area joins the cottage, meadow and foothill path; detailed vegetation is concentrated in roughly 50 x 50 m with tapered surrounding patches.
- The prior build is preserved in Builds/Visual_M1_8_1_Backup. Elemental_Preview remains available.
- Evidence: Evidence/FinishedMeadow (local, intentionally ignored by Git).

## Changes
- Selectively integrated owned Nature Package – Forest Environment v1.9: maple/forest trees, bushes, grass/flower textures and prefabs, ground soil/litter, and required URP shaders. Environment Vegetation – Bundle v1.0 was identified and inspected but not imported.
- Replaced the spike geometry with curved, alpha-textured grass clumps, bounded wind and distance fading. Added exposed soil/litter variation, composed multi-scale limestone groups and a woodland edge.
- Removed standing prototype stump platforms. Original harvesting maple bark is fitted to its unchanged capsule; its small scar conforms to the visible trunk. Felled stumps are conditional.
- Smoothed mountain high-frequency serrations and rock/grass transitions. Added authored southern cliff ledges and embedded scanned rock faces.
- Replaced the three pointed-dish neighboring islands with Blender-authored closed landforms using multiple lower masses and broken surface variation. Detached-looking distant scan accents were removed during final review. Neighbors remain scenery.
- User-requested harvesting: all 63 placed scenery trees and 68 placed rock props are now resource nodes. The original ten trees, two outcrops and three loose-stone pickups remain. The two original outcrops now display the imported Coast Rocks 05 asset within their established horizontal footprint. Their old proxy collider is replaced by the scan’s low-detail mesh collider so the shallower rock can sit on the ground and impacts match its visible shape; saved roots and IDs are unchanged.
- No M2, flight, new element, paid service or global render/input preset import.

## Harvesting and compatibility
- Trees: five hits. Limestone: eight. Existing rewards, collection radius, Earthstone speed/range/return behavior remain.
- Structural cliffs, island terrain, distant island formations, shrubs and grass are environment geometry. Loose stones retain their E pickup interaction.
- Each new scenery resource has an explicit serialized GUID on the Island prefab. Save identity does not depend on mesh/name/hierarchy.
- Earth state v3 adds 131 pristine nodes when loading older v1/v2 saves. Original node records, resources, construction records and prepared stone remain intact. New damage/depletion/collection persists.
- Scenery resources restore both visual and collision state. Embedded-resource loot is placed above the authored earth surface.
- Large scenery trees use a compact collapse rather than sweeping large crowns through buildings. Their damage feedback uses brief impact effects; persistent fitted bark scars currently apply to the original harvesting maples. This is a presentation limitation, not an extra harvesting mechanic.
- Older executables do not understand v3 visual saves; use the backed-up older fixture/build together. No personal save was converted during verification.

## Verification performed through native keyboard/mouse input
- Spawn, meadow walking, home/deck traversal, original saved stair climbing, new ground stairs and a snapped landing.
- Copy with Shift+MMB, wheel rotation both directions, Shift+wheel reach, snap cycling, free placement, placement/removal/refund and palette RMB close.
- Original tree: zero-stone summon, distinct launch, five impacts, bound repeated strikes, automatic return, local-only wood collection, partial scar, recall, save/restart.
- New scenery tree: five distinct hits, depletion, automatic return, local 12-wood award.
- New scenery limestone: eight distinct hits, disappearance, automatic return, local 12-stone award.
- Genuine restart after new scenery harvesting: all 146 node records and construction records identical.
- Protected M1.8.1 fixture: 31 pieces identical, original 15 node records identical, wood/stone/ready state identical; 131 pristine scenery records added. Original fixture was not overwritten.
- A separate new construction test saved/restarted 32 pieces with identical complete piece records and Earth state. A resting Air crate differed only by approximately 1.9e-9 m in Y after physics settling.
- Air crate lift/release, held gust, anchor rise and Ctrl descent were input-tested during this pass.
- Final Unity build succeeded with zero reported build errors. Introduced C# and shader compilation errors encountered during development were fixed; see final build log rather than intermediate logs.
- Construction contract comparison confirms stable type IDs, dimensions, origins, anchors and 70 colliders across the nine converted kit pieces.
- Input evidence and assertions: input-evidence.jsonl, legacy-upgrade-result.json, scenery-restart-result.json, genuine-reload-result.json, construction-before/after-restart.json, contracts.txt.

## Preservation
Original saves were backed up before work with SHA-256 hashes. Personal home, art and Elemental saves remained byte-identical throughout testing. Only the isolated verification-visual slot and diagnostic QA/audit files were used. Earlier builds and the separate Elemental build were preserved. No sibling project or repository publication.

## Art sources and workflow
- Assets/FinishedMeadow/ASSET_PROVENANCE.md contains exact package identities, links, licenses, selected content and shader adaptations.
- ArtSource/FinishedMeadow/Landforms.blend and author_landforms.py are editable Blender sources; FBX exports are in Assets/FinishedMeadow/Models.
- FinishedMeadowAuthor explicitly authors ordinary meshes/materials/prefabs. SceneryHarvestAuthor adds stable resource markers. Existing source GUIDs are retained for the selected owned package assets.
- Rebuild existing authored content with Skywright.Editor.MountainAeroAuthor.Build. Full regeneration is FinishedMeadowAuthor.AuthorAndBuild; inspect resource identity preservation when changing layouts.

## Remaining visual and technical limits
This is a concentrated playable sample, not a fully dressed photoreal island. Far slopes remain simpler; cliff joins and lower masses outside the representative section remain less refined. Foliage can read dark in backlight and alpha detail can shimmer in motion. Grass uses distance dithering, not a full geometric LOD chain. Some older props, the wand, prototype loose-stone/loot displays and unconverted construction pieces retain their earlier appearance. The original outcrops now use the scan’s low-detail collision mesh; exact contact differs from the old proxy collider. Saved roots and harvesting identities/rewards are unchanged.
The much older 119-piece art-preview save contains construction outside the unchanged home plateau; full traversal compatibility for that historical layout was not established. Every individual new scenery node was not manually harvested; representative tree/rock loops and all authored identities were checked.

## Performance evidence
1920 x 1080, same PC quality pipeline, VSync off, 120 FPS cap. Native input revisited the same three compositions; positions differ by at most 0.44 m, and home-facing yaw differs by about 4 degrees. These are comparable gameplay views, not identical deterministic camera replay.

| View | Before stationary average | After stationary average | After window worst |
|---|---:|---:|---:|
| Mountain | 8.33 ms | 8.33 ms | 8.54–8.63 ms |
| Home | 8.33 ms | 8.33 ms | 8.48–8.57 ms |
| Meadow/woodland | 8.33 ms | 8.33–8.34 ms | 8.54–9.38 ms |

Baseline stationary window worsts across these views were 8.45–8.74 ms. Baseline transitions included 21.30 and 27.67 ms spikes; post-change gameplay included a 34.33 ms opening/load window and a 13.11 ms window around collection/save. Do not treat transition spikes as stationary GPU cost.
The 120 FPS cap masks headroom. No uncapped GPU/CPU profiling was performed and no spare performance capacity is claimed. Evidence: baseline-player.log, matched-performance.txt, performance-player.log, and camera observations in input-evidence.jsonl. Vegetation was inspected while walking; close alpha foliage remains a visual-quality limitation.

## Screenshots and recording
Normal gameplay captures in Evidence/FinishedMeadow:
- final-home-meadow.png and final-home.png — cottage composition.
- final-grass-ground.png — grass/soil at player scale.
- final-tree-base.png and final-damaged-tree.png — grounded tree and fitted scar.
- final-rocks-woodland.png and final-meadow.png — outcrops and woodland.
- final-mountain.png — mountain-facing view.
- final-interior.png — traversable saved home.
- final-cliff-edge.png / cliff-underside.png — cliff review angles.
- final-neighbor.png — final neighboring island silhouette.
- final-limestone-replacement.png — imported visual replacing a handmade resource outcrop.
- meadow-walk-harvest.mp4 — 38-second silent gameplay capture: walk, summon/launch, five tree hits, automatic return and local collection. The raw 100-second capture is retained separately. Full decode succeeded; frames at 3/10/17/23/29/35 seconds were visually inspected (recording-review.jpg). Audio was not recorded.
Screenshots/recording show the implemented game, not generated concept imagery. Some automatic walking attempts correctly stopped at house walls/railings; traversal was then verified through the actual entrance. Failed routing attempts remain in the evidence and are not counted as passed tests.

## Final release checks
The grounded original limestone replacement was input-tested again after changing its collision: exactly eight hits, automatic return, local collection and explicit save passed. Final build: release-grounded-build.log, succeeded with zero reported build errors. Final 131 scenery GUIDs match the authored catalog exactly. The inspected gameplay recording predates only the distant-island cleanup and original-outcrop grounding correction; the final screenshots show those corrections.
The cliff/neighbor geometry is improved relative to the original fan, but still reads sculpted and simplified at distance. The nearby sidewall retains some visible layered joins; this is explicitly left for visual feedback rather than presented as final photoreal cliff art.

Final grounded-outcrop save/restart retained complete piece records, Earth state and Air state. Original save fixtures and diagnostic QA/audit files were restored after evidence copies were retained; preservation-final.json records the final SHA-256 comparison. The desktop shortcut is checked against the updated executable and opened at the menu for handoff.

Current controls are unchanged: WASD/look, Space jump; F construction, B kit, RMB palette toggle in construction, wheel rotate, Shift+wheel reach, Alt free placement, Shift+Tab snap points, MMB remove, Shift+MMB copy; Q selects Earth/Air, Tab equips magic, Earth LMB summon/launch/strike and RMB recall, Air LMB lift and held RMB gust, R hover, Space/Ctrl rise/descend, F5 save and Escape menu. Stop here for the player's visual/playtest feedback.
