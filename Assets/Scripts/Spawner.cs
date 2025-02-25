using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn;   // The object you want to spawn
    public float spawnDelay = 2f;      // Delay before spawning a new object after destruction
    private GameObject spawnedObject; // Track the spawned object
    private bool canSpawn = true;     // Flag to check if a new object can be spawned

    void Update()
    {
        // Check if the spawned object is destroyed and if a new one can be spawned
        if (spawnedObject == null && canSpawn)
        {
            // Start the spawning process after the delay
            Invoke("SpawnObject", spawnDelay);
            canSpawn = false; // Prevent multiple invocations
        }
    }

    void SpawnObject()
    {
        // Spawn the object at the spawner's position
        spawnedObject = Instantiate(objectToSpawn, transform.position, Quaternion.identity);

        // Reset the spawn flag to allow further spawning if needed
        canSpawn = true;
    }
}