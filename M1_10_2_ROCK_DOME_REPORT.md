# M1.10.2 — Rock contact and dome polish

2026-10-19, Unity 6000.4.7f1 / URP 17.4.0. Focused follow-up to checkpoint `c52ee7e`. Current executable: `Builds/Visual_Preview/Skywright.exe`, existing Visual Preview shortcut. Previous M1.10.1 retained at `Builds/Visual_M1_10_1_Backup`.

## Rock correction

The previous ground-contact fix was insufficient. The limestone scan includes a connected thin sheet of surrounding ground, so seating its minimum vertices left the main boulder suspended. Some roots also retained steep rotations from the older mountain layout after relocation onto the tutorial island.

The corrected offline authoring pass counter-rotates only the three LOD/collision presentations to the current terrain slope, preserving scenery root positions, rotations, scales and resource IDs. It buries scan vertices below the authored lower-shoulder plane against the actual triangulated home collider. All three LODs are checked for buried sheet geometry and a remaining exposed body. Seventy presentations pass: 68 scenery resources and the two original tutorial outcrops. No runtime terrain sampling was added.

An initial test left the original tutorial outcrops only 15 cm high; Earthstone struck the ground first. Their baked visual/collision presentation was enlarged to leave approximately 55 cm exposed. The final standalone test then hit and depleted the tutorial outcrop normally. Construction dimensions, transforms, harvesting rules and saved resource identities are unchanged.

## Sky-flash investigation

Jonathan describes a bright spot/disc, including while standing still and turning the camera, as well as sprinting. This rules out treating it exclusively as a sprint effect. The running preview reported the custom `Skywright/AeroSky` shader, one camera, one directional light and zero SRP lens-flare components. That sky shader draws clouds and a gradient, with no explicit sun disc.

A visible overhead sunburst was isolated to the bubble's converging animated latitude/longitude lines. It remained with post-processing disabled and with a flat sky, and disappeared when effect renderers were hidden. The shader now fades the angular pattern near both poles and fades undersampled lines using screen derivatives. The field rim, shoreline reveal, radii and flight support are unchanged. The final overhead screenshot confirms removal of the central sunburst.

**The exact intermittent bright-disc flash remains unconfirmed.** The reproduced sunburst is a plausible related artifact, not proof of the user's reported cause. No claim is made that every flash is fixed; no lighting, exposure or global quality reduction was used to hide the symptom. A recording of the remaining flash would establish whether it matches this corrected feature.

Optional `-visualqa -rendercheck` enables F6 rendering comparisons (normal / layer-2 effects hidden / post off / flat sky), with a runtime render summary in the log. This diagnostic does not accept game-state commands, and ordinary launches expose no diagnostic UI.

## Verification and evidence

- `RockFlashBuild.AuthorAndBuild` completed with `NATURAL_VISUAL_BUILD Succeeded errors=0`, including existing save, construction, field containment and travel-corridor checks.
- Real native keyboard/mouse input in the isolated `-visualqa` slot verified eight-hit scenery depletion, one 12-stone award, and save/reload retaining that award, depletion and 29 construction pieces. A saved position inside the enlarged tutorial rock recovered through the existing safe-load behavior.
- The final build verified eight-hit original tutorial outcrop depletion and another 12-stone award, reaching 24 total. Stationary camera turns, Shift ground movement and explicit R flight were exercised with ordinary rendering.
- All 70 placements received geometry checks; representative nearby, distant and formerly tilted rocks received standalone visual inspection. This is not exhaustive visual approval of every rock at every viewing angle.
- Private evidence is in ignored `Evidence/PlaytestFollowup/RockFlash/`: `seating.txt`, build/player logs, `before-rock-east.png`, `after-rock-east.png`, `after-tilted-resource.png`, `sky-isolation-0..3.png`, `final-original-outcrop.png`, `final-dome-pole.png`, `final-flight-view.png`, QA snapshots and earned test-save copies.
- Pre-test save backups and SHA256 manifest were retained. The four changed isolated verification/diagnostic files were restored; all 20 original files match their pre-test hashes (`preservation-after.json`). No test preview remains running. No publishing or sibling-project changes.

Use `PlaytestFollowupBuild.VerifyAndBuild` for a rebuild; `RockFlashBuild.AuthorAndBuild` deliberately re-seats rocks. The old follow-up authoring entry point now calls the corrected seating as well. M1.10.1 controls and frame pacing remain current. The previously documented intermittent native shutdown failure is not fixed by this work.

Stop for Jonathan's visual playtest. The remaining bright-disc report must stay open until confirmed.
