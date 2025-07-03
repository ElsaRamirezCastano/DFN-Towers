using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PathSelector : MonoBehaviour{
    [Header("Path Action UI")]
    public GameObject pathActionPanelPrefab;

    [Header("Canvas eference")]
    public Canvas targetCanvas;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference leftClickAction;
    [SerializeField] private InputActionReference mousePositionAction;

    private GameObject pathActionPanel;
    private CanvasGroup panelCanvasGroup;
    private Button deleteButton;
    private Button cancelButton;

    private float holdTime = 0.5f;
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool actionPanelActive = false;

    private Camera mainCamera;
    private Vector3Int hoveredPosition = Vector3Int.zero;
    private bool validHover = false;
    
    private bool isMousePressed = false;

    void Start(){
        mainCamera = Camera.main;

        if (targetCanvas == null){
            targetCanvas = FindFirstObjectByType<Canvas>();
        }

        CreatePathActionPanel();
        EnableInputAction();
    }

    void OnDestroy(){
        DisableInputAction();

        if (pathActionPanel != null){
            Destroy(pathActionPanel);
        }
    }

    private void EnableInputAction(){
        if (leftClickAction != null){
            leftClickAction.action.Enable();
            leftClickAction.action.started += OnLeftClick;
            leftClickAction.action.canceled += OnLeftClick;
        }

        if (mousePositionAction != null){
            mousePositionAction.action.Enable();
        }
    }

    private void DisableInputAction(){
        if (leftClickAction != null){
            leftClickAction.action.started -= OnLeftClick;
            leftClickAction.action.canceled -= OnLeftClick;
            leftClickAction.action.Disable();
        }

        if (mousePositionAction != null){
            mousePositionAction.action.Disable();
        }
    }

    private void CreatePathActionPanel(){
        if (pathActionPanelPrefab != null && pathActionPanel == null){
            if (targetCanvas != null){
                pathActionPanel = Instantiate(pathActionPanelPrefab, targetCanvas.transform);
            }
            else{
                pathActionPanel = Instantiate(pathActionPanelPrefab);
                Debug.Log("No canvas found, this may cause ui problems");
            }
            pathActionPanel.name = "PathActionPanel";

            panelCanvasGroup = pathActionPanel.GetComponent<CanvasGroup>();
            if (panelCanvasGroup == null){
                panelCanvasGroup = pathActionPanel.AddComponent<CanvasGroup>();
            }

            deleteButton = pathActionPanel.transform.Find("DeleteButton")?.GetComponent<Button>();
            cancelButton = pathActionPanel.transform.Find("CancelButton")?.GetComponent<Button>();

            SetupButtonListeners();
            HidePathActionPanel();
        }
    }

    private void SetupButtonListeners(){
        if (deleteButton != null){
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        if (cancelButton != null){
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
    }

    void Update(){
        if (actionPanelActive) return;

        UpdateHover();
        HandleHoldInput();
    }

    private void UpdateHover(){
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3Int cellPos = BuildingSystem.current.gridLayout.WorldToCell(mousePos);

        hoveredPosition = cellPos;
        validHover = BuildingSystem.current.IsAnyPathAt(cellPos);
    }

    private Vector3 GetMouseWorldPosition(){
        if (mainCamera != null && mousePositionAction != null){
            Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
            worldPos.z = 0;
            return worldPos;
        }
        return Vector3.zero;
    }


    private void HandleHoldInput(){
        if (validHover && isMousePressed){
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdTime && !isHolding){
                ShowPathActionPanel(hoveredPosition);
                isHolding = true;
            }
        }
        else{
            holdTimer = 0f;
            isHolding = false;
        }
    }

    public void OnLeftClick(InputAction.CallbackContext context){
        if (context.started){
            isMousePressed = true;
        }
        else if (context.canceled){
            isMousePressed = false;

            if (actionPanelActive && !IsClickOnPanel()){
                HidePathActionPanel();
            }
        }
    }

    private bool IsClickOnPanel(){
        if (pathActionPanel == null || mousePositionAction == null) return false;

        Vector2 mousePos = mousePositionAction.action.ReadValue<Vector2>();
        RectTransform panelRect = pathActionPanel.GetComponent<RectTransform>();
        if (panelRect != null){
            return RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos, targetCanvas?.worldCamera);
        }
        return false;
    }

    private void ShowPathActionPanel(Vector3Int position){
        if (pathActionPanel == null) return;

        hoveredPosition = position;

        Vector3 worldPos = BuildingSystem.current.gridLayout.CellToWorld(position);
        worldPos.x += BuildingSystem.current.gridLayout.cellSize.x / 2;
        worldPos.y += BuildingSystem.current.gridLayout.cellSize.y / 2;

        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        RectTransform rectTransform = pathActionPanel.GetComponent<RectTransform>();
        if (rectTransform != null){
            rectTransform.position = screenPos;
        }

        pathActionPanel.SetActive(true);

        if (panelCanvasGroup != null){
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        actionPanelActive = true;

        Debug.Log($"Showing path action panel at position {position}");
   }

    private void HidePathActionPanel(){
        if (pathActionPanel != null){
            if (panelCanvasGroup != null){
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
            Debug.Log("Hiding path action panel");
            pathActionPanel.SetActive(false);
            Debug.Log("Path action panel hidden");
            actionPanelActive = false;
        }
   }

    private void OnDeleteButtonClicked(){
        Debug.Log($"Delete button clicked for position {hoveredPosition}");

        Vector3 worldPos = BuildingSystem.current.gridLayout.CellToWorld(hoveredPosition);
        worldPos.x += BuildingSystem.current.gridLayout.cellSize.x / 2;
        worldPos.y += BuildingSystem.current.gridLayout.cellSize.y / 2;

        System.Action confirmAction = () =>{
            BuildingSystem.current.RemovePath(hoveredPosition);
            HidePathActionPanel();
        };

        System.Action cancelAction = () =>{
            HidePathActionPanel();
        };

        if (ConfirmationUi.instance != null){
            ConfirmationUi.instance.ShowConfirmation(null, worldPos, confirmAction, cancelAction);
        }
        else{
            BuildingSystem.current.RemovePath(hoveredPosition);
            HidePathActionPanel();
        }
   }

    private void OnCancelButtonClicked(){
        Debug.Log("Cancel button clicked");
        HidePathActionPanel();
   }
}


public static class ConfirmationUiExtension{
    public static void ShowConfirmation(this ConfirmationUi ui, GameObject targetObject, Vector3 position, System.Action confirmAction, System.Action cancelAction, string customMessage = null){
        if (string.IsNullOrEmpty(customMessage)){
            ui.ShowConfirmation(targetObject, position, confirmAction, cancelAction);
        }
        else{
            if(NotificationSystem.instance != null){
                NotificationSystem.instance.ShowNotification(customMessage);
            }
            ui.ShowConfirmation(targetObject, position, confirmAction, cancelAction);
        }
    }
}
