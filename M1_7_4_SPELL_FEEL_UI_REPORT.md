# M1.7.4 — Spell Feel, Hover Capture and Aero UI

Completed 2026-10-18 on MashPC (2026-10-19 UTC). Parent checkpoint: 7fc045d. Stop for Jonathan's playtest. No M2, airships, new elements or world-art conversion.

## Launch
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Elemental_Preview\Skywright.exe
- Scene: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Assets\Scenes\FirstHome.unity
- Desktop shortcut: Skywright Elemental Preview. Verified target and arguments: -elemental -screen-fullscreen 0 -screen-width 1600 -screen-height 900.
- Unity6000.4.7f1 / URP17.4.0. Previous complete executable preserved in Builds/Elemental_M1_7_3_Backup.
- Normal slot remains elemental-trial.json; testing used only -airqa / verification-elemental.json after preserving the live personal session.

## Changes
Tab equips/stows the selected wand; Shift+Tab cycles construction snap points. G is unbound. Q still selects Earth/Air. Construction RMB kit, MMB removal, Shift+MMB type copy, wheel rotation, Shift+wheel reach and Alt free placement remain.

Trees require five valid contacts and limestone outcrops eight, including the first projectile impact. Precision no longer adds a damage unit. The stone still binds a surviving breakable, waits and performs one strike per valid click; extra active-strike clicks do not queue damage. Local yields, fractures, IDs, paid building records and inventory remain.

Unaffected contacts reflect from the hit normal and immediately remain mouse-guided. A short geometric surface-separation constraint prevents steering into the same face before clearing it; it expires after clearance. There is no unsteerable ricochet state or two-second bounce phase. Shared flight budgets and a three-bounce cap prevent endless shots.

Reproduced the reported moving-player bug before editing: the stone remained Returning while travelling at approximately4m/s with the walking player, ignored LMB, then caught when the player stopped. The previous catch required world speed below2m/s. Return now includes moving-socket velocity, tests relative arrival speed and finishes with a short continuous attachment blend. The held orientation continues from the caught pose. Launch starts at the actual held position, without recreating the projectile or resetting its scale.

Summoning takes approximately0.30s, with distinct presses and no early-click queue. Airborne source checks use the explicit EarthSurface component and first solid surface below the player; names and appearance are not used. Higher summons gather near the wand after validating a clear earth-to-wand path; the solid rock does not teleport upward from the ground.

Personal lift/descent now captures anchor support on entry, arrests downward velocity and preserves horizontal movement. Falling back into support captures again without another R press. Explicit R cancellation suppresses automatic capture in that same anchor until leaving or deliberately reactivating it. Rendering and movement retain the shared AnchorVolume ellipsoid.

The existing IMGUI now uses reusable rounded nine-slice pearl/aqua panels, dark text, compact contextual HUD, quieter notices and short notice fade. The permanent duplicate Wind controls panel is removed. Menu, palette, help and errors retain their functions; there is no blur or new UI package.

Successful placement alone uses a new original light water-bubble synthesis and three bounded audio voices. Pickup synthesis and its factory are source-identical to7fc045d; pickup volume, variation and rate limit are unchanged. Wind audio is unchanged. Exact WAV exports are available for listening review.

## Current controls
| Context | Input | Action |
|---|---|---|
| Magic | Tab / Q | Equip or stow / select Earth or Air |
| Earth | LMB | Summon; launch when ready; strike while waiting at a target |
| Earth in flight | Mouse / RMB click | Guide, including after ricochet / recall |
| Air | LMB / held RMB | Lift or release / continuous wind |
| Air carrying | Wheel / Shift+wheel | Distance / height |
| Hover | R / Space / Ctrl | Toggle / rise / descend |
| Construction | F / B or RMB | Toggle / open kit |
| Construction | LMB / MMB / Shift+MMB | Place / remove / select looked-at type |
| Construction | Shift+Tab | Cycle named snap points |
| Construction | Wheel / Shift+wheel / held Alt | Rotate45 degrees / reach / free placement |
| General | F5 / Escape | Explicit save / menu |

## Final tuning
| Setting | Value |
|---|---|
| Trees / limestone | 5 / 8 hits; one damage per valid impact |
| Outbound speed / steering | 22m/s /150deg/s |
| Range from current player | 40m; checked during flight and target attachment |
| Accumulated outbound path / active time | 80m /6s; no reset on bounce or steering |
| Ricochet safety | Three impacts maximum; reflected speed retention0.88; guidance resumes immediately |
| Summon | 0.10s emergence +0.20s travel; compact high summon0.30s |
| Earth search / compact threshold | 30m downward / more than2.5m above earth |
| Recall | 26m/s max,65m/s² acceleration; existing280m/12s budget |
| Catch | Within0.30m, relative speed under3m/s, clear swept path;0.12s attachment blend |
| Blocked return | Wait0.8s, visibly dismiss over0.3s; summon availability restored |
| Collision | Existing0.31m swept sphere, curved-motion steps at most10ms |
| Strike / rebound | Existing0.16s anticipation,12m/s strike,90m/s² acceleration,4m/s rebound |
| Collection | Existing3.5m local radius and line-of-sight rule |
| Placement audio | Original0.19s resonator;760→705Hz gentle curvature, quiet540Hz satellite, volume0.42, ±1.2% pitch, three voices,70ms rate limit |

At continuous22m/s the80m path limit precedes6s. The independent time guard remains useful for lower future speeds but was not independently triggered through normal input with current tuning.

## Save compatibility
SaveData and AirState remain version1. EarthState is version2. A valid v1 state is validated before making an upgraded copy; the source is not mutated or rewritten on load.

| Legacy target | Old damage | New damage |
|---|---:|---:|
| Tree | 0 /1 /2 /depleted3 | 0 /2 /3 /depleted5 |
| Outcrop | 0 /1 /depleted2 | 0 /4 /depleted8 |

Collected/depleted flags, fracture points, IDs, inventory, ready flag and construction records stay intact. No resources are awarded or refunded by migration. Earlier missing preparedStone fields still default false. Creative saves remain separate. Invalid or unsupported data is rejected before world replacement. v2 upgrades are idempotent. Future changes to damage thresholds require an explicit compatibility decision.

## Verification
Actual Win32 keyboard/mouse input drove the standalone player. QA JSON was read-only observation; it never supplied state commands. Tests used protected disk fixtures only for compatibility/failure cases. Automated migration and serializer checks supplement the input tests.

| Check | Result and evidence |
|---|---|
| Original movement bug | Reproduced in before-repro.log and old-moving-catch observation. World velocity about4m/s, still Returning; stopping allowed catch/relaunch. |
| Tab / G / Shift+Tab | Tab toggled from gameplay and construction; G did not equip; Shift+Tab changed snap mode without equipping magic. Menus consumed Tab and mouse input. |
| Free summon / timing | Zero Stone summon succeeded. Logged low summon0.3013s initially and0.3000s subsequently; high summon0.3001s; final obstruction-recovery summon0.3001s. Distinct summon/launch presses retained. |
| Tree count | Recorded damage1,2,3,4 with surviving target; damage5 depleted and automatically returned. |
| Limestone count | Recorded damage1 through7 with surviving target; damage8 depleted and returned. |
| Extra contacts / clicks | Extended waiting added no damage. A second press during anticipation did not queue another hit. Subsequent deliberate presses each added one. |
| Local collection | Wood stayed0 after distant depletion. Moving near the bundle logged collection at3.48m and awarded12 Wood. Outcrop collection logged3.47m and awarded12 Stone. |
| Guided ricochets | Ground and normally placed wall impacts were followed by changed mouse aim and continued Guided state. Recorded two bounces with steering still active. |
| Ricochet into breakable | Final-build ground bounce reached tree3 and entered Waiting, increasing damage1→2 once. See ground-bounce-to-tree and play-final-720.log. |
| Range and path | Sky shot logged distance-limit automatic return. At elevated anchor hover, continuous mouse circling logged path-limit return with accumulated path80m; steering did not reset the budget. |
| Moving catch / relaunch | Walking, sprint-strafing and turning-strafing relaunches reached Guided without stopping. Repeated the original stationary launch→recall→start walking→catch→LMB sequence successfully. A separate hover-moving relaunch also reached Guided. |
| Catch presentation | Recorded approach/catch/relaunch; sampled gameplay frames show continuous approach at player scale. Position/rotation/scale use the same projectile and continuous blend. Subjective smoothness remains for playtest. |
| Airborne earth | Compact summon succeeded at height3.68m above eligible earth. Hovering above open sky and a wooden deck rejected without creating a stone; moving back above earth allowed summoning. |
| Wall clearance | Moving the ready wand close to a wall rejected launch, retained the ready stone and did not duplicate it. |
| Blocked return recovery | Final test walked around the left side of a saved wall, recalled from behind it, logged RETURN_BLOCKED then RETURN_FALLBACK and Idle. Moving onto clear earth allowed another free summon to Prepared. See obstruction-final.log. |
| Anchor upward entry | Personal lift at the spawn boundary immediately logged HOVER_SUPPORT_REGAINED; Space then raised the supported player without another R press. |
| Falling reentry / cancellation | Near anchor center, rising past the upper boundary logged support loss and falling recapture repeatedly. Releasing rise settled near7.99m. R cancellation descended inside the same volume without immediate recapture; deliberate reactivation worked. |
| Air regression | LMB grabbed crate1, next LMB released. Held RMB logged wind targets and stopped on release. Furniture behavior code was unchanged. |
| Building regression | Placed a normal free wall and played placement audio; RMB opened kit. Shift+MMB selected the wall without count change; MMB removed it,9→8. Existing narrow collider gaps can cause a center ray to miss a board seam; aiming at the adjacent board worked. Collider contracts were preserved. |
| Genuine ready restart | Closed executable, restarted and continued: pieces and complete Earth state exact. One crate Y/safe coordinate settled by3.725e-9m; identity and other state retained. |
| Nonzero-resource restart | After actual local harvesting and building removal, closed/restarted again. Pieces, Earth state including12 Stone/12 Wood/ready/partial/depleted records, and complete Air state matched exactly. |
| Legacy in-game load | Unchanged protected M1.7.3 fixture loaded through Continue. Tree1→2,2→3,depleted3→5 matched expected state exactly; construction records exact, inventory/ready/fractures retained. Original fixture hash unchanged. |
| Migration gates | Checked tree/outcrop partial and depleted mappings, collected retention, inventory/ready preservation, source immutability, v2 idempotence and invalid depletion rejection. Existing creative/Earth/Air serializer gates passed. |
| Protected failure | Deliberately invalid QA save was rejected from the menu with a visible error; current9-piece world, prepared state and damage remained. Original QA slot restored afterward. |
| Audio separation | Pickup factory and synthesis source match7fc045d exactly; placement call is separate with bounded voices. Successful placement logged the new cue. Subjective listening is not claimed. |
| UI | Inspected menu, HUD, palette and controls captures at1280×720 and1920×1080, plus1600×900 gameplay. Text fits its panels on bright sky, grass, timber and shaded foliage backgrounds. |
| Logs/build | Final build succeeded, errors=0, Earth/Air serializer gates passed. Inspected player logs had no runtime exception, assertion or shader-error matches; expected invalid-load warning retained as test evidence. |

The first compile caught a removed ricochetTime reference in the explicit old rock-authoring utility; that obsolete assignment was removed. Subsequent builds passed without regenerating artwork. An early harvesting harness read a stale Waiting snapshot before the next input had appeared in QA; it was corrected to wait for the action, and the full counts were rerun successfully. Some early aim/reach attempts missed and are retained in raw observations; their descriptive tags alone are not pass claims.

## Preservation
Initial Git tree was clean at7fc045d. Read project brief, maintenance, M1.7.3 report and actual source/assets/save validation. No applicable AGENTS.md was found in the inspected project/parents.

The user's normal preview was open with unsaved progress. Existing save files and the complete build were backed up first. The live session was explicitly saved through F5 before closing; personal-live-session.json preserves that exact saved result (12 Stone,36 Wood,8 pieces). The personal primary still matches that snapshot by SHA-256. Its pre-pass version remains in saves-before. This intentional live-progress save is the only personal primary change; testing never replaced it.

Fourteen other original save/backup files remain byte-identical, including the restored verification-elemental.json and its .bak. Read-only qa-state.json and m15-loaded-audit.json changed as expected. preservation-before.json / preservation-after.json list hashes; saves-before retains all17 initial JSON/backup/diagnostic files. The legacy source fixture remains byte-identical. Previous builds and sibling projects were not modified. No repository publishing or external service was used.

## Evidence and review material
Folder: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Evidence\SpellFeelUI

- guided-ricochet-moving-relaunch.gif:12s, final-build normal-input wall/ground bounce, steering, recall and moving relaunch.
- airborne-anchor.gif:15s, normal-input personal lift into anchor support and airborne summon; an early relaunch press during Catching was correctly ignored. Successful hover relaunch is separately recorded in input-evidence.jsonl.
- moving-relaunch.gif:10s reproduction of the corrected original movement sequence. Captures are silent10fps GIFs; source frames retained.
- hud-sky-1080.png, hud-grass-1080.png, menu-1080.png, palette-1080.png, controls-1080.png and corresponding720p captures.
- placement-bubble.wav: exact generated placement clip, mono44.1kHz PCM16. pickup-unchanged.wav exported separately for comparison.
- input-evidence.jsonl, compatibility-results.json, migration-contracts.txt, ready-before/after.json, gameplay-final-save.json, nonzero-restart.json, legacy-source/observed/audit.json, protected-load-failure.png.
- build-a.log, build-b.log, build-final.log; before-repro.log, play-a.log, play-final-720.log, play-final-1080.log, legacy-load.log, obstruction-final.log.
- Original input/capture/audit helpers and preservation copies stay local. Evidence and generated builds are excluded from Git; source, current documentation and this report are checkpointed.

## Limits and outstanding review
- No subjective remote audio audition. Please listen to placement-bubble.wav and build several pieces to judge the new cue; collection was deliberately unchanged.
- The independent6s flight guard was code-inspected, not separately triggered because80m is reached first at the current speed. The third-bounce cap, bound-target40m separation and moving-obstacle interception during an attack were not separately driven to their limits in this pass.
- Wall launch and return obstructions were input-tested; a dedicated low-ceiling summon case was not separately reproduced. Earth eligibility/path checks remain conservative and intentionally reject intervening decks/roofs.
- Frame-by-frame anchor entry was tested at normal frame rate, including upward/sideways travel and falling reentry; extreme low-frame-rate traversal and overlapping-anchor cancellation were not stress-tested.
- UI was inspected on the tested backgrounds/resolutions, not ultrawide displays or a newly built fully dark enclosed room. Tiny board-collider seams can still miss a thin interaction ray, as noted above; no geometry contract was silently changed.
- Ordinary1600×900 samples were around8.33–8.35ms at the existing120fps cap. This is not an uncapped benchmark or a controlled performance comparison.
- The rock's artwork and the unconverted construction kit remain as approved previously. This pass changes handling and UI, not the broader art conversion.

## Short playtest
Launch Skywright Elemental Preview and Continue. Use Tab for the wand, Q for Earth/Air, and Shift+Tab while building for snap points. Summon and launch with separate clicks; guide a ground bounce, recall while running, then relaunch without stopping. Strike a fresh tree five times or limestone eight times. Use R outside an anchor, move into its bubble, and try Space/Ctrl plus exit/reentry. Hover over earth and summon. Build several pieces to judge the lighter placement cue and inspect the rounded UI. F5 explicitly saves.

Technical verification does not establish enjoyment or audio/art approval. This milestone stops here for Jonathan's playtest. The local Git checkpoint ID is supplied in the handoff and available through git log -1.
