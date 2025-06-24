using UnityEngine;
using UnityEngine.UI;

public class PathDeleteButton : MonoBehaviour{
    [Header("UI References")]
    public Button deleteButton;
    public Button cancelButton;
    public GameObject deleteButtonUI;

    private Vector3Int pathPosition;
    private Camera mainCamera;
    private bool isVisible = false;

    private void Awake(){
        mainCamera = Camera.main;
        if (deleteButton != null){
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }

        if (cancelButton != null){
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }

        HideDeleteButton();
    }

    public void ShowDeleteButton(Vector3Int position){
        pathPosition = position;

        if (deleteButtonUI != null){
            Vector3 worldPos = BuildingSystem.current.gridLayout.CellToWorld(position);
            worldPos.x += BuildingSystem.current.gridLayout.cellSize.x / 2;
            worldPos.y += BuildingSystem.current.gridLayout.cellSize.y / 2;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            RectTransform rectTransform = deleteButtonUI.GetComponent<RectTransform>();
            if (rectTransform != null){
                rectTransform.position = screenPos;
            }

            deleteButtonUI.SetActive(true);
            isVisible = true;
        }
    }

    public void HideDeleteButton(){
        if (deleteButtonUI != null){
            deleteButtonUI.SetActive(false);
            isVisible = false;
        }
    }

    private void OnDeleteButtonClicked(){
        BuildingSystem.current.RemovePath(pathPosition);
        HideDeleteButton();
    }

    private void OnCancelButtonClicked(){
        HideDeleteButton();
    }

    public bool IsVisible(){
        return isVisible;
    }

    public Vector3Int GetCurrentPathPosition(){
        return pathPosition;
    }
}
