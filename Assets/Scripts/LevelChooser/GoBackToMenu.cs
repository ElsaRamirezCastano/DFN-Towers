using UnityEngine;
using UnityEngine.SceneManagement;

public class GoBackToMenu : MonoBehaviour{
   public void BackToMenu(){
        SceneManager.LoadScene(0);
        Debug.Log("Game Quit");
    }
}
