using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f; // Starting health of the target

    public void TakeDamage(float amount)
    {
        health -= amount; // Reduce health by the damage amount

        // Check if health has fallen to zero or below
        if (health <= 0f)
        {
            Die(); // Handle death
        }
    }

    void Die()
    {
        // Here you can include any effects, animations, or other logic you want to happen when the target dies
        Destroy(gameObject); // Destroy the target object
    }
}
