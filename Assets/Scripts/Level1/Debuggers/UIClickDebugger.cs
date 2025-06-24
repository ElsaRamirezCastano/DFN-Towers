using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class UIClickDebugger : MonoBehaviour{
    void Update(){
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame){
            Debug.Log("=== CLICK DEBUG START ===");

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Debug.Log($"Mouse Position: {mousePos}");
            
            if (EventSystem.current == null){
                Debug.LogError("NO EVENTSYSTEM FOUND!");
                return;
            }
            
            var pointerData = new ExtendedPointerEventData(EventSystem.current){
                position = mousePos,
                device = Mouse.current
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"Total objects hit: {results.Count}");
            
            if (results.Count == 0){
                Debug.LogWarning("NO OBJECTS HIT BY RAYCAST!");
            }
            else{
                for (int i = 0; i < results.Count; i++){
                    GameObject hitObject = results[i].gameObject;
                    string canvasInfo = "";
                    
                    Canvas parentCanvas = hitObject.GetComponentInParent<Canvas>();
                    if (parentCanvas != null){
                        canvasInfo = $" (Canvas: {parentCanvas.name}, SortOrder: {parentCanvas.sortingOrder})";
                    }
                    
                    UnityEngine.UI.Image img = hitObject.GetComponent<UnityEngine.UI.Image>();
                    string imageInfo = "";
                    if (img != null){
                        imageInfo = $" [Image: RaycastTarget={img.raycastTarget}, Color={img.color}]";
                    }
                    
                    CanvasGroup cg = hitObject.GetComponent<CanvasGroup>();
                    string cgInfo = "";
                    if (cg != null){
                        cgInfo = $" [CanvasGroup: BlocksRaycasts={cg.blocksRaycasts}, Alpha={cg.alpha}, Interactable={cg.interactable}]";
                    }
                    
                    Debug.Log($"Hit {i}: {hitObject.name}{canvasInfo}{imageInfo}{cgInfo}");
                    Debug.Log($"  - Position: {hitObject.transform.position}");
                    Debug.Log($"  - Active: {hitObject.activeInHierarchy}");
                    
                    UnityEngine.UI.Button btn = hitObject.GetComponent<UnityEngine.UI.Button>();
                    if (btn != null){
                        Debug.Log($"  - BUTTON FOUND! Interactable: {btn.interactable}, Listeners: {btn.onClick.GetPersistentEventCount()}");
                    }
                }
            }
            
            Debug.Log("=== CLICK DEBUG END ===");
        }
    }
}
