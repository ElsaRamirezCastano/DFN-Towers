using UnityEngine;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;
    private bool waypointsAssigned=false;

    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;
    private SpriteRenderer spriteRenderer;

    private Vector3 lastPosition;
    private float directionChangeThreshold = 0.01f;


    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;

        if(!waypointsAssigned){
            WaypointSystem waypointSystem = FindFirstObjectByType<WaypointSystem>();
                if(waypointSystem != null){
                    foreach(Transform waypoint in waypointSystem.GetWaypoints()){
                        waypoints.Add(waypoint);
                    }
                }
                else{
                    Debug.LogError("WaypointSystem not found");
                }
        }
    }

    void Update(){

        if(waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count){
            return;
        }
        Transform currentWaypoint = waypoints[currentWaypointIndex];

        transform.position = Vector2.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

        if(Vector3.Distance(transform.position, lastPosition) > directionChangeThreshold){
            UpdateSpriteDirection(transform.position - lastPosition);
            lastPosition = transform.position;
        }

        if(Vector2.Distance(transform.position, currentWaypoint.position) < 0.2f){
            currentWaypointIndex++;

            if(currentWaypointIndex >= waypoints.Count){
                if(GameManager.Instance != null){
                    GameManager.Instance.EnemyReachedEnd();
                    Debug.Log("Enemy reached end");
                }
                Destroy(gameObject);
            }
        }
    }

    void UpdateSpriteDirection(Vector3 direction){
        if(spriteRenderer == null){
            return;
        }

        direction.Normalize();

            if(Mathf.Abs(direction.x) > Mathf.Abs(direction.y)){
                if(direction.x > 0){
                    if(rightSprite!= null) spriteRenderer.sprite = rightSprite;
                }
                else{
                    if(leftSprite!= null) spriteRenderer.sprite = leftSprite;
                }
            }
            else{
                if(direction.y > 0){
                    if(upSprite!= null) spriteRenderer.sprite = upSprite;
                }
                else{
                    if(downSprite!= null) spriteRenderer.sprite = downSprite;
                }
            }
    }

    public void SetWaypoints(List<Transform> newWaypoints){
        waypoints = new List<Transform>(newWaypoints);
        currentWaypointIndex = 0;
        waypointsAssigned = true;
    }
}
