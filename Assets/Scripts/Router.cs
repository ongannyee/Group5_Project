using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Router : MonoBehaviour
{
    public bool isActivated = false;

    public GameObject visualOn;
    public GameObject visualOff;
    public GameObject dash;
    private Vector2 dashPosition;

    public float connectionRadius = 3f; //For DASH to connect
    public float routerRadius ;    //For Z3ra Router Ping ability
    private bool hasShownPrompt = false;


    void Start()
    {
        UpdateVisual();

    }

    void Update()
    {
        dash = GameObject.Find("DASH");
        
        if (dash != null)
        {
            dashPosition = dash.transform.position;
        }
        else
        {
            Debug.LogError("DASH GameObject not found in scene!");
        }

        if (dash == null) return; // Safety check
        
        // Update dash position in case it moves
        dashPosition = dash.transform.position;
        
        if (IsInRange(dashPosition))
        {
            if (!hasShownPrompt)
            {
                hasShownPrompt = true;
                InspectPromptManager.Instance.ShowPromptRouter();
            }
        }
        else
        {
            if (hasShownPrompt)
            {
                hasShownPrompt = false;
                InspectPromptManager.Instance.HidePromptRouter();
            }
        }
   
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

        Dictionary<GameObject, string> originalLayers = new();
        HashSet<GameObject> currentlyVisible = new();

        float timer = 0f;

        while (timer < duration)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, routerRadius, enemyLayers);
            HashSet<GameObject> detectedThisFrame = new();

            foreach (var hit in hits)
            {
                GameObject obj = hit.gameObject;
                detectedThisFrame.Add(obj);

                if (!originalLayers.ContainsKey(obj))
                {
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        originalLayers[obj] = sr.sortingLayerName;
                        sr.sortingLayerName = "Enemy"; // Make temporarily visible
                        currentlyVisible.Add(obj);

                        if (obj.layer == LayerMask.NameToLayer("Guard"))
                            Debug.Log($"Pinged enemy: {obj.name}");
                        else if (obj.layer == LayerMask.NameToLayer("CCTV"))
                            Debug.Log($"Pinged CCTV: {obj.name}");
                    }
                }
            }

            // Revert objects that exited range
            List<GameObject> toRemove = new();
            foreach (var obj in currentlyVisible)
            {
                if (!detectedThisFrame.Contains(obj))
                {
                    if (obj != null && originalLayers.ContainsKey(obj))
                    {
                        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.sortingLayerName = originalLayers[obj];
                    }
                    toRemove.Add(obj);
                }
            }

            foreach (var obj in toRemove)
                currentlyVisible.Remove(obj);

            timer += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        // Final cleanup
        foreach (var obj in currentlyVisible)
        {
            if (obj != null && originalLayers.ContainsKey(obj))
            {
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.sortingLayerName = originalLayers[obj];
            }
        }
    }


}
