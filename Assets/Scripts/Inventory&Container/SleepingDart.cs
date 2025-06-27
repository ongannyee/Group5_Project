using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SleepingDart : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;

    private Vector2 direction;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;

        // Rotate dart to face the movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 135f);
        
        Destroy(gameObject, lifetime); // auto-destroy after 5s
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Guard"))
        {
            EnemyMovement guard = other.GetComponent<EnemyMovement>();
            if (guard != null)
            {
                guard.GoToSleep();
                Destroy(gameObject); // Destroy the dart
            }
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
           Destroy(gameObject); // Hit wall, destroy
        }
    }
}
