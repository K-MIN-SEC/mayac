using UnityEngine;

[ExecuteAlways]
public sealed class ShadowDropPixelMaskCollider2D : MonoBehaviour
{
    [SerializeField] private Texture2D walkableMask;
    [SerializeField] private float pixelsPerUnit = 64f;
    [SerializeField] private int cellSizePixels = 4;
    [SerializeField, Range(0f, 1f)] private float blockedThreshold = 0.65f;
    [SerializeField] private bool whiteMeansWalkable = true;
    [SerializeField] private bool boundaryOnly = true;
    [SerializeField, Min(1)] private int boundaryThicknessCells = 2;

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

        bool[,] blockedCells = new bool[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                blockedCells[row, column] = IsBlockedCell(pixels, width, height, column, row);
            }
        }

        for (int row = 0; row < rows; row++)
        {
            int runStart = -1;

            for (int column = 0; column <= columns; column++)
            {
                bool blocked =
                    column < columns
                    && blockedCells[row, column]
                    && (
                        !boundaryOnly
                        || IsBoundaryCell(blockedCells, columns, rows, column, row)
                    );

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

        if (boundaryOnly)
        {
            CreateMapBorder(root.transform, width, height);
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

    private bool IsBoundaryCell(bool[,] blockedCells, int columns, int rows, int column, int row)
    {
        int thickness = Mathf.Max(1, boundaryThicknessCells);
        for (int offsetY = -thickness; offsetY <= thickness; offsetY++)
        {
            for (int offsetX = -thickness; offsetX <= thickness; offsetX++)
            {
                int neighborColumn = column + offsetX;
                int neighborRow = row + offsetY;
                if (
                    neighborColumn >= 0
                    && neighborColumn < columns
                    && neighborRow >= 0
                    && neighborRow < rows
                    && !blockedCells[neighborRow, neighborColumn]
                )
                {
                    return true;
                }
            }
        }

        return false;
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

    private void CreateMapBorder(Transform root, int textureWidth, int textureHeight)
    {
        float width = textureWidth / pixelsPerUnit;
        float height = textureHeight / pixelsPerUnit;
        float thickness = Mathf.Max(cellSizePixels, 1) / pixelsPerUnit;

        CreateBorderBox(root, "MapBorder_Top", new Vector2(0f, height * 0.5f + thickness * 0.5f), new Vector2(width + thickness * 2f, thickness));
        CreateBorderBox(root, "MapBorder_Bottom", new Vector2(0f, -height * 0.5f - thickness * 0.5f), new Vector2(width + thickness * 2f, thickness));
        CreateBorderBox(root, "MapBorder_Left", new Vector2(-width * 0.5f - thickness * 0.5f, 0f), new Vector2(thickness, height));
        CreateBorderBox(root, "MapBorder_Right", new Vector2(width * 0.5f + thickness * 0.5f, 0f), new Vector2(thickness, height));
    }

    private static void CreateBorderBox(
        Transform root,
        string name,
        Vector2 localPosition,
        Vector2 size
    )
    {
        GameObject box = new GameObject(name);
        box.transform.SetParent(root, false);
        box.transform.localPosition = localPosition;
        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.size = size;
    }
}
