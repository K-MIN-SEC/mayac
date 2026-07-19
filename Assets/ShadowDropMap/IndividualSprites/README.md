# Individual Building Example

The `IndividualSprites` folder contains five buildings and two trees extracted
from the supplied RGB asset sheet. The baked checkerboard was removed, each
sprite was upscaled 2x, and every sprite uses a bottom-center pivot.

Use `Shadow Drop > Place Individual Building Example` in the prototype scene.
The command disables the legacy full-map building sheet and creates seven
independent objects under `IndividualBuildings_Example`.

The sample placement is intentionally sparse. Every object can be moved and
scaled independently while `ShadowDropSpriteYSorter` keeps its bottom anchor
available for player/building ordering.
