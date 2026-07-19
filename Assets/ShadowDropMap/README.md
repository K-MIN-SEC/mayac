# ShadowDrop Map Prototype

This folder contains a 2x map setup for the supplied quarter-view city image.

- `shadowdrop_map_base.png`: road-only background with the buildings removed.
- `shadowdrop_walkable_mask.png`: white means asphalt road or connected alley; everything else is blocked. The movement mask includes 3 source pixels of clearance so a player collider can pass narrow roads without crossing the visual wall boundary.
- `shadowdrop_occlusion_layer.png`: transparent PNG drawn above the player. Buildings, perimeter walls, fences, sidewalks, yards, and other off-road foreground remain opaque.
- `shadowdrop_debug_preview.png`: cyan shows walkable ground, orange outlines show the pixel-mask boundary.
- `shadowdrop_collider_debug_preview.png`: previews the actual 4px collider-cell result used in Unity.

The original building-filled base is preserved as `shadowdrop_map_base_with_buildings.png` beside the Unity textures and in the prototype output folder.

The runtime textures are `2896 x 2172`. Keep the map objects at Transform Scale `1, 1, 1`; `18.285714` pixels per unit matches the previous scene's 7x visual scale without stretching. The editor importer keeps these textures uncompressed with a 4096 maximum texture size.

In Unity, open `Shadow Drop > Build Prototype Map Scene`.
The builder creates `Assets/ShadowDropMap/Scenes/ShadowDrop_MapPrototype.unity` with the base map, occlusion sprite, camera, and generated BoxCollider2D rows from the walkable mask.

For an existing scene, use `Shadow Drop > Apply 2x Map To Current Scene`. It preserves the rest of the scene, resets the two map renderers and collision root to scale `1, 1, 1`, applies 4px cells with a 0.65 blocked threshold, and rebuilds the map colliders.

The visual occlusion mask remains separate from the widened movement mask, so buildings and perimeter walls still draw above the player while narrow roads remain traversable.
