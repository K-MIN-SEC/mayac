using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class QuarterViewWalkableNavigator2D : MonoBehaviour
{
    public static QuarterViewWalkableNavigator2D instance { get; private set; }

    [SerializeField] private Texture2D walkableMask;
    [SerializeField] private Transform mapTransform;
    [SerializeField] private Camera inputCamera;
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private LayerMask interactionIconLayer;
    [SerializeField] private float pixelsPerUnit = 32f;
    [SerializeField, Min(4)] private int pathCellPixels = 8;
    [SerializeField, Range(0f, 1f)] private float mapAlphaCoverage = 0.12f;
    [SerializeField, Min(0f)] private float obstaclePadding = 0.12f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
    [SerializeField, Min(0.01f)] private float waypointTolerance = 0.08f;
    [SerializeField, Min(0)] private int destinationSnapRadiusCells = 16;

    private static readonly int[] NeighborX = { -1, 0, 1, -1, 1, -1, 0, 1 };
    private static readonly int[] NeighborY = { 1, 1, 1, 0, 0, -1, -1, -1 };

    private Rigidbody2D body;
    private Texture2D navigationTexture;
    private bool[] walkableGrid;
    private int gridWidth;
    private int gridHeight;
    private readonly List<Vector2> path = new List<Vector2>();
    private int waypointIndex;

    //임시 다이얼로그 변수
    public bool isDialogue;
    public bool isStop;
    public Queue<Dialogue> dialogueBox = new();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        instance = this;
        if (inputCamera == null)
        {
            inputCamera = Camera.main;
        }

        BuildGrid();
    }

    private void Update()
    {
        if (TryReadPointerDown(out Vector2 screenPosition, out int pointerId))
        {
            if(isDialogue)
            {
                isStop = DialogueManager.instance.TriggerDialogue();
                return;
            }

            if (IsPointerOverUI(pointerId))
            {
                return;
            }

            Vector3 world = inputCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -inputCamera.transform.position.z)
            ); 

            Collider2D iconHit = Physics2D.OverlapPoint(world, interactionIconLayer);
            if (iconHit != null)
            {
                InteractionIcon2D icon = iconHit.GetComponentInParent<InteractionIcon2D>();
                if (icon != null)
                {
                    icon.OnIconTapped();
                    return;
                }
            }

            SetDestination(world);
        }
    }

    private void FixedUpdate()
    {
        if (waypointIndex >= path.Count || isDialogue)
        {
            return;
        }

        Vector2 current = body.position;
        Vector2 target = path[waypointIndex];
        Vector2 next = Vector2.MoveTowards(current, target, moveSpeed * Time.fixedDeltaTime);
        body.MovePosition(next);

        if (characterRenderer != null && Mathf.Abs(next.x - current.x) > 0.001f)
        {
            characterRenderer.flipX = next.x < current.x;
        }

        if (Vector2.Distance(next, target) <= waypointTolerance)
        {
            waypointIndex++;
        }
    }

    public bool SetDestination(Vector2 worldPosition)
    {
        if (walkableGrid == null || mapTransform == null)
        {
            return false;
        }

        Vector2Int start = WorldToCell(body.position);
        Vector2Int goal = WorldToCell(worldPosition);
        if (!FindNearestWalkable(start, destinationSnapRadiusCells, out start)
            || !FindNearestWalkable(goal, destinationSnapRadiusCells, out goal))
        {
            return false;
        }

        List<Vector2Int> cells = FindPath(start, goal);
        if (cells.Count == 0)
        {
            return false;
        }

        path.Clear();
        Vector2Int previousDirection = new Vector2Int(int.MinValue, int.MinValue);
        for (int index = 0; index < cells.Count; index++)
        {
            Vector2Int direction = index == 0
                ? Vector2Int.zero
                : cells[index] - cells[index - 1];
            bool directionChanged = index > 1 && direction != previousDirection;
            if (directionChanged)
            {
                path.Add(CellToWorld(cells[index - 1]));
            }

            if (index > 0)
            {
                previousDirection = direction;
            }
        }

        path.Add(CellToWorld(cells[cells.Count - 1]));
        waypointIndex = 0;
        return true;
    }

    public void RebuildNavigationGrid()
    {
        BuildGrid();
    }

    private void BuildGrid()
    {
        navigationTexture = ResolveNavigationTexture();
        if (navigationTexture == null || mapTransform == null || pixelsPerUnit <= 0f)
        {
            Debug.LogError("Quarter-view navigator is missing its readable map texture or map transform.", this);
            return;
        }

        Color32[] pixels;
        try
        {
            pixels = navigationTexture.GetPixels32();
        }
        catch (UnityException exception)
        {
            Debug.LogError($"Map texture must be readable. {exception.Message}", this);
            return;
        }

        gridWidth = Mathf.CeilToInt(navigationTexture.width / (float)pathCellPixels);
        gridHeight = Mathf.CeilToInt(navigationTexture.height / (float)pathCellPixels);
        walkableGrid = new bool[gridWidth * gridHeight];

        for (int cellY = 0; cellY < gridHeight; cellY++)
        {
            for (int cellX = 0; cellX < gridWidth; cellX++)
            {
                int startX = cellX * pathCellPixels;
                int startY = cellY * pathCellPixels;
                int endX = Mathf.Min(startX + pathCellPixels, navigationTexture.width);
                int endY = Mathf.Min(startY + pathCellPixels, navigationTexture.height);
                int total = 0;
                int opaque = 0;

                for (int y = startY; y < endY; y++)
                {
                    for (int x = startX; x < endX; x++)
                    {
                        if (pixels[y * navigationTexture.width + x].a >= 16)
                        {
                            opaque++;
                        }

                        total++;
                    }
                }

                walkableGrid[ToIndex(cellX, cellY)] =
                    total > 0 && opaque / (float)total >= mapAlphaCoverage;
            }
        }

        BlockFootprints();
    }

    private Texture2D ResolveNavigationTexture()
    {
        if (mapTransform != null)
        {
            SpriteRenderer mapRenderer = mapTransform.GetComponent<SpriteRenderer>();
            if (mapRenderer != null && mapRenderer.sprite != null)
            {
                return mapRenderer.sprite.texture;
            }
        }

        return walkableMask;
    }

    private void BlockFootprints()
    {
        QuarterViewFootprintObstacle2D[] obstacles =
            FindObjectsByType<QuarterViewFootprintObstacle2D>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (QuarterViewFootprintObstacle2D obstacle in obstacles)
        {
            Collider2D footprint = obstacle.Footprint;
            if (footprint == null || !footprint.enabled)
            {
                continue;
            }

            Bounds bounds = footprint.bounds;
            Rect blocked = Rect.MinMaxRect(
                bounds.min.x - obstaclePadding,
                bounds.min.y - obstaclePadding,
                bounds.max.x + obstaclePadding,
                bounds.max.y + obstaclePadding
            );
            Vector2Int minimum = WorldToCell(blocked.min);
            Vector2Int maximum = WorldToCell(blocked.max);
            Vector2 cellSize = GetCellWorldSize();
            float cellRadius = cellSize.magnitude * 0.5f;

            for (int y = minimum.y; y <= maximum.y; y++)
            {
                for (int x = minimum.x; x <= maximum.x; x++)
                {
                    Vector2 center = CellToWorld(new Vector2Int(x, y));
                    Vector2 closest = footprint.ClosestPoint(center);
                    if (footprint.OverlapPoint(center)
                        || Vector2.Distance(center, closest) <= obstaclePadding + cellRadius)
                    {
                        walkableGrid[ToIndex(x, y)] = false;
                    }
                }
            }
        }
    }

    private Vector2 GetCellWorldSize()
    {
        Vector3 scale = mapTransform.lossyScale;
        float localSize = pathCellPixels / pixelsPerUnit;
        return new Vector2(localSize * Mathf.Abs(scale.x), localSize * Mathf.Abs(scale.y));
    }

    private List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        int nodeCount = gridWidth * gridHeight;
        int[] cost = new int[nodeCount];
        int[] parent = new int[nodeCount];
        bool[] closed = new bool[nodeCount];
        Array.Fill(cost, int.MaxValue);
        Array.Fill(parent, -1);

        int startIndex = ToIndex(start.x, start.y);
        int goalIndex = ToIndex(goal.x, goal.y);
        cost[startIndex] = 0;
        MinHeap open = new MinHeap();
        open.Push(startIndex, Heuristic(start, goal));

        while (open.Count > 0)
        {
            int currentIndex = open.Pop();
            if (closed[currentIndex])
            {
                continue;
            }

            if (currentIndex == goalIndex)
            {
                return Reconstruct(parent, goalIndex);
            }

            closed[currentIndex] = true;
            Vector2Int current = FromIndex(currentIndex);
            for (int directionIndex = 0; directionIndex < NeighborX.Length; directionIndex++)
            {
                int nextX = current.x + NeighborX[directionIndex];
                int nextY = current.y + NeighborY[directionIndex];
                if (!IsWalkable(nextX, nextY))
                {
                    continue;
                }

                bool diagonal = NeighborX[directionIndex] != 0 && NeighborY[directionIndex] != 0;
                if (diagonal
                    && (!IsWalkable(current.x + NeighborX[directionIndex], current.y)
                        || !IsWalkable(current.x, current.y + NeighborY[directionIndex])))
                {
                    continue;
                }

                int nextIndex = ToIndex(nextX, nextY);
                if (closed[nextIndex])
                {
                    continue;
                }

                int nextCost = cost[currentIndex] + (diagonal ? 14 : 10);
                if (nextCost >= cost[nextIndex])
                {
                    continue;
                }

                cost[nextIndex] = nextCost;
                parent[nextIndex] = currentIndex;
                open.Push(nextIndex, nextCost + Heuristic(new Vector2Int(nextX, nextY), goal));
            }
        }

        return new List<Vector2Int>();
    }

    private List<Vector2Int> Reconstruct(int[] parent, int goalIndex)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        int current = goalIndex;
        while (current >= 0)
        {
            result.Add(FromIndex(current));
            current = parent[current];
        }

        result.Reverse();
        return result;
    }

    private bool FindNearestWalkable(Vector2Int origin, int radius, out Vector2Int result)
    {
        for (int currentRadius = 0; currentRadius <= radius; currentRadius++)
        {
            for (int y = origin.y - currentRadius; y <= origin.y + currentRadius; y++)
            {
                for (int x = origin.x - currentRadius; x <= origin.x + currentRadius; x++)
                {
                    if (currentRadius > 0
                        && x > origin.x - currentRadius
                        && x < origin.x + currentRadius
                        && y > origin.y - currentRadius
                        && y < origin.y + currentRadius)
                    {
                        continue;
                    }

                    if (IsWalkable(x, y))
                    {
                        result = new Vector2Int(x, y);
                        return true;
                    }
                }
            }
        }

        result = origin;
        return false;
    }

    private Vector2Int WorldToCell(Vector2 worldPosition)
    {
        Vector3 local = mapTransform.InverseTransformPoint(worldPosition);
        int pixelX = Mathf.FloorToInt(local.x * pixelsPerUnit + navigationTexture.width * 0.5f);
        int pixelY = Mathf.FloorToInt(local.y * pixelsPerUnit + navigationTexture.height * 0.5f);
        return new Vector2Int(
            Mathf.Clamp(pixelX / pathCellPixels, 0, gridWidth - 1),
            Mathf.Clamp(pixelY / pathCellPixels, 0, gridHeight - 1)
        );
    }

    private Vector2 CellToWorld(Vector2Int cell)
    {
        float pixelX = Mathf.Min((cell.x + 0.5f) * pathCellPixels, navigationTexture.width - 1);
        float pixelY = Mathf.Min((cell.y + 0.5f) * pathCellPixels, navigationTexture.height - 1);
        Vector3 local = new Vector3(
            (pixelX - navigationTexture.width * 0.5f) / pixelsPerUnit,
            (pixelY - navigationTexture.height * 0.5f) / pixelsPerUnit,
            0f
        );
        return mapTransform.TransformPoint(local);
    }

    private bool IsWalkable(int x, int y)
    {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && walkableGrid[ToIndex(x, y)];
    }

    private int ToIndex(int x, int y)
    {
        return y * gridWidth + x;
    }

    private Vector2Int FromIndex(int index)
    {
        return new Vector2Int(index % gridWidth, index / gridWidth);
    }

    private static int Heuristic(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        return 10 * (dx + dy) - 6 * Mathf.Min(dx, dy);
    }

    private static bool TryReadPointerDown(out Vector2 screenPosition, out int pointerId)
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                screenPosition = touch.position;
                pointerId = touch.fingerId;
                return true;
            }

            screenPosition = default;
            pointerId = -1;
            return false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPosition = Input.mousePosition;
            pointerId = -1;
            return true;
        }

        screenPosition = default;
        pointerId = -1;
        return false;
    }

    private static bool IsPointerOverUI(int pointerId)
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return pointerId >= 0
            ? EventSystem.current.IsPointerOverGameObject(pointerId)
            : EventSystem.current.IsPointerOverGameObject();
    }

    private sealed class MinHeap
    {
        private readonly List<Entry> entries = new List<Entry>();

        public int Count => entries.Count;

        public void Push(int node, int priority)
        {
            entries.Add(new Entry(node, priority));
            int index = entries.Count - 1;
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (entries[parent].Priority <= entries[index].Priority)
                {
                    break;
                }

                (entries[parent], entries[index]) = (entries[index], entries[parent]);
                index = parent;
            }
        }

        public int Pop()
        {
            Entry root = entries[0];
            int lastIndex = entries.Count - 1;
            entries[0] = entries[lastIndex];
            entries.RemoveAt(lastIndex);
            int index = 0;

            while (true)
            {
                int left = index * 2 + 1;
                int right = left + 1;
                if (left >= entries.Count)
                {
                    break;
                }

                int smallest = right < entries.Count && entries[right].Priority < entries[left].Priority
                    ? right
                    : left;
                if (entries[index].Priority <= entries[smallest].Priority)
                {
                    break;
                }

                (entries[index], entries[smallest]) = (entries[smallest], entries[index]);
                index = smallest;
            }

            return root.Node;
        }

        private readonly struct Entry
        {
            public Entry(int node, int priority)
            {
                Node = node;
                Priority = priority;
            }

            public int Node { get; }
            public int Priority { get; }
        }
    }
}
