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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, routerRadius);
    }

    //Router Ping hacking ability
    public void RevealEnemies(float duration)
    {
        StartCoroutine(RouterPingCoroutine(duration));
    }

    IEnumerator RouterPingCoroutine(float duration)
    {
        // Define which layers to check (Guards and CCTV)
        LayerMask enemyLayers = LayerMask.GetMask("Guard", "CCTV");
        
        // Only detect colliders on the specified layers
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, routerRadius, enemyLayers);
        
        // Store original layers to revert later
        Dictionary<GameObject, string> originalLayers = new Dictionary<GameObject, string>();
        
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            
            SpriteRenderer sr = hit.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            
            // Store original layer
            originalLayers[hit.gameObject] = sr.sortingLayerName;
            
            // Change to enemy layer
            sr.sortingLayerName = "Enemy";
            
            // Check which layer the hit object is on
            if (hit.gameObject.layer == LayerMask.NameToLayer("Guard"))
            {
                Debug.Log($"Pinged enemy: {hit.name}");
                // Additional logic for Guards
            }
            else if (hit.gameObject.layer == LayerMask.NameToLayer("CCTV"))
            {
                Debug.Log($"Pinged CCTV: {hit.name}");
                // Additional logic for CCTV
            }
        }
        
        yield return new WaitForSeconds(duration);
        
        // Revert all objects to their original layers
        foreach (var kvp in originalLayers)
        {
            if (kvp.Key != null)
            {
                SpriteRenderer sr = kvp.Key.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = kvp.Value;
                }
            }
        }
    }
}
