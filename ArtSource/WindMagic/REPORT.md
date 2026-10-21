# Wind Magic visual overhaul — 2026-10-20

Status: implemented in isolated review builds; awaiting Jonathan's visual/gameplay review.

## Review builds

- Island: `Builds/WindMagic_Preview/Review Wind.cmd` launches `Skywright.exe -visualqa -noqaoutput` in the isolated verification save slot, with diagnostics disabled.
- Sentinel: `Builds/WindMagic_Combat/StoneSentinelCombat.exe`, session-only combat.
- Build method: `Skywright.Editor.WindMagicBuild.Build`. It uses the existing FirstHome and Sentinel scenes and does not regenerate terrain, models, or the encounter.
- The original Visual Preview, Sentinel combat preview and static model viewer are retained.

## Controls

With Air equipped: hold LMB for suction, hold RMB for continuous push, Shift+LMB click to grab an eligible object or release the currently held object. Shift need not stay held. Wheel changes carry distance; Shift+wheel changes height. Q changes element; Tab stows/equips.

Each press owns a gesture until both mouse buttons are released. Pressing Shift during suction does not change the spell. The first wind button wins when another is pressed; simultaneous fresh presses choose the grab chord first, otherwise RMB. Releasing the owning button while the other remains held stops casting and requires neutral. Grab clicks remain consumed even if Shift is released. Both forces are suppressed while controlling an object. Pause, focus loss, stow, element switch and save cancel casting; held objects release safely. Existing cast rearming, object eligibility, swept carrying, distances and movement controls remain.

## Visual implementation

Two prefabs, `Resources/WindMagic/Suction.prefab` and `Push.prefab`, share the new `AeroFlow` shader/material. Suction curls inward with narrowing, tapered pearl/aqua wisps; push accelerates outwards with a shallower curl, a sparse broken pressure crescent and small surface dispersal eddies. Near-hand curls connect the motion to the wand. Actual affected props and incoming Aether can receive short surrounding arcs.

Wisps retain a world-space casting orientation rather than snapping every existing ribbon to the latest aim. They fade when aim diverges, spawn into the new direction, and taper through startup and release. Cosmetic decay retains its original range and never continues force. Ray-segment clipping, depth fade, near-eye fade and projected-width limits address geometry intersections and broad white sheets. The crosshair and target remain visible.

Unity 6000.4.7f1, URP 17.4.0 and Input System 1.19.0 were verified from project files. VFX Graph is not installed. A CPU-generated combined ribbon mesh and URP HLSL shader suit this small bounded effect without package changes. No pipeline migration, global exposure/bloom/lighting/FOV changes, paid operations, texture downloads or distortion were introduced. Each spell has one dynamic mesh with fixed storage for 2,560 vertices / 2,480 triangles, with one shared material. Inactive spell renderers are disabled. Quality/density, width, curvature, turbulence, color, intensity, range/radius scale, startup and fade are exposed on the prefabs. Range is supplied by the spell, rather than defining a separate force system.

Old cast ribbons are retained only as an explicitly selected verification baseline; the existing carry and hover presentation remains in use. The baseline selector is not a normal gameplay control.

## Verification

- 40 automated wind checks passed in the actual island player: press ownership, modifier changes, simultaneous buttons, grab toggle/hold/release, rearming, cancellation, terrain/anchor rejection, obstruction, crate movement, furniture, five movable props, bounded geometry and cleanup. Authored Aether, six-wood crate drops (without duplicate awards), and twelve-stone limestone drops were collected through the existing ledgers.
- All 131 existing Sentinel regression checks passed in the separate combat build, including RMB push and suction stagger/resistance, Earthstone auto-ready/swat/recall, attacks, leap, awakening and armor/death behavior. Existing Earth and Air serialization contracts passed during every successful build.
- Native input confirmed LMB suction, RMB push, Shift during held suction, simultaneous-button ownership, grab persistence after Shift release, suppressed push while carrying, 4.75 m distance adjustment, height adjustment, release/regrab, Tab stow/equip, Q swaps, menu cancellation and focus-loss release. It also confirmed R hover, Shift fast flight with rush=1 while pushing, approximately 60-degree camera turns, and ground sprinting while maintaining suction. The initial missed grab after pushing the crate away is recorded; the grab suite was repeated successfully from fresh positions.
- Successful test players exited cleanly. No managed exceptions or shader errors were found in successful verification/native/tour logs. The visible performance run emitted transient Direct3D 12 presentation warnings when the initially hidden window was shown; these are retained in its log and are not described as a completely warning-free run.

Native input and automated input are distinct. Automated checks inject Unity Input System events in standalone players, and the visual tours use scripted camera movement and inputs. Native tests use the user's previously approved Windows input helper, restricted to the isolated preview process, with focus guards and key/button release on exit. The computer-use API did not expose the running Unity player, so it was not used to claim native-input success.

The native fixture temporarily attunes the island in memory to exercise fast flight; it does not serialize that state. No editor Play Mode test is claimed: live tests were standalone Windows players using the actual gameplay scene and lighting.

## Performance

Measured on an Intel Core i7-14700F / RTX 4070 SUPER. Milliseconds, mean unless indicated. CPU frame timing includes rendering/presentation waits; Air update time is separately measured active CPU work.

| Mode | CPU frame | GPU frame | Air update | Frame p99 | Worst frame |
| --- | ---: | ---: | ---: | ---: | ---: |
| disabled | 4.067 | 4.042 | 0.103 | 4.624 | 4.969 |
| legacy-suction | 4.140 | 4.114 | 0.129 | 4.894 | 6.236 |
| legacy-push | 4.095 | 4.069 | 0.131 | 4.627 | 5.312 |
| suction | 4.061 | 4.036 | 0.576 | 4.590 | 6.010 |
| push | 4.053 | 4.027 | 0.627 | 4.585 | 5.637 |

The new flow itself averaged 0.467 ms CPU for suction and 0.512 ms for push. Total Air update increased by approximately 0.446 / 0.496 ms relative to the corresponding legacy spell. GPU differences were within sequential-run variation (less than 0.09 ms); the data does not justify claiming zero GPU cost or that the new effects make rendering faster. The clearing sustained roughly 246 rendered FPS uncapped, with no sampled frame exceeding 6.24 ms. Normal gameplay retains the existing 60 FPS cap. No quality reduction was needed on this PC; density/quality remain available on the prefabs.

Allocation counting is **unavailable** in this runtime: `GC.GetAllocatedBytesForCurrentThread()` returned zero even for an intentional 8,192-byte calibration allocation. The zero columns in the CSV must not be read as proof of allocation-free execution. New geometry uses preallocated arrays and persistent meshes/materials; counters remained bounded throughout the long casts. Whole-game allocation profiling requires a supported Unity Profiler measurement.

The hidden-window run skipped presentation and was discarded. Use only `Evidence/WindMagic/PerformanceVisible`. The valid run keeps the window visible, uses 1920×1080, unchanged rendering quality, vSync off and uncapped rendering, and resets five eligible props to matched positions each frame. Camera and scene are identical between modes. Each phase has five seconds of warmup; upgraded suction and push each have 60 seconds of measured continuous casting. No recording or per-frame QA-file writes occur during performance sampling. Instrumentation measures total Air update and new flow update separately; it does not establish zero allocations across the whole game.

## Evidence

`ArtSource/WindMagic/` contains actual Unity images and Motion JPEG AVI recordings (1280×720, 15 captured frames per second, silent). Each recording shows startup, sustained casting, changing aim/movement and release. Recordings are rendered gameplay-camera evidence, not concept art or offline renders. The original frame sequences and detailed logs remain private in ignored `Evidence/WindMagic/`.

- `before-suction.jpg` / `.avi`, `before-push.jpg` / `.avi`: old cast visuals.
- `suction.jpg` / `.avi`, `push.jpg` / `.avi`: new cast visuals.
- `grab-release.avi`: toggle grab, carry and release.
- `switch.avi`: suction → push → suction with neutral transitions.
- `surface.avi`: a temporary verification wall and dispersing contact airflow.

## Files

New: `Assets/Skywright/Wind/AirGesture.cs`, `WindFlow.cs`, `WindReview.cs`, `WindReviewGame.cs`, `Editor/WindMagicBuild.cs`, and `Assets/Resources/WindMagic/` shader/material/two prefabs, plus Unity metadata. `ArtSource/WindMagic/pack_recording.py` packages captured frames into AVI without altering their content.

Modified: `AirTrial.cs` (gesture mapping, cancellation and presentation hooks), `AirEffects.cs` (new presentation integration, baseline and opt-in timing), `AetherSystem.cs` (read-only incoming-bubble presentation hook), `SkywrightUI.cs` and `StoneSentinelCombat/CombatGameBridge.cs` (hints), `SentinelVerification.cs` (RMB push input in the existing regression), `PROJECT_BRIEF.md`, `MAINTENANCE.md`, and `.gitignore` (private wind evidence).

## Limits and preservation

All **566 protected files** matched their pre-pass SHA-256 hashes: 21 personal save/diagnostic files and backups, original preview builds, FirstHome scene, authored island prefab and protected settings. Audit: `Evidence/WindMagic/protected-after.json`. No personal save was loaded or written; no scene/terrain/model regeneration ran. User-authored geometry, stable identities, saved transforms and progression format were not changed. Existing unrelated working-tree edits remain. `git diff --check` on changed source/docs is clean; the unchanged MountainPipeline asset retains two pre-existing serialization-whitespace warnings.

Visual approval remains Jonathan's decision. The comparison scene is one representative clearing, not a benchmark of every island location or future combat load. A single input session and sampled recordings cannot prove the absence of every intermittent flash. The historical unrelated sky-disc/shutdown reports are not declared fixed. Cosmetic wrapping is deliberately capped at four actual affected targets. Distortion is omitted for readability and predictable cost. No new throwing, physics powers, creatures or combat mechanics were added.
