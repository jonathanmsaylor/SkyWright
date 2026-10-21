# M1.7.1 — Interaction polish
Date: 2026-10-18. Authorized pass only; M2 has not started.
Parent checkpoint: 07969d1. This report is committed with the implementation.

## Launch
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Elemental_Preview\Skywright.exe
- Desktop shortcut: Skywright Elemental Preview (target verified; -elemental, windowed 1600x900).
- Scene: Assets/Scenes/FirstHome.unity
- Unity 6000.4.7f1, URP 17.4.0, Input System 1.19.
- Previous complete preview retained at Builds/Elemental_M1_7_Backup.
- Direct executable launch remains Elemental via its existing marker. Normal play uses elemental-trial.json; QA used verification-elemental.json only.

## Changes and current controls
| Context | Input | Action |
|---|---|---|
| Magic | Q / G | Select Earth/Air / equip selected magic |
| Earth | LMB, then mouse aim | Launch one stone; guide its flight |
| Air | Hold/release LMB | Start/stop sustained wind |
| Air | RMB | Lift/release crate, bench, planter or lantern |
| Carrying | Wheel / Shift+wheel | Distance / height |
| Movement | R | Initial lift/hover, or cancel active hover |
| Hover | Space / either Ctrl | Up / down; C has no action |
| Construction | F / B | Equip/put away / kit |
| Construction | LMB / RMB | Place / open kit |
| Construction | MMB / Shift+MMB | Remove / copy type without removal |
| Construction | Wheel / Shift+wheel | Rotate 45 degrees / reach |
| Construction | Alt / Tab | Free placement / named snap points |
| General | E / F5 / Escape | Nearby stone pickup / explicit save / menu |

Guided Earth uses continuous swept motion with limited turning, one active projectile and one committed charge. Menus pause guidance; saving, switching tools and losing focus cancel unresolved flight without damage or refunds. Previously resolved harvesting and started collection remain persistent. Distant breakables receive the blue marker; unaffected scenery does not.

Air is a single sustained presentation and sound loop with short release. Deliberate movement sweeps and line-of-sight tests remain authoritative. Furniture reuses its existing BuiltPiece identity, collider contracts and payment record. Unactivated furniture does not begin falling on old-save load. Structures and anchors are excluded by explicit type eligibility.

R inside an anchor provides initial lift; leaving support arms automatic recovery on re-entry before landing. Deliberate R cancellation disarms recovery. Descent is faster. Hover hints show controls, without movement-state narration.

Wind ribbons fit visual bounds with corner clearance; movement bounds remain separate. Thin transparent anchor shells share the exact gameplay volume definition. UI labels account for measured height, including descenders; notifications cannot cover help or palette. Original synthesized bubble pops replace pickup and successful placement sounds.

## Tuning
- Earth speed: 22 m/s; steering: 150 degrees/s; total path budget: 240 m; flight lifetime: 12 s.
- Swept sphere radius 0.15 m; curved flight substeps at most 10 ms.
- Existing emergence 0.42 s and suspension 0.16 s retained.
- Visible neighboring island centers are approximately 120–224 m away. They remain scenery, without new visitable terrain or harvest content.
- Pickup interactions remain short range; automatic wood collection remains 3.5 m.
- Wind range 7 m; horizontal acceleration 10 m/s²; speed cap 3 m/s.
- Descent 3 m/s with acceleration smoothing; initial lift approximately 2 m.
- Anchor volume changed from a cylinder to an ellipsoid: center 3 m above anchor, radii (10,5,10) m. Bottom -2 m, top +8 m. Reach is 10 m at center height, approximately 8 m at anchor ground level. Eligibility uses player feet. Existing anchor transforms are unchanged.
- Bubble is cosmetic and has no collider. Overlap and removal use the same shared volume data.

## Verification performed
All gameplay checks below used actual keyboard/mouse input in the Windows player. QA JSON and runtime logs observed results; they did not command game state.

| Area | Result and evidence |
|---|---|
| Build | Final Windows build succeeded with errors=0; existing collider-contract and Earth/Air serializer checks passed. No runtime exceptions or shader errors found in inspected play logs. |
| Guided flight | Mouse changes visibly curved flight. Extra clicks did not queue casts or spend more stone. One recorded miss expired at exactly 240 m. |
| Distant harvesting | Tree 6 was hit from approximately 19.6 m, damaged, then felled. Wood stayed at zero until approaching within 3.5 m, then became 12. |
| Earth interruption | F5 canceled unresolved flight at 63.80 m without refund/reward; tool change canceled at 26.77 m. Menu paused guidance. Actual Alt+Tab canceled at 16.69 m with the charge spent. |
| Earth collision | Tree impacts exercised swept collision. A large aim change redirected a shot back toward the house and produced an unaffected surface impact after 57.57 m of travel, with no resource or piece damage. No explicit thin-obstacle tunneling stress test. |
| Continuous wind | Holding/releasing LMB starts/stops wind; a crate moved approximately 3.86 m in a 1.4 s gust and settled. Held objects were excluded. Menu interruption stopped wind and required button release before restarting. |
| Movable furniture | Bench, planter and lantern were placed, lifted, moved and released through normal controls. Player/collider obstruction was observed while moving the lantern. Their original IDs/payment records survived save/restart. |
| Exclusions | Wind anchor rejected lifting and did not move under wind. Structural exclusion and self-lifting guards were inspected in code; not every excluded type or self-lifting configuration was input-tested. |
| Wind wrap | Crate and elongated bench effects inspected in gameplay and screenshots with exterior clearance and normal occlusion. A complete front/side/above orbit for every furniture type was not captured. |
| Hover controls | R inside anchor raised feet from ~0.035 to ~2.306 m. Outside personal lift reached ~2.13 m. C did not descend; Ctrl did. Both Ctrl bindings exist; left Ctrl has the clear measured test. |
| Support recovery | Leaving support caused descent; returning restored hover automatically. Recorded support-loss and support-regained logs. R cancellation landed inside support without recapture. |
| Anchor lifecycle | Removing the last anchor ended support. With two overlapping anchors, removing one retained support from the other. |
| Ceiling / traversal | Entered the sample doorway. After placing the missing opposite roof slope, upward hover stopped below the completed roof (~2.682 m foot height). Earlier escape through the open half-roof was not a collision failure. |
| Bubble | Preview and placed shell inspected from outside; terrain/builds remained readable. Horizontal exit/re-entry tested against the shared volume. Vertical tip boundary and dense multi-anchor overdraw were not separately stress-tested. |
| Construction | RMB opened kit without removal. MMB removed the aimed anchor only. Shift+MMB copied anchor type without changing piece count. Placement/removal of test anchors and the roof used normal gameplay. |
| Sounds | Pickup-pop and placement-pop event logs confirmed intended triggers. Audio quality/mix could not be heard through this connection. |
| UI | HUD/resource panel, hover hints, bottom spell panel, kit and controls visually inspected at windowed 1280x720, 1600x900, 1920x1080. Save-toast overlap with help title was found and fixed before final build. |
| Genuine reload | Final 11-piece save/restart preserved piece records, Earth state and Air state exactly. Earlier airborne restart preserved spent personal lift rather than granting a refill. |
| Earlier M1.7 save | Unchanged protected copy loaded: all 10 piece records, Earth state and Air state matched. Resource/crate comparisons use runtime QA; the legacy loaded-audit file only records pieces. |

## Preservation and evidence
Original saves and backup files match their pre-pass SHA256 hashes after restoring the QA working slot. Only diagnostic qa-state.json and m15-loaded-audit.json changed. No personal save was replaced. Original source save fixtures remain in Evidence/InteractionPolish/saves-before.

Evidence directory: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Evidence\InteractionPolish
- compatibility-results.json: exact final and earlier-save comparisons.
- preservation-before.json / preservation-after.json: hash evidence.
- build-final.log; play-a.log; reload-a.log; final-play.log; ui720.log; ui1080-oldsave.log.
- input-evidence.jsonl: observations, including unsuccessful harness attempts. Tags alone are not pass assertions.
- final-before.json / final-after.json: final 11-piece fixture.
- airborne-before.json / airborne-after.json: airborne reload fixture.
- Screenshots: controls-final-1600.png; hud-720.png; palette-720.png; controls-720.png; hud-1080.png; palette-1080.png; controls-1080.png; anchor-preview.png; anchor-bubble-exterior.png; crate-wrap.png; bench-wrap.png.
- Gameplay recordings: earth-gameplay.gif, wind-gameplay.gif, hover-gameplay.gif. Each is approximately 7.2 seconds; full PNG frames are retained. The hover capture harness initially lacked keyboard scan codes; the final recording includes actual support loss/recovery.
- Early pickup misses, rejected anchor placement, unsafe launch rejection, and open-roof hover attempts were corrected/retested rather than counted as passes.

## Assets and implementation notes
New bubble shader/material, procedural wind presentation and synthesized pop audio are original project work. No third-party assets, purchases, packages, runtime services or accounts were added. Existing kit dimensions, stable IDs and collision contracts are retained.

AnchorVolume/AnchorBubbles share gameplay/render shape. AirCrate adapts explicitly eligible furniture at runtime. Furniture remains normal PieceRecord persistence; AirState retains only the two demonstration crates. Save settles active movable objects before serializing piece transforms. PolishSetup is explicit one-time configuration; normal rebuilds use Skywright.Editor.BuildElemental.Build and do not regenerate artwork.

## Limits and playtest
- Long-range harvesting is verified beyond normal interaction reach on the home island, not at 200+ m or on decorative horizon islands.
- Some edge cases remain inspection-only: all structural exclusions, furniture self-lifting combinations, right Ctrl separately, thin obstacle interception at sharp turns, vertical bubble tips and large overlapping bubble counts.
- Controlled upright prop movement is not general rigidbody/structural physics.
- Actual sound quality needs the user's listening test.
- Idle final-player logs reached the configured 120 FPS cap (~8.33 ms average); this is not an uncapped benchmark or proof of large-settlement performance.
- No M2, airships, additional elements or broad art conversion.

Suggested playtest: launch the desktop preview, select/equip Earth, guide a shot past a tree and correct toward its trunk; confirm remote wood waits for approach. Switch to Air, hold wind on a crate and RMB carry furniture. Place an anchor, R lift, fly out and return, then R land deliberately. Try Ctrl descent, construction RMB/MMB/Shift+MMB, and F5 followed by a full restart. Listen to pickup and placement pops.

Stop here for the user's playtest.
