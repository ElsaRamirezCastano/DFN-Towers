using UnityEngine;

public class Projectile : MonoBehaviour
{
   [SerializeField] private float speed = 10f;
   [SerializeField] private float damage = 2f;
   [SerializeField] private float explosionRadius = 0f;

   private Transform target;
   private bool canMove = false;

   void Start(){
    Invoke("CheckPlacement", 0.1f);
    Destroy(gameObject, 5f);
   }

   private void CheckPlacement(){
        PlaceableObject placeableObject = GetComponentInParent<PlaceableObject>();

        if(placeableObject != null){
            canMove = placeableObject.Placed;
        }
        else{
            canMove = true;
        }
   }

    public void Seek(Transform target){
        this.target = target;
    }
    void Update(){
        if(!canMove){
            return;
        }
        if(target == null){
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        if(dir.magnitude <= distanceThisFrame){
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget(){
        if(explosionRadius > 0f){
            Explode();
        }
        else{
            Damage(target);
        }
        Destroy(gameObject);
    }

    void Explode(){
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach(Collider col in colliders){
            if(col.CompareTag("Enemy")){
                Damage(col.transform);
            }
        }
    }

    void Damage(Transform enemy){
        EnemyHealth e = enemy.GetComponent<EnemyHealth>();
        if(e != null){
            e.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected(){
        if(explosionRadius > 0f){
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
