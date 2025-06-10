using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Router : MonoBehaviour
{
    public bool isActivated = false;

    public GameObject visualOn;
    public GameObject visualOff;

    public float connectionRadius = 3f; //For DASH to connect
    public float routerRadius = 10f;    //For Z3ra Router Ping ability

    void Start()
    {
        UpdateVisual();
    }

    public void Activate()
    {
        isActivated = true;
        UpdateVisual();
        Debug.Log("Router Activated!");
        // Optionally notify Z3ra system here
    }

    private void UpdateVisual()
    {
        if (visualOn != null) visualOn.SetActive(isActivated);
        if (visualOff != null) visualOff.SetActive(!isActivated);
    }

    public bool IsInRange(Vector2 unitPosition)
    {
        return Vector2.Distance(transform.position, unitPosition) <= connectionRadius;
    }

    // Show connection rad in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, connectionRadius);
    }

    //Router Ping hacking ability
    public void RevealEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, routerRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out EnemyMovement enemy))
            {
                // Reveal logic or alert indicator
                Debug.Log($"Pinged enemy: {enemy.name}");
            }
            else if (hit.CompareTag("CCTV"))
            {
                Debug.Log($"Pinged CCTV: {hit.name}");
            }
        }
    }
}
