# M1.8 — Natural Materials and Frutiger Aero Environment, Checkpoint A

2026-10-18 local / 2026-10-19 UTC. Parent checkpoint 0da711e. Stop for Jonathan's visual approval. This sample does not authorize full kit/island conversion or M2.

## Launch and isolation
- Project: C:\Users\jsayl\Desktop\UnityProjects\Skywright
- Unity 6000.4.7f1; URP 17.4.0; existing Assets/Scenes/FirstHome.unity unchanged.
- New Windows preview: Builds/Visual_Preview/Skywright.exe
- Desktop shortcut: Skywright Visual Preview
- Shortcut arguments: -visualpreview -screen-fullscreen 0 -screen-width 1600 -screen-height 900
- The sibling visual.preview marker also makes direct executable launches use the visual mode.
- Normal preview save: %USERPROFILE%\AppData\LocalLow\Skywright\Skywright\visual-preview.json
- Verification: same directory, verification-visual.json with -visualqa. Each explicit replacement retains .bak.
- Authored demo: Assets/StreamingAssets/natural-sample.json, 22 ordinary usable construction pieces.
- Choose Explore visual sample. Reset visual sample resets only the current session; F5 explicitly saves the isolated slot. Continue loads that slot.
- Existing Elemental executable/shortcut and all previous builds remain available and were not rebuilt or replaced.

## Change summary
A geographically limited garden and deck sample now combines pale birch surfaces, rounded pearl frames, aqua glazing, teal roof panels, warm lamps and planters with a grass texture, sparse original blades/flowers, imported fern clumps, a fuller broadleaf canopy, pale irregular cliff dressing and a small visual turquoise pool. A 4K natural sky panorama supplies cloud detail and environment reflections. The cottage has two open entrances and a usable rear deck.

Preview-only catalog variants retain the nine existing M1.5 piece types. Stair, post, beam, bench, wind anchor, other trees and distant island art remain provisional. The first harvestable tree (earth-trial-tree-0 at -4,0,-3) receives the new visual canopy; a non-harvestable decorative instance frames the pool at -8,0,-11. These share authored geometry/materials.

Initial gameplay inspection found the natural foliage/rock too dark and grass entering the deck footprint. Color/material adjustment brightened the sample; grass is excluded from the sample deck area and cliff dressing is kept outside the rear deck footprint. Final tree assets include a half-density distant foliage mesh and culling; fern groups use distance culling.

## Asset workflow and provenance
Full source links, licenses, resolutions and modifications: Assets/NaturalAero/ASSET_PROVENANCE.md.
Imported free CC0 sources: ambientCG Wood091A, Grass004, Rock030; Poly Haven Fern02 and Kloofendal48d Partly Cloudy Pure Sky; cgbookcase GreenLeaf12.
Most textures are 2K. Sky uses 4K because it spans the entire view. Leaf/fern opacity is packed into color alpha. Rock albedo is adapted toward pale limestone; birch contrast/color is restrained. Original bark maps, branch/canopy meshes, grass/flowers, cliff meshes and sample composition were authored for Skywright.
No purchases, accounts, runtime services, package replacement, Blender dependency, Addressables or render-pipeline migration.

Assets/Resources/NaturalAero contains ordinary prefabs, meshes, materials and Catalog.asset. PieceCatalog selects this explicit catalog only in visual mode; ordinary mode retains SkywrightCatalog. The root, stable type ID, collision branch, dimensions, origin and snap anchors are copied unchanged. Birch UVs are metre-scaled on cloned visual meshes. Thumbnails continue to derive from the actual selected prefab geometry. Swapping a visual does not resnap or migrate saved objects.

NaturalPreview.Tree replaces rendered presentation under the existing EarthNode crown. The original harvesting collider, crown pivot, explicit state identity and fall/loot rules remain authoritative. No second harvesting system.

Rebuild without regenerating art: Skywright.Editor.BuildNaturalPreview.Build.
Intentional sample asset regeneration: Skywright.Editor.BuildNaturalPreview.AuthorAndBuild. This overwrites NaturalAero authored assets; do not run it after hand-editing them without reviewing changes. It does not regenerate the original kit/Earth/Air assets.
Both use the existing FirstHome bootstrap scene. Runtime launch flags/marker select the sample. To view this mode in the editor, launch that editor/project with -visualpreview; ordinary editor Play retains its existing mode.

## Verification performed
Actual standalone Win32 keyboard/mouse input; read-only QA observations, no game-state command injection.

- 1920x1080 and 1280x720 gameplay inspected. Bottom glass HUD remains readable; retained menu/palette, crosshair and ghost controls operate.
- Walked through the front doorway, under the pitched roof, through the rear entrance and onto the deck. Inspected roof interior/exterior and material detail.
- Placed a free-positioned deck, snapped a wall, snapped a roof to that wall and a railing to the deck. Piece counts 22 -> 23 -> 24 -> 25 -> 26.
- Wheel +45 then -45 restored rotation; Shift+wheel changed reach 8 -> 8.5 without rotation; Shift+Tab selected Bottom and showed the named snap connection.
- Shift+MMB copied the wall type without adding/removing a piece; MMB removed exactly that wall (26 -> 25) and left the roof/deck/railing.
- B-opened and RMB-opened palettes both closed into construction with RMB. Holding the closing press did not reopen/cast; counts remained stable.
- Earth summon/launch against the new harvestable tree: each impact increased damage once; hits 1–4 survived, hit 5 depleted. Falling visual, fracture and rebound aligned with the trunk. Automatic return restored ready state.
- Yield remained uncollected at distance (0 wood), then entering the collection area awarded 12 wood. The placed deck consumed 6, leaving 6. No spell resource cost.
- Manual RMB recall restored ready state after an outbound launch.
- Held RMB wind recorded WIND_BEGIN / WIND_TARGETS count=1 / WIND_STOP and moved the crate. An initial Air pickup aim missed; a corrected aim subsequently recorded AIR_GRAB air-trial-crate-1 and an observed held crate. This correction is retained in raw evidence.
- Anchor hover rose from y0.035 to y5.009 with Space, Ctrl lowered to y3.417, R cancellation returned to ground.
- Genuine executable close/restart and explicit re-save: all 25 piece records, complete Earth state, complete Air state, player position/yaw/pitch and schema/discriminator fields matched exactly. Includes depleted/collected sample tree, 6 wood, ready stone and moved crate.
- Protected copy of the user's earlier save loaded in the shipping build: all 8 piece records, identities/transforms/payments and player position matched. It is an Earth v1 fixture: existing M1.7.4 migration changed schema1->2, depleted trees3->5 and limestone2->8 as intended. Inventory stayed 36 wood / 12 stone, ready flag and all depletion/collection/fracture records retained. One Air crate's y/safe.y settled by -3.725290298e-9 metres; no meaningful position drift. No new migration was added.
- Build contracts compare all nine sample type IDs, dimensions, origins, named anchors and 70 colliders against the original catalog; identical. Existing Earth/Air save gates pass.
- Shipping build succeeded, errors=0. Final log/preservation checks are recorded below.

## Performance observations
Machine: NVIDIA GeForce RTX 4070 SUPER. Standalone 1920x1080, target120fps, vSync0.
Before, stationary at starting lawn: three samples averaged 8.34 / 8.33 / 8.33 ms (1199/1200/1200 frames per ~10s).
Sample build gameplay likewise held approximately 8.33ms at the cap in stable views. Capture/interaction windows include occasional longer frames; these are not claimed as uncapped GPU benchmarks. The cap masks available headroom. No claim about low-end hardware, peak memory or production-scale vegetation.

## Preservation
Initial tree clean at 0da711e; no live Unity/game session. Required documents and actual code/assets/save contracts inspected; no applicable AGENTS.md found in project/parents. Original save files were backed up to ignored Evidence/NaturalAero/saves-before before tests.
Testing used verification-visual.json plus a protected source copy, never the personal save. No previous build replaced. Original resources, gameplay tuning, IDs, personal construction and unrelated work preserved.
Earth remains 18m/s, 20m player range, 40m accumulated path, 3s active flight; tree5/outcrop8. Return, catch, strike, summon, local collection, Air and hover code is unchanged except the visual-only tree hook.

## Evidence
Local, Git-ignored: Evidence/NaturalAero.
- baseline.png and baseline.log.
- overall-corner.png, interior-materials.png, roof-interior.png, roof-exterior.png, placement-view.png.
- harvest-tree.png, tree-falling.png, compatibility-720.png, vegetation-cliff-720.png.
- Final shipping captures use final-* names.
- input-evidence.jsonl, saved-before/after-restart.json, restart-comparison.json.
- compatibility-fixture.json, compatibility-resaved.json, compatibility-result.json.
- contracts.txt, build/final logs, shortcut.json, preservation manifests.
- Download sources and offline preparation scripts. Source archives are not packaged as player dependencies.

## Limits and visual decisions
This is a representative in-game sample, not concept-render parity or a full realistic environment conversion. Surrounding legacy trees, distant islands/cloud spheres, original planter leaves and several unconverted pieces remain visibly simpler. The natural sky is static; lower-hemisphere panorama detail and bright cloud highlights are provisional. No volumetric weather.

Pool is bounded visual dressing with no water collision, swimming or simulation; grass, ferns, roots and cliff dressing add no movement blockers. Existing terrain collider remains unchanged. New player builds can intersect decorative planting, as with the prior non-colliding sample garden.

Leaves use the front scan on both sides, with cutout rendering and two density levels; no physically based leaf transmission or per-leaf wind. Harvestable crown retains existing subtle sway and fall; decorative tree is static. Tree art can be refined further after review.

No exhaustive all-piece/all-save/ultrawide/low-end benchmark. No new blocked-return stress run or independent flight-limit stress run; unchanged tuning and logic checked, ordinary casting/recall exercised. Technical checks do not establish visual approval.

Stop for Jonathan's playtest. No Checkpoint B, full island conversion or M2.

## Final review notes
The matching performance view used player (0,0.035,-10), yaw0.033/pitch11.986 versus baseline yaw0/pitch12, same1920x1080 and120fps cap. Stable shipping samples were8.33ms, with worst frame8.67/8.54ms in those windows. Initial loading/input window averaged8.35ms with23.84ms worst. This establishes capped responsiveness only.
Final lamp adjustment reduces only the preview lantern's point light from2.4 to0.9, range5 to3.8m, to restrain hot spots on the surrounding timber and white frames. Emissive diffuser and original Elemental lantern stay unchanged.

Final preservation audit: all16 original save/backup files are byte-identical. The two pre-existing player logs are also unchanged. Only the pre-existing read-only QA diagnostic outputs (qa-state.json and m15-loaded-audit.json) changed. The new verification-visual slot remains isolated; no personal visual save was created by testing. The original Elemental build's newest file timestamp remains01:18:51UTC, before this pass.


Final shipping verification: build-shipping.log reports NATURAL_VISUAL_BUILD Succeeded errors=0 and Unity exits with code 0. Final runtime logs contain no introduced compilation, shader, exception, missing-reference or assertion errors. The final lamp-adjusted executable was launched and traversed through normal player input. Unity and Skywright were closed after verification.
Personal elemental save SHA256 remains 017553DCF8F30CB2D7338FFB208548995CC5ACA7C4DB7BB7B659CD09B0B0CFF5. Local Git checkpoint accompanies this report; no repository publication.
