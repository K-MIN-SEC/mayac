# ShadowDrop Fresh 2x Map

This map is rebuilt from the supplied road-only and building-sheet images.

- `shadowdrop_road_base.png`: opaque 2x road and lot background.
- `shadowdrop_buildings.png`: aligned 2x building layer with transparent background.
- `shadowdrop_walkable_mask.png`: white asphalt pixels are walkable.
- `shadowdrop_blocked_mask.png`: white non-road pixels are blocked.

All runtime textures are `2896 x 2172`. Keep `RoadBase`, `Buildings`, and
`CollisionFromWalkableMask` at Transform Scale `1, 1, 1`. The importer uses
`18.285714` pixels per unit, preserving the previous seven-times map size
without transform stretching.

Use `Shadow Drop > Build Prototype Map Scene` for a clean scene, or
`Shadow Drop > Apply Fresh 2x Map To Current Scene` to replace the map layers
inside an existing `ShadowDrop_Map` hierarchy.

The walkable mask is extracted directly from the asphalt in the road source.
It does not use an AI-redrawn road mask, so its geometry stays in the same
pixel coordinate system as the visible map.
