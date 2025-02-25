using UnityEngine;

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

    void Start()
    {
        myCC = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        currentHealth = maxHealth;
        alreadyAttacked = false;
    }

    void Update()
    {
        GetInput();
        MovePlayer();
        RotatePlayer();
        FindNearestEnemy();
        AttackEnemy();
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

