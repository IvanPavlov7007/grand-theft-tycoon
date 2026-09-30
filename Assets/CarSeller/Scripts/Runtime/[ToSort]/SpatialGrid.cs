using System.Collections.Generic;
using UnityEngine;

public sealed class SpatialGrid
{
    public int gridSizeX { get; private set; }
    public int gridSizeY { get; private set; }
    public float cellSize { get; private set; }
    public Vector2 originPosition { get; private set; }

    public SpawnGridCell[,] gridCells { get; private set; }

    public SpatialGrid(int gridSizeX, int gridSizeY, float cellSize, Vector2 originPosition)
    {
        Debug.Assert(gridSizeX > 0, "gridSizeX must be > 0.");
        Debug.Assert(gridSizeY > 0, "gridSizeY must be > 0.");
        Debug.Assert(cellSize > 0f, "cellSize must be > 0.");

        this.gridSizeX = gridSizeX;
        this.gridSizeY = gridSizeY;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridCells = new SpawnGridCell[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                gridCells[x, y] = new SpawnGridCell(x, y);
            }
        }
    }

    public static SpatialGrid CreatePopulated(int gridSizeX, int gridSizeY, float cellSize, Vector2 originPosition)
    {
        return new SpatialGrid(gridSizeX, gridSizeY, cellSize, originPosition);
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY;
    }

    public SpawnGridCell GetCell(int x, int y)
    {
        Debug.Assert(IsInBounds(x, y), $"Cell index out of bounds: ({x},{y}) in [{gridSizeX}x{gridSizeY}].");
        return gridCells[x, y];
    }

    public bool TryGetCell(int x, int y, out SpawnGridCell cell)
    {
        if (!IsInBounds(x, y))
        {
            cell = null;
            return false;
        }

        cell = gridCells[x, y];
        return true;
    }

    public Vector2Int WorldToCellIndex(Vector2 worldPosition)
    {
        var local = worldPosition - originPosition;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        return new Vector2Int(x, y);
    }

    public bool TryGetCell(Vector2 worldPosition, out SpawnGridCell cell)
    {
        var index = WorldToCellIndex(worldPosition);
        return TryGetCell(index.x, index.y, out cell);
    }

    public SpawnGridCell GetCell(Vector2 worldPosition)
    {
        var index = WorldToCellIndex(worldPosition);
        return GetCell(index.x, index.y);
    }

    public bool TryGetCell(Vector3 worldPosition, out SpawnGridCell cell)
    {
        return TryGetCell(new Vector2(worldPosition.x, worldPosition.y), out cell);
    }

    public SpawnGridCell GetCell(Vector3 worldPosition)
    {
        return GetCell(new Vector2(worldPosition.x, worldPosition.y));
    }

    public Vector2 GetCellWorldMin(int x, int y)
    {
        return originPosition + new Vector2(x * cellSize, y * cellSize);
    }

    public Vector2 GetCellWorldCenter(int x, int y)
    {
        return GetCellWorldMin(x, y) + new Vector2(cellSize * 0.5f, cellSize * 0.5f);
    }

    public List<SpawnGridCell> GetCellsInRadius(Vector2 playerPos, float activationRadius)
    {
        List<SpawnGridCell> cellsInRadius = new List<SpawnGridCell>();
        var playerCellIndex = WorldToCellIndex(playerPos);
        int radiusInCells = Mathf.CeilToInt(activationRadius / cellSize);
        for (int x = playerCellIndex.x - radiusInCells; x <= playerCellIndex.x + radiusInCells; x++)
        {
            for (int y = playerCellIndex.y - radiusInCells; y <= playerCellIndex.y + radiusInCells; y++)
            {
                if (IsInBounds(x, y))
                {
                    var cellCenter = GetCellWorldCenter(x, y);
                    if (Vector2.Distance(playerPos, cellCenter) <= activationRadius)
                    {
                        cellsInRadius.Add(GetCell(x, y));
                    }
                }
            }
        }
        return cellsInRadius;
    }
}

public sealed class SpawnGridCell
{
    public int X { get; }
    public int Y { get; }

    public List<CityEntity> CityEntities { get; }
    public List<City.CityMarker> Markers { get; }

    public SpawnGridCell(int x, int y)
    {
        X = x;
        Y = y;

        CityEntities = new List<CityEntity>();
        Markers = new List<City.CityMarker>();
    }
}
