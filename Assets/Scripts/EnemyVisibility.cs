using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    public Transform[] playerTransforms; // Kieran & DASH
    public FieldOfView[] playerFOVs;     // their respective FOVs
    public float peripheralRadius = 1.5f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        bool visible = false;

        for (int i = 0; i < playerTransforms.Length; i++)
        {
            Transform player = playerTransforms[i];
            FieldOfView fov = playerFOVs[i];

            // 1. Check cone visibility
            if (IsInFOV(fov))
            {
                visible = true;
                break;
            }

            // 2. Check peripheral circular radius
            if (Vector2.Distance(transform.position, player.position) <= peripheralRadius)
            {
                visible = true;
                break;
            }
        }

        sr.enabled = visible;
    }

    bool IsInFOV(FieldOfView fov)
    {
        List<Vector3> verts = fov.GetWorldVertices();
        for (int i = 1; i < verts.Count - 1; i++)
        {
            if (PointInTriangle(transform.position, verts[0], verts[i], verts[i + 1]))
                return true;
        }
        return false;
    }

    bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 v0 = c - a;
        Vector3 v1 = b - a;
        Vector3 v2 = p - a;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;
        if (denom == 0) return false;

        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        return (u >= 0) && (v >= 0) && (w >= 0);
    }
}

