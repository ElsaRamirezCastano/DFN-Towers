using UnityEngine;
using UnityEngine.UI;

public class BuildingModeUI : MonoBehaviour{
    public static BuildingModeUI instance;

    [Header("UI References")]
    public GameObject buildingModePanel;
    public Button towerButton;
    public Button pathButton;
    public Button closeButton;

    private void Awake(){
        if (instance == null){
            instance = this;
        }
        else{
            Destroy(gameObject);
            return;
        }

        HideBuildingModeUI();
    }

    private void Start(){
        ValidateReferences();
        SetupButtons();
    }

    private void ValidateReferences(){
        if (buildingModePanel == null){
            buildingModePanel = transform.Find("BuildingButtonPanel")?.gameObject;
            if (buildingModePanel == null){
                Debug.Log("BuildingButtonPanel not found in BuildingModeUI.");
            }
        }

        if (towerButton == null){
            towerButton = GetComponentInChildren<Button>();
            Transform towerButtonTransform = transform.Find("BuildingButtonPanel/TowerBuilding");
            if (towerButtonTransform != null){
                towerButton = towerButtonTransform.GetComponent<Button>();
            }
        }

        if (pathButton == null){
            Transform pathButtonTransform = transform.Find("BuildingButtonPanel/PathBuilding");
            if (pathButtonTransform != null){
                pathButton = pathButtonTransform.GetComponent<Button>();
            }
        }

        if(closeButton == null){
            Transform closeButtonTransform = transform.Find("BuildingButtonPanel/CancelBuilding");
            if (closeButtonTransform != null){
                closeButton = closeButtonTransform.GetComponent<Button>();
            }
        }
    }

    public void SetupButtons(){
        if (towerButton != null){
            towerButton.onClick.RemoveAllListeners();
            towerButton.onClick.AddListener(() => OnTowerButtonClicked());
            Debug.Log("Tower button setup complete.");
        }
        if (pathButton != null){
            pathButton.onClick.RemoveAllListeners();
            pathButton.onClick.AddListener(() => OnPathButtonClicked());
            Debug.Log("Path button setup complete.");
        }

        if (closeButton != null){
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => OnCloseButtonClicked());
            Debug.Log("Close button setup complete.");
        }
    }

    public void OnTowerButtonClicked(){
        Debug.Log("Tower button clicked.");
        if (BuildingSystem.current != null){
            BuildingSystem.current.SetBuildingMode(BuildingType.Tower);
            HideBuildingModeUI();
        }
    }

    public void OnPathButtonClicked(){
        if (BuildingSystem.current != null){
            BuildingSystem.current.SetBuildingMode(BuildingType.Path);
            HideBuildingModeUI();
        }
    }

    public void OnCloseButtonClicked(){
        HideBuildingModeUI();
        if (BuildingSystem.current != null){
            BuildingSystem.current.ExitBuildMode();
        }
    }

    public void ShowBuildingModeUI(){
        if (buildingModePanel != null){
            buildingModePanel.SetActive(true);
            StartCoroutine(ConfigureCanvasGroupDelayed());
            ValidateReferences();
            SetupButtons();
        }
    }

    private System.Collections.IEnumerator ConfigureCanvasGroupDelayed(){
        yield return null;

        CanvasGroup panelGroup = buildingModePanel.GetComponent<CanvasGroup>();
        if (panelGroup != null){
            panelGroup.alpha = 1f;
            panelGroup.interactable = true;
            panelGroup.blocksRaycasts = true;
        }
    }

    public void HideBuildingModeUI(){
        if (buildingModePanel != null){
            buildingModePanel.SetActive(false);
            CanvasGroup panelGroup = buildingModePanel.GetComponent<CanvasGroup>();
            if (panelGroup != null){
                panelGroup.alpha = 0f;
                panelGroup.interactable = false;
                panelGroup.blocksRaycasts = false;
            }
        }
    }

    public void ResetUI(){
        SetupButtons();
        HideBuildingModeUI();
    }
}

public enum BuildingType{
    None,
    Tower,
    Path
}
