using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WaveSpawner : MonoBehaviour
{
    public Wave[] waves;
    private Wave currentWave;

//In the first level this will be unecessary, but we may want to add more spawnpoints in further levels to increase dificulty
    [SerializeField]
    private Transform[] spawnpoints;
    [SerializeField]
    private WaypointSystem waypointSystem;

    [SerializeField]
    private float timeBetweenEnemies = 0.5f;

    private float timeBtwnSpawns;
    private int i = 0;

    private bool stopSpawning = false;
    private bool isSpawning = false;
    [SerializeField]private GameObject winPanelPrefab;

    private void Awake(){
        currentWave = waves[i];
        timeBtwnSpawns = currentWave.TimeBeforeThisWave;

        if(waypointSystem == null){
            waypointSystem = FindObjectOfType<WaypointSystem>();
            if(waypointSystem == null){
                Debug.LogError("WaypointSystem not found, the enemies will not move");
            }
        }
        if(winPanelPrefab != null){
            winPanelPrefab.SetActive(false);
        }
    }

    void Start() {
        timeBtwnSpawns = Time.time + currentWave.TimeBeforeThisWave;
    }

   void Update(){
        if(stopSpawning || isSpawning){
            WinPanel();
            return;
        }

        if(Time.time >= timeBtwnSpawns){
            if(currentWave.EnemiesInWave == null || currentWave.EnemiesInWave.Length == 0){
                Debug.LogError("No enemies in this wave");
                IncWave();
                if(!stopSpawning){
                    timeBtwnSpawns = Time.time + currentWave.TimeBeforeThisWave;
                }
                return;
            }
            StartCoroutine(SpawnWaveSequentially());
            IncWave();

            if(!stopSpawning){
                timeBtwnSpawns = Time.time + currentWave.TimeBeforeThisWave;
            }
        }
    }

    private IEnumerator SpawnWaveSequentially(){
        isSpawning = true;

        int numberToSpawn = Mathf.RoundToInt(currentWave.NumberToSpawn);
        for(int i = 0; i< numberToSpawn; i++){
            // this codde will select a random enemy from the list of enemies in the current wave
            int randomEnemy = Random.Range(0, currentWave.EnemiesInWave.Length);
            // when we add more spawnpointis this will select one of them randomly si the player will never know from where the enemies will come even if he replays the same level
            // right now this code doesn't do much
            int randomSpawner = Random.Range(0, spawnpoints.Length);

            // this code will instantiate the enemy selected in the selected spawnpoint
            GameObject enemy = Instantiate(currentWave.EnemiesInWave[randomEnemy], spawnpoints[randomSpawner].position, spawnpoints[randomSpawner].rotation);

            ConfigureEnemyMovement(enemy, randomSpawner);

            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        isSpawning = false;
    }

    private void ConfigureEnemyMovement(GameObject enemy, int randomSpawner){
        EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
        if(enemyMovement != null && waypointSystem != null){
           List<Transform> path = new List<Transform>();
            foreach(Transform waypoint in waypointSystem.GetWaypoints()){
                path.Add(waypoint);
            }
            enemyMovement.SetWaypoints(path);
        }

    }

    private void IncWave(){
        if(i + 1 < waves.Length){
            i++;
            currentWave = waves[i];
        }
        else{
            stopSpawning = true;
        }
    }

    private void WinPanel(){
        if(stopSpawning && GameObject.FindGameObjectsWithTag("Enemy").Length == 0){
            GameManager.Instance.WinGame();
            Debug.Log("You win");
        }
    }
}
