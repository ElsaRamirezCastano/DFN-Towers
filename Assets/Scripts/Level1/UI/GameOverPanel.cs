using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverPanel : MonoBehaviour{

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject counterPanel;
    
    public void RestartGameButton(){
        HideAllPanels();
        if (GameManager.Instance != null){
            GameManager.Instance.RestartGame();
        }
        Time.timeScale = 1f;
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Game Restarted");
    }

    public void BackToMenu(){
        HideAllPanels();
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
        Debug.Log("Game Quit");
    }

    private void HideAllPanels(){
        if(winPanel != null){
            winPanel.SetActive(false);
        }
        if(gameOverPanel != null){
            gameOverPanel.SetActive(false);
        }
        if(counterPanel != null){
            counterPanel.SetActive(false);
        }
    }

    public void GoNextLevel(){
        Time.timeScale = 1f;
        HideAllPanels();
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if(nextIndex < SceneManager.sceneCountInBuildSettings){
            SceneManager.LoadScene(nextIndex);
        }
        else{
            Debug.Log("No more levels available");
        }
    }
}
