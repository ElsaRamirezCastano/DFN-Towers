using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class RuleTileAutoTiilingSystem : MonoBehaviour{
    public static RuleTileAutoTiilingSystem Instance { get; private set; }

    [Header("Tilemap references")]
    public Tilemap pathTilemap;
    public Tilemap previewTilemap;

    [Header("Rule tile configration")]
    public RuleTile pathRuleTile;

    [Header("Preview settings")]
    public TileBase previewTile;

    [Header("Path detection")]
    public bool autoDetectPathTiles = true;
    public TileBase[] pathTileTypes;

    private readonly Vector3Int[] neighborDirections = new Vector3Int[]{
        new Vector3Int(-1, 1, 0), // Top-Left
        new Vector3Int(0, 1, 0),  // Top
        new Vector3Int(1, 1, 0),  // Top-Right
        new Vector3Int(-1, 0, 0), // Left
        new Vector3Int(1, 0, 0),  // Right
        new Vector3Int(-1, -1, 0),// Bottom-Left
        new Vector3Int(0, -1, 0), // Bottom
        new Vector3Int(1, -1, 0), // Bottom-Right
    };

    private HashSet<Vector3Int> pendingUpdates = new HashSet<Vector3Int>();
    private bool isUpdating = false;

    private void Awake(){
        if (Instance != null && Instance != this){
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Start(){
        InitializeSystem();
    }

    private void InitializeSystem(){
        if (pathRuleTile == null){
            Debug.LogError("Path Rule Tile is not assigned in the inspector.");
            return;
        }

        if (pathTilemap == null){
            Debug.LogError("Path Tilemap is not assigned in the inspector.");
            return;
        }

        if (!autoDetectPathTiles && (pathTileTypes == null || pathTileTypes.Length == 0)){
            pathTileTypes = new TileBase[] { pathRuleTile };
        }

        Debug.Log("RuleTileAutoTiilingSystem initialized successfully.");
    }

    public void PlacePathTile(Vector3Int position){
        if (pathTilemap == null || pathRuleTile == null) return;

        pathTilemap.SetTile(position, pathRuleTile);

        UpdateTileAndNeighbors(position);

        Debug.Log("Placed path tile at: " + position);
    }

    public void PlacePathTiles(Vector3Int[] positions){
        if (pathTilemap == null || pathRuleTile == null || positions == null) return;

        TileBase[] tiles = new TileBase[positions.Length];
        for (int i = 0; i < tiles.Length; i++){
            tiles[i] = pathRuleTile;
        }

        pathTilemap.SetTiles(positions, tiles);

        HashSet<Vector3Int> positionsToUpdate = new HashSet<Vector3Int>();
        foreach (var pos in positions){
            positionsToUpdate.Add(pos);
            foreach (var dir in neighborDirections){
                positionsToUpdate.Add(pos + dir);
            }
        }

        UpdateTilesCascade(positionsToUpdate);

        Debug.Log($"{positions.Length} path tiles placed.");
    }

    public void RemovePathTile(Vector3Int position){
        if (pathTilemap == null) return;

        pathTilemap.SetTile(position, null);
        UpdateNeighbors(position);

        Debug.Log("Removed path tile at: " + position);
    }

    public void RemovePathTiles(Vector3Int[] positions){
        if (pathTilemap == null || positions == null) return;

        TileBase[] nullTiles = new TileBase[positions.Length];
        pathTilemap.SetTiles(positions, nullTiles);

        HashSet<Vector3Int> positionsToUpdate = new HashSet<Vector3Int>();
        foreach (var pos in positions){
            foreach (var dir in neighborDirections){
                Vector3Int neighborPos = pos + dir;
                
                if(!System.Array.Exists(positions, p => p == neighborPos)){
                    positionsToUpdate.Add(neighborPos);
                }
            }
        }

        UpdateTilesCascade(positionsToUpdate);

        Debug.Log($"{positions.Length} path tiles removed.");
    }

    private void UpdateTileAndNeighbors(Vector3Int position){
        HashSet<Vector3Int> positionsToUpdate = new HashSet<Vector3Int>();
        positionsToUpdate.Add(position);

        foreach (var dir in neighborDirections){
            positionsToUpdate.Add(position + dir);
        }

        UpdateTilesCascade(positionsToUpdate);
    }

    private void UpdateNeighbors(Vector3Int position){
        HashSet<Vector3Int> positionsToUpdate = new HashSet<Vector3Int>();

        foreach(var dir in neighborDirections){
            positionsToUpdate.Add(position + dir);
        }

        UpdateTilesCascade(positionsToUpdate);
    }

    private bool UpdateSingleTileCascade(Vector3Int position){
        if (pathTilemap == null || pathRuleTile == null) return false;

        TileBase currentTile = pathTilemap.GetTile(position);
        if (currentTile == null) return false;

        if (IsPathTile(currentTile)) {
            pathTilemap.SetTile(position, pathRuleTile);
            return true;
            Debug.Log($"Updated tile at {position}.");
        }
        return false;
    }

    private void UpdateTilesCascade(HashSet<Vector3Int> startPositions){
        var toUpdate = new Queue<Vector3Int>(startPositions);
        var updated = new HashSet<Vector3Int>(startPositions);

        while (toUpdate.Count > 0){
            var pos = toUpdate.Dequeue();
            bool changed = UpdateSingleTileCascade(pos);

            if (changed){
                foreach (var dir in neighborDirections){
                    var neighbor = pos + dir;
                    if (!updated.Contains(neighbor)){
                        toUpdate.Enqueue(neighbor);
                        updated.Add(neighbor);
                    }
                }
            }
        }

        pathTilemap.CompressBounds();
        RefreshTilemap();
    }

    private void RefreshTilemap(){
        if (pathTilemap == null) return;

        pathTilemap.RefreshAllTiles();
    }

    public bool IsPathTileAt(Vector3Int position){
        if (pathTilemap == null) return false;

        TileBase tile = pathTilemap.GetTile(position);

        return IsPathTile(tile);
    }

    private bool IsPathTile(TileBase tile){
        if (tile == null) return false;

        if (autoDetectPathTiles)return true;
        else{
            if (tile == pathRuleTile) return true;
            if (pathTileTypes != null){
                foreach (var pathType in pathTileTypes){
                    if (tile == pathType) return true;
                }
            }
            return false;
        }
    }

    public List<Vector3Int> GetAllPathPositions(){
        List<Vector3Int> pathPositions = new List<Vector3Int>();

        if (pathTilemap == null) return pathPositions;

        BoundsInt bounds = pathTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                if (IsPathTileAt(position)){
                    pathPositions.Add(position);
                }
            }
        }
        return pathPositions;
    }

    public void RefreshAllPathTiles(){
        if (pathTilemap == null || pathRuleTile == null) return;

        pathTilemap.CompressBounds();
        BoundsInt bounds = pathTilemap.cellBounds;

        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;
        
        HashSet<Vector3Int> pathPositions = new HashSet<Vector3Int>();

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase currentTile = pathTilemap.GetTile(position);

                if (IsPathTile(currentTile)){
                    pathPositions.Add(position);
                    if (currentTile != pathRuleTile){
                        pathTilemap.SetTile(position, pathRuleTile);
                    }
                }
            }
        }

        UpdateTilesCascade(pathPositions);

        Debug.Log("Tilemap refreshed completely.");
    }

    public void SyncWithExistingPaths(){
        if (pathTilemap == null || pathRuleTile == null) return;

        HashSet<Vector3Int> existingPaths = new HashSet<Vector3Int>();
        BoundsInt bounds = pathTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase existingTile = pathTilemap.GetTile(position);

                if (IsPathTile(existingTile)){
                    existingPaths.Add(position);
                    if (existingTile != pathRuleTile){
                        pathTilemap.SetTile(position, pathRuleTile);
                    }
                }
            }
        }

        HashSet<Vector3Int> allPositionsToUpdate = new HashSet<Vector3Int>();
        foreach (var pos in existingPaths){
            allPositionsToUpdate.Add(pos);
            foreach (var dir in neighborDirections){
                Vector3Int neighborPos = pos + dir;
                if (IsPathTileAt(neighborPos)){
                    allPositionsToUpdate.Add(neighborPos);
                }
            }
        }

        UpdateTilesCascade(allPositionsToUpdate);
        Debug.Log($"Synchronized with {existingPaths.Count} existing path tiles.");
    }

    public void ShowPreview(Vector3Int position){
        if (previewTilemap == null || previewTile == null) return;

        ClearPreview();
        previewTilemap.SetTile(position, previewTile);
    }

    public void ShowMultiplePreviews(Vector3Int[] positions){
        if (previewTilemap == null || previewTile == null || positions == null) return;

        ClearPreview();
        TileBase[] tiles = new TileBase[positions.Length];
        for (int i = 0; i < tiles.Length; i++){
            tiles[i] = previewTile;
        }
        previewTilemap.SetTiles(positions, tiles);
    }

    public void ClearPreview(){
        if (previewTilemap == null) return;
        previewTilemap.CompressBounds();
        BoundsInt bounds = previewTilemap.cellBounds;

        if (bounds.size.x > 0 && bounds.size.y > 0){
            TileBase[] emptyTiles = new TileBase[bounds.size.x * bounds.size.y * bounds.size.z];
            previewTilemap.SetTilesBlock(bounds, emptyTiles);
        }
    }

    public int GetNeighborCount(Vector3Int position){
        int count = 0;
        foreach (var dir in neighborDirections){
            if (IsPathTileAt(position + dir)){
                count++;
            }
        }
        return count;
    }

    public void ForceCompleteUpdate(){
        if (pathTilemap == null || pathRuleTile == null) return;

        pathTilemap.CompressBounds();
        BoundsInt bounds = pathTilemap.cellBounds;

        if (bounds.size.x <= 0 || bounds.size.y <= 0) return;

        HashSet<Vector3Int> pathPositions = new HashSet<Vector3Int>();

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase currentTile = pathTilemap.GetTile(position);

                if (IsPathTile(currentTile)){
                    pathPositions.Add(position);
                }
            }
        }
        foreach (var pos in pathPositions){
            pathTilemap.SetTile(pos, null);
        }

        foreach (var pos in pathPositions){
            pathTilemap.SetTile(pos, pathRuleTile);
            pathTilemap.SetTileFlags(pos, TileFlags.None);
        }

        RefreshTilemap();

        Debug.Log("Force complete update done.");
    }
}
