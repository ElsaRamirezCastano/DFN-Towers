using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour{
   [SerializeField] private TextMeshProUGUI levelNameText;

   public LevelData LevelData {get; set;}
   public AreaData CurrentArea {get; set;}
   private Button button;
   private Image image;

   public Color ReturnColor {get; set;}

   private void Awake(){
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        ReturnColor = Color.grey;
   }

   public void Setup(LevelData level, AreaData area, bool isUnlocked){
        LevelData = level;
        CurrentArea = area;
        levelNameText.SetText(level.levelID);

        button.onClick.RemoveAllListeners();
        button.interactable = isUnlocked;

        if(isUnlocked){
            button.onClick.AddListener(LoadLevel);
            ReturnColor = Color.white;
            image.color = ReturnColor;
        }else{
            ReturnColor = Color.grey;
            image.color = ReturnColor;
        }
   }

   public void Unlock(){
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(LoadLevel);
        ReturnColor = Color.white;
        image.color = ReturnColor;
   }

   private void LoadLevel(){
          PlayerPrefs.SetString("CurrentArea", CurrentArea.areaName);
          PlayerPrefs.Save();
        SceneManager.LoadScene(LevelData.Scene);
   }
}
