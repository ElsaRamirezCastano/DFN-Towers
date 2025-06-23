using UnityEngine;
using UnityEngine.UI;

public class CanvasGroupDebugger : MonoBehaviour
{
    [Header("Debug Options")]
    public bool debugOnStart = true;
    public bool debugOnClick = true;
    public bool showDetailedLogs = true;
    
    void Start()
    {
        if (debugOnStart)
        {
            DebugAllCanvasGroups();
        }
    }
    
    void Update()
    {
        if (debugOnClick && Input.GetMouseButtonDown(0))
        {
            DebugButtonUnderMouse();
        }
    }
    
    public void DebugAllCanvasGroups()
    {
        Debug.Log("=================== CANVAS GROUP DEBUG START ===================");
        
        // Encontrar todos los CanvasGroups en la escena
        CanvasGroup[] allCanvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
        
        Debug.Log($"TOTAL CANVAS GROUPS ENCONTRADOS: {allCanvasGroups.Length}");
        
        if (allCanvasGroups.Length == 0)
        {
            Debug.LogWarning("NO SE ENCONTRARON CANVAS GROUPS EN LA ESCENA");
            return;
        }
        
        foreach (CanvasGroup cg in allCanvasGroups)
        {
            Debug.Log($"--- CanvasGroup: {cg.name} ---");
            Debug.Log($"  Alpha: {cg.alpha}");
            Debug.Log($"  Interactable: {cg.interactable}");
            Debug.Log($"  BlocksRaycasts: {cg.blocksRaycasts}");
            Debug.Log($"  IgnoreParentGroups: {cg.ignoreParentGroups}");
            Debug.Log($"  GameObject Active: {cg.gameObject.activeInHierarchy}");
            Debug.Log($"  Path: {GetGameObjectPath(cg.gameObject)}");
            
            // ALERTA SI HAY PROBLEMAS
            if (cg.alpha <= 0)
            {
                Debug.LogError($"  *** ALERTA: Alpha = 0 en {cg.name} ***");
            }
            if (!cg.interactable)
            {
                Debug.LogError($"  *** ALERTA: No interactable en {cg.name} ***");
            }
            if (!cg.blocksRaycasts)
            {
                Debug.LogError($"  *** ALERTA: No bloquea raycasts en {cg.name} ***");
            }
            
            // Verificar si tiene CanvasGroups padres
            CanvasGroup[] parentGroups = cg.GetComponentsInParent<CanvasGroup>();
            if (parentGroups.Length > 1)
            {
                Debug.Log($"  TIENE {parentGroups.Length - 1} CANVAS GROUPS PADRES:");
                for (int i = 1; i < parentGroups.Length; i++)
                {
                    CanvasGroup parent = parentGroups[i];
                    Debug.Log($"    Padre {i}: {parent.name}");
                    Debug.Log($"      - Interactable: {parent.interactable}");
                    Debug.Log($"      - BlocksRaycasts: {parent.blocksRaycasts}");
                    Debug.Log($"      - Alpha: {parent.alpha}");
                    
                    if (!parent.interactable || !parent.blocksRaycasts || parent.alpha <= 0)
                    {
                        Debug.LogError($"      *** PROBLEMA CRÍTICO: Padre '{parent.name}' está BLOQUEANDO interacción! ***");
                    }
                }
            }
            
            Debug.Log(""); // Línea en blanco para separar
        }
        
        Debug.Log("=================== CANVAS GROUP DEBUG END ===================");
    }
    
    public void DebugButtonUnderMouse()
    {
        Vector3 mousePos = Input.mousePosition;
        Debug.Log($"========== DEBUG BOTÓN BAJO MOUSE ({mousePos}) ==========");
        
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log($"Total botones en escena: {allButtons.Length}");
        
        bool foundButtonUnderMouse = false;
        
        foreach (Button btn in allButtons)
        {
            if (!btn.gameObject.activeInHierarchy) continue;
            
            RectTransform rectTransform = btn.GetComponent<RectTransform>();
            if (rectTransform == null) continue;
            
            Canvas canvas = btn.GetComponentInParent<Canvas>();
            if (canvas == null) continue;
            
            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;
            Vector2 localMousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mousePos, camera, out localMousePos);
            
            if (rectTransform.rect.Contains(localMousePos))
            {
                foundButtonUnderMouse = true;
                Debug.Log($"*** BOTÓN ENCONTRADO BAJO EL MOUSE: {btn.name} ***");
                Debug.Log($"  Botón Interactable: {btn.interactable}");
                Debug.Log($"  Botón IsInteractable(): {btn.IsInteractable()}");
                Debug.Log($"  Botón IsActive(): {btn.IsActive()}");
                Debug.Log($"  Path: {GetGameObjectPath(btn.gameObject)}");
                Debug.Log($"  Listeners: {btn.onClick.GetPersistentEventCount()}");
                
                // Verificar Image del botón
                Image btnImage = btn.GetComponent<Image>();
                if (btnImage != null)
                {
                    Debug.Log($"  Image RaycastTarget: {btnImage.raycastTarget}");
                    Debug.Log($"  Image Color: {btnImage.color}");
                }
                
                // VERIFICAR TODOS LOS CANVASGROUPS
                CanvasGroup[] affectingGroups = btn.GetComponentsInParent<CanvasGroup>();
                Debug.Log($"  CanvasGroups que afectan este botón: {affectingGroups.Length}");
                
                bool isBlockedByCanvasGroup = false;
                
                if (affectingGroups.Length == 0)
                {
                    Debug.Log("  ✓ NO HAY CANVAS GROUPS afectando este botón");
                }
                else
                {
                    for (int i = 0; i < affectingGroups.Length; i++)
                    {
                        CanvasGroup cg = affectingGroups[i];
                        bool groupBlocks = (!cg.interactable || !cg.blocksRaycasts || cg.alpha <= 0) && !cg.ignoreParentGroups;
                        
                        Debug.Log($"    [{i}] {cg.name}:");
                        Debug.Log($"        - Interactable: {cg.interactable}");
                        Debug.Log($"        - BlocksRaycasts: {cg.blocksRaycasts}");
                        Debug.Log($"        - Alpha: {cg.alpha}");
                        Debug.Log($"        - IgnoreParentGroups: {cg.ignoreParentGroups}");
                        
                        if (groupBlocks)
                        {
                            Debug.LogError($"        *** ESTE CANVAS GROUP ESTÁ BLOQUEANDO EL BOTÓN! ***");
                            isBlockedByCanvasGroup = true;
                        }
                        else
                        {
                            Debug.Log($"        ✓ Este CanvasGroup NO bloquea");
                        }
                    }
                }
                
                // DIAGNÓSTICO FINAL
                if (isBlockedByCanvasGroup)
                {
                    Debug.LogError($"*** DIAGNÓSTICO: El botón '{btn.name}' está BLOQUEADO por CanvasGroups ***");
                }
                else if (!btn.IsInteractable())
                {
                    Debug.LogError($"*** DIAGNÓSTICO: El botón '{btn.name}' NO es interactuable ***");
                }
                else
                {
                    Debug.Log($"*** DIAGNÓSTICO: El botón '{btn.name}' DEBERÍA FUNCIONAR ***");
                    
                    // Intentar forzar click para confirmar
                    Debug.Log("Intentando forzar click...");
                    btn.onClick.Invoke();
                }
                
                Debug.Log(""); // Separador
            }
        }
        
        if (!foundButtonUnderMouse)
        {
            Debug.LogWarning("*** NO SE ENCONTRÓ NINGÚN BOTÓN BAJO EL MOUSE ***");
        }
        
        Debug.Log("========== FIN DEBUG BOTÓN BAJO MOUSE ==========");
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    
    // Método para llamar desde el inspector o otros scripts
    [ContextMenu("Debug Canvas Groups Now")]
    public void DebugCanvasGroupsNow()
    {
        DebugAllCanvasGroups();
    }
    
    [ContextMenu("Debug Button Under Mouse Now")]
    public void DebugButtonUnderMouseNow()
    {
        DebugButtonUnderMouse();
    }
}