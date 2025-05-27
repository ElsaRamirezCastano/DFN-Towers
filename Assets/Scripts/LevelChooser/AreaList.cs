using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "LevelData/Area List", fileName = "AreaList")]
public class AreaList : ScriptableObject
{
    public List<AreaData> allAreas;
}
