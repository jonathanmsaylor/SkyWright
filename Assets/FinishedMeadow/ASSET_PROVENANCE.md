# M1.8.2 owned assets and original environment work

## Adopted owned package
Nature Package - Forest Environment, ALP (current listing: OE Objective Environment), asset 165645, version 1.9, published 2024-11-21, original Unity 2021.3.38f1. Version and identity were read from the actual local package header, not inferred from a screenshot.
Source: https://assetstore.unity.com/packages/3d/vegetation/nature-package-forest-environment-165645
License: Standard Unity Asset Store EULA, https://unity.com/legal/as-terms . This is owned commercial content, NOT CC0. Incorporate in the game; do not redistribute the source assets separately or publish this repository.

Selected content: MapleSpringTree01/03, ForestTree01/03, ForestBush01/02, MapleSpringBushes01, GrassPlant03/04, FlowerGrass01/03, Grass01 atlas, GroundGrass01, PathGround01, GroundForest01, their required meshes, maps, URP materials and two required ALP shaders. Other small selected prefabs were imported for evaluation; actual references are in the authored Island and FinishedMeadow prefabs. See ignored Evidence/FinishedMeadow/selected-assets.json for the exact GUID/path manifest.

The cached package and its URP/shader subpackages were inspected outside Assets. Only selected native files and original .meta GUIDs were staged through Unity's ordinary asset import pipeline. No vendor demo scenes, terrain assets, quality/graphics presets, controllers or global lighting tools were adopted. No account sign-in, purchase or model upload was needed.

Included shaders: ALP Cutout Translucency Wind and ALP Surface Detail Wind, DE Environment v11.2.0, distributed as part of the owned package. The supplied ReadMe and Change Log remain in Assets/ALP_Assets/Core/Shaders. The supplied release notes explicitly cover URP 17x; actual Unity 6000.4.7f1 / URP 17.4.0 build verification is documented in the milestone report. Amplify Shader Editor was used by the publisher to create the shader source; it is not introduced as a project dependency.

Adaptations are separate materials/prefabs in Assets/Resources/FinishedMeadow: restrained wind, adjusted daylight brightness, shorter consistent LOD transitions, removal of scenic billboard LODs, 2K selected terrain maps. Harvesting maple meshes are baked to player scale with lower bark geometry fitted to the existing 0.3 m radius / 3.6 m harvesting capsule. Gameplay roots, capsule dimensions and saved node coordinates remain unchanged. A readable render-only bark surface fits the small damage scar; it is not a physics collider.

## Evaluated but not adopted
Environment Vegetation - Bundle, ALP, asset 273059, version 1.0, published 2023-12-25, original Unity 2021.3.8f1. Exact identity verified from local cache metadata.
https://assetstore.unity.com/packages/3d/vegetation/environment-vegetation-bundle-273059
Its cached inventory includes Birch Forest Environment, European Vegetation Pack One, Oak Forest Pack, Spruce Trees Pack, Swamp Forest and Realistic Grass Flowers Pack. The latter primarily supplies grass/flower texture cards. No bundle assets were imported into the project; the Forest Environment set was sufficient for a coherent sample. No claim of complete runtime evaluation of every bundle model.
The other owned stylized/city packages were not needed.

## Reused CC0 sources
The existing Poly Haven Coast Rocks 05 limestone and Fern 02, plus ambientCG Rock030 are retained. Existing house materials remain. See Assets/MountainAero/ASSET_PROVENANCE.md and Assets/NaturalAero/ASSET_PROVENANCE.md for source links and earlier transformations. These licenses remain separate from the owned Asset Store content.

## Original work
- Authored curved grass-card geometry using the owned Grass01 atlas; original URP grass shader with restrained world-phased wind and distance dithering.
- Ground meadow/soil/litter blending and mountain blend corrections.
- Deliberate woodland/outcrop composition stored as ordinary Unity prefab instances.
- Blender-authored quad-grid floating islands with multiple uneven lower masses, plus weathered limestone buttresses. These do not converge on a central underside triangle fan.
- Tree render-surface scar fitting and conditional felled stump presentation.

Editable landform source: ArtSource/FinishedMeadow/Landforms.blend and author_landforms.py. Explicit exports: Assets/FinishedMeadow/Models/*.fbx. Recorded source mesh budgets: ArtSource/FinishedMeadow/landform-budgets.json. Authoring script: Assets/Skywright/Editor/FinishedMeadowAuthor.cs. Grass and harvesting meshes are editable Unity assets in Assets/Resources/FinishedMeadow.

Shader compatibility edits: replaced obsolete FORWARD_PLUS keyword/macros with URP 17.4 CLUSTER_LIGHT_LOOP equivalents in the two selected ALP shaders; removed a duplicate soft-shadow pragma. These are targeted source adaptations, not a project-wide material conversion. Final builds report no shader errors.

The existing CC0 Coast Rocks 05 model is also reused for structural cliff accents and the replacement original limestone harvesting visuals. No additional rock package was purchased or represented as owned Asset Store content.


## M1.9 adaptations
The earlier lower-bark fitting described above was removed: source trunk shape, normals and UVs now remain continuous under uniform .36 scale. Deliberate capsule collision was fitted to it (radius .17, height 3.3); identities and original node roots remain unchanged.
No new third-party content or accounts were used. Existing owned ALP trees, shrubs and grass, CC0 limestone/ferns/Rock030, and the existing original Blender buttresses are reused.
New original work: offline CompactIslandAuthor meshes and layout, mineral variation in MountainGround.shader, AetherBubble shader, explicit bubble catalog, bounded wind-current lines and feeding/pulse presentation. These do not change the source asset licenses.
Current editable landform recipe: Assets/Skywright/Editor/CompactIslandAuthor.cs; ordinary generated mesh assets: Assets/Resources/CompactIsland/Mesh_*.asset. Prior Blender source is retained at ArtSource/FinishedMeadow/Landforms.blend. No new Blender animation/export is claimed.
