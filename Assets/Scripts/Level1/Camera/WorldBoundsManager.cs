using UnityEngine;

public class WorldBoundsManager : MonoBehaviour{
    [Header("World Bounds")]
    [SerializeField] private float minX = -50f;
    [SerializeField] private float minY = -50f;
    [SerializeField] private float maxX = 50f;
    [SerializeField] private float maxY = 50f;


    [Header("Debug")]
    [SerializeField] private bool showBounds = true;
    [SerializeField] private Color boundsColor = Color.blue;

    private void Awake(){
        Globals.InitializeWorldBounds(minX, minY, maxX, maxY);
    }

    private void OnDrawGizmos(){
        if(!showBounds) return;

        Gizmos.color = boundsColor;
        Gizmos.DrawLine(new Vector3(minX, minY, 0), new Vector3(maxX, minY, 0));
        Gizmos.DrawLine(new Vector3(maxX, minY, 0), new Vector3(maxX, maxY, 0));
        Gizmos.DrawLine(new Vector3(maxX, maxY, 0), new Vector3(minX, maxY, 0));
        Gizmos.DrawLine(new Vector3(minX, maxY, 0), new Vector3(minX, minY, 0));
    }
}
