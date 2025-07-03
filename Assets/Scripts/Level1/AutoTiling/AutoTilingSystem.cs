using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TileConnectivityRule{
    [Header("Tile Configuration")]
    public string ruleName;
    public TileBase tileToPlace;
    public int priority = 0;

    [Header("Required Connections  (8-directional)")]
    [Tooltip("Connections that MUST exist")]
    public bool topLeft;
    public bool top;
    public bool topRight;
    public bool left;
    public bool right;
    public bool bottomLeft;
    public bool bottom;
    public bool bottomRight;

    [Header("Forbidden Connections  (8-directional)")]
    [Tooltip("Connections that MUST NOT exist")]
    public bool forbiddenTopLeft;
    public bool forbiddenTop;
    public bool forbiddenTopRight;
    public bool forbiddenLeft;
    public bool forbiddenRight;
    public bool forbiddenBottomLeft;
    public bool forbiddenBottom;
    public bool forbiddenBottomRight;

    [Header("Pattern Matching")]
    [Tooltip("If it's marked every neighbor must match")]
    public bool exactMatch = false;

    //[Tooltip("If marked only the conections marked as true will matter")]
    //public bool ignoreUnmarkedConnections = true;

    [Header("Connection Limits")]
    [Tooltip("Minimun number of total connections required")]
    public int minConnectionsRequired = -1;

    [Tooltip("Maximun number of total conections allowed")]
    public int maxConnectionsAllowed = -1;

    /*[Header("Optional: Specific Tile Requirements")]
    [Tooltip("If empty any path tile counts as connectted")]
    public List<TileBase> specificTileToCheck = new List<TileBase>();*/

    public bool[] GetConnectionPattern(){
        return new bool[] { topLeft, top, topRight, left, right, bottomLeft, bottom, bottomRight };
    }

    public bool[] GetForbiddenPattern(){
        return new bool[] { forbiddenTopLeft, forbiddenTop, forbiddenTopRight, forbiddenLeft, forbiddenRight, forbiddenBottomLeft, forbiddenBottom, forbiddenBottomRight };
    }

    public bool MatchesPattern(bool[] connections)
    {
        if (connections == null || connections.Length != 8) return false;

        bool[] requiredPattern = GetConnectionPattern();
        bool[] forbiddenPattern = GetForbiddenPattern();

        int totalConnections = connections.Count(c => c);

        if (minConnectionsRequired >= 0 && totalConnections < minConnectionsRequired) return false;

        if (maxConnectionsAllowed >= 0 && totalConnections > maxConnectionsAllowed) return false;


        if (exactMatch)
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (connections[i] != requiredPattern[i]) return false;
            }
            //return true;
        }
        else
        {
            for (int i = 0; i < connections.Length; i++)
            {
                if (requiredPattern[i] && !connections[i]) return false;
            }

            for (int i = 0; i < connections.Length; i++)
            {
                if (forbiddenPattern[i] && connections[i]) return false;
            }
            //return true;
        }
        return true;
    }
}

public class AutoTilingSystem : MonoBehaviour{
    public static AutoTilingSystem Instance { get; private set; }

    [Header("Tilemap References")]
    public Tilemap pathTilemap;
    public Tilemap previewTilemap;

    [Header("Default tiles")]
    public TileBase defaultPathTile;
    public TileBase previewTile;

    [Header("Auto-Tilling Rules")]
    [SerializeField] private List<TileConnectivityRule> connectivityRules = new List<TileConnectivityRule>();

    /*[Header("Flexible Matching")]
    public bool allowPartialMatches = true;
    public bool prioritizeCardinalConnections = true;

    [SerializeField] protected List<TileConnectivityRule> flexibleRules = new List<TileConnectivityRule>();*/

    [Header("Tile Recognition")]
    [Tooltip("Tiles that are considered as Path for de conections")]
    public List<TileBase> pathTiles = new List<TileBase>();

    [Header("Debug Settings")]
    public bool enableDebugLogs = false;
    public bool showDebugGizmos = false;


    private readonly Vector3Int[] neighborDirections = new Vector3Int[]{
        new Vector3Int(-1, 1, 0), //Top-Left
        new Vector3Int(0, 1, 0), //Top
        new Vector3Int(1, 1, 0), //Top-Right
        new Vector3Int(-1, 0, 0), //Left
        new Vector3Int(1, 0, 0), //Right
        new Vector3Int(-1, -1, 0), //Bottom-Left
        new Vector3Int(0, -1, 0), //Bottom
        new Vector3Int(1, -1, 0), //Bottom.Right
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
        if (defaultPathTile != null && !pathTiles.Contains(defaultPathTile)){
            pathTiles.Add(defaultPathTile);
        }

        foreach (var rule in connectivityRules){
            if (rule.tileToPlace != null && !pathTiles.Contains(rule.tileToPlace)){
                pathTiles.Add(rule.tileToPlace);
            }
        }

       /* foreach (var rule in flexibleRules){
            if (rule.tileToPlace != null && !pathTiles.Contains(rule.tileToPlace)){
                pathTiles.Add(rule.tileToPlace);
            }
        }*/

        connectivityRules = connectivityRules.OrderByDescending(r => r.priority).ToList();
       // flexibleRules = flexibleRules.OrderByDescending(r => r.priority).ToList();

        if (enableDebugLogs){
            Debug.Log($"AutoTilingSystem inicializado con {connectivityRules.Count} reglas y {pathTiles.Count} tipos de tile de camino");
        }
    }

    public void UpdateTileAtPosition(Vector3Int position){
        if (pathTilemap == null) return;

        TileBase currentTile = pathTilemap.GetTile(position);
        if (currentTile == null) return;

        if (!IsPathTile(currentTile)) return;

        TileBase newTile = GetAppropriateTile(position);

        if (newTile != null && newTile != currentTile){
            pathTilemap.SetTile(position, newTile);

            if (enableDebugLogs){
                Debug.Log($"AutoTilling: Updated tile at {position} to {newTile.name}");
            }
        }
    }

    /*public void UpdateTilesInArea(BoundsInt area){
        foreach (Vector3Int position in area.allPositionsWithin){
            UpdateTileAtPosition(position);
        }
    }*/

    public void UpdateTileAndNeighbors(Vector3Int position){
        UpdateTileAtPosition(position);

        foreach (Vector3Int direction in neighborDirections){
            Vector3Int neighborPos = position + direction;
            UpdateTileAtPosition(neighborPos);
        }
    }

    public virtual TileBase GetAppropriateTile(Vector3Int position){
        bool[] connections = GetNeighborsConnections(position);

        if (enableDebugLogs){
            string connectionStr = string.Join(", ", connections.Select((b, i) => $"{GetDirectionName(i)}:{b}"));
        }

       /* foreach (TileConnectivityRule rule in flexibleRules){
            if (rule.MatchesPattern(connections)){
                if (enableDebugLogs){
                    Debug.Log($"AutoTiling: Rule {rule.ruleName} matched at {position}");
                }
                return rule.tileToPlace;
            }
        }*/

        foreach (TileConnectivityRule rule in connectivityRules){
            if (rule.MatchesPattern(connections)){
                if (enableDebugLogs){
                    Debug.Log($"AutoTilling: Rule {rule.ruleName} matched for position {position}");
                }
                return rule.tileToPlace;
            }
        }
        if (enableDebugLogs){
            Debug.Log($"AutoTiling: Ninguna regla coincide para {position}, usando tile por defecto");
        }
        return defaultPathTile;
    }

    private bool[] GetNeighborsConnections(Vector3Int position){
        bool[] connections = new bool[8];

        for (int i = 0; i < neighborDirections.Length; i++){
            Vector3Int neighborPos = position + neighborDirections[i];
            connections[i] = IsPathTileAt(neighborPos);
        }
        return connections;
    }

    private bool IsPathTileAt(Vector3Int position){
        if (pathTilemap == null) return false;

        TileBase tileAtPosition = pathTilemap.GetTile(position);
        return IsPathTile(tileAtPosition);
    }

    private bool IsPathTile(TileBase tile){
        if (tile == null) return false;
        return pathTiles.Contains(tile);
    }

    //Verifes if a connections pattern matches with a rule
    /*private bool DoesPatternMatch(bool[] connections, bool[] pattern)
    {
        if (connections.Length != pattern.Length) return false;

        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] != pattern[i]) return false;
        }

        return true;
    }*/

    public void OnPathPlaced(Vector3Int position, TileBase placedTile = null){
        if (placedTile != null && !pathTiles.Contains(placedTile)){
            pathTiles.Add(placedTile);
        }
        UpdateTileAndNeighbors(position);
    }

    //Updates the neighbors of the deleted tile
    public void OnPathRemoved(Vector3Int position)
    {
        foreach (Vector3Int direction in neighborDirections)
        {
            Vector3Int neigborpos = position + direction;
            UpdateTileAtPosition(neigborpos);
        }
    }

    public void RefreshAllPathTiles(){
        if (pathTilemap == null) return;

        BoundsInt bounds = pathTilemap.cellBounds;
        int tilesUpdated = 0;

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase currentTile = pathTilemap.GetTile(position);
                if (IsPathTile(currentTile)){
                    UpdateTileAtPosition(position);
                    tilesUpdated++;
                }
            }
        }

        if (enableDebugLogs){
            Debug.Log("AutoTilling: Refreshed all path tiles");
        }
    }

    public virtual void AddConnectivityRule(TileConnectivityRule rule){
        if (rule != null && rule.tileToPlace != null){
            connectivityRules.Add(rule);

            if (!pathTiles.Contains(rule.tileToPlace)){
                pathTiles.Add(rule.tileToPlace);
            }

            connectivityRules = connectivityRules.OrderByDescending(r => r.priority).ToList();
        }
    }

    public void ClearConnectivityRules() => connectivityRules.Clear();

    /*public void AddFlexibleConnectivityRule(TileConnectivityRule rule){
        if (rule != null && rule.tileToPlace != null){
            flexibleRules.Add(rule);

            if (!pathTiles.Contains(rule.tileToPlace)){
                pathTiles.Add(rule.tileToPlace);
            }

            flexibleRules = flexibleRules.OrderByDescending(r => r.priority).ToList();
        }
    }

    public void ClearFlexibleRules() => flexibleRules.Clear();

    public void ClearAllConnectivityRules(){
        ClearConnectivityRules();
        ClearFlexibleRules();
    }*/

    public string GetDebugInfo(Vector3Int position){
        bool[] connections = GetNeighborsConnections(position);
        string info = $"Position {position}\n";
        info += $"Connections: TL:{connections[0]} T:{connections[1]} TR:{connections[2]} L:{connections[3]} R:{connections[4]} BL:{connections[5]} B:{connections[6]} BR:{connections[7]}\n";

        TileBase appropiateTile = GetAppropriateTile(position);
        info += $"Suggested Tile: {(appropiateTile != null ? appropiateTile.name : "None")}";

        return info;
    }

    private string GetDirectionName(int index){
        string[] names = { "TL", "T", "TR", "L", "R", "BL", "B", "BR" };
        return index >= 0 && index < names.Length ? names[index] : "?";
    }

    private void OnDrawGizmosSelected(){
        if (!showDebugGizmos || pathTilemap == null) return;

        Camera cam = Camera.current;
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;
        BoundsInt viewBounds = new BoundsInt(
            Mathf.FloorToInt(camPos.x - 10),
            Mathf.FloorToInt(camPos.y - 10),
            0, 20, 20, 1
        );

        foreach (Vector3Int pos in viewBounds.allPositionsWithin){
            if (IsPathTileAt(pos)){
                DrawConnectionGizmos(pos);
            }
        }
    }

    private void DrawConnectionGizmos(Vector3Int position){
        Vector3 worldPos = pathTilemap.CellToWorld(position) + pathTilemap.cellSize * 0.5f;
        bool[] connections = GetNeighborsConnections(position);

        for (int i = 0; i < neighborDirections.Length; i++){
            Vector3Int neighborPos = position + neighborDirections[i];
            Vector3 neighborWorldPos = pathTilemap.CellToWorld(neighborPos) + pathTilemap.cellSize * 0.5f;

            Gizmos.color = connections[i] ? Color.green : Color.red;
            Gizmos.DrawLine(worldPos, Vector3.Lerp(worldPos, neighborWorldPos, 0.8f));
        }
    }
}
