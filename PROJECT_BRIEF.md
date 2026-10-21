# Skywright

## Wind Magic visual overhaul — authorized 2026-10-20
Current follow-up implements dimensional suction/push VFX and Air controls: hold LMB suction, hold RMB push, Shift+LMB click toggle grab/release. It preserves existing spell forces, object eligibility, movement, Sentinel stagger and saved progression. Separate review builds live in `Builds/WindMagic_Preview` (launch with `-visualqa`) and `Builds/WindMagic_Combat`; prior executables remain retained. Read `ArtSource/WindMagic/REPORT.md` and the latest maintenance entry. Stop for Jonathan's visual/gameplay review after verification; no further combat or progression scope is authorized by this milestone.
First-person creative building and exploration, Unity 6000.4.7f1 / URP 17.4.0.

## Stone Sentinel combat trial — authorized 2026-10-19
Latest animation pass uses the four supplied FBX motions for walking, combo slams, a telegraphed gap-closing leap, and awakening from dormant terrain rocks. Current handoff: `ArtSource/StoneSentinelTrial/ANIMATION_REPORT.md`. This remains the isolated combat preview; stop for Jonathan's animation/feel review.
Current follow-up: auto-ready Earthstone on Q/Tab equip, reliable moving attachments, a three-hit swat requiring release/recast after recovery, repeat pull/push stagger, three armor-loss stages, stone breakup, refined limestone drops, and pursuit hysteresis to prevent abrupt disengagement. Keep this in the separate one-Sentinel combat trial. See the current revision in `ArtSource/StoneSentinelTrial/COMBAT_REPORT.md`.

Jonathan approved the actual Sentinel visual and explicitly chose "Playable combat: animations plus Earthstone damage and wind stagger." Implement one isolated playable encounter, preserving ordinary gameplay and personal saves. The aggregate Tripo cap remains $3; generation plus biped rig consumed 85 credits ($0.85). Animations are authored offline in Blender. Dedicated combat executable: `Builds/StoneSentinel_Combat/StoneSentinelCombat.exe`; original static viewer remains `Builds/StoneSentinel_Trial/StoneSentinel.exe`. Current handoff: `ArtSource/StoneSentinelTrial/COMBAT_REPORT.md`. Stop for Jonathan's combat playtest before adding other creatures or expanding combat into the saved island.

Current follow-up: M1.10.2 corrects the still-floating limestone presentations and removes the dome's polar sunburst. Jonathan's exact intermittent bright sky-disc flash remains unconfirmed; do not claim it resolved. See `MAINTENANCE.md` and `M1_10_2_ROCK_DOME_REPORT.md`. M1.10.1 movement/input, shoreline enclosure and frame pacing remain in place. Complete this integrated preview and stop for Jonathan's playtest; no new milestone or publishing is authorized.

## Core experience
Spot a distant island, prepare a way to reach it, explore, bring discoveries home, expand your creations.
The home island stays stationary. Aether-fed wind anchors expand local hover space and attune the island, opening a wind-stream journey to one neighboring island. Airships remain optional future content.

## Priorities
1. Comfortable, expressive construction.
2. Personal architecture and decoration.
3. Readable, rewarding exploration and eventual island-to-island travel.
4. Discoveries that inspire construction.
5. Reliable interactions and persistence.

Handcraft the starting archipelago. Frutiger Aero mixed with cozy fantasy: pale timber, pearl-white frames, aqua accents and teal roofs; lush green vegetation, limestone cliffs, blue/cyan sky and warm lamps. Matte wood/stone, satin frames, controlled glass reflections. Preserve first-person construction readability.
Use free, already-owned or original assets. No paid runtime APIs or mandatory online accounts.

## Milestone 1: First Home
One island; first-person movement; approximately 12–15 compatible pieces; snapping, rotation, free placement, removal; unlimited materials; safe recovery; local saving; coherent visual atmosphere.
Verify construction and traversal through actual player input, then persistence across a session restart.
Technical verification does not establish enjoyment. Stop for Jonathan's playtest.

## Milestone 1.5: Art Direction and Replaceable Building Kit
M1 passed Jonathan's initial playtest. Checkpoint A is authorized: wheel rotation, Shift+wheel reach, a representative prefab/catalog art set and playable isolated visual sample.
Checkpoint A stops for Jonathan's in-game visual review. Technical success is not visual approval.
Checkpoint B (remaining kit conversion and full island presentation) requires explicit approval. No M2 systems.
Three approved concept images guide palette/materials/shapes; they are not existing assets or a guarantee of offline-render detail.
Sample directions: pale natural timber, rounded pearl-white frames, blue-green glass, teal roofs; limited lush meadow/limestone planting and a bounded turquoise decorative pool.
Keep current building area, IDs, pivots, snapping and deliberate collision. Avoid dark medieval, all-brown rustic, neon sci-fi, resort/spaceship styling and costly photorealism.

## Historical Milestone 2 proposal: First Expedition — NOT AUTHORIZED
A fixed-hull starter airship with limited customization, reliable walking aboard, forgiving travel/docking, one neighboring destination, one collectible usable at home, and persistent ship/cargo state.
Unlimited common materials do not automatically unlock the destination's unique discovery.
Requires explicit authorization following M1 feedback.

## Later candidates, not commitments
Creative and Expedition modes, cargo cranes, lifts, greenhouses, simple powered devices, larger ships.
Resource gathering and deeper progression follow validation of building and travel.

## Exclusions for first two milestones
No moving islands, multiplayer, combat, NPC crews, procedural worlds, survival meters, recurring base destruction, freeform arbitrary ships, structural simulation, broad technology trees or complex automation.

## Working rules
Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright on MashPC.
Do not modify sibling projects. Preserve existing work and local instructions.
Implement sequential checkpoints, test actual gameplay, document failures honestly.
Do not publish the repository or upload project contents without separate authorization.

## M1.5 playtest follow-up (2026-10-18)
User feedback authorized focused snapping and rendering corrections within Checkpoint A: ground-aware stair placement, rotation about attachment points, roof-to-roof joins, named Tab snap modes and Shift + middle-click piece selection. Preserve saved transforms and existing functional dimensions. This follow-up does not initiate Checkpoint B or M2. Current controls and verification are recorded in MAINTENANCE.md and M1_5_ART_DIRECTION_REPORT.md.


## M1.6 Earthcasting experiment — authorized 2026-10-18
Implement an isolated trial: collect stone charges, launch earth at trees/outcrops, collect wood within a tunable nearby radius, spend wood on deck extensions. Existing creative building remains unlimited. No survival meters, mana, combat, terrain deformation, airships or full kit conversion. Uncollected yield waits at its resource node and survives saving/reloading. Finish a playable Windows preview and stop for Jonathan's playtest.

## Future air magic — design direction, NOT current implementation
Jonathan approves exploring controlled levitation for building; lifting/carrying materials and cargo; slow descent; local updrafts; local aimed gusts; later sail propulsion, docking assistance and simple wind-powered equipment. Local wind anchors may sustain building hover; away from anchors, brief lift and slow descent should preserve airships' travel role. Favor visible effects on leaves/sails/chimes over global weather control. Ship wind focus may later sustain a breeze without repetitive casting.
Do not levitate trees or harvested wood at this stage: trees use radius-based automatic wood collection. Use dedicated example crates on the ground for the future air-spell lifting test. Do not make hauling a mandatory replacement for automatic harvesting. Tree-fall guidance remains a later candidate, not an authorized tree-hover mechanic. The air milestone should test crates and construction/traversal utility first. No air ability is authorized as part of M1.6.


## M1.7 — Air magic and creative mobility (2026-10-18)
M1.6 passed Jonathan's playtest. This separately authorized milestone implements free-aim Earthcasting, two explicit liftable crates, a small directed gust, one placeable wind anchor, local building hover, limited personal lift and gentle descent. Both internal checkpoints are authorized; stop after the integrated Windows preview for Jonathan's feedback.
Air uses thin tapered white/aqua ribbons, sparse leaves, eased movement and original sound layers, guided by the three attached crate-lift, hover and gust storyboards. The existing island and unconverted building art remain unchanged. The compact first-person focus is intentional; concept hands/full bodies are not claimed as implemented.
Hover is independent of the selected construction/magic tool. Earth remains harvesting; trees and waiting wood are never liftable. Existing 3.5 m automatic collection remains. No mana, stamina, hunger, combat, airships, M2, global weather or full M1.5 conversion.
Elemental saves are isolated; Earthcasting can be imported as a validated copy. Preserve explicit save markers and stable identities. Current controls and evidence are in MAINTENANCE.md and M1_7_AIR_MAGIC_REPORT.md. Earlier milestone restrictions above are historical to those milestones.


## M1.7.1 — Interaction polish (authorized 2026-10-18)
User playtest approved the M1.7 foundation and authorized guided long-range Earthcasting, sustained Air wind, movable existing furniture, corrected object wind wrapping, Ctrl descent, initial anchor lift, support-loss recapture, faster descent, a thin visible anchor bubble, bubble-pop interactions and UI/control corrections. Building RMB opens the kit; MMB removes; Shift+MMB remains copy-type. Furniture preserves existing construction identity and refunds. Distant harvesting never bypasses the local collection radius. Stop after the verified integrated preview for playtest. No M2, new elements, airships, survival meters or full kit conversion.


## M1.7.2 — Reusable Earthcasting and spell audio polish (authorized 2026-10-18)
User feedback authorizes Air LMB levitation and held RMB wind; gentler bubble-pop interactions; continuous comfortable wind audio; and a reusable Earth stone: prepare, launch, mouse-guide, release to gravity, hold RMB recall, then relaunch. One charge owns one stone until impact/expiry; recall cannot duplicate ammunition or bypass obstacles. Prepared stones persist through tool changes and saving. Keep existing local harvesting, construction, hover and save isolation. Stop after verified preview and local Git checkpoint for the user's gameplay/audio review. No M2 or additional elements.


## M1.7.3 — Returning Earthstone (authorized 2026-10-18)
Replace resource-consuming Earthcasting with one freely reusable magical stone. LMB summon, launch and repeat-strike an explicitly bound surviving breakable; RMB click recalls. Preserve outbound mouse guidance; remove ballistic-release input. Breakable depletion and bounded unaffected-surface ricochets automatically return the stone. Returning never attacks; blocked returns have bounded visible recovery. Preserve stored resources without historic refunds, local harvesting collection, Air, hover, construction, save identities and protected load failures. Replace only the spell rock with a larger coherent natural limestone mesh suited to the existing bright art direction. No mana/stamina/cooldown economy, new elements, airships or M2. Complete standalone input verification, document limitations, and stop for Jonathan's playtest. This supersedes prior Earth ammo and recall controls.


## M1.7.4 — Spell Feel, Hover Capture and Aero UI (authorized 2026-10-18)
User feedback authorizes Tab wand equip/stow and Shift+Tab construction snapping, five-hit trees/eight-hit limestone with explicit legacy damage migration, guided surface ricochets with shorter shared flight limits, continuous moving-wand catch/relaunch, faster free summon including airborne above explicitly eligible earth, immediate wind-anchor capture with intentional-cancel protection, placement-only gentle bubble audio, and a calmer rounded translucent pearl/aqua interface. Preserve the single stone, bound-target click strikes, local harvesting yields, Air, building, saves and existing art contracts. No new elements, flight/airships, survival meters or M2. Complete protected standalone/input verification, document limits, and stop for Jonathan's playtest. Current controls supersede previous milestone bindings; historical reports remain historical.

## M1.7.5 — Compact Glass HUD and Earthstone Tuning (authorized 2026-10-18)
Focused playtest polish: narrow nearly clear bottom HUD only; outbound Earthstone speed18m/s with20m player range,40m cumulative path and3s active flight budget; distinct RMB closes the building palette into construction and consumes that press. Preserve recall/catch/strikes/summon, five/eight-hit harvesting, single free stone, Air, construction contracts and saves. No M2 or unrelated systems. Stop after verified preview for Jonathan's playtest.

## M1.8 — Natural Materials and Frutiger Aero Environment, Checkpoint A
Authorized 2026-10-18: a playable garden/deck art sample using free CC0 natural surfaces, realistic fern/leaf sources and a natural daylight panorama, preserving the pale timber/pearl/aqua/teal fantasy palette. Improve representative geometry and materials while keeping the kit's root, identity, dimensions, snapping, collision and save contracts.
The separate Visual Preview uses its own save slot and preview-only catalog. One harvestable tree and one decorative tree demonstrate the new canopy; the wider island/remaining kit are not converted. Preserve Earth/Air/hover/harvesting and M1.7.5 tuning. Existing Elemental preview stays available.
Report: M1_8_NATURAL_AERO_VISUAL_REPORT.md. Stop for Jonathan's visual approval. Technical verification does not authorize full conversion or M2.

## M1.8.1 — Mountain Island and Visual Rebuild
The user rejected the M1.8 sample as insufficiently improved. M1.8.1 authorizes a substantial representative environment rebuild using the three supplied concept references: cottage with mountains, first-person deck materials, and a foothill route. Those references are composition/palette targets, not verified Unity screenshots or an offline-render quality promise.

Current direction: believable limestone landforms and vegetation with pale timber, satin pearl frames, aqua glazing, teal roofs, rich blue/cyan sky and restrained warm lighting. The larger stationary home island is approximately 1.15 km by 0.78 km, with a connected range reaching roughly 240 m, a preserved home plateau and a short walkable foothill route. Neighboring islands remain distant scenery. Keep the bright Frutiger Aero/cozy fantasy identity and practical creative-building focus.

This pass uses free CC0 scanned tree/rock/fern sources and existing ambientCG materials, plus original authored terrain, foliage placement and preview-specific URP lighting. Blender use is authorized; editable sources belong in ArtSource/MountainIsland. Stable construction contracts, personal saves, current magic/harvesting rules and the separate Elemental build remain protected. No airships, additional magic, procedural gameplay world, fluid simulation or full kit conversion.

The representative build is ready only for the user's visual assessment after documented technical verification. Do not treat successful compilation, imported textures or passing tests as visual approval. Further island detailing and full construction-kit conversion require approval of this actual playable sample.
Current report: M1_8_1_MOUNTAIN_ISLAND_VISUAL_REPORT.md. Current build workflow and controls: MAINTENANCE.md, M1.8.1 section.


## M1.8.2 — Finished Meadow, Natural Cliffs, and Owned Asset Integration
The M1.8.1 scale was useful, but its visual presentation was rejected. This authorized pass concentrates meadow, woodland-edge, outcrop and ground-cover detail around the home and the beginning of the foothill route; retains the larger island and mountain range; corrects nearby cliffs and neighboring island silhouettes; and repairs tree-base/damage-mark integration. It does not authorize full-island dressing, full kit conversion or M2.

Owned Nature Package - Forest Environment 1.9 is selectively integrated under its Asset Store license. Environment Vegetation - Bundle 1.0 was identified and inspected but not imported. Keep licensed source content local. Blender is authorized and editable landform sources are in ArtSource/FinishedMeadow. The direction remains believable natural materials within bright Frutiger Aero/cozy fantasy, with functional first-person building space.

The home plateau, construction identities/contracts, harvesting roots/capsules, resources, Earth/Air tuning and personal saves remain protected. Fine dressing is bounded; distant peaks and scenery are not fully detailed destinations. Technical checks and imported assets do not establish visual approval. See M1_8_2_FINISHED_MEADOW_CLIFFS_REPORT.md for the actual results. Stop after this sample for Jonathan's visual review.

M1.8.2 steering: placed scenery trees and limestone rock props are harvestable with the established five/eight-hit Earthstone rules and local collection. Replace the two handmade limestone resource visuals with the imported rock asset. Structural cliffs and distant scenery remain intact. Preserve prior node identities/resources through an explicit v3 harvesting-state upgrade and serialize new scenery GUIDs in the authored island.


## M1.9 current direction: Compact Sky Island, Daylight, and Aether
This milestone supersedes earlier mandatory-airship progression and Air mouse bindings. The Visual Preview has a compact handcrafted home island, central limestone ridge, five scenic neighbors, continuous natural harvesting-tree trunks, stable daylight, and island-wide vegetation treatment. Source dimensions are approximately 270 x 215 m with a sampled 48.58 m ridge peak; the original home radius of 38 m retains its ground plane and construction transforms.

Air held LMB pulls eligible props and physical Aether bubbles; Shift + held RMB pushes; plain RMB grabs/releases a prop. Earth remains harvesting. Aether is finite, hand-placed and permanently collected. Feeding the primary anchor grows Workshop, Garden, Settlement and Island fields, with no upkeep. One active anchor carries island progression; extras remain dormant. The final current is a persistent visual landmark, not a usable travel corridor.

Both implementation checkpoints were authorized. No airships, actual inter-island travel, new elements, survival meters, recurring costs, procedural runtime world or broad kit conversion. Existing paid/owned assets remain private. Technical verification is not visual approval; stop for Jonathan's playtest. Current controls, costs and compatibility limits are in MAINTENANCE.md and M1_9_COMPACT_ISLAND_AETHER_REPORT.md.


## M1.10 Tutorial Wind (2026-10-19)
The user authorized the compact tutorial island, fixed anchor, island-relative dome growth, suction/drop/harvest improvements, double-jump flight, full-attunement fast flight and one working wind-stream journey to a collidable neighboring landing. This supersedes the earlier visual-only current restriction for this one journey. Preserve construction identities/transforms, earned progression, explicit resource IDs and isolated personal saves; retain M1.9 for incompatible layouts. No airships, new elements, survival systems, broad multi-island progression or wider kit conversion. Deliver the integrated preview, report and local Git checkpoint, then stop for Jonathan's playtest. Technical checks are not gameplay or visual approval.
