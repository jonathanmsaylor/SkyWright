# Skywright maintenance

## Wind Magic visual overhaul — 2026-10-20

Current review: `Builds/WindMagic_Preview/Skywright.exe -visualqa`; Sentinel counterpart: `Builds/WindMagic_Combat/StoneSentinelCombat.exe`. Existing Visual Preview, Sentinel combat and static model viewer are retained. The island review must use `-visualqa` to keep any user-initiated saves in the isolated slot. Current report and recordings: `ArtSource/WindMagic/REPORT.md`.

Air bindings now supersede the historical entries below: hold LMB suction, hold RMB continuous push, Shift+LMB click grab/release. The Shift modifier is sampled only on a fresh left press. The active gesture owns both buttons until neutral; simultaneous new presses choose grab chord, otherwise RMB. Held objects suppress both wind forces. Pause, focus loss, stow, switching and saving cancel/rearm casts and release control safely. Wheel distance, Shift+wheel height and all movement controls remain.

Two authored prefabs in `Assets/Resources/WindMagic` share `AeroFlow.shader` and one material. `WindFlow.cs` animates bounded world-space ribbon geometry with soft depth/near-camera fading; `AirGesture.cs` owns the button state. No VFX Graph package, pipeline migration, global lighting changes or terrain authoring. Tune the prefab fields directly; rebuild preserves authored tuning.

Build with `Skywright.Editor.WindMagicBuild.Build`. This validates Earth/Air save contracts and builds the two existing scenes into new folders. It does not call the terrain authoring or Sentinel model/animation generators. Source gameplay changes apply in Editor and future builds; older executables retain older bindings.

Opt-in standalone verification: `-windverify <absolute-folder>`; visual tour: `-windtour <absolute-folder>` (add `-finalflow` for the updated spells, grab, transitions and surface scene); performance: `-windperf <absolute-folder>`. Always include `-visualqa -noqaoutput` for these island tests. Performance must run in a visible window: a hidden window can skip rendering. `-windqa <absolute-folder>` is a native-input fixture at the crates, fully attuned only in memory, with build-independent read-only diagnostics. None loads or writes personal saves. These are verification fixtures, not normal gameplay options.

Final verification: 40 wind checks and 131 Sentinel regressions passed, with separate native input checks. Visible uncapped 1080p GPU frames averaged about 4.03 ms; new flow CPU work averaged 0.47 ms suction / 0.51 ms push. Allocation counter calibration failed, so no zero-allocation claim is made. All 566 protected file hashes matched. Recordings and full methods/limits are in the report. Await Jonathan's review.

## Sentinel supplied animations — 2026-10-19

Current combat build now uses Jonathan's four FBX clips: Mutant Walking for locomotion, Dual Weapon Combo for a three-hit slam sequence, Mutant Jump Attack for an 8–16 m gap-closing leap, and Zombie Stand Up for the initial reveal. The Sentinel rests among stones with its core dark until approached within 8 m with clear sight or disturbed by Earth/wind. It rises for 3.1 s before fighting; retry restores the sleeping reveal.

Leap: 0.65 s warning, 1 s collision-aware arc, fixed landing target, 1.15 s exposed recovery, 7 s cooldown. Wind interrupts takeoff; flight is committed. Landing damage is 25 once. Combo: 0.9 s windup, three hits of 8/8/9 during a 2.65 s clip, 0.8 s recovery. Prior swat, stone recovery, repeat wind and armor rules remain. This is still the separate one-creature combat trial.

Offline retarget: `ArtSource/StoneSentinelTrial/retarget_imported_animations.py`; editable output `StoneSentinelAnimated.blend`; 13 exported clips. The script reads the retained authored `StoneSentinelCombat.blend` and the four source copies in `ImportedAnimations/`. Run retarget after any regeneration with `animate_sentinel.py`, then use the existing combat Build method. Original model/weights remain; shoulder/elbow ranges are adapted to its proportions. See `ArtSource/StoneSentinelTrial/ANIMATION_REPORT.md` for timings, evidence and remaining visual limits.

Final verification: 131 checks passed, actual Unity poses inspected, and native input confirmed awakening, walking, leap/dodge, three-hit damage, pause and retry. All 386 protected hashes and all four original animation files are unchanged. Final automated player exited 0; the previously documented intermittent Unity shutdown issue is not claimed fixed. No new Tripo charges. Await Jonathan's animation/feel review.

## Stone Sentinel combat follow-up — 2026-10-19

This supersedes the first-trial controls below. Rebuild only `Builds/StoneSentinel_Combat/StoneSentinelCombat.exe` with `Skywright.Editor.SentinelCombatBuild.Build`; preserve the ordinary Visual Preview, static Sentinel viewer and personal saves.

- Earth readies automatically when equipped through Q or Tab. LMB launches immediately, hold repeats bound strikes, RMB recalls. Equipping never attacks. Switching/stowing recalls an active cast without creating a replacement; swat/recovery survives switches. Obstructed returns use a bounded fade-back of the same stone.
- Three accepted hits on the current attachment trigger a 0.9 s swat, contact at 0.48 s, followed by 0.35 s stone knockback and recall. Held LMB cannot relaunch: release after readiness, then press again. Pending swats reject further damage until resolved. A stagger/slam recovery can delay the swat.
- Air LMB pull and Shift+LMB push both stagger repeatedly after the existing resistance expires. Fixed release/repress rearming and close-body aiming. HUD shows remaining resistance and the swat/return state.
- Armor sheds at 18/12/6 of 24 health. Six separated plates expose textured inner stone; the chest crystal remains intact until death. Death removes the body and produces solid limestone fragments. Debris is cosmetic, non-colliding, capped at 32, and expires after 14 s. Retry restores all armor and removes prior debris.
- Polished limestone drops are three solid, bevelled shapes derived from the already-paid full-resolution source, with fresh UVs and the existing 2K limestone color/normal textures. `make_limestone_debris.py` produces `LimestoneDebris.blend` and its FBX; build-time normalization produces three runtime mesh assets. Stone burst effects use these shapes in this trial. Particle size is assigned on creation, fixing an oversized first-frame cube flash.
- Aggro now acquires within 12 m with clear sight, retains pursuit out to 20 m, grants a 3 s lost-contact grace period, and uses a 22 m home leash. Returning does not heal/reset health. This fixes stopping at the acquisition threshold or when the player passed the old 16 m distance from home. Initial detection includes vertical distance. HUD identifies the deliberate return-home state.

Eight local Blender animation clips now include Swat. The outer model retains 69,999 triangles across 24 body/armor sections, plus six inner sections. No additional Tripo operations or charges: recorded aggregate remains $0.85 of the $3 cap. Detailed verification, current limits and actual Unity captures are recorded in `ArtSource/StoneSentinelTrial/COMBAT_REPORT.md` and `CombatRevision2Review/`.

Final follow-up verification: 105 automated combat checks and the existing Earth/Air save contracts passed; native input confirmed swat/recast, repeated wind and pursuit beyond the old cutoff. All 386 protected files match their baseline hashes. Polished debris shares mesh assets and settles using actual vertices. The previously documented native Unity shutdown access violation remains unresolved; verification completed before that exit failure. See the report for evidence and limits.

## Stone Sentinel playable combat trial — 2026-10-19

Jonathan approved the visual model and selected animations plus Earthstone damage and wind stagger. New isolated build: `Builds/StoneSentinel_Combat/StoneSentinelCombat.exe`. Opens paused; Resume begins the encounter. The ordinary Visual Preview and static review executables remain available.

WASD/mouse move and aim; Shift sprint; Space jump/double-jump glide; Q switches Earth/Air; Tab equips/stows. Earth LMB summons, then hold LMB to launch/repeat strikes; RMB recalls. Air held LMB or Shift+LMB focuses wind within 8 m to stagger. Esc pauses/retries; Enter retries after defeat. Leave the slam circle, leap, or interrupt with wind. Session-only health and automatic recovery; no resource penalty, saved combat state or construction editing.

Scene/prefab/scripts: `Assets/StoneSentinelCombat/`. Blender source: `ArtSource/StoneSentinelTrial/StoneSentinelCombat.blend`; offline animation/export: `animate_sentinel.py`. Seven skeletal clips, 69,999-triangle skinned mesh, shared approved Sentinel textures/material. Build with `Skywright.Editor.SentinelCombatBuild.Build`. Do not regenerate the island. The Animation engine module is explicitly included. Gameplay hooks activate only when a SentinelCombat scene owner exists.

Aggregate paid cost: 60 generation + 25 rig credits = $0.85; 215 API credits remain, no pending charge. No purchased animation or second model. Details, tests and limitations: `ArtSource/StoneSentinelTrial/COMBAT_REPORT.md`; ledger: `combat-cost-ledger.json`. `-combatqa` enables a build-local state snapshot; normal play does not write it. `-combatverify <absolute-folder>` runs deterministic checks and animation-pose captures, then exits. These are distinct from real-input playtesting. Await Jonathan's assessment before expanding the encounter.

## Stone Sentinel visual review — 2026-10-19

Separate executable: `Builds/StoneSentinel_Trial/StoneSentinel.exe`. Separate scene/prefab/materials/models: `Assets/StoneSentinelTrial/`. It uses the existing island and MountainEnvironment lighting, has no SkywrightGame or persistence owner, and never loads or writes a gameplay slot. The ordinary Visual Preview remains the gameplay build.

RMB drag orbits, wheel zooms, 1–6 select views, Space starts/stops turntable, H hides the interface, Esc exits. 60 FPS focused / 15 unfocused. This original viewer remains static. Jonathan approved its model; the authorized playable continuation is described above.

Editable source: `ArtSource/StoneSentinelTrial/StoneSentinel.blend`; original Tripo GLB and exact generation settings are preserved beside it. Offline preparation: `refine_sentinel.py`. Unity build: `Skywright.SentinelReview.Editor.SentinelTrialBuild.Build`. Do not run historical island authoring for this trial. Cost: one generation, 60 credits ($0.60), of the authorized $3 maximum. See `ArtSource/StoneSentinelTrial/README.md` and `cost-ledger.json` for provenance and limits. Actual Unity review images are in `ArtSource/StoneSentinelTrial/Review/`; private build/capture/preservation evidence is under ignored `Evidence/PlaytestFollowup/StoneSentinel/`.

## Current M1.10.2 rock contact and dome follow-up

Working folder: `C:\Users\jsayl\Desktop\UnityProjects\Skywright`. Current executable remains `Builds/Visual_Preview/Skywright.exe` through the existing Visual Preview shortcut. See `M1_10_2_ROCK_DOME_REPORT.md`.

- The earlier seating pass was insufficient. All 68 scenery outcrops now orient their LOD/collision presentation to the current terrain slope, countering inherited mountain tilts without moving resource roots. The scan's surrounding ground sheet is buried against the actual terrain collider. All three LODs are checked. The two original outcrops use the same seating, with enlarged visual/collision presentation so the exposed body is reachable by Earthstone before terrain collision.
- Dome lines fade near the poles and when narrower than a pixel, removing the reproduced overhead sunburst while retaining the rim and shoreline reveal. This is a concrete visual correction, **not confirmation that Jonathan's intermittent bright disc is fixed**. He reports it during stationary camera turns as well as sprinting. Runtime diagnosis found one camera, one directional light, no SRP flare component, and the custom AeroSky shader (which draws no sun disc).
- Ordinary launches keep all rendering effects. Optional `-visualqa -rendercheck` enables F6 rendering-only comparisons: normal, layer-2 effects hidden, post-processing disabled, flat sky. These modes do not change gameplay or saves and are absent from the ordinary UI.
- Rebuild with `Skywright.Editor.PlaytestFollowupBuild.VerifyAndBuild`. To deliberately re-seat rocks, use `Skywright.Editor.RockFlashBuild.AuthorAndBuild`; the earlier `PlaytestFollowupBuild.AuthorAndBuild` also calls the corrected seating after rebaking the field. Do not run old terrain authoring casually.
- Previous M1.10.1 executable is retained in `Builds/Visual_M1_10_1_Backup`. Private evidence and save backups: `Evidence/PlaytestFollowup/RockFlash/`. M1.10.1 controls, frame caps and save rules below remain current. Stop for Jonathan's visual playtest.

## Current M1.10.1 playtest follow-up

Working folder: `C:\Users\jsayl\Desktop\UnityProjects\Skywright`. Launch `Builds/Visual_Preview/Skywright.exe` through the existing Visual Preview shortcut, or with `-visualpreview`. See `M1_10_1_PLAYTEST_REPORT.md` for measurements, input evidence and remaining review.

This supersedes the M1.10 controls and field clipping below:

- Air: hold LMB suction, hold Shift+LMB push, plain RMB grab/release. Switching Shift while holding LMB changes the stream. Menus/tool changes interrupt it and require release/repress.
- Earth: hold LMB to repeat a bound strike at the existing animation cadence. Release stops subsequent strikes; RMB recalls. Destruction, recall and pause disarm repetition. A newly prepared stone requires a fresh press to launch. Terrain ricochets; explicit resources, crates, furniture and built pieces can bind. Unbreakable pieces remain undamaged.
- Visual Earth tuning: 26 m/s, 13 m separation budget with early braking, 26 m cumulative outbound path, 3 s active flight, 130 m/s² recall acceleration. Stationary-player casts measured up to 12.93 m total separation, including the return arc; the outbound braking point was about 10.3–10.6 m. Moving players and obstacles can change the return path.
- Double jump is personal leap/glide everywhere. Crossing a field does not capture glide into flight. R explicitly starts/cancels field flight; a saved explicit flight can restore, a saved glide cannot. Full-attunement Shift flight remains 10.5 m/s; sprint/glide remains 7 m/s.
- Field radii are (94,73,94), with horizontal stages 23.5/47/70.5/94 m. The baked 256-direction shoreline reveals the lower shell gradually beyond the actual land footprint. Full coverage encloses all home-landform vertices and excludes the neighbor. Flight uses the same reveal profile. Explicit supported flight can descend below the old -25 m recovery floor; unsupported falling still recovers.
- Seventy limestone presentations were seated using their visual geometry, including the connected ground skirt. Resource roots/IDs and construction transforms remain stable. The two original outcrops now use baked visual/collision prefabs.
- Wind ribbons fade near the camera and soften when their projected width is excessive. This corrects a reproduced broad pale-band flash; confirmation of Jonathan's exact ground/flight flash remains part of playtest.
- Default frame policy: gameplay 60 FPS, menus/palette/help/anchor panel 30, unfocused 15; VSync remains off. No global visual-quality reduction. The optional `-profilepreview` writes timing CSV beside the executable; `-profile120` permits a diagnostic 120 FPS comparison, and `-noqaoutput` suppresses QA serialization without changing the isolated slot. Ordinary launches do not enable the observer.

Use `Skywright.Editor.PlaytestFollowupBuild.VerifyAndBuild` for the current contract-checked build. `AuthorAndBuild` additionally rebakes shoreline/shell and seats the limestone presentations; use deliberately when editing the terrain. Do not run historical terrain authoring workflows casually.

The preceding playable build is retained at `Builds/Visual_M1_10_Backup`; the instrumented old-behavior baseline is `Builds/Visual_M1_10_Profile`. Private evidence and save backups are in ignored `Evidence/PlaytestFollowup/`. Preserve personal slots and use `-visualqa` for testing. The previously known native shutdown crash still occurs intermittently; it is not fixed here. Stop for Jonathan's gameplay/visual/audio review.

## Historical M1.10 Tutorial Wind preview
Executable: `Builds/Visual_Preview/Skywright.exe` with `-visualpreview` (existing desktop shortcut). Isolated testing adds `-visualqa`. Working folder: `C:\Users\jsayl\Desktop\UnityProjects\Skywright`.

This section supersedes the historical M1.9 tuning below. The latest request authorizes one working wind-stream journey; airships and broader island progression remain out of scope. See `M1_10_TUTORIAL_WIND_REPORT.md` for evidence and remaining review. Native exit failures reproduced in both this build and the untouched M1.9 backup after shutdown; cause unresolved, saved progression reloads correctly. Audio listening review remains outstanding.

- Home land area is 24.8% of M1.9, around 135 × 108 m, with the original flat 38 m home clearing. One neighboring island has a collidable landing; other distant islands remain scenery.
- One fixed anchor at (12,0,-20), identity `tutorial-island-anchor`. The Visual kit has 13 pieces, excluding anchors; the fixed anchor cannot be copied, deleted or Air-manipulated.
- Upper-hemisphere dome, final radii (91,72.8,91), horizontal stages 22.75/45.5/68.25/91 m; island-relative quarter-radius stages, with 2.4 s offering and 3.2 s eased expansion. Costs remain 6/12/18 Aether, 6/12/18 wood, 4/8/12 stone. Top-right HUD shows current/needed supplies.
- Suction: 8 m acquisition; Aether/drop retention 12 m and 65° half-angle (props 9 m/30°). Aether acquisition half-angle 18°, pursuit up to 14 m/s, 1.05 m capture, 0.65 s aim-loss grace. Clear line of sight and swept movement required. Eligible props and drops shimmer. Wood/stone drops award their existing ledger once.
- Distinct second Space press activates glide outside a field or flight inside. R remains available. Space/Ctrl rise/descend; Shift grants 10.5 m/s flight only at full attunement. Sprint/glide remains 7 m/s. Motion eases at 12 m/s²; edge gusts and wind audio fade with movement.
- Full attunement enables the wind ring at (58,4,0), carrying the player to (151,17.12,0). Travel reaches 14 m/s (2× ground sprint), owns movement, and permits mouse look. Move away and re-enter the destination ring to return. Pause freezes travel; save during travel settles safely at the home entrance; recovery cancels it.
- Earthstone: runtime Visual tuning 20 m/s, 18 m outbound recall trigger and 36 m cumulative outbound path, existing 3 s flight budget. Original Elemental tuning stays unchanged. Solid objects can bind; furniture/buildings ding without damage. Trees remain five hits; limestone eight. Trees fall with bounded cosmetic rotation, preserving scale.
- Only the two identified tutorial cargo crates break: three hits, animated fragments, six wood each. Depletion and collection persist; crate recovery cannot revive them.
- Construction hints: MMB deletes in build mode, Shift+MMB selects aimed piece type, Alt disables snapping. F5 saves; Escape menu retains unsaved-change warnings.

### M1.10 saves and builds
Personal slot remains `visual-preview.json`; isolated slot remains `verification-visual.json` in `%USERPROFILE%/AppData/LocalLow/Skywright/Skywright`. Schema 1 now writes `worldLayout=tutorial-v1`; Air version 3 stores crate damage/depletion/collection.

Existing progression and explicit resource IDs remain. Earlier player-placed anchors are retired in favor of the fixed identity; anchors were free trial pieces, so there is no refund or cost loss. Their earned island ledger transfers intact. Other construction keeps its IDs/transforms. Older-layout construction beyond the preserved 38 m clearing is rejected before replacing the active world. Source saves remain unchanged; `Builds/Visual_M1_9_Backup` remains playable for those layouts. Unknown layouts and malformed ledgers are rejected.

Use `Skywright.Editor.TutorialWindBuild.VerifyAndBuild` for the current contract-checked rebuild. `RefreshAnchorAndBuild` reauthors only Aether configuration; `AuthorAndBuild` deliberately rebuilds this tutorial terrain and catalog. Do not invoke historical full authoring workflows. Evidence, audio samples and protected save copies live in ignored `Evidence/TutorialWind/` and must stay private.

## Historical M1.9 Visual Preview
Executable: Builds/Visual_Preview/Skywright.exe
Desktop shortcut: Skywright Visual Preview (launches -visualpreview).
Scene: Assets/Scenes/FirstHome.unity. Older milestone sections below are historical where this section supersedes them.

- Air: hold LMB suction; hold Shift + RMB push; plain RMB prop grab/release. Wheel adjusts held distance and Shift+wheel height as before.
- Q switches Earth/Air; Tab equips/stows the selected wand; R personal lift/hover; Space rises and Ctrl descends inside the active field.
- E while aiming at the active anchor opens its growth panel. Exact costs and balances are shown; Feed performs one atomic payment and closes the panel.
- Construction: F equip/stow; B or construction RMB opens kit; distinct RMB closes kit into construction. LMB place, MMB remove, Shift+MMB copy type, wheel rotate, Shift+wheel reach, Alt free placement, Shift+Tab named snap mode. Do not press Alt+Tab; Windows uses that shortcut.
- F5 explicit save. Pause/focus loss/tool changes stop streams; uncollected bubbles return to their authored positions without an award. Resume requires releasing/re-pressing the stream button.
- Earth remains free, one reusable stone, 5-hit trees/8-hit limestone; outbound 18 m/s, 20 m separation, 40 m path, 3 s active-flight budgets. Existing local yield collection remains.

### Aether tuning and permanent upgrades
Ordinary asset: Assets/Resources/CompactIsland/Aether.asset. Fifty-six individually identified bubbles; 36 required. Catch range 16 m, retained range 17 m; initial half-angle 14 degrees, retained 24 degrees; clear line of sight and swept movement required. Bubble top speed 9 m/s; eligible props 3 m/s divided by size weight, acceleration 9 m/s2, brake distance 2.2 m plus bounds radius. Push tuning remains in Resources/AirMagic/Tuning.asset.
Costs below are per upgrade, not cumulative:

| Stage | Horizontal radius / vertical extent | Aether | Wood | Stone |
| --- | --- | --- | --- | --- |
| Workshop | 12 m / -2 to +10 m at anchor | 0 | 0 | 0 |
| Garden | 25 m / -2 to +26 m | 6 | 6 | 4 |
| Settlement | 55 m / -2 to +64 m | 12 | 12 | 8 |
| Island | fixed ellipsoid, center (-32,30,58), radii (145,82,121) | 18 | 18 | 12 |

Early fields are ellipsoids, so the listed horizontal radius is their widest section, not a cylinder. Total cost: 36 Aether, 36 Wood, 24 Stone. Radius and center interpolate over 0.9 seconds, with identical gameplay and visible bounds. Final field is authored for the home island, not neighboring islands. Route endpoints (68,17,-4) and (215,39,-70) define a visual-only current.

Only one placed anchor is active. Preserve its explicit ID while it exists; if removed, choose a remaining anchor deterministically by stable ID. If none remains, keep progression and resources but disable support until a new anchor exists. Extras do not grant fields, refunds or completion. Final attunement remains island-owned.

### Save compatibility
Visual user slot: %USERPROFILE%/AppData/LocalLow/Skywright/Skywright/visual-preview.json.
Isolated -visualqa slot: verification-visual.json. Keep personal slots separate.
Save schema remains 1 with optional worldLayout=compact-v1; Air state version 2 adds Aether ledger. Earlier Air v1 restores an empty ledger without changing wood/stone. Explicit bubble IDs, balance, tier and primary anchor are persisted. Reject unknown/duplicate IDs, impossible tiers or imbalance: collected count must equal current balance plus Aether spent on completed stages.
In-transit bubbles are never awarded or serialized; interruption safely returns them. No resource reservation/refund migration.
All 131 prior scenery resource IDs remain, although remote roots were deliberately reauthored onto the smaller island. Original 15 Earth trial node roots remain.
Historical saves with construction beyond the protected 38 m home radius are rejected before replacing the current world. Their source files stay untouched. Use Builds/Visual_M1_8_2_Backup for those layouts; do not call this a full-world migration. Current compact saves can build elsewhere normally.

### Authoring and builds
Assets/Skywright/Editor/CompactIslandAuthor.cs is the editable offline landform/layout source. It writes ordinary meshes in Resources/CompactIsland and Resources/MountainAero/Island.prefab; nothing generates terrain at runtime.
AetherAuthor.cs authors the explicit bubble catalog. Preserve IDs when editing placements; never regenerate identities or reorder the catalog as a shortcut.
FinishedMeadowAuthor.HarvestTree preserves source trunk geometry/normals at uniform scale .36. Runtime harvesting capsule is radius .17 m, height 3.3 m, center (.015,1.65,0). Thin trunks require more accurate aiming than the old .3 m capsule. Scar fitting uses the natural bark surface; unsuitable hits use transient feedback.
Use Skywright.Editor.AetherSaveChecks.VerifyAndBuild for a contract-checked build; Skywright.Editor.MountainAeroAuthor.Build for a plain rebuild. Do not run the older full authoring entry points; they regenerate the previous large layout.
Existing Blender source ArtSource/FinishedMeadow/Landforms.blend and exported buttresses are reused. No new Blender session or new third-party downloads were needed in M1.9.
Daylight: sun 36/-48 degrees, intensity 1.35, fixed exposure +.1, shadow distance 110 m/four cascades, subtle fog .00115. High SMAA; far clip 1600 m. Evaluate future edits in the six documented gameplay views.

## Project
Machine: MashPC
Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
Unity: 6000.4.7f1
Pipeline: Universal Render Pipeline 17.4.0
Input: Unity Input System 1.19.0, keyboard/mouse
Playable scene: Assets/Scenes/FirstHome.unity
Previous Elemental executable: Builds/Elemental_Preview/Skywright.exe
Original First Home executable: Builds/Windows/Skywright.exe

Open the project with the specified Unity editor. Open FirstHome and press Play.
The scene contains the SkywrightGame bootstrap. The handcrafted world is created at runtime; an empty Edit Mode view is expected. Nine M1.5 sample pieces instantiate authored prefab assets; stairs, post, beam and bench still use the M1 generator pending Checkpoint B.
Do not run the editor build script after hand-editing the playable scene without reviewing it: the current build method regenerates the bootstrap scene.

## Controls
- WASD move; mouse look; Shift sprint; Space jump.
- Tab equips/stows the selected wand; Q switches Earth/Air. G is unbound.
- B opens/closes the illustrated construction palette. A distinct RMB in the palette closes it into construction with the selected piece and safely stows magic.
- F equips/puts away the selected piece.
- 1–9 select the first nine structural pieces; [ and ] cycle the kit (14 pieces in the Elemental preview, including the wind anchor).
- Left click places; right click opens the kit; middle click removes the highlighted piece. Shift + middle click selects its type without removal.
- Mouse wheel rotates in 45-degree steps; Q/E no longer rotate.
- Hold Alt for free placement; Shift + Tab cycles Auto snap and named attachment points for the selected piece. The HUD shows the selected point, target point and a contact marker.
- Shift + middle mouse click selects the type of the built piece under the crosshair, even with construction put away. It does not duplicate an instance or its save ID.
- Shift + mouse wheel adjusts reach by 0.5 m per notch, from 2 to 12 metres, without rotating.
- F5 saves; Escape opens the menu.
- Controls menu includes mouse sensitivity.
- Recover position returns safely to the island. Falling below the island also recovers.
- Menus prevent movement and construction. Closing/selecting a menu has a short input guard.

## Placement rules
Four-metre decks, three-metre walls, two half-roof slopes per four-metre room. Posts are 2.6 metres tall to fit beneath a deck at the next three-metre level; crossbeams snap with an endpoint over the post.
Wall/railing connections face inward. Roof slopes rise inward from wall tops; gables close the remaining ends. Roofs also join at their ridge and side edges. Eave / bottom mode can continue a slope upward.
Stairs rise three metres over approximately four metres. Their pivot is beneath the high end.
Stairs place by their low foot on terrain. Auto snap can attach either landing to a deck; Shift + Tab chooses Bottom landing or Top landing explicitly. Buried stair landings are rejected. Attach a deck to the upper landing to continue building.
Decks can extend from existing decks without ground below. Posts are aesthetic; no collapse or support-weight simulation.
Free placement still requires a surface hit. Snap connections can extend over empty space.
Rotation is solved around the selected local contact (target minus rotated source anchor), so beam endpoints remain attached while rotating. Auto prioritizes contacts on the directly aimed piece; explicit modes allow its more distant anchors within reach.
Intentional joints have a small overlap tolerance. Substantial intersections and player entrapment are rejected.
Demolition is individual; no chain destruction. Creative play has unlimited materials without inventory costs. The isolated Earthcasting trial charges wood only for decks, as described below.

## Persistence
User slot: %USERPROFILE%\AppData\LocalLow\Skywright\Skywright\home.json
Verification slots: same directory, verification-m15.json with -qa; verification-snapping.json with -snapqa. The original M1 executable still uses verification.json.
Backup: each slot's .bak file, created on replacement.
Bundled demo: Assets/StreamingAssets/demonstration.json, copied into the build.
Loading the demonstration does not overwrite the user slot unless the player explicitly saves it.

Schema v1 stores stable piece ID, type ID, parent ID, position and rotation, player position, yaw/pitch, and UTC timestamp.
All current pieces belong to home-island. Their stored transforms are local to the construction root.
Read and validate the full save before staging its replacement. Write to a temporary file, validate, then replace with a backup.
No autosave on exit. Save explicitly; the menu reports unsaved construction changes.
Logs: normal Player.log under the same LocalLow directory, unless overridden with -logFile.
-qa additionally writes read-only diagnostic state to qa-state.json. The game never reads commands from it.

## Code
- Kit.cs: stable type IDs, prefab instantiation and legacy geometry fallback.
- PieceCatalog.cs / Resources/SkywrightCatalog.asset: explicit prefab definitions and named local anchors.
- SnapRules.cs: compatible anchor pairs and defaults for unconverted pieces; no save-transform migration.
- BuiltPiece.cs: runtime instance ID and stable type ID on the functional prefab root.
- World.cs: authored island, scenery, lighting and atmosphere.
- Builder.cs: targeting, snap candidates, preview, collision validation, placement/removal.
- SkywrightGame.cs: first-person controller, input ownership, session/save operations.
- SkywrightUI.cs: menu, palette, HUD and geometry-derived thumbnails.
- Persistence.cs: save schema and validated disk operations.
- Editor/BuildSkywright.cs: launch scene, URP settings, shader resource assets, Windows build.

The Lit and sky materials in Assets/Resources retain shaders in standalone builds. Do not replace them with runtime-only Shader.Find calls.
Before M1.8, no third-party art or audio had been imported. M1.8 asset provenance is recorded below. Geometry, textures, thumbnails and placement sound were created for this project. Unity packages retain their own licenses.
No airships, NPCs or multiplayer are implemented. Isolated harvesting and air-magic trials are documented below; they do not add survival meters or a broad resource grind.

## Rebuilding
Close the standalone player before rebuilding.
Unity batch invocation:
Unity.exe -batchmode -quit -projectPath C:\Users\jsayl\Desktop\UnityProjects\Skywright -executeMethod Skywright.Editor.BuildSkywright.Build -logFile <log-path>
Alternatively use Unity's Build Settings with FirstHome as the only enabled scene.
Keep Evidence logs and milestone reports. Do not commit Library, Temp, Builds or machine-generated caches.

## M1.5 Checkpoint A preview
Executable: Builds/M1_5_Preview/Skywright.exe
Launch with -artpreview, or use the desktop Skywright Art Preview shortcut.
FirstHome remains the only playable launch scene. This preview uses art-preview.json, isolated from home.json.
Choose Explore art sample to load the separate 23-piece authored review layout. This layout was composed from the M1 demonstration; it is not claimed to have been constructed entirely through input.
The original M1 executable and demonstration remain available.
-qa uses verification-m15.json and emits read-only qa-state.json, m15-loaded-audit.json and PERF log samples. None accepts game commands.
Without -artpreview the preview executable uses the normal home slot; use the shortcut for isolated review.

## Sample asset authoring workflow
Assets/Resources/SkywrightCatalog.asset contains nine definitions: floor, wall, window, doorway, roof, gable, railing, planter, lantern.
Each definition uses an explicit typeId (save identity), display name, hint, nominal dimensions, origin description, prefab and named local anchors. Legacy connections remain serialized for historical compatibility but are not used by the revised solver.
Functional prefabs live in Assets/Resources/SkywrightArt. The root has BuiltPiece, an identity transform, and two child branches:
- Collision — M1 contract: original deliberately retained box/convex-mesh collider layout; no renderers.
- Visuals — replace freely: authored render meshes/materials and lantern light; no colliders.
Edit or replace Visuals in Prefab Mode. Keep root, Collision and catalog anchors unchanged for visual-only work.
Never derive saved identities from asset paths, object names or child order. Runtime object IDs remain unique; load restores saved IDs.
Meshes and materials are ordinary Unity assets. The editor authoring utility created the initial originals and combined visual geometry by material. Runtime does not regenerate converted pieces.
Do NOT regenerate art after manual prefab/model edits unless intentionally replacing those edits.
Skywright/Author Checkpoint A sample is explicit regeneration; Skywright/Build Checkpoint A preview only validates and builds current assets.
Build method Skywright.Editor.BuildArtPreview.Build preserves the existing FirstHome scene and original Builds/Windows executable.
BuildArtPreview compares the converted collider contracts to the M1 generator and fails if they differ.
Catalog anchors drive the revised solver for converted pieces. SnapRules.Defaults supplies matching local anchors for unconverted stairs, posts, beams and benches without converting their art. AuthorArtSample writes the default anchors when explicitly regenerating the sample. Edit authored anchors directly for manual kit work. Anchor changes affect future placements only; saved transforms are never resnapped.
The live HUD and -snapqa diagnostics expose snap mode, contact label, target and attachment error. QA files remain read-only outputs. Mouse wheel, Tab / Shift+Tab and piece selection are blocked in menus; only left clicks activate menu/palette buttons.
Thumbnails derive from actual prefab visual meshes; collision geometry is excluded.
Glass is a visual treatment. Window openings and railing gaps retain their M1 collision behavior; this avoids silently introducing new blockers in old builds.
ArtEnvironment enables only the preview garden/lighting/sky when -artpreview is present. Existing island terrain collision and layout remain unchanged.
SampleGarden is a separate non-colliding decorative treatment. The shallow bounded pool has no water interaction.
AeroSky is a static gradient/cloud shader; no weather simulation. Existing distant islands/cloud geometry remain provisional.
## Surface clarity
PC_Renderer keeps the ScreenSpaceAmbientOcclusion feature disabled: its noise caused mottling/shimmer on the sample's rounded white frames. Normal lighting and shadows remain enabled; shadow bias is unchanged.
Closed glass boxes use backface culling, and sample mesh interiors avoid coplanar backing/trim faces. Preserve these separations when replacing visuals.
Previously misplaced pieces are never moved on load. Re-place them to adopt corrected snapping. See the 2026-10-18 follow-up in M1_5_ART_DIRECTION_REPORT.md for input-test evidence and limits.


## Historical M1.6 Earthcasting trial
Separate Windows preview: Builds/Earthcasting_Preview/Skywright.exe. Its sibling earthcasting.preview marker defaults this build to the isolated trial, including direct double-click launch. The desktop Earthcasting shortcut also passes -earthcasting. -earthqa uses verification-earthcasting.json; normal trial uses earthcasting-trial.json. FirstHome remains the only launch scene. Older Windows and M1_5_Preview builds remain untouched.
G equips/puts away Earthcasting; LMB casts when equipped; E collects a nearby aimed loose-stone pile. F, palette selection, structural hotkeys, and Shift+MMB return to construction. Casting owns input until its short recovery finishes. Menus pause trial effects and collection. Existing wheel/Shift-wheel/Alt/Tab controls remain.
Trial only: decks cost 6 wood; other pieces are free trial pieces. A tree yields 12 wood, a loose pile 18 stone, an outcrop 12 stone after two casts. No mana, hunger, stamina, rest or equipment durability. Creative saves retain unlimited materials.
Wood and broken-outcrop yield wait at their stable resource node. Enter a 3.5 m collection radius with a clear line to the bundle to begin its collection animation. After collection starts it completes, even if the player backs away. No tree/wood levitation is implemented. Two static cargo crates are present for future air-magic testing.

### Earth assets and tuning
Assets/Resources/Earthcasting/Tuning.asset (EarthConfig) references the authored tree, stones, wood bundle, casting focus, cargo crate, materials and seven original audio clips. Tune range, collection radius, cast phases, fall duration, resource yields and deck cost here. Do not use asset filenames as node save IDs.
EarthTrial owns the explicit resource layout, targeting, cast state, damage, resource ledger and proximity collection. EarthNode separates the deliberate harvesting collider, crown visual, stump and transient fracture/bundle display. EarthEffects provides bounded cosmetic fragments, ripples and audio voices; effects never own inventory rewards. No terrain deformation or tree rigidbody simulation.
Skywright/Author Earthcasting assets is explicit regeneration. Preserve manually edited prefabs: ordinary rebuilds use Skywright.Editor.BuildEarthcasting.Build, which validates existing building collider contracts and builds only Earthcasting_Preview. AuthorAndBuild regenerates Earthcasting assets and should be used only intentionally. It does not regenerate the existing M1.5 kit.
All new assets and synthesized sound layers are original project work; no third-party packs, purchases or runtime services were added.

### Trial persistence
SaveData remains schema v1 with an explicit harvestingTrial boolean (absent/false for legacy creative saves), EarthState (its own version 1) when that flag is true, and optional PieceRecord.paidWood (legacy/default 0). JsonUtility creates default inline class objects for absent/null fields, so never infer trial mode from EarthState nullness after deserialization. Persistence.Read clears that placeholder for creative saves. Creative saves may not be loaded into a resource trial or vice versa. Paid amounts are stored per instance; free/demo/legacy pieces refund 0. Only decks can carry a wood payment.
EarthState records inventory and explicit node IDs/kinds, damage, local fracture location, depletion and collection completion. Validation checks the exact expected layout, unique identities and consistent depletion. No scene rebuild respawns depleted resources.
Saving settles a committed cast into one impact or a blocked/spent outcome, completes active falls, and awards already-started collections exactly once. Yield that has not entered collection remains waiting. Animation trajectories are not serialized. Explicit saving and protected load failures remain; no autosave is added.
The trial reset button resets only the current trial session; it replaces that slot only when explicitly saved. Resource changes set the unsaved-state indicator.
Future air direction and the crate-first restriction are recorded in PROJECT_BRIEF.md; M2 and air abilities remain separate milestones.


## Historical Elemental preview — M1.7
Builds/Elemental_Preview/Skywright.exe, scene Assets/Scenes/FirstHome.unity. Desktop shortcut: Skywright Elemental Preview. Its elemental.preview marker makes direct launches isolated; -elemental is explicit. -airqa selects verification-elemental.json; normal play uses elemental-trial.json. -snapqa -artpreview bypasses Elemental mode for legacy checks. Previous executables and their controls remain historical as documented above.
Q selects Earth/Air. G equips/puts away the selected magic. Earth LMB casts at any aimed surface or max-range sky endpoint when ground emergence is safe; only valid breakables receive the blue target circle and damage. One stone per committed cast; E still collects nearby loose stones.
Air LMB lifts/releases a targeted demonstration crate. Wheel adjusts carry distance, Shift+wheel adjusts height. RMB emits a bounded gust; the held crate ignores gust force. F, a kit choice, number keys or Shift+MMB switch to construction and safely release a held crate. Normal construction wheel/Shift-wheel/Alt/Tab remain unchanged.
R toggles anchor hover / descent, or uses a short personal lift outside anchor range. While hovering, Space rises and C descends. Building selection and palette use preserve hover. Menus freeze movement, props and effects and block actions; resuming has the existing input guard. Grounding on a crate does not re-arm personal lift. Leaving the approximately 10 m radius or 8 m height, or removing the supporting anchor, causes gentle descent. Multiple anchors retain current eligible support before choosing the nearest alternative.
The menu's Recover demonstration crates action finds clear supported ground. Falling crates automatically return to their last safe supported resting position or a clear trial fallback. A new/imported crate overlapping scenery is recovered once after validation. No crate inventory is implemented.

### Air authoring and physics
AirMagic/Tuning.asset is an AirConfig with lift/grab/hover/gust limits and prefab/material/audio references. Assets/Resources/AirMagic holds the original ring mesh, pearl/timber/aqua materials, transparent ribbon material, four original synthesized sounds, compact focus, explicit liftable-crate prefab and wind-anchor prefab. AirMagic/Catalog.asset contains only the new wind-anchor definition; the existing nine-piece SkywrightCatalog remains untouched.
The wind-anchor type ID is explicit. Its root/Collision are functional; Visuals can be replaced independently. Its Bottom anchor and 1 x 1.5 x 1 m collision are local contracts. Existing piece definitions, dimensions, origins, colliders and snaps are unchanged. Palette thumbnails derive from the actual prefab geometry.
AirCrate performs bounded swept-box movement with deliberate collision. AirTrial owns manipulation/gust/player support state; the CharacterController remains the player collision owner. No Rigidbody or render animation competes for crate transforms. AirEffects is presentation only: ten 48-point ribbons, fourteen cosmetic leaves and four sound voices. No new packages/services or ambient-occlusion changes.
Skywright.Editor.AuthorAirMagic.Author explicitly regenerates ONLY new air assets; preserve manual prefab edits by using ordinary Skywright.Editor.BuildElemental.Build. AuthorAndBuild is for intentional regeneration. BuildElemental validates existing collider contracts and both Earth/Air serializer checks, then builds the existing FirstHome scene into Elemental_Preview.

### Elemental persistence
SaveData v1 adds an explicit elementalTrial discriminator and versioned AirState. Missing/false markers clear JsonUtility's default inline-object placeholders. AirState stores two explicit crate IDs, upright transforms, last safe positions, lift-used state and the supporting anchor ID/descent state. Wind anchors use normal piece records and IDs.
F5 settles held/moving crates downward through swept collision, or recovers them when no support exists; it cancels gust animation. Existing committed Earth casts/falls/started collection settle once. Waiting wood remains waiting. Hover/airborne saves resume into gentle descent with lift already spent; the player may intentionally reactivate an eligible anchor. Animation frames and held-tool state are not saved. No autosave on exit.
Import Earthcasting save — copy reads and validates the old Earthcasting slot, stages its builds and exact inventory/node/paid state, adds the trial crates and one stable starter anchor, and marks the session unsaved. F5 writes only the Elemental slot. Failed validation leaves the current world and source save intact. Import is available only when that source slot exists; QA uses an isolated import-earthcasting.json fixture.
See M1_7_AIR_MAGIC_REPORT.md for performed tests and remaining limitations. Do not start the next milestone automatically.


## Historical Elemental preview — M1.7.1
Same project, FirstHome scene, Elemental_Preview executable, desktop shortcut and isolated save slots. M1.7 build is retained in Builds/Elemental_M1_7_Backup. See M1_7_1_INTERACTION_POLISH_REPORT.md for evidence and limitations.

M1.7.1 inputs (historical): Q selects Earth/Air; G equips; Earth LMB launches one guided stone and mouse aim steers until impact/expiry. Air HOLD LMB sustains wind; RMB lifts/releases crates, bench, planter or lantern. Wheel carry distance; Shift+wheel carry height. R lifts/hover/lands; Space rises; either Ctrl descends. C has no hover action. Construction LMB places, RMB opens kit, MMB removes, Shift+MMB copies type only. B/F/numbers/brackets/wheel/Shift-wheel/Alt/Tab and F5/Escape retain their existing roles.

EarthConfig: range and total path length 240 m, speed 22 m/s, turning 150 degrees/s, flight lifetime 12 s. Curved motion uses at most 10 ms substeps and swept sphere collision. Existing 0.42 s emerge and 0.16 s suspension remain. Mouse aim selects a point, not a locked target. One active cast; extra clicks spend nothing. Pickup reach and 3.5 m automatic collection remain local. Save, tool change and focus loss cancel unresolved flight without damage/refund; menus pause it. Resolved falls and already-started collections settle once. This supersedes earlier cast-on-save settlement descriptions.

AirConfig: sustained wind acceleration 10 m/s², capped horizontal speed 3 m/s, range 7 m. Solid line-of-sight and swept motion remain authoritative. Held objects and objects supporting the player ignore wind. Menus/focus/tool changes stop wind; release the button before restarting after interruption. Original looping wind and synthesized bubble-pop pickup/placement audio have bounded voice counts; pickup sounds rate-limit at 90 ms. BubbleAudio holds the original generated-pop synthesis.

AirCrate is now the explicit movable adapter for the two demo crates and whitelisted BuiltPiece types bench/planter/lantern. Furniture attaches at runtime without replacing its root, ID, collider layout or paidWood. Unmoved furniture does not simulate gravity on load. Activated furniture uses deliberate local collision bounds; visual orbit bounds are measured separately from mesh bounds. Furniture persists solely in ordinary piece records. AirState continues to serialize only the two demo crates. Saving settles activated movable objects through swept collision or safe recovery. No structural parenting, duplication or chain destruction is introduced.

AnchorVolume is shared between rendering and hover tests. The previous cylinder (radius 10 m, relative height -2..8 m) is now an ellipsoid centered 3 m above the anchor, radii (10,5,10) m. Maximum horizontal reach remains 10 m at center height; it narrows to about 8 m at the anchor's ground level and to zero at the top/bottom. Eligibility uses player feet. This is an intentional volume-shape change; saved anchor/build transforms do not migrate. AnchorBubbles creates visual-only shells outside piece hierarchies, including preview shells; it never adds colliders or affects palette thumbnails. AnchorBubble.shader is a small URP transparent Fresnel shader, no refraction.

R inside support adds the same initial lift profile as personal lift when lift is available. Leaving support arms automatic recapture before landing; deliberate R cancellation disarms recapture. Re-entering support arrests descent and resumes hover. Descent is 3 m/s with acceleration smoothing. Airborne saves preserve liftUsed and resume descending; they do not automatically grant another lift. Existing CharacterController collision remains authoritative.

Build using Skywright.Editor.BuildElemental.Build. PolishSetup.Build is the explicit one-time tuning/material setup plus build; it does not regenerate existing art. AuthorAirMagic remains explicit art regeneration, not an ordinary rebuild step. Main HUD labels use sufficient measured text height and padding; transient notices do not cover palette/help. Current UI verified at the resolutions recorded in the report.


## Historical Elemental preview — M1.7.2
Same FirstHome scene, Elemental preview executable, desktop shortcut and isolated save slots. Previous full executable retained at Builds/Elemental_M1_7_1_Backup. See M1_7_2_EARTH_RECALL_AUDIO_REPORT.md.

Air: LMB lifts/releases existing eligible objects. Hold RMB for continuous wind, release to stop. Carry wheel/Shift+wheel and hover R/Space/Ctrl remain unchanged. Construction RMB still opens the kit, MMB removes, Shift+MMB copies type. Q selects Earth/Air; G equips.

Earth: first distinct LMB prepares one stone above a named hold socket on the focus; second launches; mouse guides; LMB during guided flight releases to gravity. LMB during preparation, ballistic flight or return is ignored. Hold RMB from guided/ballistic flight to recall. Releasing RMB before arrival resumes the prior flight mode. An intact returned stone becomes prepared and can launch again without another charge. Recall collides with the world and does not navigate around obstacles.

EarthState v1 adds preparedStone (absent defaults false). Preparation commits one stone and sets this paid reservation; launch clears the prepared flag, successful recall restores it without adding inventory. Impact/expiry/cancellation destroys airborne stone without refund. Stowing or saving preparation resolves it to a prepared reservation. Prepared stones survive tool changes and save/restart; their view model is recreated when equipped. Saving, tool changes and focus loss cancel unresolved airborne flight. Menus pause flight and consume input; interrupted recall requires release before restarting. Saved animations/trajectories are not required. Earlier paid pieces/node identities are unchanged.

EarthConfig retains outbound 22 m/s, 150 degrees/s, 240 m/12 s. Gravity 9.81 m/s². Recall uses maximum 26 m/s, acceleration 65 m/s², a separate cumulative 280 m/12 s return budget per launch, and distance-based braking. Toggling recall never resets either budget. Prepared time has no expiry. Every actual relaunch starts fresh outbound/return budgets. Sweeps use 0.18 m radius and <=10 ms curved-motion steps. Held visual scale is 0.28, flight 0.72; return scales down within 3 m. Blocked hold sockets hide the prepared model and reject launches until clear.

BubbleAudio generates original damped water-like resonators with soft attack, low harmonic weight, restrained 4% pitch curvature and a quiet secondary bubble. Pickup: 0.18 s/470 Hz, volume 0.50. Placement: 0.23 s/365 Hz, volume 0.55. Random pitch variation is only ±1.8%; clustered pickup rate limit remains 90 ms. Change these constants for later listening feedback.

SoftWindAudio generates one 32-second original filtered-noise loop (22.05 kHz, two-second wrap crossfade, slow periodic modulation). One continuously playing source uses sample-level smoothed gain; target volume 0.35 in AirConfig. Menus/focus/tool changes target zero, avoiding hard cuts. The one source stays silent when inactive, so there is no repeated attack or restarted noise phrase. 65 ms exponential gain smoothing reaches near silence within about 0.45 s. The callback does not allocate or call Unity APIs.

Ordinary rebuild: Skywright.Editor.BuildElemental.Build. Explicit setup/export/rebuild: Skywright.Editor.RecallSetup.Build; this updates only new tuning fields and exports original synthesis WAVs into Evidence/EarthRecall. It does not regenerate kit artwork. Prepared/missing-field serialization checks are included in the existing build gate.


## Historical Elemental preview — M1.7.3 Returning Earthstone
Executable and shortcut remain Elemental_Preview / Skywright Elemental Preview, FirstHome scene. Previous full M1.7.2 build: Builds/Elemental_M1_7_2_Backup. Personal elemental-trial.json and isolated -airqa verification-elemental.json retain their existing paths. See M1_7_3_RETURNING_EARTHSTONE_REPORT.md for actual verification and limitations. Earlier controls above are historical.

Earth: G equips; first LMB summons one free stone, next LMB launches. Mouse aim guides only outbound flight. A valid breakable impact applies one hit and binds the surviving target. LMB while waiting strikes that same target once, regardless of crosshair aim. The existing precision-fracture bonus remains: a tree normally takes an initial hit and one precise repeat strike. Clicking during summon, flight, rebound, active strike, ricochet or return does not queue actions. RMB CLICK recalls from flight, waiting, strike and ricochet; release never cancels return. Broken targets, missed-shot limits and bounded ricochets automatically start the same return. There is no ballistic-release control or resource cost. Retained inventory is labelled Stone; E pickup and local yields remain independent resource interactions.

Air remains LMB lift/release and held RMB wind; construction remains LMB place, RMB kit, MMB remove, Shift+MMB copy type. Hover remains R / Space / Ctrl. Tree and wood collection remains local at 3.5m. No remotely granted harvest rewards.

One owner holds a single projectile reference. Prepared bool is now free ready state, not a paid reservation. Earlier true values restore ready, missing fields default false; inventory is neither deducted nor refunded. Ready state survives tool stowing and save/restart. Menus pause motion and consume input. Interrupted summoning resolves to ready without inventing an impact. Save, tool change or focus loss dismisses unresolved flight/attachment/strike/return and restores summon availability; resolved harvesting remains. No trajectory or target attachment is serialized. No autosave.

EarthConfig: outbound 22m/s, steering 150deg/s, 240m or 12s active outbound budget; waiting does not consume flight time. Collision is a deliberate 0.31m sphere swept in <=10ms curved-motion steps, independent of visual meshes. Target contact and normal are local to the explicit EarthNode. Rebound speed 4m/s; waiting offset 0.55m beyond the collision radius; strike anticipation 0.16s, speed12m/s, acceleration90m/s², 1.2s missing-contact safety limit. Each strike needs actual swept contact with the owned target; an unrelated blocker cancels the attack into return.

Ricochet: actual surface-normal reflection, speed retention0.88, at most3 impacts or2s after the first bounce, with the outbound budget also retained. Sweep stops8mm short and attempts a further15mm swept separation. Return never damages breakables. Recall max26m/s, acceleration65m/s², 280m/12s per launch, distance-based braking; arrival within0.10m at speed<2m/s and a clear swept path. Blocked return waits up to0.8s for a clear path, then visibly shrinks/dismisses over0.3s and returns to free summon availability. No pathfinding or disabled collision.

Rock: Assets/Resources/ReturningEarthstone/Earthstone.prefab has an identity root and replaceable Visuals child, with an authored original continuous limestone mesh, color texture, normal texture and standard URP Lit material. EarthTrial owns collision; the prefab deliberately has no gameplay collider or Rigidbody. Fixed identity scale through launch, waiting and return; only dismissal shrinks it. The hold socket is offset to leave the crosshair clear; blocked sockets hide the ready visual and reject launch until clear. Do not change collision radius merely when replacing visuals; fit replacements within it or deliberately review the consequence.

Ordinary rebuild: Skywright.Editor.BuildElemental.Build. Explicit rock-only authoring and build: Skywright.Editor.ReturningStoneSetup.Build. This regeneration is intentional and will overwrite the new rock artwork, not the construction kit or original Earth assets. Authoring checks winding, degenerate faces, radial bounds and signed volume, and stores the mesh/material/prefab as ordinary Unity assets. No new packages, third-party assets or services.


## Historical Elemental preview — M1.7.4 Spell Feel, Hover Capture and Aero UI
Same FirstHome scene, Elemental_Preview executable, shortcut and isolated save slots. Previous complete build: Builds/Elemental_M1_7_3_Backup. Report: M1_7_4_SPELL_FEEL_UI_REPORT.md. Prior milestone controls/tuning above are historical.

Tab equips/stows the currently selected Earth/Air wand, including from construction. Q selects the element. G is unbound. Shift+Tab cycles named construction snap points without equipping magic. Menus/palette/help consume gameplay inputs. Other construction controls remain: F toggle, RMB kit, MMB individual removal, Shift+MMB copy type, wheel rotate, Shift+wheel reach, Alt free placement. Air remains LMB lift/release and held RMB wind; hover R, Space up, Ctrl down.

Earth LMB summons one free reusable stone, the next distinct press launches; mouse guidance remains available after unaffected-surface reflections. A surviving breakable owns the waiting stone; each subsequent valid LMB performs one contact-based strike. RMB click recalls. Extra clicks during preparation/flight/strike/return/catch do not queue actions. Fresh trees require exactly5 impacts, limestone outcrops8; precision cues no longer multiply damage. Yields and local3.5m collection remain. No ammo, mana, cooldown or remote collection was added.

EarthConfig / Resources/Earthcasting/Tuning.asset: treeHits5, outcropHits8; flight22m/s, turn150deg/s, maxPlayerDistance40m, accumulated maxTravelDistance80m, maxFlightTime6s. Shared budgets survive bounces; waiting pauses flight time but enforces player distance. Three total ricochets remain a safety cap. Contact reflection is immediately guided, with a short geometric separation constraint released once the stone clears the face; no unsteerable ricochet state/timer. The time guard is redundant at uninterrupted22m/s under the80m path cap, but protects slower future tuning. No tuning change should silently reinterpret saved damage thresholds.

Return uses moving socket velocity plus bounded relative approach, max26m/s and65m/s² acceleration. Catch begins within0.30m with relative speed under3m/s and clear collision; a0.12s continuous socket-relative blend preserves position, orientation and size. Prepared rotation continues from the caught pose; launch uses the actual held position. Return collision,0.8s blocked wait,0.3s dismissal and12s/280m return safety remain. No projectile navigation or collision disabling. Return, tool/save/focus interruption and single-stone accounting retain M1.7.3's free-availability behavior.

Summoning is0.10s emergence +0.20s travel, or0.30s compact gathering when more than2.5m above earth. EarthSurface is an explicit component on the colliding home island; source queries never infer eligibility from names. The first solid surface below the player must be eligible earth within30m, with clear hold space and a swept source-to-wand path. Decks, roofs, crates and open sky reject. High airborne gathering begins near the wand after source validation; the visible solid stone never teleports upward from the ground. Low summons retain ground emergence.

EarthState schema2; SaveData and AirState remain1. Persistence.Read validates legacy Earth v1 before copying/upgrading it. Trees1→2,2→3,depleted3→5; outcrops1→4,depleted2→8; zero remains zero. Inventory, prepared flag, fractures, node IDs, collected/depleted flags and pieces are unchanged. Invalid legacy state remains invalid. Upgrade is idempotent, source files are not rewritten on load, and creative saves retain no Earth state. A later explicit save writes v2. Existing earlier absent preparedStone fields still default false.

AnchorVolume remains the authoritative feet-based ellipsoid shared with the bubble. Airborne personal-lift/descent entry immediately gains supported hover and zeroes vertical descent while preserving horizontal velocity. Leaving resumes descent; reentry catches again. Explicit R cancel suppresses capture within that same anchor until leaving it or intentionally reactivating hover. No added upward impulse on capture. Entry checks run every movement frame; no projectile/player teleport or support-volume resize.

UI stays IMGUI, with reusable original64x64 nine-slice rounded textures, pearl/aqua translucency, dark high-contrast text, compact contextual HUD, and short notice fade. No blur, package or art-framework replacement. Placement-only BubbleAudio.Placement now generates an original0.19s rounded watery resonator,760→705Hz gentle curvature with a quiet540Hz satellite, volume0.42, ±1.2% variation, three bounded voices and70ms minimum interval. Pickup synthesis/volume/pitch/rate limit and continuous wind audio are unchanged. Failed placement never plays success audio. Exact generated WAVs are exported in Evidence/SpellFeelUI.

Ordinary rebuild: Skywright.Editor.BuildElemental.Build. M1.7.4 verification/export build: Skywright.Editor.SpellFeelBuild.Build (migration checks and WAV exports, then ordinary build). Do not use historical PolishSetup/RecallSetup to rebuild this pass: those explicit old setup tools can restore old tuning. ReturningStoneSetup is still intentional rock regeneration only; its obsolete ricochetTime assignment was removed to compile against the current config. No artwork was regenerated.

## Current Elemental preview — M1.7.5 Compact Glass HUD and Earthstone Tuning
Same FirstHome scene, Elemental_Preview executable, desktop shortcut and isolated slots. Previous complete build: Builds/Elemental_M1_7_4_Backup. Report: M1_7_5_COMPACT_HUD_FLIGHT_REPORT.md.

EarthConfig defaults and Resources/Earthcasting/Tuning.asset now agree: outbound18m/s, steering150deg/s, player range20m, accumulated outbound path40m, active flight3s. Waiting pauses time; attached targets enforce20m separation. Steering, reflection and player movement never reset budgets. Current full-speed path cap is reached before3s. Recall26m/s, acceleration65m/s², catch0.12s, summon0.30s, strikes12m/s/rebound4m/s and harvesting5/8 remain unchanged.

Distinct RMB in the building palette explicitly stows magic, retains selection, enables construction, locks the cursor, consumes that frame and guards input for0.2s. Held RMB cannot reopen the palette because opening requires a fresh press. B/Escape retain their previous close behavior. Ordinary IMGUI buttons still reject non-left clicks; pause/help do not use the shortcut.

Only the bottom HUD changes style:600 logical pixels wide at1600x900,16px bottom margin,18%-opacity aqua/pearl rounded background; white18px primary/16px secondary text with a restrained dark1px outline. Normal two-line height is61px; construction adds a measured feedback line (about81px, more if wrapping is needed). Content is measured rather than clipped. Essential contextual controls remain here; complete bindings remain in Controls. Menu, palette, notices and top panels retain their established presentation.

Ordinary rebuild remains Skywright.Editor.BuildElemental.Build. Do not use old setup/regeneration commands to rebuild. Save schemas, inventory, IDs, geometry, sound and art assets are unchanged. Evidence/CompactHUD contains original input observations, screenshots, logs and private preservation copies; it is excluded from Git.

## Historical visual sample (superseded by M1.8.1 below) — M1.8 Checkpoint A
Launch Desktop/Skywright Visual Preview, then Explore visual sample. Executable Builds/Visual_Preview/Skywright.exe; scene Assets/Scenes/FirstHome.unity. Shortcut arguments: -visualpreview -screen-fullscreen 0 -screen-width 1600 -screen-height 900. visual.preview beside the executable also isolates direct launches. The existing Elemental executable/shortcut is unchanged.

Personal visual slot: %USERPROFILE%\AppData\LocalLow\Skywright\Skywright\visual-preview.json. QA flag -visualqa uses verification-visual.json. Both are separate from elemental-trial.json and home.json. F5 saves explicitly; Reset visual sample reloads the bundled 22-piece natural-sample.json into the current session and does not replace disk until saved. Earlier Elemental saves can be tested only as backed-up copies in the verification slot. Existing Earth v1->v2 migration still applies.

All controls remain M1.7.5: Tab current wand; Q element; Earth LMB summon/launch/strike, RMB recall; Air LMB grab/release, held RMB wind; R hover, Space up, Ctrl down. Construction F, B palette, RMB open/close into construction, MMB remove, Shift+MMB copy type, wheel45deg, Shift+wheel reach, Shift+Tab snap mode, Alt free placement.

Preview catalog: Assets/Resources/NaturalAero/Catalog.asset. PieceCatalog chooses it only in visual mode. Normal mode retains SkywrightCatalog and AirMagic/Catalog. Functional roots/colliders/anchors/IDs are unchanged. Only cloned visual meshes receive metre-scaled birch UVs/materials. Stairs, post, beam, bench and anchor are not converted.
NaturalPreview.Environment adds the bounded garden and lighting, and applies a geographically blended grass material without changing the terrain collider. NaturalPreview.Tree substitutes rendered geometry under the existing crown for explicit earth-trial-tree-0; harvesting identity/collider/fall state remain unchanged. New sample vegetation/cliff/pool are decorative and non-colliding.

Authoring: ordinary Unity textures, materials, meshes, prefabs and explicit catalogs. Edit Visuals in prefab mode; preserve root/Collision and catalog contracts. NaturalAeroAuthor creates the original canopy/trunk/cliff/grass assets; its output is persistent, not generated during player gameplay. Tree has near and half-density distant foliage LODs; fern groups cull at distance. The pool and decorative tree are static visual treatments.

Build existing assets with Skywright.Editor.BuildNaturalPreview.Build. Explicitly regenerate only this sample with Skywright.Editor.BuildNaturalPreview.AuthorAndBuild; do not run regeneration after manual art changes without reviewing them. The build compares all nine sample construction contracts with the original catalog and checks Earth/Air saves plus exact M1.7.5 tuning. FirstHome is preserved. To run this mode in the editor, launch the project/editor with -visualpreview; ordinary editor Play keeps the established mode.

Third-party asset source/license/import notes: Assets/NaturalAero/ASSET_PROVENANCE.md. All selected downloadable sources are free CC0; no runtime online dependency. The former no-third-party-art statement describes pre-M1.8 history.
Evidence/NaturalAero is ignored and includes private preservation copies, actual input observations, screenshots and logs. Report: M1_8_NATURAL_AERO_VISUAL_REPORT.md. Stop at Checkpoint A for visual approval.

## Current visual preview: M1.8.1 Mountain Island
The prior M1.8 art sample was rejected. This representative replacement is awaiting Jonathan's visual review; do not start full island detailing, full kit conversion or M2.

Launch **Skywright Visual Preview** on the desktop, then **Explore visual sample**. Executable: Builds/Visual_Preview/Skywright.exe. Scene: Assets/Scenes/FirstHome.unity. Existing Elemental preview is unchanged; complete prior visual build is Builds/Visual_M1_8_Backup. The launch scene and isolated visual-preview.json / verification-visual.json slots remain unchanged. The bundled natural-sample.json is now a 30-piece, eight-metre-wide cottage/deck. Reset changes the session only; F5 explicitly saves.

Current controls remain: WASD, mouse look, Shift sprint, Space jump; F construction; B kit; construction RMB opens kit and palette RMB returns to building; LMB place; MMB remove; Shift+MMB select aimed piece type; wheel rotates 45 degrees; Shift+wheel changes reach; Shift+Tab cycles snap points; Alt free placement. Tab equips/stows the selected wand, Q selects Earth/Air. Earth LMB summon/launch/strike and RMB recall. Air LMB lift/release and held RMB gust. R hover, Space rise, Ctrl descend. F5 save, Escape menu.

### Assets and rendering
MountainEnvironment instantiates Resources/MountainAero/Island only in visual mode. It applies the dedicated MountainPipeline/MountainRenderer and Daylight profile. Normal game modes retain their existing environment. The camera far plane is 3000 m only in visual mode. MountainGround supports Forward+ lighting and uses slope/height/path blending plus triplanar limestone. The fixed island is authored into 107 collidable mesh chunks and a closed cliff underside. It is not a runtime procedural-world system.

Preserved home terrain: radius 38 m at y=0. Outline approximately 1150 x 780 m; highest sampled ridge 239.55 m. A 131.24 m foothill route ends near (-110,36,98). Detailed routes across the high ridge and neighboring scenery are not part of this pass.
Construction terrain checks now use the existing EarthSurface component instead of the literal object name "Home island", preserving stair burial checks and floor/post ground allowances across the new chunks. Functional piece dimensions, pivots, anchors, colliders, save IDs and magic tuning are unchanged.
All ten existing harvesting trees use the new rendered broadleaf model under their original crown pivot. Their original identities, harvesting collider, five-hit damage and collection radius remain. Decorative woodland trees are scenery, not additional harvesting nodes.

The nine-piece NaturalAero catalog remains the construction palette. No entire-kit conversion. Ordinary visual replacement still happens beneath the stable functional root; preserve Collision and metadata. New scenery lives in Assets/Resources/MountainAero and Assets/MountainAero. Models use explicit FBX exports and three LODs; editable packed .blend sources are in ArtSource/MountainIsland. Source/license records: Assets/MountainAero/ASSET_PROVENANCE.md.

### Build and regeneration
**Build existing assets with Skywright.Editor.MountainAeroAuthor.Build.** This wrapper temporarily selects the preview pipeline during the build so URP retains its SSAO resources, then restores project pipeline settings in finally. Do not use the historical NaturalAeroAuthor or direct BuildNaturalPreview entry point for the current visual preview.
Explicit regeneration: Skywright.Editor.MountainAeroAuthor.AuthorAndBuild recreates the authored environment and bundled demonstration. It may overwrite manual changes to generated assets: inspect before running it. Height, outline, fixed route, scenic placement and useful material settings are in that editor author; resulting meshes/prefabs are directly editable Unity assets.
Blender preparation scripts are offline tools, not runtime dependencies. Download source files remain in ignored Evidence/MountainAero/sources. Prepared .blend files contain packed textures. Actual LOD triangle counts are recorded in ArtSource/MountainIsland/model-budgets.json.
The build retains original construction-contract comparisons, save checks and exact Earth tuning gates. Evidence/MountainAero contains original gameplay screenshots, input observations, recordings, logs and private preservation fixtures; it is ignored by Git. Full verification and limitations: M1_8_1_MOUNTAIN_ISLAND_VISUAL_REPORT.md.


## Current visual preview: M1.8.2 Finished Meadow
Use the existing desktop **Skywright Visual Preview** shortcut and choose **Explore visual sample**. Executable remains Builds/Visual_Preview/Skywright.exe; scene remains Assets/Scenes/FirstHome.unity. Personal visual-preview.json and isolated verification-visual.json remain separate. The bundled 30-piece cottage is unchanged. The full M1.8.1 executable is backed up in Builds/Visual_M1_8_1_Backup. Elemental_Preview is unchanged.

Controls are unchanged from M1.8.1: Tab wand; Q element; Earth LMB summon/launch/strike and RMB recall; Air LMB lift/release and held RMB gust; R hover, Space rise and Ctrl descend. F construction, B palette, RMB opens/closes the palette into building; LMB place, MMB remove, Shift+MMB copy type, wheel rotate, Shift+wheel reach, Shift+Tab snap point, Alt free placement. F5 explicit save; Escape menu/recovery.

### Current authoring workflow
- Build existing assets using Skywright.Editor.MountainAeroAuthor.Build. This retains the preview URP resource-stripping safeguard and restores original project pipeline settings.
- Explicit full sample regeneration is now Skywright.Editor.FinishedMeadowAuthor.AuthorAndBuild. It runs the fixed terrain author, prepares selected owned assets, bakes harvesting visuals, then dresses the bounded sample and builds. It does NOT replace the bundled sample or personal saves. Inspect before regenerating; manual edits to generated assets can be overwritten.
- Do not run the historical MountainAeroAuthor.AuthorAndBuild for M1.8.2: it recreates the rejected grass and old distant island layout before the new dressing stage.
- FinishedMeadowAuthor.PolishExistingAndBuild refreshes only the authored cliff rock formations and scenic surface masks, then builds. Ordinary rebuilds need only MountainAeroAuthor.Build.
- Mesh/prefab outputs: Assets/Resources/FinishedMeadow and the existing Resources/MountainAero/Island.prefab. Original owned sources and GUIDs: Assets/ALP_Assets. Separate adapted materials/prefabs preserve source assets apart from two documented URP shader compatibility edits.
- Blender source: ArtSource/FinishedMeadow/Landforms.blend, author_landforms.py, landform-budgets.json. Explicit FBX exports: Assets/FinishedMeadow/Models. Do not depend on automatic .blend importing.

The grass uses curved textured clumps grouped into 6 m tiles, broad density variation, clear build/path space, softly upward-biased normals, a restrained world-phased breeze and a 55–82 m dithered distance fade. Meshes are authored offline, not generated during gameplay. The densest dressing spans roughly 60 x 60 m including transition edges around the requested central review area; the rest of the island remains comparatively sparse. Selected owned vegetation retains LODs/crossfades and shares materials. No vendor controller, demo scene, quality preset or global lighting setup was installed.

The ground shader blends meadow, soil, woodland litter and limestone. Keep tiled terrain textures separate from scan UV atlases: the Coast Rocks atlas belongs on its rock models, not tiled across mountains. The protected home radius 38 m remains y=0. Mountain crest smoothing applies beyond 80 m and avoids the route corridor. Neighboring scenery uses closed quad-grid meshes with several lower masses and embedded limestone formations, not a central underside fan.

### Harvesting visual contracts
NaturalPreview.Tree instantiates FinishedMeadow/HarvestTree below the existing crown pivot. All ten existing stable roots and harvesting capsules remain unchanged (radius 0.3 m, height 3.6 m, local center x=0.03/y=1.8). The owned maple is baked to player scale, with its lower bark contour fitted to that envelope. This intentionally corrects visual contact without changing saved roots or collision. The old standing-tree stump platform is disabled; a smaller stump is shown after felling and on depleted-node reload.

TreeSurfacePresentation holds a readable render-only bark surface. A saved fracture point is projected to that surface to create a small conforming scar; it neither adds a collider nor changes damage. The scar follows the crown and clears with it. The original bright block-shaped scar remains only in historical, non-visual modes. Aim/target cues remain spell feedback and are separate from physical bark damage.

Source/license details: Assets/FinishedMeadow/ASSET_PROVENANCE.md. Nature Package and its included ALP/DE shaders are commercial Asset Store content, not CC0. The two selected shader sources were updated to the current URP cluster-light keyword and one duplicate soft-shadow pragma removed. No purchases or runtime online dependency.

Evidence/FinishedMeadow is ignored and contains private save backups, input observations, screenshots, recordings, baseline/build/runtime logs and package evaluation data. Do not publish it or the repository. Final verification and limitations: M1_8_2_FINISHED_MEADOW_CLIFFS_REPORT.md.

### M1.8.2 scenery harvesting addition (user-requested)
All 63 placed scenery trees and 68 placed limestone rocks now use the existing Earthstone harvesting loop (five/eight hits). Structural cliff faces, island landforms, distant island formations, shrubs, and grass remain environment geometry. The original two handmade limestone outcrops now display the existing Poly Haven Coast Rocks 05 asset, fitted to their established collision footprint. Their identities and rewards are unchanged.

SceneryResource components on the Island prefab contain explicit serialized GUID identities. Earth state v3 preserves the original 15 node records and adds 131 scenery records; v1/v2 imports are copied and upgraded without changing inventory. Do not delete/reassign GUIDs or move resource roots without a deliberate compatibility decision. Reset and reload restore active/depleted visuals and collision. Newly authored decorative trees use a compact collapse to avoid sweeping oversized crowns through buildings; original harvesting trees retain their established fall animation. Scenery trees have transient impact feedback; fitted persistent bark scars are currently specific to the original harvesting maples.

SceneryHarvestAuthor.ConvertAndBuild applies markers to the current authored island and builds. FinishedMeadowAuthor.AuthorAndBuild captures existing GUIDs before deterministic regeneration and reattaches them by kind/position; inspect the catalog before accepting any changed layout. Runtime identity never derives from names, positions, or hierarchy order. Visual Preview v3 saves are forward-only with respect to the old binary: use the protected earlier save when running the backed-up M1.8.1 build. The separate Elemental slot/build is unchanged.

Final original-outcrop correction: the scanned rock is shallower than the old cluster. Its underside is grounded at the saved root minus 0.035 m; its LOD2 mesh supplies collision instead of the old harvesting proxy. The horizontal footprint stays bounded by the old proxy. Saved roots, node IDs, damage and yield are unchanged, but exact impact contact now follows the scan. This deliberately supersedes the earlier preserved-collider description for these two resource outcrops only.
