using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem current; 
    public GridLayout gridLayout;
    public Tilemap MainTilemap;
    public TileBase takenTile;

    public GameObject buildingPrefab;
    public GameObject confirmationUIPrefab;
    public GameObject notificationSystemPrefab;

    [Header("Max towers per level")]
    [SerializeField] private int maxTowersPerLevel = 5;
    [SerializeField] private List<GameObject> towers = new List<GameObject>();

    private bool isInBuildMode = false;
    private GameObject currentBuildingObject;

    private void Awake(){
        if (current != null && current != this){
            Destroy(this);
        }
        else{
            current = this;
        }

        if(ConfirmationUi.instance == null && confirmationUIPrefab != null){
            Instantiate(confirmationUIPrefab);
        }

        if(NotificationSystem.instance == null && notificationSystemPrefab != null){
            Instantiate(notificationSystemPrefab);
        }
    }

    public bool CanPlaceTower(){
        towers.RemoveAll(tower => tower == null);
        bool canPlace = towers.Count < maxTowersPerLevel;

        if(!canPlace && NotificationSystem.instance != null){
            NotificationSystem.instance.ShowNotification("You cannot place more towers");
        }
        return canPlace;
    }

    public void RegisterPlacedTower(GameObject tower){
        if(tower != null){
            towers.Add(tower);
        }
    }

    public void ResetTowerCount(){
        towers.Clear();
    }

    public int GetCurrentTowerCount(){
        towers.RemoveAll(tower => tower == null);
        return towers.Count;
    }


    #region Tilemap Management
    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap){
        TileBase[] array = new TileBase[area.size.x * area.size.y];
        int counter = 0;

        foreach(var v in area.allPositionsWithin){
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
        for(int i= 0; i< array.Length; i++){
            array[i] = tileBase;
        }
    }

    public void ClearArea(BoundsInt area, Tilemap tilemap){
        SetTilesBlock(area, null, tilemap);
    }
    #endregion

    #region Building Placement

    public void ToggleBuildMode(){
        isInBuildMode = !isInBuildMode;

        if(isInBuildMode){
            if(NotificationSystem.instance != null){
                NotificationSystem.instance.ShowNotification("You are now in building mode");
            }
            PlaceNewObject();
        }
        else{
            if(currentBuildingObject != null){
                Destroy(currentBuildingObject);
                currentBuildingObject = null;
            }
            if(NotificationSystem.instance != null){
                NotificationSystem.instance.ShowNotification("You are now in view mode");
            }
        }
    }

    private void InitializeBuildingMode(){
        if(currentBuildingObject != null){
            Destroy(currentBuildingObject);
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        InitializeWithObject(buildingPrefab, mousePosition);
    }

    public void InitializeWithObject(GameObject building, Vector3 pos){
        pos.z = 0;
        Vector3Int cellPos = gridLayout.WorldToCell(pos);
        Vector3 position = gridLayout.CellToLocalInterpolated(cellPos);

        position.x += gridLayout.cellSize.x / 2;
        position.y += gridLayout.cellSize.y / 2;

        currentBuildingObject = Instantiate(building, position, Quaternion.identity);
        PlaceableObject temp = currentBuildingObject.transform.GetComponent<PlaceableObject>();

        if(temp == null){
            temp = currentBuildingObject.AddComponent<PlaceableObject>();
            temp.area = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1));
        }

        currentBuildingObject.AddComponent<ObjectDrag>();
    }

    public bool CanTakeArea(BoundsInt area){
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);

        foreach(var b in baseArray){
            if(b==takenTile){
                return false;
            }
        }
        return true;
    }

    public void TakeArea(BoundsInt area){
       SetTilesBlock(area, takenTile, MainTilemap);
    }

    public void PlaceNewObject(){
        if(isInBuildMode){
            if(CanPlaceTower()){
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;
                InitializeWithObject(buildingPrefab, mousePosition);
            }
        }
    }
    #endregion

    private void Update(){
        if(Input.GetKeyDown(KeyCode.B)){
            ToggleBuildMode();
        }

        if(isInBuildMode && Input.GetMouseButtonDown(0)){
            //PlaceNewObject();
        }
    }

    public void OnTowerPlacementFinished(){
        if(isInBuildMode && CanPlaceTower()){
            PlaceNewObject();
        }
    }
}
