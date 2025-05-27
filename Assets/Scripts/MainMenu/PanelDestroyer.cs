using UnityEngine;
using System.Collections;

public class PanelDestroyer : MonoBehaviour{
    public string panelName = "WinPanel";

    void Start(){
        StartCoroutine(DestroyPanelNextFrame());
    }

    IEnumerator DestroyPanelNextFrame(){
        yield return null;
        GameObject panel = GameObject.Find(panelName);
        if (panel != null){
            Destroy(panel);
        }
    }
}
