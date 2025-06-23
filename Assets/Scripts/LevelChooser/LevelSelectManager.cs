using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{

    public List<AreaData> Areas;
    public int CurrentAreaIndex = 0;
    public Button nextWorldButton;
    public Button previousWorldButton;
    public Transform LevelParent;
    public GameObject LevelButtonPrefab;
    public TextMeshProUGUI AreaHeaderText;
    public TextMeshProUGUI LevelHeaderText;
    public AreaData CurrentArea;

    public HashSet<string> UnlockedLevelsID = new HashSet<string>();

    private LevelSelectEventSystemHandler eventSystemHandler;
    private new Camera camera;

    private List<GameObject> levelButtons = new List<GameObject>();
    private Dictionary<GameObject, Vector3> buttonPositions = new Dictionary<GameObject, Vector3>();

    private void Awake()
    {
        camera = Camera.main;
        eventSystemHandler = GetComponentInChildren<LevelSelectEventSystemHandler>(true);
    }

    private void Start()
    {
        if (PlayerPrefs.GetInt("WorldUnlocked" + Areas[0].areaName, 0) == 0)
        {
            PlayerPrefs.SetInt("WorldUnlocked" + Areas[0].areaName, 1);
            PlayerPrefs.Save();
        }
        CurrentAreaIndex = Areas.FindIndex(a => a == CurrentArea);
        if (CurrentAreaIndex < 0) CurrentAreaIndex = 0;
        AssingAreaText();
        LoadUnlockedLevels();
        CreateLevelButtons();
    }

    public void AssingAreaText()
    {
        AreaHeaderText.SetText(CurrentArea.areaName);
    }

    private void LoadUnlockedLevels()
    {
        foreach (var level in CurrentArea.levels)
        {
            if (level.IsUnlockedByDefault || PlayerPrefs.GetInt(level.levelID, 0) == 1)
            {
                UnlockedLevelsID.Add(level.levelID);
            }
        }
    }

    private void CreateLevelButtons()
    {
        for (int i = 0; i < CurrentArea.levels.Count; i++)
        {
            GameObject buttonGO = Instantiate(LevelButtonPrefab, LevelParent);
            levelButtons.Add(buttonGO);

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();

            buttonGO.name = CurrentArea.levels[i].levelID;
            CurrentArea.levels[i].levelButtonObj = buttonGO;

            LevelButton levelButton = buttonGO.GetComponent<LevelButton>();
            levelButton.Setup(CurrentArea.levels[i], CurrentArea, UnlockedLevelsID.Contains(CurrentArea.levels[i].levelID));

            Selectable selectable = buttonGO.GetComponent<Selectable>();
            eventSystemHandler.AddSelectable(selectable);

            StartCoroutine(AddLocationAfterDelay(buttonGO, buttonRect));
        }
        StartCoroutine(SetupButtonNavigation());

        LevelParent.gameObject.SetActive(true);
        eventSystemHandler.InitSelectables();
        eventSystemHandler.SetFirstSelected();
    }

    private IEnumerator AddLocationAfterDelay(GameObject buttonGo, RectTransform buttonRect)
    {
        yield return null;
        Vector2 buttonScreenPoint = RectTransformUtility.WorldToScreenPoint(camera, buttonRect.position);
        Vector3 buttonWorldPos = camera.ScreenToWorldPoint(new Vector3(buttonScreenPoint.x, buttonScreenPoint.y, camera.nearClipPlane));
        buttonPositions.Add(buttonGo, buttonWorldPos);
    }

    private IEnumerator SetupButtonNavigation()
    {
        yield return null;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            GameObject currentButton = levelButtons[i];
            Vector3 currentPos = buttonPositions[currentButton];
            Selectable currentSelectable = currentButton.GetComponent<Selectable>();
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

            if (i > 0 && UnlockedLevelsID.Contains(CurrentArea.levels[i].levelID))
            {
                GameObject prevButton = levelButtons[i - 1];
                Vector3 prevPos = buttonPositions[prevButton];
                Vector3 dirToPrev = (prevPos - currentPos).normalized;

                if (Vector3.Dot(dirToPrev, Vector3.right) > 0.7f)
                    nav.selectOnRight = prevButton.GetComponent<Selectable>();
                else if (Vector3.Dot(dirToPrev, Vector3.left) > 0.7f)
                    nav.selectOnLeft = prevButton.GetComponent<Selectable>();
                else if (Vector3.Dot(dirToPrev, Vector3.up) > 0.7f)
                    nav.selectOnUp = prevButton.GetComponent<Selectable>();
                else if (Vector3.Dot(dirToPrev, Vector3.down) > 0.7f)
                    nav.selectOnDown = prevButton.GetComponent<Selectable>();
            }

            if (i < levelButtons.Count - 1 && UnlockedLevelsID.Contains(CurrentArea.levels[i + 1].levelID))
            {
                GameObject nextButton = levelButtons[i + 1];
                Vector3 nextPos = buttonPositions[nextButton];
                Vector3 dirToNext = (nextPos - currentPos).normalized;

                if (Vector3.Dot(dirToNext, Vector3.right) > 0.7f)
                    nav.selectOnRight = nextButton.GetComponent<Selectable>();
                else if (Vector3.Dot(dirToNext, Vector3.left) > 0.7f)
                    nav.selectOnLeft = nextButton.GetComponent<Selectable>();
                else if (Vector3.Dot(dirToNext, Vector3.up) > 0.7f)
                    nav.selectOnUp = nextButton.GetComponent<Selectable>();
                else if (Vector3.Dot(dirToNext, Vector3.down) > 0.7f)
                    nav.selectOnDown = nextButton.GetComponent<Selectable>();
            }
            currentSelectable.navigation = nav;
        }
    }

    #region Helper Methods
    public void UnlockLevel(string levelID, LevelButton levelButton)
    {
        UnlockedLevelsID.Add(levelID);
        levelButton.Unlock();
        PlayerPrefs.SetInt(levelID, 1);
        PlayerPrefs.Save();
        StartCoroutine(SetupButtonNavigation());
    }

    [ContextMenu("Test Level Unlock")]
    public void UnlockLevelTwoExample()
    {
        LevelButton levelButton = levelButtons[1].GetComponent<LevelButton>();
        string levelToUnlock = levelButton.LevelData.levelID;
        UnlockLevel(levelToUnlock, levelButton);
    }
    #endregion

    public void GoToNextWorld()
    {
        if (CurrentAreaIndex < Areas.Count - 1 && IsWorldUnlocked(Areas[CurrentAreaIndex + 1].areaName))
        {
            CurrentAreaIndex++;
            LoadCurrentArea();
            UpdateWorldNavigationButtons();
        }
    }

    public void GoToPreviousWorld()
    {
        if (CurrentAreaIndex > 0)
        {
            CurrentAreaIndex--;
            LoadCurrentArea();
            UpdateWorldNavigationButtons();
        }
    }

    private void LoadCurrentArea()
    {
        CurrentArea = Areas[CurrentAreaIndex];
        AssingAreaText();
        UnlockedLevelsID.Clear();
        LoadUnlockedLevels();
        CreateLevelButtons();
        FindFirstObjectByType<WorldBackgroundSwitcher>().SetWorldBackground(CurrentArea.areaName);
    }

    private void UpdateWorldNavigationButtons()
    {
        previousWorldButton.gameObject.SetActive(CurrentAreaIndex > 0);
        bool nextWorldUnlocked = false;
        if (CurrentAreaIndex < Areas.Count - 1)
        {
            nextWorldUnlocked = IsWorldUnlocked(Areas[CurrentAreaIndex + 1].areaName);
            nextWorldButton.gameObject.SetActive(nextWorldUnlocked);
        }
    }

    private bool IsWorldUnlocked(string worldName)
    {
        return PlayerPrefs.GetInt("WorldUnlocked" + worldName, 0) == 1;
    }
}
