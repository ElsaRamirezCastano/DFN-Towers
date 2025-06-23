using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ButtonDiagnostic : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool enableDetailedLogging = true;
    public bool drawDebugLines = true;
    
    void Start()
    {
        DiagnoseUISetup();
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DiagnoseMouseClick();
        }
        
        if (drawDebugLines)
        {
            DrawDebugInfo();
        }
    }
    
    void DiagnoseUISetup()
    {
        Debug.Log("=== DIAGNÓSTICO COMPLETO DE UI ===");
        
        // 1. Verificar Canvas
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Debug.Log($"Canvases encontrados: {canvases.Length}");
        
        foreach (Canvas canvas in canvases)
        {
            Debug.Log($"Canvas: {canvas.name}");
            Debug.Log($"  - Render Mode: {canvas.renderMode}");
            Debug.Log($"  - Sort Order: {canvas.sortingOrder}");
            Debug.Log($"  - Override Sorting: {canvas.overrideSorting}");
            
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                Debug.Log($"  - Graphic Raycaster: ✓");
                Debug.Log($"  - Ignore Reversed Graphics: {raycaster.ignoreReversedGraphics}");
                Debug.Log($"  - Blocking Objects: {raycaster.blockingObjects}");
            }
            else
            {
                Debug.LogError($"  - Graphic Raycaster: ✗ FALTA");
            }
        }
        
        // 2. Verificar EventSystem
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            Debug.Log($"EventSystem: ✓ {eventSystem.name}");
            Debug.Log($"  - Current Selected: {eventSystem.currentSelectedGameObject}");
            Debug.Log($"  - Send Navigation Events: {eventSystem.sendNavigationEvents}");
        }
        else
        {
            Debug.LogError("EventSystem: ✗ FALTA");
        }
        
        // 3. Análisis detallado de botones
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log($"\nBotones encontrados: {buttons.Length}");
        
        foreach (Button button in buttons)
        {
            AnalyzeButton(button);
        }
    }
    
    void AnalyzeButton(Button button)
    {
        Debug.Log($"\n--- ANÁLISIS BOTÓN: {button.name} ---");
        
        // Configuración básica
        Debug.Log($"Interactable: {button.interactable}");
        Debug.Log($"Enabled: {button.enabled}");
        Debug.Log($"GameObject Active: {button.gameObject.activeInHierarchy}");
        
        // Transform y posición
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            Debug.Log($"Position: {rectTransform.position}");
            Debug.Log($"Local Position: {rectTransform.localPosition}");
            Debug.Log($"Anchored Position: {rectTransform.anchoredPosition}");
            Debug.Log($"Size Delta: {rectTransform.sizeDelta}");
            Debug.Log($"Scale: {rectTransform.localScale}");
            
            // Verificar si está dentro de los límites del Canvas
            Canvas parentCanvas = button.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                bool isInsideCanvas = RectTransformUtility.RectangleContainsScreenPoint(
                    canvasRect, rectTransform.position, parentCanvas.worldCamera);
                Debug.Log($"Dentro del Canvas: {isInsideCanvas}");
            }
        }
        
        // Componente Image
        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            Debug.Log($"Image Raycast Target: {image.raycastTarget}");
            Debug.Log($"Image Color: {image.color}");
            Debug.Log($"Image Alpha: {image.color.a}");
            Debug.Log($"Image Enabled: {image.enabled}");
        }
        else
        {
            Debug.LogWarning("No tiene componente Image");
        }
        
        // Verificar componentes hijos que puedan interferir
        Component[] childComponents = button.GetComponentsInChildren<Component>();
        foreach (Component comp in childComponents)
        {
            if (comp is Graphic graphic && graphic.raycastTarget && comp.gameObject != button.gameObject)
            {
                Debug.LogWarning($"Componente hijo con raycast: {comp.GetType().Name} en {comp.gameObject.name}");
            }
        }
        
        // Verificar eventos asignados
        if (button.onClick.GetPersistentEventCount() > 0)
        {
            Debug.Log($"Eventos onClick asignados: {button.onClick.GetPersistentEventCount()}");
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                Debug.Log($"  - Target: {button.onClick.GetPersistentTarget(i)}");
                Debug.Log($"  - Method: {button.onClick.GetPersistentMethodName(i)}");
            }
        }
        else
        {
            Debug.LogWarning("No hay eventos onClick asignados");
        }
    }
    
    void DiagnoseMouseClick()
    {
        Vector2 mousePosition = Input.mousePosition;
        Debug.Log($"\n=== CLIC DETECTADO EN: {mousePosition} ===");
        
        // Verificar si hay EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogError("No hay EventSystem activo!");
            return;
        }
        
        // Crear datos del puntero
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        
        // Hacer raycast
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        Debug.Log($"Objetos detectados por raycast: {results.Count}");
        
        if (results.Count == 0)
        {
            Debug.LogWarning("¡No se detectó ningún objeto UI!");
            
            // Verificar si el mouse está sobre algún botón manualmente
            Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                if (IsMouseOverButton(button, mousePosition))
                {
                    Debug.LogError($"El mouse ESTÁ sobre {button.name} pero NO se detecta por raycast!");
                }
            }
        }
        else
        {
            foreach (RaycastResult result in results)
            {
                Debug.Log($"Detectado: {result.gameObject.name}");
                Debug.Log($"  - Componente: {result.gameObject.GetComponent<Graphic>()?.GetType().Name}");
                Debug.Log($"  - Raycast Target: {result.gameObject.GetComponent<Graphic>()?.raycastTarget}");
                Debug.Log($"  - Distance: {result.distance}");
                Debug.Log($"  - Sort Order: {result.sortingOrder}");
                
                Button button = result.gameObject.GetComponent<Button>();
                if (button != null)
                {
                    Debug.Log($"  - Es un botón: {button.name}");
                    Debug.Log($"  - Interactable: {button.interactable}");
                    
                    // Simular clic manual
                    if (button.interactable)
                    {
                        Debug.Log($"  - SIMULANDO CLIC EN: {button.name}");
                        button.onClick.Invoke();
                    }
                }
            }
        }
    }
    
    bool IsMouseOverButton(Button button, Vector2 mousePosition)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        Canvas canvas = button.GetComponentInParent<Canvas>();
        
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition, null);
        }
        else
        {
            return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition, canvas.worldCamera);
        }
    }
    
    void DrawDebugInfo()
    {
        // Dibujar información visual en la pantalla
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        
        foreach (Button button in buttons)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            
            // Dibujar un marco alrededor del botón
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            for (int i = 0; i < 4; i++)
            {
                Debug.DrawLine(corners[i], corners[(i + 1) % 4], Color.red, 0.1f);
            }
        }
    }
    
    // Método para llamar desde el Inspector
    [ContextMenu("Diagnóstico Completo")]
    public void RunCompleteDiagnostic()
    {
        DiagnoseUISetup();
    }
    
    // Métodos de prueba para conectar a botones
    public void TestButtonClick()
    {
        Debug.Log("¡BOTÓN FUNCIONANDO! - TestButtonClick ejecutado");
    }
    
    public void TestRestartButton()
    {
        Debug.Log("¡BOTÓN RESTART FUNCIONANDO!");
    }
    
    public void TestMenuButton()
    {
        Debug.Log("¡BOTÓN MENU FUNCIONANDO!");
    }
}