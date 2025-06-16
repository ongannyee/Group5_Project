using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Locker : MonoBehaviour
{
    public bool isOccupied = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyMovement guard = collision.GetComponent<EnemyMovement>();
        if (guard != null && guard.IsSleeping() && guard.isBeingDragged && !isOccupied)
        {
            // Store the guard
            isOccupied = true;
            guard.isBeingDragged = false;
            guard.draggedBy = null;
            Destroy(guard.gameObject); // Or use SetActive(false) if you want to reuse it later
            Debug.Log("Guard has been stored in locker.");
        }
    }
}

