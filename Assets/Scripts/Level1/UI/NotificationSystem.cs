using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class NotificationSystem : MonoBehaviour
{
   public static NotificationSystem instance;

   [SerializeField] private GameObject notificationPanel;
   [SerializeField] private TextMeshProUGUI notificationText;
   [SerializeField] private float displayTime = 3f;
   [SerializeField] private float fadeTime = 0.5f;

   private CanvasGroup canvasGroup;
   private Coroutine fadeCoroutine;

   private void Awake(){
    if(instance != null && instance != this){
        Destroy(gameObject);
    }
    else{
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
    if(canvasGroup == null){
        canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
    }

    canvasGroup.alpha = 0f;
    notificationPanel.SetActive(false);
   }

   private void Start(){
    ShowNotification("Press B to access building mode");
   }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        canvasGroup.alpha = 0f;
        notificationPanel.SetActive(false);
        ShowNotification("Press B to access building mode");
   }

    private void OnDestroy(){
        SceneManager.sceneLoaded -= OnSceneLoaded;
   }

   public void ShowNotification(string message){
        if (fadeCoroutine != null){
            StopCoroutine(fadeCoroutine);
        }

        notificationText.text = message;
        notificationPanel.SetActive(true);
        fadeCoroutine = StartCoroutine(FadeNotification());
    }

   private IEnumerator FadeNotification(){
    canvasGroup.alpha = 0f;
    float elapsedTime = 0f;

    while(elapsedTime < fadeTime){
        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime/fadeTime);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    canvasGroup.alpha = 1f;

    yield return new WaitForSeconds(displayTime);

    elapsedTime = 0f;

    while(elapsedTime < fadeTime){
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime/fadeTime);
        elapsedTime += Time.deltaTime;
        yield return null;
    }
    canvasGroup.alpha = 0f;
    notificationPanel.SetActive(false);
   }
}
