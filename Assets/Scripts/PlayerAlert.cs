using UnityEngine;
using System.Collections.Generic;

public class PlayerAlert : MonoBehaviour
{
    public static Transform Kieran;
    public static Transform DASH;

    void Awake()
    {
        if (Kieran == null)
            Kieran = GameObject.FindWithTag("Kieran")?.transform;

        if (DASH == null)
            DASH = GameObject.FindWithTag("DASH")?.transform;
    }

    // !!NOT WORKING YET. Returns the visible player to chase, prioritizing Kieran
    public static Transform GetVisiblePlayerToChase(List<Transform> visibleTargets)
    {
        if (Kieran != null && visibleTargets.Contains(Kieran))
            return Kieran;

        if (DASH != null && visibleTargets.Contains(DASH))
            return DASH;

        return null;
    }
}
