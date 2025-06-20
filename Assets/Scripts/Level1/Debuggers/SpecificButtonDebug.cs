using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SpecificButtonDebug : MonoBehaviour
{
    [Header("Referencias directas (arrastra tus botones aquí)")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;
    
    void Start()
    {
        Debug.Log("=== DIAGNÓSTICO ESPECÍFICO DE BOTONES ===");
        
        // Si no tienes referencias, buscarlos automáticamente
        if (restartButton == null || menuButton == null)
        {
            FindButtonsAutomatically();
        }
        
        // Análisis específico
        if (restartButton != null) AnalyzeSpecificButton(restartButton, "RESTART");
        if (menuButton != null) AnalyzeSpecificButton(menuButton, "MENU");
        
        // Configurar eventos de prueba
        SetupTestEvents();
    }
    
    void FindButtonsAutomatically()
    {
        Button[] allButtons = FindObjectsOfType<Button>();
        Debug.Log($"Botones encontrados automáticamente: {allButtons.Length}");
        
        foreach (Button btn in allButtons)
        {
            Debug.Log($"Botón encontrado: {btn.name}");
            
            // Intentar identificar por nombre
            if (btn.name.ToLower().Contains("restart"))
                restartButton = btn;
            else if (btn.name.ToLower().Contains("menu"))
                menuButton = btn;
        }
    }
    
    void AnalyzeSpecificButton(Button button, string buttonName)
    {
        Debug.Log($"\n--- ANÁLISIS DETALLADO: {buttonName} ---");
        Debug.Log($"Nombre del GameObject: {button.name}");
        Debug.Log($"Activo en jerarquía: {button.gameObject.activeInHierarchy}");
        Debug.Log($"Activo por sí mismo: {button.gameObject.activeSelf}");
        Debug.Log($"Componente Button habilitado: {button.enabled}");
        Debug.Log($"Interactable: {button.interactable}");
        
        // Verificar RectTransform
        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            Debug.Log($"Posición mundial: {rect.position}");
            Debug.Log($"Posición local: {rect.localPosition}");
            Debug.Log($"Escala: {rect.localScale}");
            Debug.Log($"Tamaño: {rect.sizeDelta}");
            
            // Verificar si está visible en pantalla
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Debug.Log($"Esquinas del botón: {corners[0]} a {corners[2]}");
        }
        
        // Verificar Image
        Image img = button.GetComponent<Image>();
        if (img != null)
        {
            Debug.Log($"Image raycast target: {img.raycastTarget}");
            Debug.Log($"Image color: {img.color} (Alpha: {img.color.a})");
            Debug.Log($"Image habilitado: {img.enabled}");
            Debug.Log($"Target Graphic asignado: {button.targetGraphic != null}");
            Debug.Log($"Target Graphic es la misma Image: {button.targetGraphic == img}");
            
            if (img.color.a < 0.01f)
            {
                Debug.LogError($"¡PROBLEMA! El botón {buttonName} es casi transparente (alpha muy bajo)");
            }
        }
        else
        {
            Debug.LogError($"¡PROBLEMA! El botón {buttonName} no tiene componente Image");
        }
        
        // Verificar jerarquía padre
        Transform parent = button.transform.parent;
        while (parent != null)
        {
            Debug.Log($"Padre: {parent.name} - Activo: {parent.gameObject.activeInHierarchy}");
            
            // Verificar si hay un Canvas Group que pueda estar bloqueando
            CanvasGroup canvasGroup = parent.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"Canvas Group encontrado en {parent.name}:");
                Debug.Log($"  - Alpha: {canvasGroup.alpha}");
                Debug.Log($"  - Interactable: {canvasGroup.interactable}");
                Debug.Log($"  - Blocks Raycasts: {canvasGroup.blocksRaycasts}");
                
                if (!canvasGroup.interactable || !canvasGroup.blocksRaycasts)
                {
                    Debug.LogError($"¡PROBLEMA! Canvas Group en {parent.name} está bloqueando interacción");
                }
            }
            
            parent = parent.parent;
        }
        
        // Verificar eventos
        Debug.Log($"Eventos persistentes: {button.onClick.GetPersistentEventCount()}");
        if (button.onClick.GetPersistentEventCount() == 0)
        {
            Debug.LogWarning($"No hay eventos asignados al botón {buttonName}");
        }
    }
    
    void SetupTestEvents()
    {
        // Limpiar eventos existentes (solo para prueba)
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => TestButtonClick("RESTART"));
        }
        
        if (menuButton != null)
        {
            menuButton.onClick.RemoveAllListeners();
            menuButton.onClick.AddListener(() => TestButtonClick("MENU"));
        }
        
        Debug.Log("Eventos de prueba configurados. Haz clic en los botones para probar.");
    }
    
    void TestButtonClick(string buttonName)
    {
        Debug.Log($"¡¡¡ÉXITO!!! Botón {buttonName} clickeado correctamente");
        
        // Efecto visual para confirmar
        if (buttonName == "RESTART")
        {
            Debug.Log("Simulando reinicio de juego...");
        }
        else if (buttonName == "MENU")
        {
            Debug.Log("Simulando ir al menú...");
        }
    }
    
    void Update()
    {
        // Detección manual de clics para debugging
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Debug.Log($"Clic detectado en: {mousePos}");
            
            // Verificar manualmente si el mouse está sobre cada botón
            if (restartButton != null && IsMouseOverButton(restartButton, mousePos))
            {
                Debug.Log("Mouse está sobre RESTART button - debería funcionar");
            }
            
            if (menuButton != null && IsMouseOverButton(menuButton, mousePos))
            {
                Debug.Log("Mouse está sobre MENU button - debería funcionar");
            }
            
            // Raycast manual
            PointerEventData eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePos;
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            
            Debug.Log($"Objetos detectados por raycast: {results.Count}");
            foreach (var result in results)
            {
                Debug.Log($"  - {result.gameObject.name} (Distancia: {result.distance})");
                
                Button btn = result.gameObject.GetComponent<Button>();
                if (btn != null)
                {
                    Debug.Log($"    Es un botón: {btn.interactable}");
                }
            }
        }
    }
    
    bool IsMouseOverButton(Button button, Vector2 mousePosition)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        Canvas canvas = button.GetComponentInParent<Canvas>();
        
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePosition, cam);
    }
    
    // Métodos para llamar desde el Inspector
    [ContextMenu("Forzar Análisis")]
    public void ForceAnalysis()
    {
        Start();
    }
    
    [ContextMenu("Simular Clic Restart")]
    public void SimulateRestartClick()
    {
        if (restartButton != null)
        {
            restartButton.onClick.Invoke();
        }
    }
    
    [ContextMenu("Simular Clic Menu")]
    public void SimulateMenuClick()
    {
        if (menuButton != null)
        {
            menuButton.onClick.Invoke();
        }
    }
}