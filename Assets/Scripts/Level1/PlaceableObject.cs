using UnityEngine;
using System;

public class PlaceableObject : MonoBehaviour
{
   public bool Placed {get; private set;}
   private Vector3 origin;
   private SpriteRenderer sr;

   public BoundsInt area;

    private void Awake(){
        sr = GetComponent<SpriteRenderer>();
    }

    private BoundsInt GetAdjustedArea(){
        if (BuildingSystem.current == null) {
            Debug.LogError("BuildingSystem.current es nulo en PlaceableObject.GetAdjustedArea");
            return area;
        }

        Vector3Int positionInt = BuildingSystem.current.gridLayout.WorldToCell(transform.position);
        BoundsInt areaTemp = area;

        areaTemp.position = positionInt;
        return areaTemp;
    }

   public bool CanBePlaced(){
    if (BuildingSystem.current == null) {
        Debug.LogError("BuildingSystem.current es nulo en PlaceableObject.CanBePlaced");
        return false;
    }

    if(!BuildingSystem.current.CanPlaceTower()){
        if(NotificationSystem.instance != null){
            NotificationSystem.instance.ShowNotification("You cannot place more towers");
        }
        return false;
    }

    BoundsInt areaTemp = GetAdjustedArea();

    if(BuildingSystem.current.CanTakeArea(areaTemp)){
        return true;
    }
    return false;
   }

   public void Place(){
    if(CanBePlaced()){
        BoundsInt areaTemp = GetAdjustedArea();
        BuildingSystem.current.TakeArea(areaTemp);
        Placed = true;
        if(sr != null){
            Color color = sr.color;
            sr.color = new Color(color.r, color.g, color.b, 1f);
        }
        BuildingSystem.current.RegisterPlacedTower(gameObject);
    }
    else{
        Destroy(gameObject);
    }
   }

   public bool CheckPlacement(){
        if(CanBePlaced()){
            Place();
            origin = transform.position;
            return true;
        }
        else{
            Destroy(gameObject);
            return false;
        }
        
   }
}
