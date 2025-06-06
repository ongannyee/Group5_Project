using UnityEngine;

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

    public static Transform GetPlayerToChase()
    {
        if (Kieran != null && DASH != null)
        {
            // Example: prioritize Kieran if both are seen — change if needed
            return Vector3.Distance(Kieran.position, DASH.position) < 1f ? DASH : Kieran;
        }

        return Kieran ?? DASH;
    }
}
