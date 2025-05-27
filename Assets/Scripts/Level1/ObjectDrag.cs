using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectDrag : MonoBehaviour{
    private Vector3 startPos;
    private float deltaX, deltaY;
    private PlaceableObject placeableObject;
    private Camera mainCamera;
    private SpriteRenderer sr;
    private bool waitingConfirmation = false;
    private bool towerRegistered = false;

    void Start(){
        mainCamera = Camera.main;
        placeableObject = GetComponent<PlaceableObject>();
        sr = GetComponent<SpriteRenderer>();

        if(sr != null){
            Color color = sr.color;
            sr.color = new Color(color.r, color.g, color.b, 0.6f);
        }

        startPos = Input.mousePosition;
        startPos = Camera.main.ScreenToWorldPoint(startPos);

        deltaX = startPos.x - transform.position.x;
        deltaY = startPos.y - transform.position.y;
    }

    void Update(){
        if(waitingConfirmation){
            return;
        }
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 pos = new Vector3(mousePos.x - deltaX, mousePos.y - deltaY, 0);

        Vector3Int cellPos = BuildingSystem.current.gridLayout.WorldToCell(pos);
        transform.position = BuildingSystem.current.gridLayout.CellToLocalInterpolated(cellPos);

        transform.position += new Vector3(BuildingSystem.current.gridLayout.cellSize.x / 2, BuildingSystem.current.gridLayout.cellSize.y / 2, 0);

        if (sr != null ){
            bool canPlace = placeableObject.CanBePlaced();
            sr.color = canPlace ? new Color(1f, 1f, 1f, 0.6f) : new Color(1f, 0.5f, 0.5f, 0.6f);
        }
    }

    private void LateUpdate(){
        if(Input.GetMouseButtonUp(0) && !waitingConfirmation){
            bool canPlace = placeableObject.CanBePlaced();
            bool withinTowerLimit = BuildingSystem.current.CanPlaceTower();
            ShowConfirmationUI(canPlace && withinTowerLimit);
        }
    }

    private void ShowConfirmationUI(bool canPlace){
        waitingConfirmation = true;
        if(towerRegistered){
            towerRegistered = false;
        }
        if(canPlace){
            System.Action confirmAction = () => {
                placeableObject.Place();
                var buildingSystem = BuildingSystem.current;
                Destroy(this);
                BuildingSystem.current.OnTowerPlacementFinished();
            };

            System.Action cancelAction = () => {
                waitingConfirmation = false;
                towerRegistered = false;
                if(!placeableObject.Placed){
                    Destroy(gameObject);
                }
                BuildingSystem.current.OnTowerPlacementFinished();
            };

            ConfirmationUi.instance.ShowConfirmation(gameObject, transform.position, confirmAction, cancelAction);
        }
        else{
            if(placeableObject != null && !placeableObject.Placed && gameObject != null){
                Destroy(gameObject);
            }
            waitingConfirmation = false;
            BuildingSystem.current.OnTowerPlacementFinished();
        }
    }
}
