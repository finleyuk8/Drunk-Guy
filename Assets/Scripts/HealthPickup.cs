using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    private float healAmount = 10f; // Heals player for 10 health

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove player = other.GetComponent<PlayerMove>();
            if (player != null)
            {
                player.Heal(healAmount);
                Destroy(gameObject); // Remove pickup after collection
            }
        }
    }
}