# Stone Sentinel visual trial

Original static visual trial, authorized and visually approved 2026-10-19. Jonathan subsequently authorized a playable Sentinel with animations, Earthstone damage and wind stagger. See `COMBAT_REPORT.md` for that separate continuation; this document records the preserved static viewer.

## Sources and cost

- Design sheet: `../CombatReferences/stone-sentinel-reference-v1.png`.
- Generation input: `../CombatReferences/stone-sentinel-input-v1.png`, original project concept artwork generated with the built-in image tool.
- Image-to-model: Tripo v3.1, detailed geometry and detailed v3.5 PBR texture; one task. Exact settings and task ID are in `generation-settings.json`.
- Static generation consumed **60 credits / $0.60**. The later authorized rig added 25 credits: aggregate **$0.85 of $3**, remaining balance 215. See `cost-ledger.json` for the original stage and `combat-cost-ledger.json` for the continuation.
- The original paid result is preserved as `sentinel-original.glb`. Account credentials are never included. Provider output is generated through Jonathan's Tripo account; this is not a claim that it is CC0 or a commissioned hand-sculpted asset.

## Editable source and Unity export

`StoneSentinel.blend` contains the full imported master, packed textures and three reduced meshes. `refine_sentinel.py` reproduces the offline preparation from the original GLB using Blender 5.1.2.

Preparation sets the height to 2.4 metres, centers the pivot on the ground, normalizes facing, exports explicit FBX files, and derives a restrained cyan-only emission map. Limestone stays matte and nonmetallic. The original imported UVs and texture detail are retained. The near/mid/far meshes contain approximately 70k/28k/10k triangles and share one 4K material set. Exact counts are in `asset-stats.json`.

Unity files: `Assets/StoneSentinelTrial/`. Explicit FBX files avoid automatic `.blend` importing. The prefab has no collider, harvesting adapter, damage logic, rig or animation.

## Review

Separate executable: `Builds/StoneSentinel_Trial/StoneSentinel.exe`.

RMB drag orbits; wheel zooms; 1–6 select review views; Space toggles turntable; H hides the interface; Esc exits. Uses the existing island prefab, MountainPipeline, sky, directional daylight, ambient lighting and Daylight volume. No added studio lights or altered global lighting. The review scene never instantiates SkywrightGame or accesses persistence. Gameplay is capped at 60 FPS, unfocused at 15.

`Review/` holds actual Unity camera renders, not generated illustrations or Blender beauty renders. Capture command: launch the trial with `-sentinelcapture <absolute-output-folder>`. It exits automatically after six views. `convert_captures.py` converts its TGA framebuffer data to PNG without visual changes.

Build only this trial with `Skywright.SentinelReview.Editor.SentinelTrialBuild.Build`. It creates the dedicated review scene and prefab, verifies height/ground pivot, temporarily uses the existing preview pipeline for shader inclusion, and restores project pipeline settings. It does not regenerate the island or replace the ordinary Visual Preview build.

## Verification on 2026-10-19

- Unity 6000.4.7f1 Windows standalone build succeeded with zero build errors. Imported LOD0 bounds: centre (0, 1.20, 0), extents (1.33, 1.20, 0.64); height and foot pivot checks pass.
- Inspected six actual URP camera renders: three-quarter, front, side, back, material close-up and eight-metre viewing distance. Also inspected the ordinary live window. No missing/pink materials or visibly hovering base in these views.
- Front-view shortcut verified using the previously authorized native input helper, targeted to this viewer's PID. Computer-use key injection alone did not reach Unity. Six automated presets were exercised by the capture pass. The orbit input was sent but its follow-up screenshot was occluded by another window, so orbit and other individual controls are not claimed as independently input-verified.
- Initial hidden-window framebuffer captures were black; replaced with Unity's supported `UniversalRenderPipeline.SingleCameraRequest` path and regenerated all six images. Current captures are valid. Final capture log has ready/completed markers and no managed exceptions.
- SHA-256 comparison: all 21 existing save/diagnostic JSON and backup files unchanged. The ordinary Visual Preview executable, FirstHome scene, authored island prefab and three project settings files also match their pre-trial hashes. Build-generated whitespace changes in MountainPipeline were reverted after inspection.
- Private logs, API status responses and preservation hashes: `Evidence/PlaytestFollowup/StoneSentinel/`. Final result is awaiting Jonathan's visual approval. No gameplay regression session was needed because the dedicated scene adds no gameplay or save hooks.

## Limits

Single-image reconstruction interprets unseen surfaces: the back is not an exact recreation of the separate reference-sheet rear view. Fine fern shapes, engraved spirals and the faceted chest crystal are softer than the illustration. This original viewer remains static. The separate combat continuation adds a rig and locally authored animations; production retopology and rigid articulation remain future refinement.

Do not rerun `tripo_trial.py submit` for a second charge. It records a submission lock and task ID in ignored local evidence, refuses duplicate submission, and has no automatic POST retry. Polling and downloading the existing task do not create models. Any further paid work must remain within the user's original aggregate cap and current task scope.
