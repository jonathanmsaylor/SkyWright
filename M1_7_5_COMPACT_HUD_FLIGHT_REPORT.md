# M1.7.5 — Compact Glass HUD and Earthstone Tuning

2026-10-18 local / 2026-10-19 UTC. Parent checkpoint568faf9. Focused polish; no M2 or unrelated systems.

## Launch
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Executable: Builds/Elemental_Preview/Skywright.exe
- Playable scene: Assets/Scenes/FirstHome.unity
- Desktop: Skywright Elemental Preview; target verified, arguments -elemental -screen-fullscreen 0 -screen-width 1600 -screen-height 900.
- Unity6000.4.7f1, URP17.4.0.
- Previous complete build preserved in Builds/Elemental_M1_7_4_Backup.
- Normal personal slot remains elemental-trial.json. All playtests used -airqa / verification-elemental.json.

## Changes
Bottom HUD only:600 logical pixels wide,16px bottom margin,18%-opacity pearl/aqua glass with a subtle rounded rim. White18px primary and16px secondary lettering has a1px dark outline; no blur/refraction. Normal two-line height is61px. Construction adds a third line (about81px); measured wrapping can increase height for long snapping/error feedback. Full controls remain in Controls. Essential state hints, snap labels, reach when valid and placement/obstruction failures remain visible. Large thumbnails and redundant control lists were removed from this HUD, not from the palette. Menus, top panels, palette cards and notices retain their previous styling. The palette footer now describes its explicit close shortcut.

A distinct RMB press inside the building palette stows magic, retains the selected piece, enables construction, locks the cursor and consumes the frame, followed by a0.2s input guard. Opening still requires a fresh press; holding the closing button cannot reopen or cast. B/Escape retain previous behavior. Non-left clicks still cannot activate ordinary IMGUI buttons. Pause and controls screens do not use the new shortcut.

## Final flight tuning
| Setting | Value |
|---|---|
| Outbound guided speed |18m/s, previously22 |
| Steering |150degrees/s, unchanged |
| Distance from current player |20m, previously40 |
| Accumulated outbound path |40m, previously80 |
| Active outbound flight |3s, previously6 |
| Recall |26m/s max,65m/s² acceleration, unchanged |
| Catch |0.30m approach, relative speed below3m/s,0.12s blend; unchanged |
| Strike / rebound |12m/s /4m/s, unchanged |
| Summon |0.10s +0.20s, unchanged |
| Harvest |Trees5, limestone8; one free reusable stone |
| Ricochets |Three-impact safety cap; immediate mouse guidance, unchanged |

EarthConfig defaults and Resources/Earthcasting/Tuning.asset both changed. Steering/bounces/player movement do not reset cumulative budgets. Bound targets enforce20m separation; waiting pauses active flight time. No range meter or warning spam. All return/recovery logic, meshes, collision, sound and save schemas remain unchanged.

## Verification
Standalone player controlled by actual Win32 keyboard/mouse input. QA files were read-only observations. No runtime state injection or scripted in-game movement bypasses.

- RMB closing passed from B-opened and construction-RMB-opened palettes. Holding the closing press left the palette closed, kept piece count unchanged, stowed Earth/Air and did not trigger wind or Earth actions. Confirmed at1080p and720p.
- B and Escape palette closing retained. RMB on pause did not resume or activate buttons; controls remained open after RMB.
- HUD screenshots inspected at1920x1080 and1280x720: Earth, Air, construction, bright sky, grass, timber and shaded foliage. Held stone/crosshair/placement remain visible. Text fits; long feedback expands vertically instead of clipping.
- Free summon and separate launch retained. Observed guided velocity magnitude18m/s; sky shots logged distance-limit returns. Sample at19.982m player separation preceded return. Discrete collision steps can exceed the nominal radial threshold slightly before the next check, as before.
- Continuous mouse circling from supported hover reached exactly40m cumulative path and returned. No budget reset during steering.
- Bound tree survived repeated waiting/strikes. Moving the player beyond20m logged target-out-of-range return and caught successfully.
- Ground ricochet followed changed mouse aim, hit tree3 and entered Waiting. Placed-wall and ground reflections recorded two bounces while the stone remained Guided. RMB then returned it.
- Moving-player catch and relaunch passed: logged socket speed about4.006m/s, then another launch without stopping.
- Fresh tree1 damage1–4 remained alive,5 depleted; outcrop13 damage1–7 survived,8 depleted. Extra anticipation clicks did not queue hits. Both automatically returned. Resource yields remained at location until actual nearby collection (3.49m tree,3.48m outcrop).
- Placed a free wall through normal construction, count8→9. Early placement aimed beyond reach was rejected, then repeated from spawn successfully.
- Genuine executable restart: nine construction records and complete Earth state matched exactly, including12 Stone/12 Wood, ready stone, depleted/collected tree and outcrop, and partial tree3 damage3. One crate's position.y and safe.y changed by -3.725290298e-9m from ground settling; all other Air fields retained.
- Build and runtime results, final screenshot checks and preservation audit are recorded below.

An initial helper import accidentally invoked its old harvest entry point and stowed the wand; its wait failed without affecting game state. The helper was corrected and the complete harvest sequence rerun successfully. Raw observations retain early failed attempts; descriptive tags alone are not pass claims.

## Preservation
Initial Git tree clean at568faf9; no Unity or Skywright session was open, so there was no live unsaved progress to capture. Read required docs and actual source/assets/input/flight/save contracts; no applicable AGENTS.md found in inspected project/parents.
All pre-existing JSON/backup files were copied into Evidence/CompactHUD/saves-before before testing. Personal saves were not used for testing. The isolated verification primary and backup are restored after testing; hash audit identifies only expected read-only QA diagnostics as changed. Previous builds, stable piece identities, inventory schema and sibling projects remain unchanged. No repository publishing.

## Evidence
Evidence/CompactHUD (local, Git-ignored) contains:
- earth-grass-1080.png, earth-sky-1080.png, earth-timber-1080.png, air-grass-1080.png, build-grass-1080.png.
- earth-720.png, air-720.png, build-720.png, controls-720.png; final-* captures show the final rim/footer polish.
- input-evidence.jsonl, flight-samples.jsonl, saved-before/after-restart.json and restart-comparison.json.
- build.log, build-final.log, play-1080.log, play-720.log and final player logs.
- preservation-before/after.json, saves-before, shortcut.json; original input/capture helpers.

## Limits
The independent3s time guard was inspected but not separately triggered at shipping tuning:40m at18m/s is about2.22s, so the path limit normally wins. No artificial tuning was introduced to claim that input test. No ultrawide, extreme low-frame-rate, fully dark-room or exhaustive every-piece/snap-label check. Existing collision-safe recall and fallback code was preserved; this pass did not repeat the previous blocked-return stress test. UI readability and enjoyment remain subject to Jonathan's playtest.

## Playtest
Open Skywright Elemental Preview, Continue. Tab equips the current wand. Launch/steer/recall while moving and judge the shorter18m/s flight. Open the kit with B, then RMB to resume construction; repeat with RMB from construction, including holding the closing press. Check the small glass HUD over sky and structures. F5 saves explicitly. Stop here for feedback.

## Final completion checks
- Final build succeeded with errors=0; Earth and Air save-contract gates passed.
- Final720p and1080p screenshots inspected after the rim/footer refinement. Final normal glass remains18% opacity, rim up to32%; no panel/background changes outside the bottom HUD.
- Final720p actual input left tree3 Waiting for more than3seconds without expiration or additional damage, then manually recalled successfully. Final RMB closing/holding and valid construction preview passed again in the shipping build.
- No runtime exception, assertion or shader-error matches in inspected initial/final720p/1080p logs.
- All16 original save/backup files are byte-identical after restoring the QA primary/backup. Only qa-state.json and m15-loaded-audit.json changed as expected (read-only diagnostics). Personal elemental-trial.json SHA-256 before/after:017553DCF8F30CB2D7338FFB208548995CC5ACA7C4DB7BB7B659CD09B0B0CFF5.
- No game or editor process left running. Shortcut target and arguments verified. Evidence remains local and excluded from Git.
- Local checkpoint ID is available from git log -1 and the handoff; no repository was published. Stop for Jonathan's playtest.
