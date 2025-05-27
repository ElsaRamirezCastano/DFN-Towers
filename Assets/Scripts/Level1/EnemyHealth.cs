using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float health = 10f;
    [SerializeField] private int worth = 5;

    public void TakeDamage(float damage){
        health -= damage;
        if(health <= 0){
            Die();
        }
    }

    void Die(){
        Destroy(gameObject);
    }
}
