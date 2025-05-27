using UnityEngine;

[CreateAssetMenu(menuName = "Level Data/Levels", fileName = "New Level")]
public class LevelData : ScriptableObject
{
    [Header("Level Stats")]
    public string levelID;
    [Tooltip("For Starting Levels")] public bool IsUnlockedByDefault;
    public SceneField Scene;

    [Header("Level Display Information")]
    public string levelName;

    public GameObject levelButtonObj {get;set;}
}
