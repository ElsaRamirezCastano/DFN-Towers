using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMainMenu : MonoBehaviour
{
    public AreaList areaList;
    public void StartGame()
    {
        string lastLevelID = PlayerPrefs.GetString("LastUnlockedLevel", "");

        if (!string.IsNullOrEmpty(lastLevelID))
        {
            LevelData lastLevel = null;
            foreach (var area in areaList.allAreas)
            {
                lastLevel = area.levels.Find(lvl => lvl.levelID == lastLevelID);
                if (lastLevel != null) break;
            }
            if (lastLevel != null)
            {
                SceneManager.LoadScene(lastLevel.Scene);
                return;
            }
        }
        var firstLevel = areaList.allAreas[0].levels[0];
        SceneManager.LoadScene(firstLevel.Scene);
    }

    public void QuitGame(){
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void ChooseLevel(){
        SceneManager.LoadScene("LevelChooser");
    }
}
