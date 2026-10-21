# Mountain island asset provenance
M1.8.1 uses ordinary exported meshes and URP materials. No runtime services or paid assets.

## New external sources
- [Tree Small 02](https://polyhaven.com/a/tree_small_02), Rico Cilliers, Poly Haven, CC0. Downloaded source Blender model and 2K trunk, branch, leaf color/normal/roughness and leaf opacity textures. Prepared three explicit LOD meshes in Blender 5.1.2; removed unused vertices, smoothed faces, exported FBX with Unity orientation, packed source textures. Leaf color and opacity combined into RGBA. Shared URP alpha-cutout leaves; conservative normal strength. This is one tree species with scale/rotation variation, not a full vegetation library.
- [Coast Rocks 05](https://polyhaven.com/a/coast_rocks_05), Rob Tuytel, Poly Haven, CC0. Downloaded source Blender model and 2K diffuse/normal/roughness maps. Exported three decimated FBX LODs. Color adjusted toward pale limestone; collision uses the low-detail source surface.
- License verified at [Poly Haven license](https://polyhaven.com/license). CC0 permits game incorporation and modification. Source metadata and download MD5 checks are in ignored Evidence/MountainAero/sources.

## Existing imported assets reused
See Assets/NaturalAero/ASSET_PROVENANCE.md for original attribution and license records:
- ambientCG Wood091A: pale timber.
- ambientCG Grass004: meadow ground.
- ambientCG Rock030: triplanar limestone cliffs and mountain slopes.
- Poly Haven Fern 02: grouped ground planting.
The existing nine-piece NaturalAero construction catalog remains in use. No full kit conversion.
The previous HDRI is not used for the mountain sky; the lightweight Skywright sky shader avoids a terrestrial lower hemisphere.

## Original project work
- Fixed island outline, connected ridges, meadow transitions, foothill route and cliff underside: authored offline by MountainAeroAuthor, saved as ordinary mesh assets.
- Deterministic woodland, rock and grass placement: stored in the Island prefab, not generated during play.
- Ground blending shader, restrained daylight profile and preview-specific render pipeline.
- Tapered grass blade geometry and the expanded player-buildable sample cottage.

## Editable sources and exports
- ArtSource/MountainIsland/Broadleaf_prepared.blend
- ArtSource/MountainIsland/Limestone_prepared.blend
- ArtSource/MountainIsland/prepare_models.py
- ArtSource/MountainIsland/prepare_textures.py
- ArtSource/MountainIsland/model-budgets.json (actual achieved counts)
- Assets/MountainAero/Models/*.fbx
- Assets/Resources/MountainAero/Island.prefab
- Assets/Skywright/Editor/MountainAeroAuthor.cs
The initial aggressive tree decimation visibly thinned the canopy and was rejected during gameplay inspection. Final tree LODs are 379,640 / 159,848 / 64,938 triangles; limestone LODs are 13,998 / 3,998 / 900. These are actual counts. The tree remains relatively expensive and needs profiling on lower-end hardware before wider scattering.
