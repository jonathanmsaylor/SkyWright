# M1.7.2 — Reusable Earthcasting and spell audio polish
Date: 2026-10-18. Parent checkpoint: ebcb36f.
Implemented only the authorized pass. Stop for the user's gameplay and listening review.

## Launch
Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Elemental_Preview\Skywright.exe
Desktop shortcut: Skywright Elemental Preview
Playable scene: Assets/Scenes/FirstHome.unity
Unity 6000.4.7f1 / URP 17.4.0 / Input System 1.19.0.
The shortcut still points at the updated executable with -elemental, windowed 1600x900.
The previous full preview is preserved at Builds/Elemental_M1_7_1_Backup.
Personal Elemental play uses elemental-trial.json; testing used verification-elemental.json.

## Current controls
| Context | Input | Result |
|---|---|---|
| Magic | Q / G | Select Earth/Air / equip |
| Earth, no stone | LMB | Prepare one stone above the wand |
| Earth, prepared | LMB | Launch the existing stone |
| Earth, guided | Mouse | Guide flight |
| Earth, guided | LMB | Release guidance; momentum and gravity continue |
| Earth, airborne | Hold RMB | Recall an intact stone |
| Earth, returning | Release RMB | Resume the previous guided/ballistic mode |
| Earth, returned | LMB | Relaunch without another charge |
| Air | LMB | Lift/release eligible furniture or crate |
| Air | Hold/release RMB | Continuous wind / stop |
| Carrying | Wheel / Shift+wheel | Carry distance / height |
| Hover | R / Space / Ctrl | Activate/cancel / up / down |
| Construction | LMB / RMB | Place / open kit |
| Construction | MMB / Shift+MMB | Remove / copy type |
| Construction | Wheel / Shift+wheel / Alt / Tab | Rotate / reach / free placement / snap points |
| General | E / F5 / Escape | Nearby stones / explicit save / menu |

## Implementation
Earth now has explicit Idle, Preparing, Prepared, Guided, Ballistic, Returning and Recovering states. One distinct press performs one action; clicks during preparation, ballistic flight or return do not queue another stone.

Preparation raises the original chunk from clear ground and brings it above a named socket on the focus. The held model is smaller than the first test version. Recall brakes near the moving socket and scales the stone down smoothly. The camera stays in first person. Obstacles retain authority; recall has no pathfinding or teleportation.

A committed preparation deducts one stone. The same stone can launch and return repeatedly without additional payment. Impact/expiry destroys it. Prepared state is a paid reservation, not an extra inventory item. EarthState v1 adds preparedStone, default false when absent. Prepared reservations survive stowing and saving. Restore recreates the view model when equipped without spending another charge.

Menus pause flight and consume input. Save, focus loss and tool changes cancel unresolved airborne flight without refunds, damage or invented hits. Interrupted preparation settles to its paid prepared reservation. Resolved harvesting and local collection remain unchanged. Construction identities, dimensions and paidWood records were not changed.

Air LMB/RMB actions were swapped in gameplay and current hints. Held props remain excluded from wind. Opening menus or losing focus stops wind and requires release before restarting.

## Selected tuning
- Outbound flight: 22 m/s, 150 degrees/s steering, 240 m total path or 12 seconds.
- Ballistic gravity: 9.81 m/s²; retained launch velocity.
- Recall: maximum 26 m/s, acceleration 65 m/s², separate cumulative budget of 280 m or 12 seconds per launch.
- Releasing/restarting recall does not reset either budget. A successful return followed by a new launch resets both. Prepared time never expires.
- Collision: swept sphere radius 0.18 m, curved-motion substeps <=10 ms; player and effects excluded.
- Prepared model scale 0.28, flight scale 0.72; return scales down within 3 m. Arrival requires a clear socket, proximity and low speed.
- Existing harvesting circle and 3.5 m collection radius retained.
- Existing hover and anchor tuning retained.

## Audio
All new audio is original synthesis, without imported packs, licensing obligations, paid services or accounts.

BubbleAudio now uses softer attack, damped rounded resonators, a quiet secondary bubble, low harmonic weight and restrained pitch movement. This changes timbre as well as pitch.
- Pickup: 0.18 s, base 470 Hz, playback volume 0.50.
- Placement: 0.23 s, base 365 Hz, playback volume 0.55.
- Pitch variation ±1.8%; pickup rate limit remains 90 ms.
- Failed placements and removal do not play placement success.

The prior wind used a roughly one-second noise loop and hard stops on interruption. SoftWindAudio now provides one 32-second filtered-noise loop, two-second wrap crossfade, slow periodic modulation and one continuously playing source. Gain is smoothed per audio sample, including menu/focus/tool interruptions. Target volume is 0.35; a 65 ms exponential envelope makes onset/release smooth without restarting the phrase. The silent source remains allocated; there is no voice accumulation.

Generated sample checks:
| Clip | RMS | Peak | Boundary difference |
|---|---:|---:|---:|
| Pickup | 0.054381 | 0.239839 | 0 |
| Placement | 0.052185 | 0.244319 | 0 |
| Wind | 0.075000 | 0.338528 | 0.004482 |

Wind boundary difference is below its maximum ordinary adjacent-sample change (0.033897). No nonfinite values or clipping were found. These are signal checks, not a listening approval. Audible playback was unavailable through the connection; perceived pleasantness, pitch and mix need the user's review.

## Verification
Performed in the standalone Windows player with actual keyboard/mouse input. Diagnostic files observed state; no game-state command injection was used.

| Check | Observed result |
|---|---|
| Build | Final build succeeded with errors=0. Earth/Air serializer and existing collider-contract gates passed. No runtime exception/error matches in inspected play logs. |
| Prepare and stow | First click changed 12 charges to 11 and left one prepared stone. A second quick click during a later preparation did not launch or spend again. Prepared stone survived tool stowing and remained available beyond the flight timeout. |
| Launch and guide | Launch spent nothing further. Mouse motion curved flight; guided steering stayed active after releasing the click. |
| Ballistic release | LMB preserved forward velocity and applied gravity. Observed velocity changed from approximately (8.57,7.45,18.84) to (8.57,-1.13,18.84) despite subsequent mouse movement. |
| Recall modes | Recall succeeded from guided and ballistic flight. Releasing RMB restored the correct prior mode. Repeated recall did not reset accumulated path counters. |
| Moving-player catch | Player strafed during return; the stone settled at the moved socket. Relaunch and another return preserved the same charge count. |
| Full recording | Recorded preparation, launch, guide adjustment, ballistic release, recall, relaunch and guided recall. Charge count stayed 10 throughout both launches/returns. |
| Breakables | Tree 1 received one impact to damage 1, then a precise impact to damage 3 and fell. No duplicated damage. Wood stayed at 12 while the player remained distant. Blue marker appeared only on the supported tree. |
| Broken-stone recall | Holding RMB after impact did not recover or duplicate the broken stone. |
| Return obstruction | A wall placed through normal construction intercepted the returning stone at approximately (1.00,2.35,-13.10). Log explicitly records phase=Returning and IMPACT_UNAFFECTED. No resource/build damage or recovered stone. An earlier trajectory cleared the wall and was not counted as an obstruction pass. |
| Expiry | A sky miss dissipated at exactly 240.00 m. |
| Save/tool/focus interruption | Unresolved flight canceled at 20.35 m on save, 20.18 m on tool change and 22.92 m on actual Alt+Tab. Spent charges remained spent; no invented impact. |
| Air controls | LMB grabbed/released crate 1. Held RMB sustained wind; release stopped it. Held crate was excluded. The released crate moved under wind. |
| Long wind hold | Actual RMB hold lasted 33.62 seconds, crossed a clip wrap, retained one WIND_BEGIN/STOP pair for that hold and gain ~0.34995. Release faded toward zero. |
| Menu/focus wind | Menu and Alt+Tab faded wind to effectively zero. Closing a menu with RMB still held did not restart wind. Release/press was required. |
| Construction/audio events | RMB opened kit without removal. Normal wall placement produced placement-pop; a rejected placement produced no extra success pop. Normal nearby stone pickup added 18 charges and logged pickup-pop. |
| UI | Updated Earth/Air mappings and hints inspected at 1600x900. Existing scaling/layout retained; 720/1080 were not repeated in this pass. |
| Prepared save/restart | Genuine executable restart restored prepared=true, 11 charges, identical Earth state and exact piece records. Crate identities/rotations and positions persisted; one settled Y value differed by only 3.73e-9 m. |
| Earlier save | Protected unchanged earlier Elemental save loaded with exact piece records, Earth state plus default prepared=false, and exact Air state. |

The initial compile found a missing namespace brace; it was corrected. Final compilation has no introduced C# warnings. Initial pickup attempts from outside actual interaction reach and the unobstructed wall-return route remain in raw observations, but were not counted as passes.

## Preservation
Original personal and verification save files, including their existing backups, match pre-pass hashes after restoring the QA working slot. Only diagnostic qa-state.json and m15-loaded-audit.json changed.
No sibling projects, personal builds or repository remotes were modified.

## Evidence and playback
Directory:
C:\Users\jsayl\Desktop\UnityProjects\Skywright\Evidence\EarthRecall

- earth-recall-gameplay.gif: 13.6-second actual gameplay capture; PNG frames retained under recall-frames. Open in a browser if the image viewer does not animate it. No audio track.
- pickup-bubble.wav, placement-bubble.wav, continuous-wind.wav: exports from the exact runtime synthesis before in-game volume scaling.
- prepared-final.png, controls-final.png: final first-person presentation and controls.
- build-final.log, play-a.log, final-play.log, input-evidence.jsonl.
- audio-analysis.txt, compatibility-results.json.
- prepared-before.json / prepared-after.json: save/restart comparison.
- final-test-save.json: isolated test construction, preserved stone and harvesting state.
- preservation-before.json / preservation-after.json and saves-before: preservation evidence.

In-game listening procedure: use E near an uncollected loose-stone pile, place one valid construction piece, then select/equip Air and hold RMB for at least 30 seconds. Compare short taps, a sustained hold, release and menu interruption. Listen at your normal game volume.

## Maintenance and limits
Normal build: Skywright.Editor.BuildElemental.Build.
Explicit audio export and new-field setup: Skywright.Editor.RecallSetup.Build.
No kit regeneration is needed.

Prepared stone socket obstruction is handled in code; every wall/ceiling/view-angle combination was not input-tested. Recall budget exhaustion at exactly its upper limit and zero-charge relaunch were not separately exercised, though bounded accounting and launch-without-charge paths were inspected. No full low-frame-rate or dense-settlement performance sweep was added. Hearing quality remains unverified remotely.

No M2, airships, new elements, structural physics or unrelated art conversion was started.
