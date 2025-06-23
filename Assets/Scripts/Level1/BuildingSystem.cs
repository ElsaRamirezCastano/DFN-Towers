using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class BuildingSystem : MonoBehaviour{
    public static BuildingSystem current;
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public Tilemap PathTilemap;

    public Tilemap PreviewPathTilemap;

    public TileBase takenTile;
    public TileBase pathTile;

    public TileBase pathPreviewTile;

    public GameObject buildingPrefab;
    public GameObject confirmationUIPrefab;
    public GameObject notificationSystemPrefab;
    public GameObject buildingModeUIPrefab;

    [Header("Max towers per level")]
    [SerializeField] private int maxTowersPerLevel = 5;
    [SerializeField] public List<GameObject> towers = new List<GameObject>();

    [Header("Path System")]
    [SerializeField] private List<Vector3Int> pathPositions = new List<Vector3Int>();
    [SerializeField] private List<Vector3Int> defaultPathPositions = new List<Vector3Int>();

    [Header("Input Actions")]
    [SerializeField] private InputActionReference toggleBuildModeAction;
    [SerializeField] private InputActionReference leftClickAction;
    [SerializeField] private InputActionReference rightClickAction;
    [SerializeField] private InputActionReference mousePositionAction;

    private BuildingType currentBuildingType = BuildingType.None;
    private bool isInBuildMode = false;
    private GameObject currentBuildingObject;
    private GameObject currentPathSelector;

    private Vector3Int lastPreviewCell = Vector3Int.one * int.MaxValue;
    private Vector3Int pendingPathPosition = Vector3Int.one * int.MaxValue;
    private bool isWaitingForConfirmation = false;

    private bool systemInitialized = false;

    private void Awake(){
        if (current != null && current != this){
            Destroy(this);
        }
        else{
            current = this;
        }
    }

    private void OnEnable(){
        EnableInputActions();
    }

    private void OnDisable(){
        DisableInputActions();
    }

    private void EnableInputActions(){
        if (toggleBuildModeAction != null){
            toggleBuildModeAction.action.Enable();
            toggleBuildModeAction.action.performed += OnToggleBuildMode;
        }

        if (leftClickAction != null){
            leftClickAction.action.Enable();
            leftClickAction.action.performed += OnLeftClick;
        }

        if (rightClickAction != null){
            rightClickAction.action.Enable();
            rightClickAction.action.performed += OnRightClick;
        }

        if(mousePositionAction != null){
            mousePositionAction.action.Enable();
        }
    }

    private void DisableInputActions(){
        if (toggleBuildModeAction != null){
            toggleBuildModeAction.action.performed -= OnToggleBuildMode;
            toggleBuildModeAction.action.Disable();
        }

        if (leftClickAction != null){
            leftClickAction.action.performed -= OnLeftClick;
            leftClickAction.action.Disable();
        }

        if (rightClickAction != null){
            rightClickAction.action.performed -= OnRightClick;
            rightClickAction.action.Disable();
        }
        
        if(mousePositionAction != null){
            mousePositionAction.action.Disable();
        }
    }

    private void Start(){
        StartCoroutine(InitializeSystemWithDelay());
    }

    private IEnumerator InitializeSystemWithDelay(){
        yield return null;

        InitializeUI();
        ScanExistingPaths();
        SaveDefaultPaths();
        systemInitialized = true;
        Debug.Log("Building System Initialized");
    }

    private void OnToggleBuildMode(InputAction.CallbackContext context){
        if (systemInitialized){
            ToggleBuildModeUI();
        }
    }

    private void OnLeftClick(InputAction.CallbackContext context){
        if (!systemInitialized) return;
        
        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3Int currentCell = gridLayout.WorldToCell(mouseWorldPos);

        if (isInBuildMode){
            switch (currentBuildingType){
                case BuildingType.Path:
                    RequestPathPlacement(currentCell);
                    break;
                case BuildingType.Tower:
                    break;
                case BuildingType.None:
                    break;
            }
        }  
    }

    private void OnRightClick(InputAction.CallbackContext context){
        if (!systemInitialized) return;

        if (isInBuildMode && currentBuildingType == BuildingType.Path){
            Vector3 mouseWorldPos = GetMouseWorldPosition();
            Vector3Int currentCell = gridLayout.WorldToCell(mouseWorldPos);
            RemovePath(currentCell);
        }
    }

    private Vector3 GetMouseWorldPosition(){
        if (mousePositionAction != null && Camera.main != null){
            Vector2 screenPos = mousePositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            worldPos.z = 0;
            return worldPos;
        }
        return Vector3.zero;
    }


    private void SaveDefaultPaths(){
        defaultPathPositions = new List<Vector3Int>(pathPositions);
        Debug.Log($"Saved {defaultPathPositions.Count} default path positions.");
    }

    public void RestoreDefaultPaths(){
        ClearAllPaths();
        foreach (Vector3Int position in defaultPathPositions){
            PathTilemap.SetTile(position, pathTile);
        }
        pathPositions = new List<Vector3Int>(defaultPathPositions);
        Debug.Log($"Restored {pathPositions.Count} default paths.");
    }
    private void InitializeUI(){
        if (ConfirmationUi.instance == null && confirmationUIPrefab != null){
            GameObject confirmInstance = Instantiate(confirmationUIPrefab);
            confirmInstance.name = "ConfirmationUI_RuntimeInstance";
        }

        if (NotificationSystem.instance == null && notificationSystemPrefab != null){
            GameObject notificationInstance = Instantiate(notificationSystemPrefab);
            notificationInstance.name = "NotificationSystem_RuntimeInstance";
        }

        if (BuildingModeUI.instance == null && buildingModeUIPrefab != null){
            GameObject uiInstance = Instantiate(buildingModeUIPrefab);
            uiInstance.name = "BuildingModeUI_RuntimeInstane";

            BuildingModeUI buildingModeUI = uiInstance.GetComponent<BuildingModeUI>();
            if (buildingModeUI != null){
                buildingModeUI.SetupButtons();
            }
        }
    }

    public void ReinitializedUI(){
        InitializeUI();

        StartCoroutine(DelayedUISetup());
    }

    private IEnumerator DelayedUISetup(){
        yield return null;

        if (BuildingModeUI.instance != null){
            BuildingModeUI.instance.SetupButtons();
            BuildingModeUI.instance.ShowBuildingModeUI();
            BuildingModeUI.instance.HideBuildingModeUI();
        }
    }

    public bool CanPlaceTower(){
        towers.RemoveAll(tower => tower == null);
        bool canPlace = towers.Count < maxTowersPerLevel;

        if (!canPlace && NotificationSystem.instance != null){
            NotificationSystem.instance.ShowNotification("You cannot place more towers");
        }
        return canPlace;
    }

    public void RegisterPlacedTower(GameObject tower){
        if (tower != null && !towers.Contains(tower)){
            towers.Add(tower);
        }
    }

    public void ResetTowerCount(){
        towers.Clear();
        pathPositions.Clear();
        RestoreDefaultPaths();
        ExitBuildMode();
    }

    public int GetCurrentTowerCount(){
        towers.RemoveAll(tower => tower == null);
        return towers.Count;
    }

    #region Building Mode Management

    public void ToggleBuildModeUI(){
        if (!systemInitialized){
            Debug.Log("System not initialized yet");
            return;
        }
        if (isInBuildMode){
            ExitBuildMode();
        }
        else{
            ShowBuildingModeSelection();
        }
    }

    private void ShowBuildingModeSelection(){
        if (BuildingModeUI.instance == null){
            ReinitializedUI();
            StartCoroutine(DelayedShowBuildingMode());
        }

        else{
            BuildingModeUI.instance.SetupButtons();
            BuildingModeUI.instance.ShowBuildingModeUI();
        }
    }

    private IEnumerator DelayedShowBuildingMode(){
        yield return null;
        if (BuildingModeUI.instance != null){
            BuildingModeUI.instance.ShowBuildingModeUI();
        }
    }

    public void SetBuildingMode(BuildingType buildingType){
        currentBuildingType = buildingType;
        isInBuildMode = true;

        string modeText = buildingType == BuildingType.Tower ? "tower building" : "path building";
        if (NotificationSystem.instance != null){
            NotificationSystem.instance.ShowNotification($"You are now in {modeText} mode");
        }

        if (buildingType == BuildingType.Tower){
            PlaceNewTower();
        }
    }

    public void ExitBuildMode(){
        isInBuildMode = false;
        currentBuildingType = BuildingType.None;

        if (currentBuildingObject != null){
            Destroy(currentBuildingObject);
            currentBuildingObject = null;
        }

        ClearPathPreview();
        pendingPathPosition = Vector3Int.one * int.MaxValue;
        isWaitingForConfirmation = false;

        if (NotificationSystem.instance != null)
        {
            NotificationSystem.instance.ShowNotification("You are now in view mode");
        }
    }
    #endregion


    #region Tilemap Management
    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap){
        TileBase[] array = new TileBase[area.size.x * area.size.y];
        int counter = 0;

        foreach (var v in area.allPositionsWithin){
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }
        return array;
    }

    private static void SetTilesBlock(BoundsInt area, TileBase tilebase, Tilemap tilemap){
        TileBase[] tileArray = new TileBase[area.size.x * area.size.y];
        FillTiles(tileArray, tilebase);
        tilemap.SetTilesBlock(area, tileArray);
    }

    private static void FillTiles(TileBase[] array, TileBase tileBase){
        for (int i = 0; i < array.Length; i++){
            array[i] = tileBase;
        }
    }

    public void ClearArea(BoundsInt area, Tilemap tilemap){
        SetTilesBlock(area, null, tilemap);
    }
    #endregion

    #region Tower Building

    public void InitializeWithObject(GameObject building, Vector3 pos){
        pos.z = 0;
        Vector3Int cellPos = gridLayout.WorldToCell(pos);
        Vector3 position = gridLayout.CellToLocalInterpolated(cellPos);

        position.x += gridLayout.cellSize.x / 2;
        position.y += gridLayout.cellSize.y / 2;

        currentBuildingObject = Instantiate(building, position, Quaternion.identity);
        PlaceableObject temp = currentBuildingObject.GetComponent<PlaceableObject>();

        if (temp == null){
            temp = currentBuildingObject.AddComponent<PlaceableObject>();
            temp.area = new BoundsInt(cellPos, Vector3Int.one);
        }
        else{
            temp.area = new BoundsInt(cellPos, temp.area.size);
        }

        if (currentBuildingObject.GetComponent<ObjectDrag>() == null){
            currentBuildingObject.AddComponent<ObjectDrag>();
        }
    }

    public bool CanTakeArea(BoundsInt area){
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        foreach (var b in baseArray){
            if (b == takenTile){
                return false;
            }
        }

        foreach (var pos in area.allPositionsWithin){
            if (IsPathAt(new Vector3Int(pos.x, pos.y, 0))){
                return false;
            }
        }
        return true;
    }

    public void TakeArea(BoundsInt area){
        SetTilesBlock(area, takenTile, MainTilemap);
    }

    public void PlaceNewTower(){
        if (isInBuildMode && currentBuildingType == BuildingType.Tower){
            if (CanPlaceTower()){
                Vector3 mousePosition = GetMouseWorldPosition();
                InitializeWithObject(buildingPrefab, mousePosition);
            }
        }
    }

    public bool CanPlaceTowerAtPosition(Vector3Int Position){
        BoundsInt area = new BoundsInt(Position, Vector3Int.one);

        if (!CanTakeArea(area)){
            return false;
        }

        if (IsPathAt(Position)){
            return false;
        }

        return true;
    }

    #endregion

    #region Path Building 

    public void RequestPathPlacement(Vector3Int position)
    {
        if (CanPlacePath(position))
        {
            pendingPathPosition = position;
            isWaitingForConfirmation = true;

            PreviewPathTilemap.SetTile(position, pathPreviewTile);
            lastPreviewCell = position;

            if (ConfirmationUi.instance != null)
            {
                Vector3 wolrdPosition = gridLayout.CellToWorld(position);
                ConfirmationUi.instance.ShowConfirmation(
                    null,
                    wolrdPosition,
                    () => ConfirmPathPlacement(),
                    () => CancelPathPlacement()
                );
            }
            else
            {
                PlacePath(position);
                isWaitingForConfirmation = false;
            }
        }
        else
        {
            if (NotificationSystem.instance != null)
            {
                NotificationSystem.instance.ShowNotification("Cannot place path here");
            }
        }
    }

    public void ConfirmPathPlacement()
    {
        if (pendingPathPosition != Vector3Int.one * int.MaxValue)
        {
            PlacePath(pendingPathPosition);
            pendingPathPosition = Vector3Int.one * int.MaxValue;
            isWaitingForConfirmation = false;
        }
    }

    public void CancelPathPlacement()
    {
        pendingPathPosition = Vector3Int.one * int.MaxValue;
        isWaitingForConfirmation = false;
    }

    public void PlacePath(Vector3Int position)
    {
        if (CanPlacePath(position))
        {
            PathTilemap.SetTile(position, pathTile);
            if (!pathPositions.Contains(position))
            {
                pathPositions.Add(position);
            }

            PreviewPathTilemap.SetTile(position, null);

            if (lastPreviewCell == position)
            {
                lastPreviewCell = Vector3Int.one * int.MaxValue;
            }
            if (NotificationSystem.instance != null)
            {
                NotificationSystem.instance.ShowNotification("Path placed successfully");
            }
        }
        else
        {
            if (NotificationSystem.instance != null)
            {
                NotificationSystem.instance.ShowNotification("Cannot place path here");
            }
        }
    }

    public void RemovePath(Vector3Int position){
        TileBase tileAtPosition = PathTilemap.GetTile(position);

        if (tileAtPosition != null && tileAtPosition == pathTile){
            PathTilemap.SetTile(position, null);
            if (pathPositions.Contains(position)){
                pathPositions.Remove(position);
            }
            if (NotificationSystem.instance != null){
                NotificationSystem.instance.ShowNotification("Path removed");
            }
        }
        else{
            if (NotificationSystem.instance != null){
                NotificationSystem.instance.ShowNotification("No path to remove at this position");
            }
        }
    }

    private void ScanExistingPaths(){
        if (PathTilemap == null) return;
        pathPositions.Clear();
        BoundsInt bounds = PathTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tile = PathTilemap.GetTile(position);
                if (tile != null && tile == pathTile){
                    pathPositions.Add(position);
                }
            }
        }
        Debug.Log($"Found {pathPositions.Count} existing path tiles.");
    }

    public bool CanPlacePath(Vector3Int position){
        TileBase mainTileAtPosition = MainTilemap.GetTile(position);
        if (mainTileAtPosition == takenTile){
            Debug.Log("Position ocupied by TakenTile");
            return false;
        }

        TileBase pathTileAtPosition = PathTilemap.GetTile(position);
        if (pathTileAtPosition != null && pathTileAtPosition == pathTile){
            Debug.Log("Path already exists here");
            return false;
        }
        Debug.Log("Position is free");
        return true;
    }

    public bool IsPathAt(Vector3Int position){
        TileBase tileAtPosition = PathTilemap.GetTile(position);
        return tileAtPosition == pathTile;
    }

    public void ClearAllPaths(){
        BoundsInt bounds = PathTilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++){
            for (int y = bounds.yMin; y < bounds.yMax; y++){
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tileAtPosition = PathTilemap.GetTile(position);

                if (tileAtPosition == pathTile){
                    PathTilemap.SetTile(position, null);
                }
            }
        }
        PreviewPathTilemap.ClearAllTiles();
        lastPreviewCell = Vector3Int.one * int.MaxValue;

        pathPositions.Clear();
        if (NotificationSystem.instance != null){
            NotificationSystem.instance.ShowNotification("All paths cleared");
        }
    }

    public void RestorePathFromList(List<Vector3Int> savedPaths, TileBase tileToUse = null){
        TileBase tileforPaths = tileToUse ?? pathTile;

        foreach (Vector3Int position in savedPaths){
            PathTilemap.SetTile(position, tileforPaths);
        }
        pathPositions = new List<Vector3Int>(savedPaths);
    }

    public List<Vector3Int> GetAllPathPositions(){
        ScanExistingPaths();
        return new List<Vector3Int>(pathPositions);
    }

    public List<Vector3Int> GetPathPositions(){
        return new List<Vector3Int>(pathPositions);
    }

    #endregion

    public void OnTowerPlacementFinished(){
        if (isInBuildMode && currentBuildingType == BuildingType.Tower && CanPlaceTower()){
            PlaceNewTower();
        }
    }

    private void Update(){
        if (!systemInitialized) return;

        if (isInBuildMode){
            switch (currentBuildingType){
                case BuildingType.Path:
                    HandlePathBuilding();
                    break;
                case BuildingType.Tower:
                    break;
                case BuildingType.None:
                    ClearPathPreview();
                    break;
            }
        }
        else{
            ClearPathPreview();
        }
    }

    private void HandlePathBuilding(){
        if (isWaitingForConfirmation)
        {
            return;
        }

        Vector3 mouseWorldPos = GetMouseWorldPosition();
        Vector3Int currentCell = gridLayout.WorldToCell(mouseWorldPos);

        if (lastPreviewCell != Vector3Int.one * int.MaxValue && lastPreviewCell != currentCell){
            PreviewPathTilemap.SetTile(lastPreviewCell, null);
            /*TileBase tileAtLastPreview = PathTilemap.GetTile(lastPreviewCell);
            if (tileAtLastPreview == pathPreviewTile){
                PathTilemap.SetTile(lastPreviewCell, null);
            }*/
        }

        //TileBase currentTileAtCell = PathTilemap.GetTile(currentCell);
        if (CanPlacePath(currentCell) && pathPreviewTile != null){
            PreviewPathTilemap.SetTile(currentCell, pathPreviewTile);
            lastPreviewCell = currentCell;
        }
        /*else if (currentTileAtCell == pathTile){
            lastPreviewCell = currentCell;
        }*/
        else{
            PreviewPathTilemap.SetTile(currentCell, null);
            lastPreviewCell = Vector3Int.one * int.MaxValue;
        }
    }
    
    private void ClearPathPreview(){
        if (lastPreviewCell != Vector3Int.one * int.MaxValue){
            /*TileBase tileAtLastPreview = PathTilemap.GetTile(lastPreviewCell);
            if (tileAtLastPreview = pathTile){
                PathTilemap.SetTile(lastPreviewCell, null);
            }*/
            PreviewPathTilemap.SetTile(lastPreviewCell, null);
            lastPreviewCell = Vector3Int.one * int.MaxValue;
        }
    }
}