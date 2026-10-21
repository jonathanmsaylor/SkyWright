# M1.7.3 — Returning Earthstone
Date: 2026-10-18 (MashPC user local date). Parent checkpoint: 65e9b1e.
Implemented the authorized Earthstone replacement. Stop for Jonathan's playtest. No M2, airships, additional elements or construction-art conversion.

## Launch
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Executable: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Builds\Elemental_Preview\Skywright.exe
- Desktop shortcut: Skywright Elemental Preview
- Scene: C:\Users\jsayl\Desktop\UnityProjects\Skywright\Assets\Scenes\FirstHome.unity
- Unity 6000.4.7f1, URP 17.4.0.
- Shortcut arguments: -elemental -screen-fullscreen 0 -screen-width 1600 -screen-height 900.
- Previous complete preview: Builds/Elemental_M1_7_2_Backup.
- Normal play uses elemental-trial.json. Tests used only -airqa / verification-elemental.json.

## Changes and controls
One free reusable stone replaces paid spell ammunition. Loose Stone inventory and pickups remain independent resources, with no new crafting uses. Existing amounts were neither charged nor refunded. Existing wood/deck costs and Creative behavior remain.

| Context | Input | Result |
|---|---|---|
| Magic | Q / G | Select Earth or Air / equip |
| Earth, absent | LMB | Summon from clear ground and settle above wand |
| Earth, ready | LMB | Launch the same stone |
| Earth, outbound | Mouse aim | Guide flight |
| Earth, waiting at target | LMB | Strike the bound target once |
| Earth, airborne or attached | RMB click | Start persistent recall; release does not cancel |
| Earth, summon/flight/strike/rebound/ricochet/return | Extra LMB | Ignored; no queued attacks |
| Air | LMB / held RMB | Lift or release / continuous wind |
| Construction | LMB / RMB | Place / open kit |
| Construction | MMB / Shift+MMB | Remove / select looked-at piece type |
| Hover | R / Space / Ctrl | Toggle / rise / descend |
| General | E / F5 / Escape | Nearby Stone pickup / explicit save / menu |

The former Earth LMB ballistic release and held-button recall have been removed. Current HUD/help reflect the new mappings. A tree impact binds its explicit EarthNode and stores local contact/normal. The stone rebounds into clear space, waits for another press, anticipates, then strikes at actual swept contact. Returning and hovering never apply damage. Moving the crosshair does not switch the bound target.

The original precision-fracture bonus remains. A fresh tree normally takes one initial hit plus one precise repeated strike (damage 1 then 3). This is intentional preservation of harvesting rules, not an extra damage tick. Fallen wood waits at its node for the existing 3.5m local collection radius.

Unaffected surfaces reflect the projectile using their contact normal. Bounces retain 88% speed and have a maximum count/time. A breakable encountered during ricochet enters ordinary impact/wait behavior. Misses return at their travel/time budget. Return tracks the player and brakes into the wand. An obstruction stops return; if it does not clear within the bounded wait, a visible shrink/dismiss restores free summoning. No pathfinding, terrain deformation, building damage or general projectile Rigidbody simulation.

## Rock and asset workflow
The old projectile combined a coarse 104-vertex limestone mesh with 1,080 vertices of separate earth clods. It was inspected before replacement; old mesh/UV/normal counts and winding diagnostics are in build-a.log. Winding measured relative to the root is diagnostic, not proof that every offset-clod face was incorrectly oriented.

The replacement is one original continuous, irregular limestone surface with broader eroded facets, smoothed seam/pole normals, UVs, matte mineral color variation and a restrained normal texture. An initial rounder version was visually inspected and revised before handoff. The shape is normalized to a maximum 0.30m radial extent inside the deliberate 0.31m collision sphere; its longest extent is roughly 0.57m. It retains identity scale through summon, flight, attachment and return. Only safe dismissal shrinks it.

Assets/Resources/ReturningEarthstone contains:
- Earthstone.prefab: identity root and replaceable Visuals child.
- Limestone.mesh.asset: 2,145 vertices, 3,968 nondegenerate outward triangles.
- Limestone-color.png and Limestone-normal.png: original 1024x512 textures.
- Limestone.mat: standard URP Lit, metallic0, smoothness0.13, normal strength0.4.

EarthTrial owns a swept sphere; detailed visual geometry does not decide collision. The socket was moved forward/right to retain a substantial rock without covering the crosshair. A blocked ready socket hides the visual and prevents launch until clear.

All new geometry and textures were authored specifically for Skywright by the included deterministic editor authoring utility. No third-party asset, paid service, purchase or new package was used. Existing Unity assets/packages keep their existing licenses. Audio was preserved from M1.7.2.

Ordinary rebuild: Skywright.Editor.BuildElemental.Build.
Explicit rock regeneration plus build: Skywright.Editor.ReturningStoneSetup.Build. Do not use explicit regeneration after manual rock edits unless intentionally replacing them. It leaves the old Earth assets and construction kit intact.

## Final tuning
| System | Values |
|---|---|
| Outbound | 22m/s; 150deg/s steering; 240m path or12s active flight |
| Summon | 0.42s emergence +0.48s travel to socket |
| Collision | Sphere radius0.31m; curved-motion substeps at most10ms |
| Waiting | Contact-normal offset = radius +0.55m; no waiting expiry; range guard240m from player |
| Rebound | Speed4m/s; acceleration90m/s²; settle within0.012m |
| Strike | Anticipation0.16s with0.12m draw-back; speed12m/s; acceleration90m/s²; missing-contact guard1.2s |
| Ricochet | At most3; at most2s after first bounce; speed retention0.88; stop8mm short, attempt15mm swept separation |
| Return | Max26m/s; acceleration65m/s²; per-launch budget280m/12s; distance-based braking |
| Catch | Clear socket and path; distance<0.10m, speed<2m/s |
| Blocked return | Wait0.8s, then0.3s visible dismissal; availability restored |
| Local collection | Existing3.5m, with existing line-of-sight rule |

Final defensive checks also treat an inactive bound target as unavailable and stop return if moving geometry overlaps its starting sphere. They do not disable collision or apply overlap damage.

## Persistence and interruptions
EarthState schema remains version1. preparedStone now means free ready state, not a paid inventory reservation. Earlier true values restore ready without a refund or deduction. Missing fields remain false. No flight or target-attachment data is serialized.

Ready state survives stowing and saving. Interrupted emergence settles to ready. Saving, changing tools or losing focus dismisses unresolved flight, attachment, strike or return without inventing a hit, spending inventory or removing future availability. Menus freeze motion and consume gameplay clicks. Resolved falls and already-started local collection keep the existing settlement behavior. Existing object IDs, piece records, paidWood and protected load validation remain.

## Input verification
Tests used actual Win32 keyboard/mouse input in the standalone player. Read-only QA files observed behavior; no in-game state-command injection was used. Test walls were placed and removed through normal construction input. Raw observations retain unsuccessful aim attempts as well as successful tests; their tags alone are not pass claims.

| Test | Evidence/result |
|---|---|
| Zero inventory / distinct presses | Fresh trial Stone0; first LMB left one prepared stone, second launched. All recorded spell-test inventory counts remained0. |
| Single ownership | Every recorded active state reported one stone or zero after dismissal; no extra queued action or inventory change observed. Owner explicitly rejects duplicate creation. |
| Guided outbound | Mouse turn changed flight direction/velocity before recall. |
| Initial hit / wait | Tree1 damage1, bound ID retained, nearby waiting stone; extended wait did not apply damage. |
| Repeated strike / crosshair away | One LMB after looking away struck the owned tree; precise damage reached3 and triggered return. Extra clicks during the cycle did not queue another impact. |
| RMB during strike | STRIKE_BEGIN followed by recall; tree0 remained damage1. |
| Manual recall | Outbound, waiting, striking and ricochet states exercised. A click/release continued the return. |
| Moving-player catch | Strafed during return; caught at the moved socket and relaunched without resources. |
| Automatic break return | Recorded complete summon, launch, wait, repeated strike, fall and return. Wood stayed0 remotely. |
| Surface ricochets | Ground and normally built walls reflected the stone; no structural damage. |
| Multiple bounces | Two placed walls produced counts1,2,3, then automatic return/catch. |
| Ricochet into breakable | Shot at rear wall, reflected, bounced on ground, hit tree3 to damage2 and entered Waiting. Log shows both RICOCHET events followed by IMPACT and TARGET_WAIT. |
| Return obstruction / recovery | Recall struck a normally placed Wall board, entered blocked wait, visibly dismissed, then a fresh LMB summoned successfully at Stone0. |
| Brief obstruction clears | A second recall stopped at the rear wall, found a clear direct return path and caught successfully. |
| Long-range miss | Traveled exactly240m, automatically returned247.40m, and caught above the wand. |
| Menus | Same projectile position/path before and after menu clicks; no flight progression or queued action. Resumed and recalled normally. |
| Tool/save/focus interruption | Unresolved flight dismissed on F, F5 and actual Alt+Tab. Damage/resource records stayed intact; summoning remained available. |
| Ready save/restart | Full executable exit and restart restored ready. Piece records and Earth state exact. One crate Y position/safe coordinate differed3.73e-9m from normal settling; no identity change. |
| Earlier unchanged save | Protected M1.7.1-era fixture loaded with exact piece records, exact Earth data plus default prepared=false, and exact Air state. Stored Stone12/Wood12 retained. Serializer gate also checks earlier prepared=true with Stone17 unchanged. |
| Construction | LMB placed walls; RMB opened kit without removal; Shift+MMB selected type without changing count; MMB removed one wall. |
| Air | LMB grabbed/released crate1; held RMB applied wind; release stopped wind. |
| Final-build smoke | Repeated tree impact/depletion and automatic catch passed after the final guards. A further shot ricocheted into tree5 and waited; recall was intercepted by another tree, dismissed safely, and applied no damage to that obstruction. Current controls screen inspected with no text clipping at1600x900. |
| Presentation | Inspected ready and bound-target screenshots plus gameplay capture frames at1600x900. Rock has a solid silhouette; crosshair remains clear. Visual approval belongs to the user. |

## Limits and unperformed checks
- An unrelated moving obstacle intercepting a strike after the stone has already entered Waiting was code-inspected but not reproduced through input. Return obstruction was input-tested; these are separate claims.
- Inactive-target handling and a moving collider overlapping the return sphere are defensive code paths, not separately reproduced input tests.
- Interruption types were exercised during unresolved flight; every possible menu/tool/save/focus combination in every attachment substate was not exhaustively repeated.
- Distant wood non-collection was verified. The full local collection traversal, all hover/anchor transitions, and every Air furniture type were not re-tested in this Earth-focused pass; their implementations were unchanged.
- No full dense-settlement or low-frame-rate benchmark. Observed ordinary player samples were approximately8.33–8.35ms per frame at the existing120fps cap; this does not establish uncapped performance or a controlled before/after comparison.
- Recordings are silent GIFs. No new audio change or subjective listening approval is claimed.
- The rock is authored procedural artwork, not a scanned production asset. Review its framing, mineral variation and silhouette in game before treating the art as final.

## Evidence
C:\Users\jsayl\Desktop\UnityProjects\Skywright\Evidence\ReturningEarthstone
- tree-cycle.gif and tree-cycle-frames:10-second actual-input tree sequence.
- ricochet-recall.gif and ricochet-recall-frames:10-second wall-bounce and manual recall sequence.
- rock-ready-final.png, bound-tree.png, ricochet-ready.png and final-smoke.png.
- build-a.log, build-b.log, build-c.log, build-final.log; play-a.log, play-final.log, restart.log, final-smoke.log.
- input-evidence.jsonl, compatibility-results.json, mesh-audit.txt.
- old-save-loaded.json / old-save-audit.json; ready-before.json / ready-after.json; final-test-save.json.
- saves-before and preservation-before/after.json.

Build-a passed. Build-b intentionally failed the authoring bound check after the larger shape revision; the geometry was normalized to the deliberate sphere and build-c passed. The final ordinary rebuild adds the inactive-target/initial-overlap guards without regenerating artwork. No paid APIs, online gameplay dependency or repository publishing.

## Short playtest
Open Skywright Elemental Preview and continue your Elemental save. Select Earth with Q and equip G. Summon with LMB, launch at a tree with the next click, let it wait, then click to strike. Watch it return and relaunch it. Click RMB during flight to recall. Try bouncing it between walls. Watch for comfortable framing, clear target ownership and satisfying contact/rebound timing. Save with F5 when ready.

## Preservation and final status
Final ordinary build succeeded with errors=0 and no C# warning/error matches. Inspected standalone logs contain no runtime exception, shader error or assertion failure matches. The final player was closed after its input smoke test.

All15 original save and backup files match their pre-pass SHA-256 hashes after restoring only the isolated verification-elemental.json and its backup. Personal saves were never replaced. Read-only diagnostic outputs and player logs changed as expected. The previous preview was copied before implementation. No sibling project, remote repository or unrelated user change was modified. The desktop shortcut was verified to target the updated executable with the intended Elemental arguments.

MAINTENANCE.md and PROJECT_BRIEF.md are updated. Source, assets and this report are included in the local Git checkpoint; generated builds and Evidence stay local and untracked. Use git log -1 for the checkpoint ID, also supplied in the handoff. No repository publishing occurred.
