using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour{
    public AreaList areaList;
    public static GameManager Instance {get; private set;}

    [SerializeField]private TextMeshProUGUI counterText;
    [SerializeField] private int MaxEnemies = 5;

    [SerializeField] private GameObject gameOverScreenPrefab; 
    [SerializeField] private GameObject winPanelPrefab;

    private int enemiesReachedEnd = 0;

    private bool gameOver = false;

    private void Awake(){
        if(Instance == null){
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            InitializePanels();
        }
        else{
            Destroy(gameObject);
            return;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void InitializePanels(){
        if (gameOverScreenPrefab != null){
            CanvasGroup panelGroup = gameOverScreenPrefab.GetComponent<CanvasGroup>();
            if (panelGroup != null){
                panelGroup.alpha = 0f;
                panelGroup.interactable = false;
                panelGroup.blocksRaycasts = false;
            }
            SetActive(gameOverScreenPrefab, false);
        }

        if (winPanelPrefab != null){
            CanvasGroup panelGroup = winPanelPrefab.GetComponent<CanvasGroup>();
            if (panelGroup != null){
                panelGroup.alpha = 0f;
                panelGroup.interactable = false;
                panelGroup.blocksRaycasts = false;
            }
            SetActive(winPanelPrefab, false);
        }
    }

    private void OnDestroy(){
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        counterText = GameObject.FindGameObjectWithTag("EnemiesInCastle")?.GetComponent<TextMeshProUGUI>();
        InitializePanels();
        ResetGameState();

        if (BuildingSystem.current != null){
            BuildingSystem.current.ReinitializedUI();
            if (BuildingModeUI.instance != null){
                BuildingModeUI.instance.ShowBuildingModeUI();
                BuildingModeUI.instance.HideBuildingModeUI();
            }
        }
        UpdateCounterUI();
    }

    void Start(){
        Time.timeScale = 1f;
        UpdateCounterUI();      
    }

    public void EnemyReachedEnd(){
        enemiesReachedEnd++;
        Debug.Log("Enemy reached end: " + enemiesReachedEnd);
        UpdateCounterUI();
        if(enemiesReachedEnd >= MaxEnemies && !gameOver){
            GameOver();
        }
    }

    private void UpdateCounterUI(){
        if(counterText != null){
            counterText.text = $"Enemies in the castle: {enemiesReachedEnd}/{MaxEnemies}";
        }
        else{
            Debug.LogWarning("Enemy counter text not found");
        }
    }

    private void GameOver(){
        Debug.Log("=== INICIANDO GAME OVER ===");
        Time.timeScale = 0f;
        if (gameOverScreenPrefab != null){
            Debug.Log("GameOverScreenPrefab encontrado");

            SetActive(gameOverScreenPrefab, true);
            Debug.Log($"GameObject activado: {gameOverScreenPrefab.activeInHierarchy}");

            CanvasGroup panelGroup = gameOverScreenPrefab.GetComponent<CanvasGroup>();
            if (panelGroup != null){
                Debug.Log("CanvasGroup encontrado, configurando...");
                panelGroup.alpha = 1f;
                panelGroup.interactable = true;
                panelGroup.blocksRaycasts = true;

                Debug.Log($"CanvasGroup configurado - Alpha: {panelGroup.alpha}, Interactable: {panelGroup.interactable}, BlocksRaycasts: {panelGroup.blocksRaycasts}");
            }
            else{
                Debug.LogWarning("No se encontró CanvasGroup en el GameOverPanel");
            }

            Button[] buttons = gameOverScreenPrefab.GetComponentsInChildren<Button>();
            Debug.Log($"Botones encontrados en GameOverPanel: {buttons.Length}");

            foreach (Button btn in buttons){
                Debug.Log($"Botón: {btn.name} - Interactable: {btn.interactable} - Enabled: {btn.enabled}");
                btn.interactable = true;
                btn.enabled = true;

                Image img = btn.GetComponent<Image>();
                if (img != null){
                    img.raycastTarget = true;
                    Debug.Log($"Image del botón {btn.name} - RaycastTarget: {img.raycastTarget}");
                }
            }
        }


        gameOver = true;
        Debug.Log("Game Over completado");
    }

    public int GetEnemyCount(){
        return enemiesReachedEnd;
    }

    public void RestartGame(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetGameState(){
        enemiesReachedEnd = 0;
        gameOver = false;
        if (BuildingSystem.current != null){
            BuildingSystem.current.towers.Clear();
            BuildingSystem.current.RestoreDefaultPaths();
            BuildingSystem.current.ExitBuildMode();
        }
        UpdateCounterUI();
    }

    private void SetActive(GameObject obj, bool active){
        if(obj != null){
            try{
                Debug.Log($"Attempting to set {obj.name} to {(active ? "active" : "inactive")}");
                obj.SetActive(active);
                Debug.Log($"New state of {obj.name}:{obj.activeSelf}");
            }
            catch(System.Exception e){
                Debug.LogError($"Error setting {obj.name}: {e.Message}");
            }
        }
        else{
            Debug.LogWarning($"GameObject {obj} is null");
        }
    }

    public void WinGame(){
        if(!gameOver){
            Time.timeScale = 0f;
            if (winPanelPrefab != null){
                SetActive(winPanelPrefab, true);
                CanvasGroup panelGroup = winPanelPrefab.GetComponent<CanvasGroup>();
                if (panelGroup != null){
                    panelGroup.alpha = 1f;
                    panelGroup.interactable = true;
                    panelGroup.blocksRaycasts = true;
                }
            }
            gameOver = true;

            string areaName = PlayerPrefs.GetString("CurrentArea", "");
            AreaData area = areaList.allAreas.Find(a => a.areaName == areaName);
            if (area != null){
                string currentSceneName = SceneManager.GetActiveScene().name;
                int currentIndex = area.levels.FindIndex(level => level.Scene == currentSceneName);
                if (currentIndex >= 0 && currentIndex < area.levels.Count - 1){
                    string nextLevelID = area.levels[currentIndex + 1].levelID;
                    PlayerPrefs.SetInt(nextLevelID, 1);
                    PlayerPrefs.SetString("LastUnlockedLevel", nextLevelID);
                    PlayerPrefs.Save();
                    Debug.Log($"Level {nextLevelID} unlocked");
                }
                else{
                    Debug.Log("No more levels available");
                }
            }
        }
    }
}
