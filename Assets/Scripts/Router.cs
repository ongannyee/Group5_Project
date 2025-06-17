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
        LayerMask enemyLayers = LayerMask.GetMask("Guard", "CCTV");

        // Store original sorting layers
        Dictionary<GameObject, string> originalLayers = new Dictionary<GameObject, string>();
        HashSet<GameObject> currentlyVisible = new HashSet<GameObject>();

        float timer = 0f;

        while (timer < duration)
        {
            // Find objects in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, routerRadius, enemyLayers);

            HashSet<GameObject> detectedThisFrame = new HashSet<GameObject>();

            foreach (var hit in hits)
            {
                GameObject obj = hit.gameObject;
                detectedThisFrame.Add(obj);

                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                if (!originalLayers.ContainsKey(obj))
                {
                    originalLayers[obj] = sr.sortingLayerName;
                }

                sr.sortingLayerName = "Enemy"; // Temporarily visible layer
                currentlyVisible.Add(obj);

                // Optional Debug
                if (obj.layer == LayerMask.NameToLayer("Guard"))
                    Debug.Log($"Pinged enemy: {obj.name}");
                else if (obj.layer == LayerMask.NameToLayer("CCTV"))
                    Debug.Log($"Pinged CCTV: {obj.name}");
            }

            // Check for objects that left the range
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var obj in currentlyVisible)
            {
                if (!detectedThisFrame.Contains(obj))
                {
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null && originalLayers.ContainsKey(obj))
                    {
                        sr.sortingLayerName = originalLayers[obj];
                    }
                    toRemove.Add(obj);
                }
            }

            // Clean up exited objects
            foreach (var obj in toRemove)
            {
                currentlyVisible.Remove(obj);
            }

            timer += 0.2f; // Scan interval
            yield return new WaitForSeconds(0.2f);
        }

        // Revert any remaining objects
        foreach (var obj in currentlyVisible)
        {
            if (obj != null && originalLayers.ContainsKey(obj))
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = originalLayers[obj];
                }
            }
        }
    }

}
