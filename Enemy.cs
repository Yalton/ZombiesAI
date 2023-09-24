using UnityEngine;

public class Enemy : Target
{
    public float speed = 5f; // Movement speed of the enemy
    public float attackRange = 1f; // Range within which the enemy can attack
    public float attackDamage = 10f; // Damage dealt by the enemy's attack
    public float attackCooldown = 1f; // Time in seconds between attacks

    private Transform playerTransform; // Reference to the player's transform
    private float timeSinceLastAttack; // Timer for tracking attack cooldown

    void Start()
    {
        // Find the player's transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Move towards the player if found
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;

            // Check if within attack range
            if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            {
                Attack();
            }
        }

        // Update attack cooldown timer
        timeSinceLastAttack += Time.deltaTime;
    }

    void Attack()
    {
        if (timeSinceLastAttack >= attackCooldown)
        {
            // Reset attack cooldown timer
            timeSinceLastAttack = 0f;

            // Apply damage to player (you'll need to implement the player's TakeDamage method)
            playerTransform.GetComponent<Player>().TakeDamage(attackDamage);
        }
    }
}
