using UnityEngine;

public class Pickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMove player = other.GetComponent<PlayerMove>();
            if (player != null)
            {
                player.CollectPickup();
                Destroy(gameObject); // Remove pickup after collection
            }
        }
    }
}