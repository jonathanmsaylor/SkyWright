# Asset boundary for the public source snapshot

The repository intentionally publishes source code and project configuration. To prevent licensed or unreviewed art sources from being redistributed, all runtime art binaries and generated world/mesh resources are omitted, including original and CC0 art. This broad boundary avoids accidentally including a baked derivative of a licensed source.

## Restore from the complete local project or authorized sources

- **Nature Package - Forest Environment**, asset 165645, v1.9, including `Assets/ALP_Assets` and its ALP/DE shaders. Source: https://assetstore.unity.com/packages/3d/vegetation/nature-package-forest-environment-165645 . Governed by the Unity Asset Store EULA.
- Authored `Resources` meshes, world/catalog assets, materials, textures, prefabs and audio for the existing scenes. Most are omitted even when their original source is CC0 or authored locally. Only original shader source and the two WindMagic VFX prefabs/material are included from the resource folders.
- Imported animation FBX files, retargeted animated Sentinel exports and Blender scene files. Restore them from the original authorized local sources; their names and retargeting steps are documented in `ArtSource/StoneSentinelTrial/ANIMATION_REPORT.md`.
- Sentinel geometry/materials/textures and their editable art sources, plus the remaining original/CC0 world art. These are part of the full local project, not this code upload.

Keep original Unity `.meta` GUIDs when restoring existing private assets. Missing references in these public scenes are expected until the omitted assets are restored. Asset provenance documents describe the complete local project and are included for reference; they do not imply that their asset contents are present in this public repository.

## Deliberately excluded

`Assets/ALP_Assets`, binary models/textures/audio, Blender/GLB sources, imported animations, generated mesh/world resources, screenshots/recordings, standalone builds, Unity caches, personal saves, private API responses, environment files and credentials. The private local Git history is excluded as well.

The source export was reviewed as a snapshot. An API-key scan checked file content without printing potential secret values. Dependency references in `Packages` remain ordinary Unity package references; no package archives or Asset Store contents are bundled.
