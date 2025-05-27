using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldBackgroundSwitcher : MonoBehaviour
{
    public Tilemap[] worldTilemaps;
    public string[] worldNames;

    public void SetWorldBackground(string worldName){
        for (int i = 0; i < worldTilemaps.Length; i++)
        {
            bool isActive = worldNames[i] == worldName;
            worldTilemaps[i].gameObject.SetActive(isActive);
        }
    }
}
