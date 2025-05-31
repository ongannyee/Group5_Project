using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyVisibility : MonoBehaviour
{
    public FieldOfView[] fovs; // assign all character FOVs here

    private Renderer rend;

    private void Start()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        bool isVisible = false;

        foreach (var fov in fovs)
        {
            if (IsInVisionCone(fov, transform.position))
            {
                isVisible = true;
                break;
            }
        }

        rend.enabled = isVisible;
    }

    bool IsInVisionCone(FieldOfView fov, Vector3 worldPos)
    {
        if (fov == null || fov.GetComponent<MeshFilter>() == null) return false;

        Vector3 localPos = fov.transform.InverseTransformPoint(worldPos);
        Vector2 point = new Vector2(localPos.x, localPos.y);

        Mesh mesh = fov.GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 p0 = vertices[triangles[i]];
            Vector2 p1 = vertices[triangles[i + 1]];
            Vector2 p2 = vertices[triangles[i + 2]];

            if (PointInTriangle(point, p0, p1, p2))
                return true;
        }

        return false;
    }

    bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1f / (2f * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1f / (2f * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        return s >= 0 && t >= 0 && (s + t) <= 1;
    }
}
