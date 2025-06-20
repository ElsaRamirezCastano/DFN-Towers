using UnityEngine;
using UnityEngine.UI;

public class PathDeleteButton : MonoBehaviour{
    [Header("UI References")]
    public Button deleteButton;
    public GameObject deleteButtonUI;

    private Vector3Int pathPosition;
    private Camera mainCamera;
    private bool isVisible = false;

    private void Awake(){
        mainCamera = Camera.main;
        if (deleteButton != null){
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
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
        Vector3 worldPos = BuildingSystem.current.gridLayout.CellToWorld(pathPosition);
        worldPos.x += BuildingSystem.current.gridLayout.cellSize.x / 2;
        worldPos.y += BuildingSystem.current.gridLayout.cellSize.y / 2;

        System.Action confirmAction = () =>{
            BuildingSystem.current.RemovePath(pathPosition);
            HideDeleteButton();
        };

        System.Action cancelAction = () =>{
            HideDeleteButton();
        };

        if (ConfirmationUi.instance != null){
            ConfirmationUi.instance.ShowConfirmation(null, worldPos, confirmAction, cancelAction, "Delete path?");
        }

        HideDeleteButton();
    }

    public bool IsVisible(){
        return isVisible;
    }

    public Vector3Int GetCurrentPathPosition(){
        return pathPosition;
    }
}
