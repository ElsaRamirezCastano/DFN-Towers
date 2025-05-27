using UnityEngine;

public class TowerSorting : MonoBehaviour{
    private SpriteRenderer sr;

    void Awake(){
        sr = GetComponent<SpriteRenderer>();
        if(sr == null){
            sr = GetComponentInChildren<SpriteRenderer>();
        }
        SetupSorting();
    }

    void SetupSorting(){
        if(sr != null){
            sr.sortingLayerName = "Towers";
            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
        }
    }
    void Update(){
        if(sr != null){
            sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
        }
    }   
}
