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

    // Returns the visible player to chase, prioritizing Kieran
    public static Transform GetVisiblePlayerToChase(List<Transform> visibleTargets)
    {
        if (visibleTargets.Contains(Kieran))
            return Kieran;

        if (visibleTargets.Contains(DASH))
            return DASH;

        return null;
    }
}
