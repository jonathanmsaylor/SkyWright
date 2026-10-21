# M1.9 — Compact Sky Island, Daylight, and Aether

Status: implemented and built; native verification results and remaining coverage are recorded below. Visual approval belongs to Jonathan's playtest.

## Build and access
Project: `C:\Users\jsayl\Desktop\UnityProjects\Skywright`
Executable: `Builds/Visual_Preview/Skywright.exe`
Scene: `Assets/Scenes/FirstHome.unity`
Desktop shortcut: **Skywright Visual Preview**, verified target and `-visualpreview` arguments.
Previous Visual build preserved at `Builds/Visual_M1_8_2_Backup`. Elemental and First Home builds remain untouched.
Baseline Git: `c1b83aa`. Final local checkpoint is the commit containing this report; its short hash is in Evidence/CompactAether/local-checkpoint.txt.

## What changed
- Compact irregular home landform, approximately 270 x 215 m, with central ridge sampled at 48.58 m. Home radius 38 m retains the original y=0 plane. Five composed scenic islands are visible around the home.
- Offline authored top/bottom grids and uneven rim geometry replace the previous large layout. Removed regular contour terraces after reviewing the built result. Limestone weathering, whole-island understory, bounded grass and trees use the existing coherent asset set.
- Harvesting trees preserve original trunk geometry, UVs and normals at uniform scale. Deliberate collision now fits the trunk: capsule radius .17 m, height 3.3 m, center (.015,1.65,0). This makes near misses behave more accurately; it does not change hit counts or rewards.
- Stable daylight, warm side sun, cool fill, restrained fog and bloom, 110 m/four-cascade shadows, fixed exposure, high SMAA.
- Air suction, physical Aether bubbles, permanent anchor upgrades, one active island field, and a persistent visual-only awakened current.

## Controls and quick playtest
Launch the desktop shortcut and choose **Explore visual sample** for a fresh isolated Visual world. A personal Visual save is written only with explicit saving. Previous incompatible world saves are never silently moved.

1. Q selects Earth/Air; Tab equips/stows the selected wand.
2. Earth LMB summons, launches and repeat-strikes; RMB recalls. Trees take five hits, limestone eight. Approach the yield to collect it.
3. With Air equipped, hold LMB to pull bubbles/eligible props; hold Shift+RMB to push; plain RMB grabs/releases a prop. Walk toward the pearl bubbles above the nearby roots and ledges.
4. Aim at the wind anchor and press E. The panel shows the exact next cost. Feed it and watch the field expand.
5. R lifts/hovers, Space rises, Ctrl descends within the field. Re-entering support during a personal lift/fall restores hover control.
6. F construction, B or construction RMB palette, LMB place, MMB remove, Shift+MMB copy type, wheel rotate, Shift+wheel reach, Alt free placement, Shift+Tab snap point. RMB closes the palette safely into construction.
7. F5 saves; quit/relaunch and Continue to inspect persistence.

## Tuning and economy
| Stage | Effective area | Cost Aether / Wood / Stone |
| --- | --- | --- |
| Workshop | 12 m widest horizontal radius, -2 to +10 m vertical | Free |
| Garden | 25 m widest horizontal radius, -2 to +26 m vertical | 6 / 6 / 4 |
| Settlement | 55 m widest horizontal radius, -2 to +64 m vertical | 12 / 12 / 8 |
| Island | Ellipsoid center (-32,30,58), radii (145,82,121) | 18 / 18 / 12 |

Early fields are ellipsoids, not vertical cylinders. Visual and effective center/radii interpolate together over .9 seconds. Total 36 Aether, 36 wood, 24 stone: three ordinary trees and two limestone clusters provided the harvested resources in the native test. 56 authored bubbles leave 20 spare. No respawn, mana, upkeep, fuel or recurring payment.

Suction: 16 m initial / 17 m retained; 14-degree half-angle / 24-degree retained; bubble speed up to 9 m/s, acceleration 24 m/s2; prop speed 3 m/s divided by size weight, acceleration 9 m/s2. Props brake at 2.2 m plus bounds radius. Push remains 7 m reach, 3 m/s, acceleration 10 m/s2. Grab range 6 m, carry cap 5 m/s. Sustained audio reuses the previous smooth loop; no new subjective audio listening result is claimed.

Earth remains 18 m/s guided flight, 20 m player separation, 40 m accumulated path and 3-second active outbound budget. Five/eight hit counts and nearby collection remain unchanged.

## Native verification evidence
Actual keyboard and mouse input operated the standalone Windows player; read-only QA snapshots recorded state. Scripts did not inject earned resources, change live state or feed gameplay commands through QA files.

Passed before final packaging:
- Fresh sample, home/woodland/foothill movement, cliff fall and safe recovery.
- Three trees each depleted in five separate impacts; two outcrops each in eight. Each valid strike increments damage by one. Distant yields were not credited until approached. Automatic return and ready stone retained.
- Elevated Aether collection; 36 bubbles collected once each; out-of-range suction rejected.
- Placed wall blocks collection. After removing it, the same four bubbles collect. Wheel rotation/reach, free placement, copy-type and removal operated through input.
- Air RMB grab/release, held Shift+RMB push, held LMB pull, and releasing Shift during an RMB gesture without accidental grab.
- Menu, save and focus-loss interruptions reset in-transit bubbles without changing balance. Repeated visits to collected locations do not award again.
- Insufficient Garden payment consumes nothing. Paid upgrades each consume exactly their recipe: Garden, Settlement, then Island.
- Garden genuine restart preserves all pieces, Earth and Aether state. Crate records differ only by tiny vertical physics settling (largest observed approximately 2.05e-8 m); an initial strict equality assertion correctly flagged that, then was investigated rather than represented as exact equality.
- Island genuine restart preserves pieces, Earth and Aether exactly; awakened current restored.
- Extra anchor remains dormant. Removing the primary while hovering transfers support to the replacement without changing tier/balance; saving succeeds.
- RMB palette closure consumes the held gesture without placement, reopening or spell activation.
- Protected earlier 31-piece save loads with every construction record/transform, all 15 existing resource records, balances and ready stone preserved. Existing Earth v2-to-v3 conversion adds 131 scenery records; Air v1-to-v2 initializes Aether. An initial whole-Earth equality check flagged the expected schema/new-record change; per-existing-record comparisons pass.
- Contract checks reject duplicate bubble identities, unbalanced ledgers and unpaid tiers. The runtime validator additionally rejects unknown identities. All 131 prior scenery resource IDs retained; 56 unique authored bubble IDs.

Navigation test scripts sometimes met physical crates/rocks and needed a different walking route. These were not counted as successful traversal. One test script accidentally used Windows Alt+Tab while free placing; its inputs were corrected and the placement/obstruction test repeated. No game fix is claimed for these test-helper mistakes.

## Save protection and compatibility boundary
All original slots were copied to `Evidence/CompactAether/saves-before` before implementation, with SHA256 manifest `preservation-before.json`. Personal home/art/elemental slots and their backups remained byte-identical during verification. Only `verification-visual.json`, its backup and diagnostics were used.

The compact save is schema 1 plus `worldLayout=compact-v1`; Air v2 carries Aether. Explicit GUIDs, collected identities, balance, tier and primary anchor persist. In-flight bubbles return uncredited to authored origins when interrupted; no trajectory save or reservation/refund is introduced.

Remote scenery roots were deliberately relocated to fit the compact island; their IDs and damage/depletion/collection state remain. Original 15 Earth trial roots and protected home ground remain. The current authoring relocation log is `resource-relocations.txt`; historical positions remain recoverable from baseline Git. This log records the latest authoring iteration, not a claimed complete baseline-to-final migration map.

Earlier layouts with any construction beyond radius 38 m are rejected before replacing the active world. Original files remain untouched; use the preserved previous build for those worlds. This is a conservative compatibility boundary, not seamless migration of arbitrary old construction.

## Performance and limitations
At 1600x900 with the existing 120 FPS cap, baseline's one ten-second window averaged 8.36 ms; subsequent test windows had median mean 8.33 ms. Largest recorded frame values were 37.50 ms baseline, 29.95 ms in the longer mechanics session and 31.37 ms in the first final-build session, including load/save activity. Samples are unequal and camera routes differ; this is a responsiveness observation, not a controlled speedup claim. The cap masks headroom. No uncapped/GPU-memory conclusion is claimed. `performance-summary.json` records samples.

The current is visual only, and scenic islands have no playable collision or travel. Field bounds deliberately exclude them. The landscape is an achievable stylized-realistic prototype, not photorealism; repeated scan assets, billboard/card foliage, grass fade and simplified distant landforms remain visible. The model/lighting direction needs human visual review. No expensive blur, global weather or water simulation was added.

## Editable sources and provenance
- `Assets/Skywright/Editor/CompactIslandAuthor.cs`: offline layout/landforms; `Resources/CompactIsland/Mesh_*.asset`: ordinary meshes.
- `Assets/Skywright/Editor/AetherAuthor.cs`, `Resources/CompactIsland/Aether.asset`: explicit bubble placements, IDs and recipes.
- `Assets/Skywright/Scripts/AetherSystem.cs`, `AetherConfig.cs`, `AnchorBubbles.cs`, Air integration: mechanics and effects.
- `ArtSource/FinishedMeadow/Landforms.blend` and prior FBX buttresses reused; no new Blender session claimed.
- `Assets/FinishedMeadow/ASSET_PROVENANCE.md`, plus existing MountainAero/NaturalAero provenance: owned ALP assets and previously imported CC0 sources. No new third-party downloads, purchases or services. Do not publish licensed source assets.

## Evidence and recording
Local evidence directory: `Evidence/CompactAether`. Gameplay screenshots and original recording are in `Evidence/FinishedMeadow`; selected handoff copies are packaged in CompactAether.
`aether-collect-expand.mp4` is a silent native gameplay capture of exploration, collection, feeding and expanded hover. Full decode succeeded and a contact sheet was inspected. It predates the final ridge geometry/SMAA polish; mechanics are unchanged. No recording of unimplemented travel is implied.

Final verification appendix and local Git checkpoint follow below.

## Final verification appendix
- Latest release authoring removed contour terraces and kept upper-ridge trees off exposed high slopes. Ground-cover eligibility now follows the actual irregular cliff outline, preventing fringe cards beyond the rim.
- Final build entry point AetherSaveChecks.VerifyAndBuild: succeeded with zero reported build errors. The retained entry point CompactIslandAuthor.AuthorAndBuild explicitly reauthors both terrain and bubble height placements before running those checks.
- Protected historical load failure was exercised through the real Continue button with an isolated negative fixture containing construction outside the protected clearing. The player remained at the menu and received the explicit old-layout message; no source/personal save was changed.
- Partial tree damage (tree-7, one impact and its local fracture point) survived close/relaunch/F5 with identical Earth records, construction records and Aether state. Native interior/doorway traversal passed after routing around the test crates.
- Workshop re-entry passed: a personal lift initiated outside the support boundary entered the field at approximately (27.69, .60, -6.79), immediately logged HOVER_SUPPORT_REGAINED, and enabled hover control. An earlier attempt started farther away and landed before reaching the field; it was not counted as a failed recapture.
- Extra-anchor placement/removal while hovering passed; final-tier progress was saved and restored afterward without additional charges.
- The camera/field view, current visibility and cliff vegetation were checked again in the final executable. Current ribbon width is .7 m at its widest, four tapered 96-point lines; the current has no collision or hover-volume component.
- No runtime exception, shader error or compilation error was found in the final inspected logs. The expected compatibility rejection is a handled load failure, not a runtime crash.

Remaining verification limits: no exhaustive sweep of every cliff, distant LOD transition, construction snap combination, furniture collision arrangement, or all possible anchor-removal sequences. The final island field's exclusion of scenic islands is verified from authored bounds; an attempted journey onto every neighbor was not performed. A dedicated sheltered overhang close-up and a complete new dock/stair build-through were not completed in this pass; interior, woodland, foothill shadows and existing contract checks cover part of that risk. The added brief feeding motes compile, but were not separately observed during a new payment; the reviewed full-loop recording predates them. Human audio listening, lower-resolution UI checks and uncapped performance measurements were not performed.

The original inspection found no project-local AGENTS.md and a clean baseline worktree. All changes stay in Skywright; no sibling project, repository publication or paid service was used.

Final preservation check: all 20 original non-log save/diagnostic files match their pre-work SHA256 hashes after restoring the isolated test slot and diagnostics. Completed earned progress is retained separately in Evidence/CompactAether/completed-earned-playtest.json. Personal Visual progress was not created or replaced. The verified old Visual executable remains in Builds/Visual_M1_8_2_Backup.

Latest normal gameplay images: handoff-current.png, handoff-island-field.png, handoff-neighbor.png, handoff-ridge.png, handoff-foothill-shadow.png; early field/tree/interior/cliff images and the reviewed recording are in the same evidence folder. No distant journey is implemented.
