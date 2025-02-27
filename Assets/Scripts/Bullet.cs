using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 5f;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet hit: " + collision.gameObject.name + " (Tag: " + collision.gameObject.tag + ")");

        // Damage Player
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMove player = collision.gameObject.GetComponent<PlayerMove>();
            if (player != null)
            {
                Debug.Log("Damaging player for " + damage);
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        // Damage Enemy
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                Debug.Log("Damaging enemy " + collision.gameObject.name + " for " + damage);
                enemy.TakeDamage((int)damage); // Cast to int for EnemyAI
            }
            else
            {
                Debug.Log("No EnemyAI component found on " + collision.gameObject.name);
            }
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Bullet hit non-target: " + collision.gameObject.name);
            Destroy(gameObject);
        }
    }
}
