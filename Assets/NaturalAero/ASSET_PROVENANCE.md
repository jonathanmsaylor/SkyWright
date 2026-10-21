# M1.8 Checkpoint A asset provenance

Downloaded 2026-10-19 UTC. Original archives and API download metadata are retained locally in Evidence/NaturalAero/sources, outside the player build. No account, purchase, subscription or runtime online dependency.

| Asset | Source | License | Imported / modifications |
|---|---|---|---|
| Wood 091 A | https://ambientcg.com/view?id=Wood091A | CC0 1.0 | 2K JPG color/OpenGL normal; roughness converted to Unity metallic-zero/smoothness-alpha map; restrained smoothness; new metre-scaled visual UVs |
| Grass 004 | https://ambientcg.com/view?id=Grass004 | CC0 1.0 | 2K color used by local world-space meadow treatment; variation and soft geographic blend; original authored 3D blades |
| Rock 030 | https://ambientcg.com/view?id=Rock030 | CC0 1.0 | 2K color/OpenGL normal/roughness; pale limestone material adjustment; original irregular visual cliff meshes |
| Fern 02 | https://polyhaven.com/a/fern_02 | CC0 1.0 | FBX plus 2K color, alpha and OpenGL normal; alpha packed into color; shared URP cutout material; scaled clumps and distance culling |
| Green Leaf 12 | https://www.cgbookcase.com/textures/green-leaf-12 | CC0 | 2K front color and opacity packed into RGBA; original branch/canopy meshes; front texture used on both sides as sample limitation |
| Kloofendal 48d Partly Cloudy Pure Sky | https://polyhaven.com/a/kloofendal_48d_partly_cloudy_puresky | CC0 1.0 | 4K HDR panorama for broad sky coverage; exposure and rotation adjusted; authors Greg Zaal and Jarod Guest |

License references inspected:
- https://docs.ambientcg.com/license/
- https://polyhaven.com/license
- https://www.cgbookcase.com/ (free/no restrictions); exact Green Leaf 12 page identifies the texture as CC0.

Website marketing/example renders were not imported as game assets. Direct downloadable assets only.

Original Skywright work: tapered bark geometry, bark color/normal maps, tree branching and leaf placement, sparse grass blades, flower geometry, irregular cliff dressing, pool treatment, sample layout, material integration and preview routing. Existing rounded construction meshes remain from M1.5; only sample visual meshes/materials are cloned or adapted. Runtime harvesting colliders and construction contracts remain separate.

Oak Veneer 01 and Fine Wood 14 remain unimported alternatives. Birch provides a consistent single wood family; no reason to add redundant materials in this sample.

Texture preparation is an offline authoring operation, not a player requirement. Python dependencies used for channel packing live only in ignored Evidence/NaturalAero/python-deps. Unity consumes ordinary textures, meshes, materials and prefabs.
