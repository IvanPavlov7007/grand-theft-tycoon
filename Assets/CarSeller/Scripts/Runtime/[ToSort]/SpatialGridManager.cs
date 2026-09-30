using System.Collections.Generic;
using UnityEngine;
using System;



#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class GridActivationData
{
    public float activationRadius;
    internal SpatialGridManager gridManager;
    internal Vector2 playerPos;
}

public class SpatialGridManager : MonoBehaviour
{
    Dictionary<CityEntity, SpawnGridCell> _entityToCellMap = new Dictionary<CityEntity, SpawnGridCell>();
    public SpatialGrid SpatialGrid;
    public CellWrapperManager CellWrapperManager { get; private set; }
    CellActivationManager _cellActivationManager;

    private void OnEnable()
    {
        GameEvents.Instance.OnCityEntityDestroyed += onCityEntityDestroyed;
    }

    private void OnDisable()
    {
        GameEvents.Instance.OnCityEntityDestroyed -= onCityEntityDestroyed;
    }

    public static SpatialGridManager Create(SpatialGridConfig config, City city)
    {
        var go = new GameObject("SpatialGridManager");
        var manager = go.AddComponent<SpatialGridManager>();
        manager.Initialize(config, city);
        DontDestroyOnLoad(go);
        return manager;
    }

    public void Initialize(SpatialGridConfig config, City city)
    {
        SpatialGrid = new SpatialGrid(config.gridSizeX, config.gridSizeY, config.cellSize, config.originPosition);
        CellWrapperManager = new CellWrapperManager(SpatialGrid);
        _cellActivationManager = new CellActivationManager();
        repopulateGrid(city);
    }

    private void repopulateGrid(City city)
    {
        foreach (var cell in SpatialGrid.gridCells)
        {
            cell.CityEntities.Clear();
        }

        _entityToCellMap.Clear();

        if (city?.Entities == null) return;

        foreach (var entity in city.Entities.Values)
        {
            var cell = SpatialGrid.GetCell(entity.Position.WorldPosition);
            cell.CityEntities.Add(entity);
            _entityToCellMap[entity] = cell;
        }

        foreach (var marker in city.QueryMarkers())
        {
            if(marker.PositionOnGraph == null)
            {
                continue;
            }

            var cell = SpatialGrid.GetCell(marker.PositionOnGraph.Value.WorldPosition);
            cell.Markers.Add(marker);
        }
    }

    private void Update()
    {
        if(!G.runIntialized)
            return;
        trackNewEntitiesPositions();
        updateActivationManager();
    }

    void updateActivationManager()
    {
        GridActivationData data = new GridActivationData
        {
            playerPos = G.VehicleController.CurrentVehicleEntity.Position.WorldPosition,
            activationRadius = 8f,
            gridManager = this
        };
        var result =  _cellActivationManager.UpdateActiveCells(data);
        G.CarSpawnManager.activeCellsUpdated(result, CellWrapperManager);
    }

    private void trackNewEntitiesPositions()
    {
        if (G.City?.Entities == null) return;

        // For each entity, check if it has moved to a different cell and update the mapping accordingly.
        foreach (var entity in G.City.Entities.Values)
        {
            if (_entityToCellMap.TryGetValue(entity, out SpawnGridCell currentCell))
            {
                var newCell = SpatialGrid.GetCell(entity.Position.WorldPosition);
                if (newCell != currentCell)
                {
                    currentCell.CityEntities.Remove(entity);
                    newCell.CityEntities.Add(entity);
                    _entityToCellMap[entity] = newCell;
                }
            }
            else
            {
                var cell = SpatialGrid.GetCell(entity.Position.WorldPosition);
                cell.CityEntities.Add(entity);
                _entityToCellMap[entity] = cell;
            }
        }
    }

    void onCityEntityDestroyed(CityEntityDestroyedEventData data)
    {
        var entity = data.DestroyedEntity;

        if (_entityToCellMap.TryGetValue(entity, out SpawnGridCell cell))
        {
            cell.CityEntities.Remove(entity);
            _entityToCellMap.Remove(entity);
        }
    }

    void OnDrawGizmos()
    {
        if (SpatialGrid == null) return;

        Gizmos.color = Color.green;

#if UNITY_EDITOR
        var labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
        labelStyle.normal.textColor = Color.white;
#endif

        foreach (var cell in SpatialGrid.gridCells)
        {
            // Draw in XY plane (Vector2 world uses X/Y)
            Vector3 cellCenter = new Vector3(
                SpatialGrid.originPosition.x + cell.X * SpatialGrid.cellSize + SpatialGrid.cellSize / 2f,
                SpatialGrid.originPosition.y + cell.Y * SpatialGrid.cellSize + SpatialGrid.cellSize / 2f,
                0f
            );

            Gizmos.DrawWireCube(cellCenter, new Vector3(SpatialGrid.cellSize, SpatialGrid.cellSize, 0.05f));

#if UNITY_EDITOR
            // "Top-left" corner in XY plane (x min, y max) with a small inset.
            float inset = SpatialGrid.cellSize * 0.08f;

            var cellMin = new Vector3(
                SpatialGrid.originPosition.x + cell.X * SpatialGrid.cellSize,
                SpatialGrid.originPosition.y + cell.Y * SpatialGrid.cellSize,
                0f
            );

            var labelPos = cellMin + new Vector3(inset, SpatialGrid.cellSize - inset, -0.01f);
            Handles.Label(labelPos, cell.CityEntities.Count.ToString(), labelStyle);
#endif
        }
    }
}

public class CellWrapperManager
{
    Dictionary<SpawnGridCell, CarSpawnCellWrapper> _cellWrappers = new Dictionary<SpawnGridCell, CarSpawnCellWrapper>();
    public CarSpawnCellWrapper GetCarSpawnWrapper(SpawnGridCell cell)
    {
        if (!_cellWrappers.TryGetValue(cell, out CarSpawnCellWrapper wrapper))
        {
            wrapper = new CarSpawnCellWrapper(cell);
            _cellWrappers[cell] = wrapper;
        }
        return wrapper;
    }

    public CellWrapperManager(SpatialGrid grid)
    {
        foreach (var cell in grid.gridCells)
        {
            _cellWrappers[cell] = new CarSpawnCellWrapper(cell);
        }
    }
}

public class CellActivationManager
{
    public List<SpawnGridCell> ActiveCells { get; private set; } = new List<SpawnGridCell>();
    public UpdatedActiveCellsData UpdateActiveCells(GridActivationData data)
    {
        ICollection<SpawnGridCell>  currentActiveCells = data.gridManager.SpatialGrid.GetCellsInRadius(data.playerPos, data.activationRadius);
        var disactivatedCells = new List<SpawnGridCell>();
        // Deactivate grids that are no longer active
        foreach (var cell in ActiveCells)
        {
            if (!currentActiveCells.Contains(cell))
            {
                disactivatedCells.Add(cell);
            }
        }

        // get a subset of currentActiveCells that are not in ActiveCells, these are the newly activated cells
        var newActiveCells = new List<SpawnGridCell>();
        foreach (var cell in currentActiveCells)
        {
            if (!ActiveCells.Contains(cell))
            {
                newActiveCells.Add(cell);
            }
        }
        ActiveCells = new List<SpawnGridCell>(currentActiveCells);
        return new UpdatedActiveCellsData(newActiveCells,disactivatedCells);
    }
}

public class UpdatedActiveCellsData
{
    public readonly List<SpawnGridCell> NewActiveCells;
    public readonly List<SpawnGridCell> DisactivatedCells;

    public bool HasChanges => NewActiveCells.Count > 0 || DisactivatedCells.Count > 0;

    public UpdatedActiveCellsData(List<SpawnGridCell> newActiveCells, List<SpawnGridCell> disactivatedCells)
    {
        NewActiveCells = newActiveCells;
        DisactivatedCells = disactivatedCells;
    }
}

public class CarSpawnCellWrapper
{
    public readonly SpawnGridCell Cell;
    public float density = 0.6f;


    public CarSpawnCellWrapper(SpawnGridCell cell)
    {
        Cell = cell;
    }

    public List<CityEntity> GetCars()
    {
        return Cell.CityEntities.FindAll(e => e.Subject is Car);
    }

    public IEnumerable<City.CityMarker> GetMarkers()
    {
        return Cell.Markers.QueryMarkers("car");
    }

}