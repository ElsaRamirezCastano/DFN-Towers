using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class RaycastInterceptorDebugger : MonoBehaviour
{
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DebugRaycastInterception();
        }
    }

    void DebugRaycastInterception()
    {
        Debug.Log("============ RAYCAST INTERCEPTION DEBUG ============");

        if (EventSystem.current == null)
        {
            Debug.LogError("NO EVENTSYSTEM FOUND!");
            return;
        }

        Vector3 mousePos = Mouse.current.position.ReadValue();
        Debug.Log($"Mouse Position: {mousePos}");

        var pointerData = new ExtendedPointerEventData(EventSystem.current)
        {
            position = mousePos,
            device = Mouse.current
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        Debug.Log($"TOTAL OBJETOS INTERCEPTANDO EL CLICK: {results.Count}");

        if (results.Count == 0)
        {
            Debug.LogError("¡NO HAY OBJETOS INTERCEPTANDO! Problema con EventSystem o Canvas.");
            return;
        }

        bool buttonFound = false;
        int buttonIndex = -1;

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hitObject = results[i].gameObject;
            Button btn = hitObject.GetComponent<Button>();

            if (btn != null)
            {
                buttonFound = true;
                buttonIndex = i;
                break;
            }
        }

        Debug.Log($"¿HAY BOTÓN EN LA LISTA? {buttonFound} (Índice: {buttonIndex})");

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hitObject = results[i].gameObject;
            RaycastResult result = results[i];

            string objectInfo = $"[{i}] {hitObject.name}";

            Canvas parentCanvas = hitObject.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                objectInfo += $" (Canvas: {parentCanvas.name}, SortOrder: {parentCanvas.sortingOrder})";
            }

            Button btn = hitObject.GetComponent<Button>();
            if (btn != null)
            {
                objectInfo += $" *** BOTÓN ***";
                Debug.Log(objectInfo);
                Debug.Log($"    - Botón Interactable: {btn.interactable}");
                Debug.Log($"    - IsInteractable(): {btn.IsInteractable()}");

                if (i == 0)
                {
                    Debug.Log($"    ✓ ESTE BOTÓN ESTÁ PRIMERO - DEBERÍA RECIBIR EL CLICK");
                }
                else
                {
                    Debug.LogError($"    *** PROBLEMA: Este botón está en posición {i}, no recibirá el click ***");
                    Debug.LogError($"    *** El objeto '{results[0].gameObject.name}' está BLOQUEANDO este botón ***");
                }
            }
            else
            {
                string componentInfo = "";

                Image img = hitObject.GetComponent<Image>();
                if (img != null)
                {
                    componentInfo += $" [Image: RaycastTarget={img.raycastTarget}, Color={img.color}, Alpha={img.color.a:F2}]";

                    if (img.raycastTarget && img.color.a < 0.1f)
                    {
                        Debug.LogError($"    *** SOSPECHOSO: Imagen casi transparente pero con RaycastTarget=true ***");
                    }
                }

                Graphic graphic = hitObject.GetComponent<Graphic>();
                if (graphic != null && !(graphic is Image))
                {
                    componentInfo += $" [Graphic: {graphic.GetType().Name}, RaycastTarget={graphic.raycastTarget}]";
                }

                CanvasGroup cg = hitObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    componentInfo += $" [CanvasGroup: Interactable={cg.interactable}, BlocksRaycasts={cg.blocksRaycasts}]";
                }

                Debug.Log(objectInfo + componentInfo);
                Debug.Log($"    - Active: {hitObject.activeInHierarchy}");
                Debug.Log($"    - Layer: {hitObject.layer}");
                Debug.Log($"    - Tag: {hitObject.tag}");
                Debug.Log($"    - Distance: {result.distance}");

                if (i == 0 && buttonFound && buttonIndex > 0)
                {
                    Debug.LogError($"    *** ESTE OBJETO ESTÁ BLOQUEANDO EL BOTÓN '{results[buttonIndex].gameObject.name}' ***");

                    if (img != null && img.raycastTarget)
                    {
                        Debug.LogError($"    >>> SOLUCIÓN: Desactiva 'Raycast Target' en la Image de '{hitObject.name}'");
                    }
                }
            }

            Debug.Log("");
        }

        if (buttonFound && buttonIndex > 0)
        {
            Debug.LogError("=== PROBLEMA IDENTIFICADO ===");
            Debug.LogError($"Tu botón está en la posición {buttonIndex} pero debería estar en la posición 0");
            Debug.LogError($"Los objetos que lo están bloqueando son:");
            for (int i = 0; i < buttonIndex; i++)
            {
                Debug.LogError($"  - [{i}] {results[i].gameObject.name}");
            }
        }
        else if (!buttonFound)
        {
            Debug.LogError("=== PROBLEMA IDENTIFICADO ===");
            Debug.LogError("No se encontró ningún botón en el raycast. Verifica que:");
            Debug.LogError("- El botón tenga una Image con RaycastTarget = true");
            Debug.LogError("- El botón esté activo en la jerarquía");
            Debug.LogError("- El Canvas tenga GraphicRaycaster");
        }
        else if (buttonIndex == 0)
        {
            Debug.Log("✓ El botón está en primera posición, debería funcionar normalmente");
        }

        Debug.Log("============ FIN RAYCAST DEBUG ============");
    }
}