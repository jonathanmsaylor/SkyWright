# Stone Sentinel playable trial

The supplied-animation pass supersedes the motion/attack timings below. See [ANIMATION_REPORT.md](ANIMATION_REPORT.md) for the current awakening, combo, walk and leap behavior. Reusable-stone, swat, wind and armor rules below remain unless explicitly superseded.

## Current follow-up: reusable stone, swat, wind, armor and pursuit

The latest authorized pass improves this same isolated encounter and includes Jonathan's follow-up requests for polished limestone drops and pursuit that does not stop/reset unexpectedly. Launch `Builds/StoneSentinel_Combat/StoneSentinelCombat.exe`. It starts paused. The ordinary Visual Preview and static viewer remain separate.

**Current controls:** Q changes Earth/Air; Tab equips/stows. Equipping Earth automatically readies one reusable stone without an attack. LMB immediately launches; hold repeats attached strikes; RMB recalls. Air LMB pulls, Shift+LMB pushes; both can stagger. Esc pauses/retries; Enter retries after victory. Existing movement and slam avoidance remain.

After three accepted hits on a current attachment, the Sentinel braces and swats the stone away. The swat lasts 0.9 s and contacts at 0.48 s; stone knockback lasts 0.35 s before recall. A stagger or committed slam/recovery can postpone the swat; pending swats reject further damage. After recovery, release LMB and press again to cast. Holding, swapping, stowing and recall cannot reset this gate. Active tool changes recall the same object; obstructed recall fades back after the existing return budget without spawning a replacement. Initial equip waits for clear wand space.

Moving attachment strikes now carry their contact and anticipation position with the target's transform, ignoring its own collider during rebound while respecting other scenery. Wind rearming occurs after stopping the previous stream, fixing quick release/repress failure. Wind targeting uses the visible body surface rather than a single chest point. Existing wind tuning remains: 0.4 s focused exposure, 0.25 s during windup, 2 s stagger, 5 s resistance measured from stagger start. The HUD shows resistance, pending swat and recovery instructions.

Armor stages occur at 18, 12 and 6 health. Six outer plates are separated along existing model components; six textured inner sections appear as those plates shed. The strong central crystal remains intact. All 69,999 original outer triangles are retained exactly once across 24 outer sections. Eight locally authored skeletal clips include Swat. Damage remains 1 armored / 2 exposed; armor stages are visual only.

Death disables attacks/attachment immediately, dims emission, and starts breakup after 0.4 s. Corrected scale compensation places detached pieces at their animated body positions. Cosmetic stone debris has no colliders, resource IDs or damage callbacks; maximum 32 pieces, 14 s lifetime with a final fade/shrink. Retry clears debris and restores armor. Three polished limestone variants replace hollow armor shells and cube-shaped combat chips: closed volumes derived from the paid full-resolution sculpt, bevelled edges, fresh UVs and existing NaturalAero Rock030 2K stone color/normal/roughness detail. Their meshes have 2,610 / 2,688 / 3,588 triangles. FBX units are normalized into runtime mesh assets. Earth burst particles receive their intended small size on creation, avoiding a one-frame oversized cube.

**Aggro correction:** the old code reused the 12 m acquisition radius as its chase cutoff and returned home when the player crossed 16 m from the spawn. Detection now requires clear sight within 12 m in 3D; an engaged Sentinel continues pursuit within 20 m and tolerates three seconds of lost sight/range. A 22 m leash bounds distance from its home. Deliberate return-home does not heal or reset health, and is identified in the HUD. Navigation remains basic CharacterController steering with ground-ahead checks; it does not have full obstacle pathfinding.

No additional Tripo calls were made. The recorded aggregate remains 85 credits / **$0.85 of the $3 cap**; this pass uses the existing paid outputs and local Blender. The previously recorded 215-credit balance was not re-queried because no paid operation was needed. Sources: `animate_sentinel.py`, `StoneSentinelCombat.blend`, `make_limestone_debris.py`, `LimestoneDebris.blend`, `armor-sections.json`, `limestone-debris-stats.json`. The approved original GLB and static trial are preserved.

Actual Unity captures under the existing lighting are in `CombatRevision2Review/`, including armor stages, Swat, breakup and a close view of the limestone drops. The opt-in regression runner exercises the real Earth/Air Tick paths using virtual Input System devices as well as direct enemy simulation; it is not a substitute for native mouse/keyboard testing. Private logs and preservation hashes are under `Evidence/PlaytestFollowup/StoneSentinel/Revision2/`.

### Follow-up verification and limits

The regression run passes 105 checks, covering auto-ready and one-stone identity, obstructed summon/recall, moving attachments, three-hit swat and release/repress recovery, repeated pull/push stagger, armor/death cleanup, and acquisition/pursuit/grace/leash behavior. Existing Earth/Air save-contract checks also passed. Verification uses virtual input for repeatability; separate native mouse/keyboard playtesting confirmed Q/Tab readiness, held strikes, swat/recast, repeat pull/push/pull stagger and pursuit beyond the old cutoff. During the native chase check the player reached z=-19.60 while the Sentinel remained engaged and walking at z=-4.79. Pause held the enemy state unchanged.

The polished fragments share immutable mesh assets rather than cloning unreadable imported mesh buffers. Ground contact uses the rotated mesh vertices, not the loose renderer bounding box. Actual Unity breakup and close-up captures were visually inspected. Native input checks preceded this final debris-only adjustment; final regression/capture checks cover that adjustment.

Preservation: all 386 protected files matched their pre-pass SHA-256 hashes, including 21 pre-existing personal save/diagnostic files and backups, ordinary Visual Preview and static Sentinel viewer contents, authored island/FirstHome assets and protected project settings. No ordinary gameplay build or save was replaced.

Known limitation: verification players completed all checks and captures but then exited with Windows access violation -1073741819 during native Unity shutdown, after `CodeReloadManager destroyed`, with no managed exception in those completed runs. A native shutdown failure was already documented in earlier ordinary previews; its cause remains unresolved. This pass does not claim to fix it. Combat feel, animation quality and the revised limestone appearance still require Jonathan's playtest approval. No additional creatures or saved-island combat were added.

## Historical first playable trial (superseded by the follow-up above)

Authorized 2026-10-19 after Jonathan approved the visual and selected “Playable combat: animations plus Earthstone damage and wind stagger.” This is one isolated encounter, awaiting his playtest.

## Play

Launch `Builds/StoneSentinel_Combat/StoneSentinelCombat.exe`, then Resume encounter. It starts paused. The ordinary Visual Preview and the original static Sentinel viewer are separate executables.

- WASD and mouse: move/aim. Shift: sprint. Space: jump; double jump: leap/glide.
- Earth: LMB summons the reusable stone; hold LMB to launch and repeat strikes on the attached Sentinel. RMB recalls.
- Q switches Earth/Air; Tab equips or stows.
- Air: hold LMB or Shift+LMB aimed at the Sentinel within 8 m to stagger. Solid cover blocks it.
- Esc pauses and offers retry. Enter retries after victory. Four unavoided slams cause a brief recovery and reset the encounter without losing supplies.

The Sentinel advances slowly, raises both fists, marks a 2.35 m circle, commits to its aim, then slams. Sprint out, leap over the ground shock, or interrupt the windup with Air. A stagger or post-slam recovery exposes its core and doubles Earthstone damage. Wind resistance prevents holding it stunned indefinitely.

## Implementation and tuning

One 2.4 m Sentinel, 24 health. Earth hits deal 1 armored / 2 exposed damage, with duplicate-contact rejection. Wind requires 0.4 s focused exposure, shortened to 0.25 s during windup. Stagger lasts 2 s; resistance lasts 5 s from its start. Slam windup is 1.25 s, with targeting locked after 0.55 s; impact occurs 0.22 s into the slam; recovery lasts 1.6 s. One impact deals 25 of the player's 100 health. Jump clearance uses foot height above 0.75 m; the marker allows for player collision radius. Recovery takes 2.5 s and returns to the encounter clearing.

The Sentinel uses a CharacterController for collision, bounded pursuit, ground-ahead checks and a return-to-home state. This is a single-enemy prototype, without a baked navigation mesh, loot economy, quest, additional creatures or saved enemy state. Obstacle navigation is basic; test this encounter in its clearing. Balance, foot sliding and automatic skin weights remain candidates for refinement after playtesting. The skinned rock mesh can soften at joints; it is not a hand-retopologized collection of rigid stone plates.

Dedicated scene, prefab and scripts: `Assets/StoneSentinelCombat/`. Existing Earth contact/binding and Air input feed the encounter. The game/UI hooks are gated by a scene-local SentinelCombat owner. Construction and save/load actions are disabled only in this trial. It never reads a personal slot; explicit save/load methods have early guards. No combat data is added to existing save schemas. Normal play does not write QA snapshots. The game retains its 60 FPS play, 30 FPS menu and 15 FPS unfocused caps.

## Asset and cost

The approved image-to-model source and all static review assets are retained. Tripo rig check was free; one biped rig task consumed 25 credits, with no retry or purchased animation. Generation 60 + rig 25 = **85 credits / $0.85 total**, within the original $3 cap. Verified remaining balance: 215 credits, zero frozen. Task IDs and charges: `generation-settings.json`, `cost-ledger.json`, `combat-cost-ledger.json`. No credentials are stored in these files.

`sentinel-rigged-original.glb` preserves the provider result. `StoneSentinelCombat.blend` contains the refined 65-bone rig, packed source textures and seven original authored skeletal clips: Idle, Walk, Windup, Slam, Recover, Stagger and Defeat. `animate_sentinel.py` reproduces the normalization, 69,999-triangle export and animations offline in Blender 5.1.2. It uses the approved shared 4K material set. No new lighting or concept-image regeneration. The original static viewer still has its three LOD meshes; this single-enemy animated prototype uses one skinned mesh.

## Rebuild and verification

1. Run Blender headless with `--python ArtSource/StoneSentinelTrial/animate_sentinel.py` if changing animations.
2. Build with Unity `-batchmode -quit -executeMethod Skywright.Editor.SentinelCombatBuild.Build`. This only rebuilds the dedicated combat scene/prefab/executable and restores the prior project pipeline settings.
3. Optional `-combatqa` writes build-local `combat-state.json` for input verification. Optional `-combatverify <absolute-output-folder>` runs deterministic combat checks, captures actual Unity animation poses and exits. Use `convert_captures.py <source-folder> <destination-folder>` for lossless TGA-to-PNG conversion.

Unity 6000.4.7f1 standalone builds succeed with zero build errors. The deterministic suite passes 27 checks: armor/exposure damage, duplicate contact, pause rejection, wind range/direction/cover, stagger and immunity, slam interruption, single-hit impact, leap/range avoidance, down/reset, defeat collider removal and seven imported clips. Existing Earth and Air save-contract suites also pass. These tests use direct simulation calls; native input checks are recorded separately below.

Actual Unity renders of the six representative animation poses are in `CombatReview/`. They are camera renders of the shipped rig under Skywright's lighting, not concept illustrations. The slam pose was revised after visual inspection to bring the fists forward and down.

Private build/runtime logs, test results, native-input observations and save/build preservation hashes live under ignored `Evidence/PlaytestFollowup/StoneSentinel/Combat/`. Initial native-input checks confirmed Earth summon, launch, held repeat hits, double exposed damage, defeat/automatic stone recall, Air stagger, paused state stability and automatic player recovery.

Final-build native input verified the opening pause, Earth summon, Q-to-Air held wind stagger, Q-to-Earth launch and held repeat strikes: Sentinel health fell from 24 to 17 while player health remained 100. From a committed windup, Shift+D moved the player 4.9 m out of the circle; the slam completed without damage. Paused state/time remained stable afterward. The final deterministic run again passed all 27 checks; final build/capture/input logs contain no managed exceptions or missing-marker errors. Six final animation-pose renders were inspected.

Preservation comparison: all 21 pre-existing save/diagnostic JSON and backup files match their baseline SHA-256 values; both existing executable files, FirstHome scene and authored Island prefab match their four protected hashes. The unused combat-save path was never created. Project pipeline settings were restored; only harmless Unity serialization whitespace in MountainPipeline needed normalization. No ordinary gameplay executable was rebuilt. This is ready for Jonathan's first combat playtest, not a claim of finished combat balance or production animation quality.
