using UnityEngine;
using System.Collections.Generic;

public class DynamicEnemyMovement : MonoBehaviour{
    public float moveSpeed = 2f;
    private List<Transform> currentPath = new List<Transform>();
    private int currentWaypointIndex = 0;
    private bool needsPathRecalculation = false;

    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private SpriteRenderer spriteRenderer;

    private Vector3 lastPosition;
    private float directionChangeThreshold = 0.01f;

    private bool waitingForPath = false;

    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;

        if (DynamicPathSystem.Instance != null){
            DynamicPathSystem.Instance.OnPathChanged += OnPathSystemChanged;
            CalculatePath();
        }
        else{
            Debug.Log("DynamicPathSystem not found");
        }
    }

    void OnDestroy(){
        if (DynamicPathSystem.Instance != null){
            DynamicPathSystem.Instance.OnPathChanged -= OnPathSystemChanged;
        }
    }

    void OnPathSystemChanged(){
        needsPathRecalculation = true;
    }

    void Update(){
        if (needsPathRecalculation){
            CalculatePath();
            needsPathRecalculation = false;
        }

        if (waitingForPath) return;

        if (currentPath.Count == 0 || currentWaypointIndex >= currentPath.Count) return;

        Transform currentWaypoint = currentPath[currentWaypointIndex];

        transform.position = Vector2.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, lastPosition) > directionChangeThreshold){
            UpdateSpriteDirection(transform.position - lastPosition);
            lastPosition = transform.position;
        }

        if (Vector2.Distance(transform.position, currentWaypoint.position) < 0.2f){
            currentWaypointIndex++;

            if (currentWaypointIndex >= currentPath.Count){
                if (GameManager.Instance != null){
                    GameManager.Instance.EnemyReachedEnd();
                    Debug.Log("Enemy reached end");
                }
                Destroy(gameObject);
            }
        }
    }

    void CalculatePath(){
        if (DynamicPathSystem.Instance != null){
            List<Transform> newPath = DynamicPathSystem.Instance.FindPath();

            if (newPath.Count > 1){
                currentPath = newPath;
                waitingForPath = false;

                float minDistance = float.MaxValue;
                int closestIndex = 0;

                for (int i = 0; i < currentPath.Count; i++){
                    float distance = Vector2.Distance(transform.position, currentPath[i].position);

                    if (distance < minDistance){
                        minDistance = distance;
                        closestIndex = i;
                    }
                }
                currentWaypointIndex = closestIndex;
                Debug.Log($"New path calculated with {currentPath.Count} waypoints");
            }
            else{
                waitingForPath = true;
                currentPath.Clear();
                Debug.Log("No valid path found! Enemy will be destroyed");
            }
        }
    }

    void UpdateSpriteDirection(Vector3 direction){
        if (spriteRenderer == null) return;

        direction.Normalize();

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)){
            if (direction.x > 0){
                if (rightSprite != null) spriteRenderer.sprite = rightSprite;
            }
            else{
                if (leftSprite != null) spriteRenderer.sprite = leftSprite;
            }
        }
        else{
            if (direction.y > 0){
                if (upSprite != null) spriteRenderer.sprite = upSprite;
            }
            else{
                if(downSprite != null)spriteRenderer.sprite = downSprite;
            }
        }
    }
}
