using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Tower : MonoBehaviour{
    [SerializeField] private float range = 3f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [SerializeField] private Vector3 rangeOffset = new Vector3(0.5f, 0f, 0f);

    private float fireCountdown = 0f;
    private Transform target;
    private List<GameObject> enemiesInRange = new List<GameObject>();

    [SerializeField] private bool showRange = true;

    private GameObject rangeCenter;
    private PlaceableObject placeableObject;

    private TowerSorting towerSorting;

    void Awake(){
        rangeCenter = new GameObject("RangeCenter");
        rangeCenter.transform.parent = transform;
        rangeCenter.transform.localPosition = rangeOffset;

        placeableObject = GetComponent<PlaceableObject>();
        towerSorting = GetComponent<TowerSorting>();

        if(towerSorting == null){
            towerSorting = gameObject.AddComponent<TowerSorting>();
        }
    }

    void Start(){
        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }

    void Update(){
        if(rangeCenter != null){
            rangeCenter.transform.localPosition = rangeOffset;
        }
        if(placeableObject == null || !placeableObject.Placed){
            return;
        }
        if(target == null){
            return;
        }
        if(fireCountdown <= 0f){
            Shoot();
            fireCountdown = 1f / fireRate;
        }
        fireCountdown -= Time.deltaTime;
    }

    void UpdateTarget(){
        if(placeableObject == null || !placeableObject.Placed){
            return;
        }
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDisatence = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach(GameObject enemy in enemies){
            float distanceToEnemy = Vector3.Distance(rangeCenter.transform.position, enemy.transform.position);
            if(distanceToEnemy < shortestDisatence && distanceToEnemy <= range){
                shortestDisatence = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }
        if(nearestEnemy != null){
            target = nearestEnemy.transform;
        }
        else{
            target = null;
        }
    }

    void Shoot(){
        if(projectilePrefab == null || firePoint == null){
            Debug.Log("Projectile or FirePoint is not assigned.");
            return;
        }

        GameObject projectileGO = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectile = projectileGO.GetComponent<Projectile>();

        if(projectile != null){
            projectile.Seek(target);
        }
        else{
            Debug.Log("Projectile script not found on the projectile prefab.");
        }
    }

    void OnDrawGizmosSelected(){
        if(showRange){
            Gizmos.color = Color.red;
            if (!Application.isPlaying && transform != null){
                Vector3 centerPosition = transform.position + rangeOffset;
                Gizmos.DrawWireSphere(centerPosition, range);
            }
            else if (rangeCenter != null){
                Gizmos.DrawWireSphere(rangeCenter.transform.position, range);
            }
        }
    }

    void OnDestroy(){
        if (rangeCenter != null){
            Destroy(rangeCenter);
        }
    }
}
