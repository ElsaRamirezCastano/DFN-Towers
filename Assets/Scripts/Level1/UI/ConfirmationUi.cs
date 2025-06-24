using UnityEngine;
using UnityEngine.UI;
using System;

public class ConfirmationUi : MonoBehaviour{
    public static ConfirmationUi instance;

    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private GameObject currentObject;
    private System.Action onConfirm;
    private System.Action onCancel;

    private void Awake(){
        if(instance != null && instance != this){
            Destroy(gameObject);
        }
        else{
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        confirmationPanel.SetActive(false);
        yesButton.onClick.AddListener(OnConfirmPLacement);
        noButton.onClick.AddListener(OnCancelPlacement);
    }

    public void ShowConfirmation(GameObject obj, Vector3 position, System.Action confirmAction, System.Action cancelAction){
        
        currentObject = obj;
        onConfirm = confirmAction;
        onCancel = cancelAction;

        confirmationPanel.SetActive(true);
        RectTransform panelRect = confirmationPanel.GetComponent<RectTransform>();
        if(panelRect != null){
            Canvas canvas = confirmationPanel.GetComponentInParent<Canvas>();
            if(canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay){
                 panelRect.anchorMin = new Vector2(0.5f, 1f);
                panelRect.anchorMax = new Vector2(0.5f, 1f);
                panelRect.pivot = new Vector2(0.5f, 1f);
                panelRect.anchoredPosition = new Vector2(0, -50f);
            }
            else{
                Vector3 screenPos = new Vector3(Screen.width / 2f, Screen.height - 50f, 0);
                confirmationPanel.transform.position = screenPos;
            }
        }
    }

    private void OnConfirmPLacement(){
        confirmationPanel.SetActive(false);
        onConfirm?.Invoke();
        ResetActions();
    }
    
    private void OnCancelPlacement(){
        confirmationPanel.SetActive(false);
        onCancel?.Invoke();
        ResetActions();
    }

    private void ResetActions(){
        currentObject = null;
        onConfirm = null;
        onCancel = null;
    }
}
