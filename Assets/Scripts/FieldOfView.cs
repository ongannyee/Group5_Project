using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    public float viewRadius = 8f;
    [Range(0, 360)] public float viewAngle = 90f;
    public int rayCount = 360;
    public LayerMask obstacleMask;
    public LayerMask targetMask; // NEW: to detect player targets
    public bool canSeeThroughWalls = false;

    private Mesh mesh;
    private Vector3 origin;
    private float angle;

    public List<Transform> visibleTargets = new List<Transform>(); // NEW: visible targets list

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
    }

    void LateUpdate()
    {
        origin = transform.position;

        DrawFieldOfView();
        FindVisibleTargets(); // NEW: update visible targets
    }

    void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;
        float angleStart = angle - viewAngle / 2f;

        List<Vector3> viewPoints = new List<Vector3>();
        viewPoints.Add(Vector3.zero); // center point of mesh

        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = angleStart + angleStep * i;
            Vector3 dir = DirFromAngle(currentAngle, true);

            Vector3 hitPoint;
            if (!canSeeThroughWalls)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, obstacleMask);
                if (hit)
                {
                    hitPoint = hit.point;
                }
                else
                {
                    hitPoint = origin + dir * viewRadius;
                }
            }
            else
            {
                hitPoint = origin + dir * viewRadius;
            }

            viewPoints.Add(transform.InverseTransformPoint(hitPoint));
        }

        int vertexCount = viewPoints.Count;
        Vector3[] vertices = viewPoints.ToArray();
        int[] triangles = new int[(vertexCount - 2) * 3];

        for (int i = 1; i < vertexCount - 1; i++)
        {
            int idx = (i - 1) * 3;
            triangles[idx] = 0;
            triangles[idx + 1] = i;
            triangles[idx + 2] = i + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(origin, viewRadius, targetMask);

        foreach (var target in targetsInViewRadius)
        {
            Vector3 dirToTarget = (target.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(DirFromAngle(angle, true), dirToTarget);

            if (angleToTarget < viewAngle / 2f)
            {
                float distToTarget = Vector3.Distance(origin, target.transform.position);

                if (canSeeThroughWalls || !Physics2D.Raycast(origin, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target.transform);
                }
            }
        }
    }


    public void SetOrigin(Vector3 newOrigin)
    {
        origin = newOrigin;
        transform.position = newOrigin;
    }

    public void SetAimDirection(Vector3 aimDir)
    {
        angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
    }

    private Vector3 DirFromAngle(float angleDeg, bool global)
    {
        if (!global) angleDeg += transform.eulerAngles.z;
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    public List<Vector3> GetWorldVertices()
    {
        List<Vector3> worldVertices = new List<Vector3>();
        Vector3[] vertices = mesh.vertices;

        foreach (Vector3 vertex in vertices)
        {
            worldVertices.Add(transform.TransformPoint(vertex));
        }

        return worldVertices;
    }
}
