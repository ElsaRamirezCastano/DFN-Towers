using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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
        SetActive(gameOverScreenPrefab, false);
        SetActive(winPanelPrefab, false);
    }

    private void OnDestroy(){
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        counterText = GameObject.FindGameObjectWithTag("EnemiesInCastle")?.GetComponent<TextMeshProUGUI>();
        InitializePanels();
        ResetGameState();

        /*if(counterText == null){
            GameObject counterOBJ = GameObject.FindGameObjectWithTag("EnemiesInCastle");
            if(counterOBJ != null){
                counterText = counterOBJ.GetComponent<TextMeshProUGUI>();
            }
        }*/
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
        Time.timeScale = 0f; 
        SetActive(gameOverScreenPrefab, true);
        gameOver = true;
    }

    public int GetEnemyCount(){
        return enemiesReachedEnd;
    }

    public void RestartGame(){
       /* Destroy(gameOverScreenPrefab);
        Destroy(winPanelPrefab);*/
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ResetGameState(){
        enemiesReachedEnd = 0;
        gameOver = false;
        if(BuildingSystem.current != null){
            BuildingSystem.current.ResetTowerCount();
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
            SetActive(winPanelPrefab, true);
            gameOver = true;

            string areaName = PlayerPrefs.GetString("CurrentArea", "");
            AreaData area = areaList.allAreas.Find(a => a.areaName == areaName);
            if (area != null)
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                int currentIndex = area.levels.FindIndex(level => level.Scene == currentSceneName);
                if (currentIndex >= 0 && currentIndex < area.levels.Count - 1)
                {
                    string nextLevelID = area.levels[currentIndex + 1].levelID;
                    PlayerPrefs.SetInt(nextLevelID, 1);
                    PlayerPrefs.SetString("LastUnlockedLevel", nextLevelID);
                    PlayerPrefs.Save();
                    Debug.Log($"Level {nextLevelID} unlocked");
                }
                else
                {
                    Debug.Log("No more levels available");
                }
            }
        }
    }
}
