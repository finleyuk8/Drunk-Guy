using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public float playerSpeed = 20f; // Speed of the player
    public float rotationSpeed = 10f; // Speed of the player rotation
    private CharacterController myCC; // Reference to the CharacterController component
    private Vector3 inputVector; // Vector to store the input values
    private Vector3 movementVector; // Vector to store the movement values
    private float myGravity = -10f; // Gravity value

    public Transform cameraTransform; // Reference to the camera's transform
    public float maxHealth = 100f; // Maximum health of the player
    private float currentHealth; // Current health of the player

    public float attackRange = 10f; // Range of the player's attack
    public GameObject projectile; // Reference to the projectile prefab
    public Transform gunTip; // Reference to the gun tip transform  
    public float timeBetweenAttacks = 1f; // Time between attacks
    private bool alreadyAttacked; // Boolean to check if the player has already attacked
    private Transform targetEnemy; // Reference to the target enemy

    [SerializeField] private GameObject healthBar; // Reference to the health bar GameObject
    private Slider healthSlider; // Reference to the health bar Slider component

    // Pickup and double damage variables
    private int pickupCount = 0; // Number of pickups collected
    private bool isDoubleDamageActive = false; // Boolean to check if double damage is active
    private float doubleDamageDuration = 20f; // Duration of double damage
    private float doubleDamageTimer = 0f; 
    private float baseDamage = 10f; // Base damage of the player

    [SerializeField] private GameObject pickupBar; // Reference to the pickup bar GameObject
    private Slider pickupSlider; // Reference to the pickup bar Slider component
    private int maxPickups = 5; // Maximum number of pickups to collect

    void Start()
    {
        myCC = GetComponent<CharacterController>(); // Get the CharacterController component
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        currentHealth = maxHealth; // Set current health to max health at the start
        alreadyAttacked = false; 

        if (healthBar != null) // Check if healthBar reference is set in the Inspector
        {
            healthSlider = healthBar.GetComponent<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
        }

        if (pickupBar != null)
        {
            pickupSlider = pickupBar.GetComponent<Slider>();
            if (pickupSlider != null)
            {
                pickupSlider.maxValue = maxPickups;
                pickupSlider.value = pickupCount;
            }
   
        }
        
    }

    void Update()
    {
        GetInput();
        MovePlayer();
        RotatePlayer();
        FindNearestEnemy();
        AttackEnemy();

        if (isDoubleDamageActive)
        {
            doubleDamageTimer -= Time.deltaTime;
            if (doubleDamageTimer <= 0)
            {
                EndDoubleDamage();
            }
        }
    }

    void GetInput()
    {
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized; // Get the input values

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward; 
            Vector3 right = cameraTransform.right;
            forward.y = 0f; // Set the y value to 0 to prevent the player from moving up
            right.y = 0f; 
            forward.Normalize(); // Normalize vectors to get the direction
            right.Normalize(); 
            movementVector = (forward * inputVector.z + right * inputVector.x) * playerSpeed; // Calculate the movement vector
        }
        movementVector += Vector3.up * myGravity; // Add gravity to the movement vector
    }

    void MovePlayer()
    {
        myCC.Move(movementVector * Time.deltaTime); // Move the player
    }

    void RotatePlayer()
    {
        if (inputVector != Vector3.zero) 
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movementVector.x, 0f, movementVector.z)); // Calculate the target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime); // Rotate the player
        }
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestDistance = Mathf.Infinity;
        targetEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance && distance <= attackRange)
            {
                closestDistance = distance;
                targetEnemy = enemy.transform;
            }
        }
    }

    void AttackEnemy()
    {
        if (targetEnemy != null && !alreadyAttacked) // Check if there is a target enemy and the player has not already attacked
        {
            transform.LookAt(targetEnemy); // Makes the player look at the target enemy
            Vector3 directionToEnemy = (targetEnemy.position - gunTip.position).normalized;
            Vector3 spawnPosition = gunTip.position + directionToEnemy * 0.5f; // Spawn the projectile slightly in front of the gun tip
            GameObject bullet = Instantiate(projectile, spawnPosition, Quaternion.LookRotation(directionToEnemy));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            float bulletSpeed = 32f; // Speed of the bullet
            rb.linearVelocity = directionToEnemy * bulletSpeed;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = isDoubleDamageActive ? baseDamage * 2 : baseDamage; // Double damage if active
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. Current health: {currentHealth}");

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        gameObject.SetActive(false);
    }

    public void CollectPickup()
    {
        if (!isDoubleDamageActive)
        {
            pickupCount++;
            Debug.Log($"Collected pickup! Total: {pickupCount}");

            if (pickupSlider != null)
            {
                pickupSlider.value = pickupCount;
            }

            if (pickupCount >= maxPickups)
            {
                StartDoubleDamage();
            }
        }
    }

    private void StartDoubleDamage()
    {
        isDoubleDamageActive = true;
        doubleDamageTimer = doubleDamageDuration;
        pickupCount = 0;
        Debug.Log("Double damage activated for 20 seconds!");
    }

    private void EndDoubleDamage()
    {
        isDoubleDamageActive = false;
        pickupCount = 0;
        if (pickupSlider != null)
        {
            pickupSlider.value = pickupCount;
        }
        Debug.Log("Double damage ended!");
    }

    // New method for healing
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth; // Cap health at maxHealth
        }
        Debug.Log($"Player healed for {amount}. Current health: {currentHealth}");

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth; // Update health slider
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}