using UnityEngine;
using UnityEngine.UI;

public class PlayerMove : MonoBehaviour
{
    public float playerSpeed = 20f;
    public float rotationSpeed = 10f;
    private CharacterController myCC;
    private Vector3 inputVector;
    private Vector3 movementVector;
    private float myGravity = -10f;

    public Transform cameraTransform;
    public float maxHealth = 100f;
    private float currentHealth;

    public float attackRange = 10f;
    public GameObject projectile;
    public Transform gunTip;
    public float timeBetweenAttacks = 1f;
    private bool alreadyAttacked;
    private Transform targetEnemy;

    [SerializeField] private GameObject healthBar;
    private Slider healthSlider;

    // Pickup and double damage variables
    private int pickupCount = 0;
    private bool isDoubleDamageActive = false;
    private float doubleDamageDuration = 20f;
    private float doubleDamageTimer = 0f;
    private float baseDamage = 10f;

    [SerializeField] private GameObject pickupBar;
    private Slider pickupSlider;
    private int maxPickups = 5;

    void Start()
    {
        myCC = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        currentHealth = maxHealth;
        alreadyAttacked = false;

        if (healthBar != null)
        {
            healthSlider = healthBar.GetComponent<Slider>();
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
            else
            {
                Debug.LogError("No Slider component found on the healthBar GameObject!");
            }
        }
        else
        {
            Debug.LogError("HealthBar reference is not set in the Inspector!");
        }

        if (pickupBar != null)
        {
            pickupSlider = pickupBar.GetComponent<Slider>();
            if (pickupSlider != null)
            {
                pickupSlider.maxValue = maxPickups;
                pickupSlider.value = pickupCount;
            }
            else
            {
                Debug.LogError("No Slider component found on the pickupBar GameObject!");
            }
        }
        else
        {
            Debug.LogError("PickupBar reference is not set in the Inspector!");
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
        inputVector = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            movementVector = (forward * inputVector.z + right * inputVector.x) * playerSpeed;
        }
        movementVector += Vector3.up * myGravity;
    }

    void MovePlayer()
    {
        myCC.Move(movementVector * Time.deltaTime);
    }

    void RotatePlayer()
    {
        if (inputVector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(movementVector.x, 0f, movementVector.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
        if (targetEnemy != null && !alreadyAttacked)
        {
            transform.LookAt(targetEnemy);
            Vector3 directionToEnemy = (targetEnemy.position - gunTip.position).normalized;
            Vector3 spawnPosition = gunTip.position + directionToEnemy * 0.5f;
            GameObject bullet = Instantiate(projectile, spawnPosition, Quaternion.LookRotation(directionToEnemy));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            float bulletSpeed = 32f;
            rb.linearVelocity = directionToEnemy * bulletSpeed;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.damage = isDoubleDamageActive ? baseDamage * 2 : baseDamage;
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