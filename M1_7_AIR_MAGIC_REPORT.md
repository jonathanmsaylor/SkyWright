# Skywright M1.7 — Air Magic and Creative Mobility
Date: 2026-10-18
Status: Implemented. Stop for Jonathan's playtest. No M2, airships, further elements, survival meters or remaining M1.5 kit conversion.

## Launch
Double-click **Skywright Elemental Preview** on MashPC's desktop. Choose **Begin elemental trial**. Unity Hub is unnecessary.
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Scene: Assets/Scenes/FirstHome.unity
- Executable: Builds/Elemental_Preview/Skywright.exe
- Shortcut: C:\Users\jsayl\Desktop\Skywright Elemental Preview.lnk
- Normal save: %USERPROFILE%\AppData\LocalLow\Skywright\Skywright\elemental-trial.json
- Marker: elemental.preview beside the executable; direct launches also select the isolated trial.
Older previews, shortcuts and personal saves are preserved. The pre-existing Earthcasting player was left running; verification used separately launched, PID-targeted windows.

## Controls and playtest
WASD/mouse move/look; Shift sprint; Space jump on foot. Q selects Earth/Air; G equips or puts away selected magic.
- Earth: E collects nearby aimed loose stones; LMB casts toward the crosshair, including scenery and sky. Only valid harvesting targets show the blue impact circle. Committed casts spend one stone.
- Air: LMB lifts/releases a nearby demonstration crate; wheel changes carry distance; Shift+wheel changes height; RMB releases a short gust. Held crates ignore gust force.
- R activates local anchor hover or a short personal lift away from anchors. Space rises and C descends while hovering. R cancels hover into gentle descent.
- B opens the kit; F equips construction; 1–9 structural pieces; [ / ] cycle the full kit. Wheel rotates 45 degrees; Shift+wheel adjusts building reach; Alt free placement; Tab named snaps; Shift+MMB selects the looked-at built type; RMB removes one piece in construction.
- F5 saves explicitly. Escape opens the menu. Recover position and Recover demonstration crates are available there. No autosave on exit.

Suggested playtest:
1. Collect loose stones in front of the starting position, try a tree, then shoot ground and sky. Approach felled wood within 3.5 m to collect it.
2. Turn toward the dock behind the starting position. Select Air and lift a crate. Carry it to the deck, adjust height/reach and place it. Try a gust against the other crate.
3. Near the pearl/aqua wind anchor, press R, then Space. Hover beside the unfinished roof. Switch to B or a structural hotkey and add the opposite roof slope or a gable.
4. Use C or R to land; try leaving the anchor radius. Away from it, try a short lift and repeat R in midair: landing on valid ground is required to re-arm lift.
5. Save, quit and relaunch. Continue the Elemental trial. Judge carry control, wind readability, hover comfort and sound.
If a prior normal Earthcasting save exists, **Import Earthcasting save — copy** preserves its contents and source file. F5 then saves the new Elemental slot. The action is disabled when no source save exists.

## Implemented and presentation
Earth no longer depends on a breakable target to own cast state. Swept impacts hit solid scenery, supported nodes receive damage at impact, and empty-space casts dissipate at range. Ground-source checks and charge rejection remain.
Air adds two explicitly liftable crates, collision-checked bounded carrying, controlled release, short gusts, recovery, one placeable wind-anchor catalog entry, local hover, limited personal lift and gentle descent. Hover operates independently of the selected magic/building tool. Crates have no inventory. Trees, stumps, wood bundles, buildings and anchors are not liftable.
The existing island and kit art remain. Original air assets add pearl/timber/aqua rings and a crystal, a compact animated first-person focus, transparent tapered ribbons, sparse cosmetic leaves and four original synthesized sound clips. Ribbons were reduced near the camera after the first recording proved too visually dominant. The approved storyboards guided palette/motion; the implementation does not claim their detailed environment, rigged hands or full character bodies.
AirMagic/Tuning.asset and AirMagic/Catalog.asset keep configuration and the single new prefab definition explicit. The anchor separates its functional root/collider from Visuals. Existing roots, collision geometry and snap contracts remain unchanged. No new package dependencies, paid assets, runtime services or accounts were introduced.

## Persistence and preservation
SaveData remains v1 with explicit harvestingTrial and elementalTrial markers and independently versioned EarthState/AirState. JsonUtility default inline objects are cleared when their marker is absent. The new crate layout IDs and placed-anchor IDs are explicit; names and hierarchy order never serve as save identities.
Saving releases/settles moving crates through collision or safe recovery, cancels gust animation, and retains existing one-time Earth cast/fall/collection settlement. Waiting wood stays waiting. Airborne saves retain spent lift and resume in gentle descent; anchor support can be intentionally reactivated. Transient animations/tool poses are not serialized.
The copy-import test preserved all original construction IDs/transforms/payments, inventory and resource-node states, including waiting wood, and added one starter anchor plus two crate identities. Its source bytes were unchanged.
Tests used verification-elemental.json, isolated import-earthcasting.json, and backed-up legacy copies. All backed-up original save files remained byte-identical, including home.json, the 119-piece art-preview.json, verification.json, verification-m15.json, verification-earthcasting.json and restored verification-snapping.json/.bak. Read-only QA output/audit files naturally changed. See Evidence/AirMagic/preservation-after.json.

## Verification through actual input
OS keyboard/mouse input drove standalone players. QA JSON, screenshots and logs were observation only, not gameplay command channels.
- Earth: harvested a tree in two precise hits, confirmed no collection beyond radius and collection beginning at 3.47 m, cast at ground/sky/crate/construction, verified one-charge accounting and no damage to unaffected objects. QA confirmed the targeting mark on a breakable and absent on ground, sky and construction. Saving during a cast retained one impact/charge.
- Crates: lifted both, adjusted carry distance/height, carried to a dock, released by click/tool change/save, paused safely in menus, tested fast turns/upward aiming, stopped against an anchor, another crate and a doorway wall, and recovered after an edge fall. No observed tunneling in these cases.
- Gust: pushed a free crate approximately 0.88 m, repeated casts, excluded the held crate, and saved during gust motion. Empty/obstructed casts remain cosmetic; there is no damage or player/building push.
- Hover: activated, rose, moved, opened the palette, selected and placed a snapped roof slope with zero reported connection error, and previewed/placed a gable. Removed an active anchor and placed its replacement normally. Radius exit and height-limit attempts produced descent. Traversed the doorway; rising inside the house stopped against its roof at approximately 2.68 m foot height.
- Personal lift: repeated R/tool changes did not renew the airborne lift. A genuine process restart retained spent lift and descending state. A saved anchor-hover state loaded into gentle descent and allowed deliberate reactivation.
- Save/load: genuine standalone restarts retained construction, resources, paid amounts, crate IDs/upright transforms and anchors. A-before/a-after and final ten-piece before/after comparisons passed with crate positional tolerance 0.00001 m. Import preserved all original fields and source bytes.
- Damaged save: a test version of 999 was rejected with the current nine-piece world/resources retained. The working test file was then restored.
- Legacy: unchanged 29-piece M1 and 23-piece M1.5 copies loaded with exact IDs/types/parents/transforms and original payments. M1 doorway traversal passed. All nine existing authored collider contracts match M1.
- Desktop shortcut: launched the normal Elemental menu without Unity Hub; Player.log confirmed elemental-trial.json. Closed without saving; no personal Elemental slot was created. See shortcut-launch.png/log.
- Build: build-release.log reports SKYWRIGHT_ELEMENTAL_BUILD Succeeded errors=0. Earth and Air serializer checks passed, including duplicate-crate rejection and preserving the last valid save after failed writes. Runtime log review found no managed exceptions or shader errors; intentional damaged-save warnings and existing graphics startup diagnostics are distinct.

An early gust/pickup test exposed the second original crate overlapping scenery. It was not counted as a pass. One-time clear-ground validation/recovery fixed it; both manipulation and gust movement were retested. Other unsuccessful harness attempts (missed stone pickup, F4 instead of F5, an initial load blocked by unsaved-state protection) remain in the evidence and are not counted as successful verification.

## Responsiveness
Same machine, 1600x900, 120 FPS cap, initial spawn/view, ten-second stationary samples:
- Prior Earthcasting preview: 1,200 frames, 8.33 ms mean, 8.77 ms worst.
- Elemental preview: 1,200 frames, 8.34 ms mean, 11.64 ms worst.
Both held the cap in this small sample. The Elemental scene adds the work area, anchor and air objects; this is not an uncapped GPU benchmark or a large-settlement claim. The user's existing background player remained present for both measurements. Logs: baseline.log and release.log. Subsequent changes only reduced gust presentation and clarified controls; idle scene mechanics were unchanged.

## Evidence and limits
Evidence/AirMagic contains private save backups (ignored by Git), input-evidence.jsonl, collider-compatibility.txt, serializer outputs, preservation records, imports, restart comparisons and build/runtime logs.
Review media: crate-levitation-final.png, hover-building-final.png, gust-frames/009.png and air-gust-gameplay.gif. The GIF is a silent 800x450 capture of actual mouse input, approximately 3.8 seconds; full frames are retained. Earlier captures are development evidence, not the final visuals.
New air asset files total about 235 KB including metadata; the crate prefab reuses original Earthcasting geometry. The ring mesh, materials, focus/anchor assembly, wind presentation and four synthesized sounds were created for this project; Unity primitives/packages retain their existing licenses. No third-party pack was introduced.
Live audio audibility/mix was not assessed through this connection. Jonathan should judge loudness, repetition and satisfaction. No exhaustive test of every geometry arrangement, multiple-anchor layout, self-lift exploit variation, long session, GPU/resolution, or legacy stair route was performed. Simple collision blocking may require repositioning the player around an obstacle; crates are not a general physics/stacking simulation. Full hands, refraction, world-wide wind response and final environmental art remain outside this implementation.
Technical verification does not establish enjoyment. Stop here for Jonathan's playtest; do not start another milestone automatically.
