# M1.10 Tutorial Wind report

## Delivered scope
The integrated Windows Visual Preview implements the compact tutorial island, fixed wind anchor, quarter-radius upper dome, suction and harvesting changes, double-jump mobility, full-attunement fast flight and one controlled journey to a collidable neighboring landing. Personal playtest and visual approval remain outstanding.

Launch `Builds/Visual_Preview/Skywright.exe -visualpreview` or the existing desktop shortcut. Working folder: `C:\Users\jsayl\Desktop\UnityProjects\Skywright`. Previous playable build: `Builds/Visual_M1_9_Backup`.

## Final tuning
- Home area is 0.2480297 of M1.9; the established 38 m flat home clearing remains. Original 15 harvesting roots and all 131 scenery resource IDs remain. Remote scenery roots were reauthored around the smaller island; the mountain landform was removed.
- Anchor position (12,0,-20), identity `tutorial-island-anchor`. It is outside the sample house, excluded from the 13-piece Visual catalog and protected against copying, deletion and Air manipulation.
- Dome center (12,-0.5,-20); final ellipsoid radii (91,72.8,91). Horizontal stages: 22.75, 45.5, 68.25, 91 m. No lower half or opaque underside. Gameplay support uses the same upper ellipsoid as the visual. Sampled terrain plus 2 m player clearance reaches at most 0.8323051 of the normalized ellipsoid equation (inside is <=1).
- Each payment uses unchanged costs: 6/12/18 Aether and wood, 4/8/12 stone. Donation motes last 2.4 s followed by 3.2 s eased expansion. Tier/payment commit together; reloading completes the paid effect without charging again. Anchor visual rotation ranges from 8 to 210 degrees/s; full tier enables the upward wind and outbound route.
- Suction acquires at 8 m/18 degrees. Bubbles/drops retain to 12 m/65 degrees; bubble aim-loss grace is 0.65 s, top pursuit 14 m/s and capture 1.05 m. Props retain to 9 m/30 degrees. Eligible targets shimmer; geometry blocks acquisition and travel. Wood/stone retain their original one-owner reward ledger.
- Double jump and R activate mobility. Ground sprint/glide is 7 m/s; full-tier Shift flight is 10.5 m/s with 12 m/s² easing. Quiet wind and screen-edge gusts accompany fast flight and travel.
- Current travels from (58,4,0) to (151,17.12,0), follows a swept, collidable curve, reaches 14 m/s, ignores movement steering and keeps mouse look. Landing is playable; return is available after leaving and re-entering the destination ring. Saving transit settles at the home entrance; pause freezes travel, recovery cancels it.
- Earthstone Visual tuning: 20 m/s, 18 m outbound recall trigger, 36 m cumulative outbound distance, existing 3 s budget. Original Elemental tuning remains 18/20/40. Trees need five hits, limestone eight; their yields stay 12 each. Fall animation rotates at unchanged scale and chooses a least-obstructed corridor. Only two identified cargo crates break, at three hits for six wood each. Buildings/furniture remain undamaged and produce a ding.
- Upgrade requirements replace the top-right piece/unsaved counters. Menus retain unsaved-change warnings. HUD teaches MMB deletion in build mode, Shift+MMB selection and Alt free placement.

## Compatibility and preservation
Save schema remains 1, with `worldLayout=tutorial-v1` and Air state version 3. Explicit Aether/resource identities and earned progression persist. Old free trial anchors are retired and progression transfers to the fixed identity without refund, duplication or paid resource loss. Other construction preserves its identities and transforms. Older layouts with construction beyond the preserved 38 m clearing fail before replacing the active world; the original files and M1.9 backup remain usable. This is not a full-world construction migration.

Personal saves were copied before work; actual playtests use only `-visualqa` and `verification-visual.json`. Evidence and protected copies remain local under ignored `Evidence/TutorialWind/`. No project content was published or uploaded.

## Verification
Current workflow: `Skywright.Editor.TutorialWindBuild.VerifyAndBuild`; use `RefreshAnchorAndBuild` only when deliberately updating anchor configuration. Existing Earth/Air/Aether contracts, quarter-area/radius checks, terrain coverage, crate-state roundtrip/rejection, destination collision and swept corridor checks pass. Windows build reports zero errors.

Native keyboard/mouse tests were authorized after the computer-use keyboard API failed to reach Unity. Tests read QA observations but never inject resources or gameplay commands. The initial anchor placement overlapped the sample house; it was moved beside the house and its dome recalculated. A duplicate preview process briefly made QA observations unreliable; both instances were closed and the earned tier-one save was then independently verified in one fresh instance.

Verified so far: five-hit tree harvesting, eight-hit limestone, wood/stone suction from about 5 m, no Aether acquisition around 11 m, four-bubble capture while sprinting backward, initial supported double jump, one atomic paid upgrade saved during animation and retained after restart. See the completed integrated checks below.

## Review boundaries
Technical checks do not approve game feel or art. Sound clips were generated and exported, but this environment did not provide usable audio listening input; bubble-pop, ding and rush listening review remains unperformed. Audio samples are in the local evidence folder. Cosmetic falling trees exert no forces but may visually intersect nearby objects if every candidate corridor is obstructed. The neighboring island is a landing and return destination, not a second progression system. Distant islands remain scenery. Every possible player building arrangement and every scenery tree/rock has not been individually playtested.


## Completed integrated checks
- All 36 required Aether, 36 wood and 24 stone were earned with native keyboard/mouse actions; the crate added six extra wood. Three atomic payments reached full attunement with six wood, zero stone and zero Aether left. No resource or gameplay-command injection was used.
- Initial supported double jump worked. Supported flight crossed the larger stages and the fully covered island, including a west-to-east pass at y=12. Fast-flight telemetry was approximately 10.4 m/s; the configured value is 10.5. The QA file updates every 0.4 s, so repeated samples were removed before estimating speed. Outside the dome, a sprint/double jump entered short lift then descent and landed safely.
- The outbound current reached a grounded neighbor arrival at approximately (150.94,17.035,0), with normal walking restored. Telemetry was approximately 13.86 m/s in its steady section, consistent with the configured 14. Sideways input held during travel left route Z deviation below 0.01 m; mouse look changed about 49.8 degrees. A second outbound trip also arrived safely in the final build.
- Pausing the return current held position for about 2.32 s. Saving mid-return settled at (58,4,0), with rush zero and the wind envelope faded to effectively zero. The resulting earned save reloaded successfully.
- A crate broke after three real impacts, yielded exactly six wood and restored as damage=3/depleted/collected after restart. Recover demonstration crates moved only the surviving crate; destroyed state and wood balance stayed unchanged.
- Earthstone attached to the demo handrail, accepted another undamaging impact and recalled to Prepared with one stone. A clear-sky cast triggered automatic distance return. Sampled Guided separation stayed below 18 m; turning during Returning briefly reached 20.90 m before recall completed. The 18 m setting is an outbound recall trigger, not a hard spherical clamp on the returning arc.
- The fixed anchor survived attempted MMB deletion and Shift+MMB selection without changing the 29-piece construction count or fixed identity. The inspected catalog contains 13 pieces and no anchor.
- The final dome was inspected from inside and from the neighboring island. Surface wind lines improve inside readability; the exterior is an upper dome. Screenshots include `final-dome-interior.png`, `final-dome-exterior.png`, `final-catalog.png`, `fast-flight.png`, `stage2-flight.png` and `harvesting.png` in the evidence folder.
- The previously earned M1.9 fixture loaded through Continue. All harvesting records, collected Aether IDs, tier and balance matched; all 29 non-anchor construction records matched exactly. The old free anchor was retired in favor of the fixed identity. An existing incompatible-layout fixture was rejected through Continue while the loaded world remained intact.
- Final build: `Evidence/TutorialWind/final-build.log`, zero build errors. Source/document whitespace checks pass. Unity-generated YAML was normalized to remove serialization-only trailing spaces.
- Final preservation: all 20 original non-log save/diagnostic files match the initial SHA256 manifest after restoring the four changed QA/diagnostic files. Completed earned progress remains separately in `Evidence/TutorialWind/completed-earned-playtest.json`. No personal Visual save was created.

## Known shutdown issue
Normal menu Quit repeatedly produced a native Windows/Unity process failure after Physics and Input shutdown, generally exit 0xC0000005 (one callback-failure variant). It also reproduced without screenshot capture and in the untouched M1.9 backup using the same launch/quit procedure. This comparison does not establish a new M1.10 regression, but the cause remains unresolved. No managed gameplay exception or save corruption was observed; full progression and crate state reloaded correctly. Logs: `player-single.log`, `final-player.log`, `exit-no-capture.log`, `baseline-exit.log`, and `legacy-player.log`.

The preview is delivered for the user's playtest. Audio listening, exhaustive scenery/contact inspection, focus-loss travel interruption beyond the shared pause handling, and every possible construction obstruction remain unverified; those are not represented as passed visual/game-feel checks.
