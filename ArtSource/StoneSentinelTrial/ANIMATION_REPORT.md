# Sentinel supplied-animation pass — 2026-10-19

This pass uses the four FBX files provided by Jonathan. It remains in the isolated `Builds/StoneSentinel_Combat/StoneSentinelCombat.exe` encounter. The ordinary saved island and static viewer are separate.

## Motion and behavior

- **Zombie Stand Up:** the Sentinel initially lies motionless among seven cosmetic limestone stones with emission disabled. Approaching within 8 m with line of sight, striking it with Earthstone, or directing wind at it starts a 3.1 s rise. It cannot attack during the rise. Its core gradually lights and the loose disguise stones recede. It wakes once per encounter; returning home does not replay the reveal. Retry restores dormancy.
- **Mutant Walking:** replaces the authored walking loop. Animation is baked in place; the existing controller moves the body and handles terrain.
- **Dual Weapon Combo:** drives a 0.9 s windup, 2.65 s three-hit slam combo and 0.8 s recovery. Damage beats occur 0.25/1.25/2.25 s into the combo for 8/8/9 damage (25 total). The marked area locks after the early windup so the player can leave it. Existing wind stagger can interrupt the combo. No weapon meshes were added.
- **Mutant Jump Attack:** drives a 0.65 s takeoff windup, 1 s flight and 1.15 s recovery. An awakened, engaged Sentinel can leap toward a visible target 8–16 m away after its cooldown. The destination locks when the warning begins; the player can dodge it. Seven-second cooldown starts on commitment. Wind can interrupt takeoff; the airborne portion is committed. Landing deals one 25-point hit within 2.5 m, and recovery exposes the core.

Leap movement belongs to the CharacterController rather than imported root motion. It follows a 2.3 m parabolic arc. Planning checks the landing footprint, slope, height, home leash and capsule clearance along the arc. Invalid destinations or obstructions keep it walking. A newly encountered obstruction cancels impact damage and settles the body. It does not leap at distant high-flying targets or deliberately cross a cliff edge.

The existing Earthstone auto-ready, moving attachment, three-hit swat/recast gate, repeat wind resistance, armor loss and polished breakup remain.

## Art pipeline

Inputs are copied, unchanged, to `ImportedAnimations/`; the original Downloads files remain untouched. `retarget_imported_animations.py` reads `StoneSentinelCombat.blend`, maps shared Mixamo bones in model space, preserves the Sentinel bone lengths, removes source travel, grounds the surface offline, and exports 13 Unity clips. Missing finger channels inherit the hand motion; the four source files do not animate all of the Sentinel's finger bones.

The large source gestures were adapted to the Sentinel's broad stone proportions: shoulder rotation is limited to 95 degrees and forearm rotation to 115 degrees. The existing geometry, UVs, materials and skin weights are retained, including the original 69,999 outer triangles. Source/output: `StoneSentinelAnimated.blend`, `imported-animation-report.json`, and `Assets/StoneSentinelCombat/Models/SentinelCombat.fbx`. The prior authored Blender file is retained. The model still uses blended skinning; final deformation/animation quality needs Jonathan's visual review.

No Tripo calls, uploads or charges. Recorded aggregate remains $0.85 of the original $3 limit.

## Verification

The final Windows build succeeded with zero build errors. All 131 regression checks passed, including the prior reusable-stone, swat, wind, armor and aggro checks plus wake-up line of sight, disturbance, protected rise, one-time reveal, leap targeting, flight, dodge, damage, cooldown, wind interruption, obstruction and retry. The final verification player exited cleanly (code 0). No managed exceptions were found.

Native input confirmed Dormant at the 10 m spawn, Awaken after walking to 7.2 m, then walking pursuit. Retreat triggered an airborne leap (observed root height 1.91 m), which landed behind the retreating player without dealing damage. The subsequent combo changed player health 100 → 92 → 84 → 75; pausing froze the attack clock and retry restored the dormant encounter. These were real keyboard/mouse events, separate from simulation.

All 386 protected-file hashes match the starting baseline, including personal saves and both existing preview builds. All four copied animation files match their untouched Downloads originals. No paid operations occurred. Actual Unity captures are in `AnimationReview/`; private build/test logs and preservation hashes are in `Evidence/PlaytestFollowup/StoneSentinel/AnimationPass/`.

Navigation remains basic controller steering rather than full obstacle pathfinding. This is a playable motion pass for Jonathan's review, not final combat balance. The previously documented intermittent native Unity shutdown failure remains a known issue; a clean exit in a single test does not establish a fix.
