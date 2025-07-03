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

        RefreshArea(position);

        Debug.Log("Placed path tile at: " + position);
    }

    public void PlacePathTiles(Vector3Int[] positions){
        if (pathTilemap == null || pathRuleTile == null) return;

        TileBase[] tiles = new TileBase[positions.Length];
        for (int i = 0; i < tiles.Length; i++){
            tiles[i] = pathRuleTile;
        }

        pathTilemap.SetTiles(positions, tiles);

        HashSet<Vector3Int> positionsToRefresh = new HashSet<Vector3Int>();
        foreach (var pos in positions){
            positionsToRefresh.Add(pos);
            foreach (var dir in neighborDirections){
                positionsToRefresh.Add(pos + dir);
            }
        }

        foreach (var pos in positionsToRefresh){
            RefreshSingleTile(pos);
        }

        Debug.Log($"{positions.Length} path tiles placed.");
    }

    public void RemovePathTile(Vector3Int position){
        if (pathTilemap == null) return;

        pathTilemap.SetTile(position, null);
        RefreshArea(position);

        Debug.Log("Removed path tile at: " + position);
    }

    public void RemovePathTiles(Vector3Int[] positions){
        if (pathTilemap == null || positions == null) return;

        TileBase[] nullTiles = new TileBase[positions.Length];
        pathTilemap.SetTiles(positions, nullTiles);

        HashSet<Vector3Int> positionsToRefresh = new HashSet<Vector3Int>();
        foreach (var pos in positions){
            positionsToRefresh.Add(pos);
            foreach (var dir in neighborDirections){
                positionsToRefresh.Add(pos + dir);
            }
        }

        foreach (var pos in positionsToRefresh){
            RefreshSingleTile(pos);
        }

        Debug.Log($"{positions.Length} path tiles removed.");
    }

    private void RefreshArea(Vector3Int centerPosition, bool logRefresh = true){
        if (pathTilemap == null) return;

        List<Vector3Int> positionsToRefresh = new List<Vector3Int>();
        positionsToRefresh.Add(centerPosition);
        //RefreshSingleTile(centerPosition);
        foreach (var dir in neighborDirections)
        {
            positionsToRefresh.Add(centerPosition + dir);
            //RefreshSingleTile(centerPosition + dir);
        }

        foreach (var pos in positionsToRefresh)
        {
            RefreshSingleTile(pos);
        }

        pathTilemap.CompressBounds();

        Debug.Log($"Refreshed area around: {centerPosition}");
    }

    private void RefreshSingleTile(Vector3Int position)
    {
        if (pathTilemap == null) return;

        TileBase currentTile = pathTilemap.GetTile(position);

        if (IsPathTileAt(position))
        {
            //pathTilemap.SetTile(position, null);
            pathTilemap.SetTile(position, pathRuleTile);

            pathTilemap.SetTileFlags(position, TileFlags.None);
        }
        /*if (autoDetectPathTiles){
            if (currentTile != null && currentTile != pathRuleTile){
                pathTilemap.SetTile(position, pathRuleTile);
            }
            else if (currentTile == pathRuleTile){
                pathTilemap.SetTile(position, pathRuleTile);
            }
        }
        else{
            if (currentTile != null && IsPathTile(currentTile)){
                pathTilemap.SetTile(position, null);
                pathTilemap.SetTile(position, pathRuleTile);
            }
        }*/
    }

    public bool IsPathTileAt(Vector3Int position){
        if (pathTilemap == null) return false;

        TileBase tile = pathTilemap.GetTile(position);

        if (autoDetectPathTiles){
            return tile != null;
        }
        else{
            return IsPathTile(tile);
        }
    }

    private bool IsPathTile(TileBase tile){
        if (tile == null) return false;

        if (autoDetectPathTiles){
            return true;
        }
        else{
            foreach (var pathType in pathTileTypes){
                if (tile == pathType) return true;
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

    public void RefreshEntireTilemap(){
        if (pathTilemap == null) return;

        pathTilemap.CompressBounds();
        BoundsInt bounds = pathTilemap.cellBounds;

        if (bounds.size.x > 0 && bounds.size.y > 0){
            for (int x = bounds.xMin; x < bounds.xMax; x++){
                for (int y = bounds.yMin; y < bounds.yMax; y++){
                    Vector3Int position = new Vector3Int(x, y, 0);
                    TileBase currentTile = pathTilemap.GetTile(position);

                    if (autoDetectPathTiles){
                        if (currentTile != null && currentTile != pathRuleTile){
                            pathTilemap.SetTile(position, pathRuleTile);
                        }
                    }
                    else{
                        if (IsPathTile(currentTile) && currentTile != pathRuleTile){
                            pathTilemap.SetTile(position, pathRuleTile);
                        }
                    }
                }
            }

            for (int x = bounds.xMin; x < bounds.xMax; x++){
                for (int y = bounds.yMin; y < bounds.yMax; y++){
                    Vector3Int position = new Vector3Int(x, y, 0);
                    RefreshSingleTile(position);
                }
            }
        }

        Debug.Log("Tilemap refreshed completely.");
    }

    public void SyncWithExistingPaths(){
        if (pathTilemap == null || pathRuleTile == null) return;

        List<Vector3Int> existingPaths = new List<Vector3Int>();
        BoundsInt bounds = pathTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase existingTile = pathTilemap.GetTile(position);

                if (autoDetectPathTiles){
                    if (existingTile != null)
                    {
                        if (existingTile != pathRuleTile)
                        {
                            pathTilemap.SetTile(position, pathRuleTile);
                        }
                        existingPaths.Add(position);
                    }
                }
                else{

                    if (IsPathTile(existingTile)){
                        if (existingTile != pathRuleTile)
                        {
                            pathTilemap.SetTile(position, pathRuleTile);
                        }
                        existingPaths.Add(position);
                    }
                }
            }
        }

        HashSet<Vector3Int> positionsToRefresh = new HashSet<Vector3Int>();
        foreach (var pos in existingPaths){
            positionsToRefresh.Add(pos);
            foreach (var dir in neighborDirections){
                positionsToRefresh.Add(pos + dir);
            }
        }

        foreach (var pos in positionsToRefresh){
            RefreshSingleTile(pos);
        }

        Debug.Log($"Synchronized with {existingPaths.Count} existing path tiles.");
    }

    public void ShowPreview(Vector3Int position){
        if (previewTilemap == null || previewTile == null) return;

        ClearPreview();
        previewTilemap.SetTile(position, previewTile);
    }

    public void ShowVariousPreviews(Vector3Int[] positions){
        if (previewTilemap == null || previewTile == null) return;

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

    public void ForceRuleTileUpdate()
    {
        if (pathTilemap == null || pathRuleTile == null) return;

        pathTilemap.CompressBounds();
        BoundsInt bounds = pathTilemap.cellBounds;

        if (bounds.size.x > 0 && bounds.size.y > 0)
        {
            for(int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for(int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    TileBase currentTile = pathTilemap.GetTile(position);

                    if (currentTile != null)
                    {
                        pathTilemap.SetTile(position, null);
                        pathTilemap.SetTile(position, pathRuleTile);
                        pathTilemap.SetTileFlags(position, TileFlags.None);
                    }
                }
            }
        }
    }

    public int GetNeighborCount(Vector3Int position)
    {
        int count = 0;
        foreach (var dir in neighborDirections)
        {
            if (IsPathTileAt(position + dir))
            {
                count++;
            }
        }
        return count;
    }
}
