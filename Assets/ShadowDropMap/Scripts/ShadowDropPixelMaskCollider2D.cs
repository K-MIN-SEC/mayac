using UnityEngine;

[ExecuteAlways]
public sealed class ShadowDropPixelMaskCollider2D : MonoBehaviour
{
    [SerializeField] private Texture2D walkableMask;
    [SerializeField] private float pixelsPerUnit = 64f;
    [SerializeField] private int cellSizePixels = 4;
    [SerializeField, Range(0f, 1f)] private float blockedThreshold = 0.65f;
    [SerializeField] private bool whiteMeansWalkable = true;

    private const string GeneratedRootName = "__GeneratedMaskColliders";

    [ContextMenu("Rebuild Colliders")]
    public void RebuildColliders()
    {
        ClearGeneratedColliders();

        if (walkableMask == null || pixelsPerUnit <= 0f || cellSizePixels <= 0)
        {
            return;
        }

        Color32[] pixels;
        try
        {
            pixels = walkableMask.GetPixels32();
        }
        catch (UnityException ex)
        {
            Debug.LogError(
                $"Mask texture must be readable. Select {walkableMask.name} and enable Read/Write. {ex.Message}",
                this);
            return;
        }

        int width = walkableMask.width;
        int height = walkableMask.height;
        int columns = Mathf.CeilToInt(width / (float)cellSizePixels);
        int rows = Mathf.CeilToInt(height / (float)cellSizePixels);

        GameObject root = new GameObject(GeneratedRootName);
        root.transform.SetParent(transform, false);

        for (int row = 0; row < rows; row++)
        {
            int runStart = -1;

            for (int column = 0; column <= columns; column++)
            {
                bool blocked = column < columns && IsBlockedCell(pixels, width, height, column, row);

                if (blocked && runStart < 0)
                {
                    runStart = column;
                }

                if ((!blocked || column == columns) && runStart >= 0)
                {
                    CreateBox(root.transform, width, height, runStart, column - 1, row);
                    runStart = -1;
                }
            }
        }
    }

    [ContextMenu("Clear Colliders")]
    public void ClearGeneratedColliders()
    {
        Transform oldRoot = transform.Find(GeneratedRootName);
        if (oldRoot == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(oldRoot.gameObject);
        }
        else
        {
            DestroyImmediate(oldRoot.gameObject);
        }
    }

    private bool IsBlockedCell(Color32[] pixels, int width, int height, int column, int row)
    {
        int startX = column * cellSizePixels;
        int startY = row * cellSizePixels;
        int endX = Mathf.Min(startX + cellSizePixels, width);
        int endY = Mathf.Min(startY + cellSizePixels, height);
        int total = 0;
        int blocked = 0;

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                Color32 color = pixels[y * width + x];
                float luminance = (color.r + color.g + color.b) / (255f * 3f);
                bool walkable = whiteMeansWalkable ? luminance >= 0.5f : luminance < 0.5f;
                if (!walkable)
                {
                    blocked++;
                }

                total++;
            }
        }

        return total > 0 && blocked / (float)total >= blockedThreshold;
    }

    private void CreateBox(Transform root, int textureWidth, int textureHeight, int startColumn, int endColumn, int row)
    {
        int startX = startColumn * cellSizePixels;
        int endX = Mathf.Min((endColumn + 1) * cellSizePixels, textureWidth);
        int startY = row * cellSizePixels;
        int endY = Mathf.Min((row + 1) * cellSizePixels, textureHeight);

        float centerX = ((startX + endX) * 0.5f - textureWidth * 0.5f) / pixelsPerUnit;
        float centerY = ((startY + endY) * 0.5f - textureHeight * 0.5f) / pixelsPerUnit;
        float sizeX = (endX - startX) / pixelsPerUnit;
        float sizeY = (endY - startY) / pixelsPerUnit;

        GameObject box = new GameObject($"MaskCollider_{startColumn}_{row}");
        box.transform.SetParent(root, false);
        box.transform.localPosition = new Vector3(centerX, centerY, 0f);

        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(sizeX, sizeY);
    }
}
