# Skywright

First-person creative building, floating-island exploration and elemental magic, built with **Unity 6000.4.7f1**, **URP 17.4.0** and the **Input System 1.19.0**.

This public repository is a **source snapshot** of the working project as of October 21, 2026. It contains current gameplay/editor code, original shaders, project/package settings, bootstrap scenes, the wind VFX prefabs and development documentation. It includes the latest Stone Sentinel combat/animation integration and Wind Suction / Wind Push overhaul.

## Contents

- `Assets/Skywright`: movement, construction, persistence, harvesting, Earthstone, Air magic, island progression, editor utilities and verification code.
- `Assets/Skywright/Wind` and `Assets/Resources/WindMagic`: wind control gestures, procedural ribbon VFX, shader, material and effect prefabs.
- `Assets/StoneSentinelCombat` and `Assets/StoneSentinelTrial`: Sentinel gameplay, animation integration, preview and verification source.
- `ArtSource`: original offline preparation scripts and implementation reports; binary art and recordings are omitted.
- `Packages` and `ProjectSettings`: the project's Unity configuration.
- `PROJECT_BRIEF.md`, `MAINTENANCE.md` and milestone reports: design history, current controls and validation notes.

## Assets required to run the current scenes

**This is not the complete playable project.** Models, textures, audio, binary art sources, imported animation files, authored world/resource assets and standalone builds remain local. The current scenes and build tools need those omitted assets; cloning this repository alone will not reproduce the screenshots or build a working preview.

In particular, the local game uses the licensed **Nature Package - Forest Environment** (ALP / OE Objective Environment). Its source, shaders, textures, meshes, generated derivatives and the project's private asset-containing Git history are not published here. Imported character animations and their animated model/Blender exports are also omitted. See [PUBLIC_ASSETS.md](PUBLIC_ASSETS.md) for the restore requirements and provenance references.

Use the complete local project for gameplay or restore legitimately obtained assets with their original Unity GUIDs before attempting its existing build workflows. Historical authoring tools deliberately regenerate world assets; read the maintenance notes before using them.

## Current Air Wand controls

- Hold **LMB**: continuous suction.
- Hold **RMB**: continuous gust push.
- **Shift + LMB click**: toggle grab/release of an eligible object.
- **Q**: change element; **Tab**: equip/stow.
- **R**: field hover/flight; double jump: personal leap/glide.

## Local-only data

Personal saves, API credentials, private task responses, diagnostic/preservation evidence, Unity caches and generated player builds are excluded. Tripo scripts read credentials from the Windows User environment when explicitly invoked; no API key is included. No paid service is required merely to read or compile the source.

The existing local Git repository has not been pushed: this snapshot extends the public repository's initial commit, so its history contains no omitted asset blobs.
