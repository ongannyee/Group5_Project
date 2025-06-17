using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    // 1. Variables related to cone vision
    public float viewRadius;
    [Range(0, 360)] public float viewAngle = 90f;
    public int rayCount = 360;
    public LayerMask obstacleMask;  // to detect obstacles like walls
    public LayerMask targetMask;    // to detect player targets, for enemy only
    public bool canSeeThroughWalls = false;

    private Mesh mesh;      // for cone drawing
    private Vector3 origin; // set cone attach to obj
    private float angle;

    PolygonCollider2D polygonCollider;

    public List<Transform> visibleTargets = new List<Transform>(); // visible targets list

    // For player vision type while wearing guard uniform
    public GuardType ownerType = GuardType.None;

    void Start()
    {
        mesh = new Mesh();  // collection of vertices, edges, and triangles that define a 3D (or 2D) shape
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
        polygonCollider = GetComponent<PolygonCollider2D>();
    }

    void LateUpdate()
    {
        origin = transform.position;

        DrawFieldOfView();
        FindVisibleTargets(); // update visible targets
    }

    // Draw cone vision
    void DrawFieldOfView()
    {
        float angleStep = viewAngle / rayCount;     // this calculates each small triangle angle
        float angleStart = angle - viewAngle / 2f;  //finds the starting angle for the first ray in the cone.

        List<Vector3> viewPoints = new List<Vector3>();
        viewPoints.Add(Vector3.zero); // center point of mesh

        List<Vector2> colliderPoints = new List<Vector2>();
        colliderPoints.Add(Vector2.zero); // origin for collider (in local space)

        // find cone vertices
        for (int i = 0; i <= rayCount; i++)
        {
            float currentAngle = angleStart + angleStep * i;
            Vector3 dir = DirFromAngle(currentAngle, true); //Converts the angle into a direction vector

            Vector3 hitPoint;
            if (!canSeeThroughWalls)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, dir, viewRadius, obstacleMask);    //Stores the details of the object hit
                if (hit)
                {
                    hitPoint = hit.point;   // Ray hit a wall or object
                }
                else
                {
                    hitPoint = origin + dir * viewRadius;   // Ray hit nothing, goes full distance
                }
            }
            else
            {
                hitPoint = origin + dir * viewRadius;
            }

            Vector3 localPoint = transform.InverseTransformPoint(hitPoint); // Convert to local space
            viewPoints.Add(localPoint);                                     // Save local-space hit point
            colliderPoints.Add(new Vector2(localPoint.x, localPoint.y));
        }

        // draw the cone
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

        //clear previous vertices
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Update PolygonCollider2D
        if (polygonCollider != null)
        {
            polygonCollider.pathCount = 1;
            polygonCollider.SetPath(0, colliderPoints.ToArray());
        }
  
        if (transform.parent != null && transform.parent.CompareTag("Kieran"))
        {
            KieranController kieran = transform.parent.GetComponent<KieranController>();
            if (kieran != null && kieran.isDisguised)
            {
                Debug.Log("Update ownertype same as kieran");
                ownerType = kieran.disguisedAs;
            }
        }

        if (transform.parent != null && transform.parent.CompareTag("DASH"))
        {
            DASHController dash = transform.parent.GetComponent<DASHController>();
            if (dash != null && dash.isInCammo)
            {
                Debug.Log("Update DASH vision layer");
                int DASHVisionLayer = LayerMask.NameToLayer("Camo");
                gameObject.layer = DASHVisionLayer;
            }
            else
            {
                Debug.Log("Change DASH vision back");
                int DASHVisionLayer = LayerMask.NameToLayer("Vision");
                gameObject.layer = DASHVisionLayer;
            }
        }
    }

    // Find target
    public void FindVisibleTargets()
    {
        visibleTargets.Clear(); // clear old data
        Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(origin, viewRadius, targetMask);  //Get nearby targets in the area

        List<Transform> tempTargets = new List<Transform>();    // For prioritization

        foreach (var target in targetsInViewRadius)
        {
            Vector3 dirToTarget = (target.transform.position - origin).normalized;          // Calculate direction to target
            float angleToTarget = Vector3.Angle(DirFromAngle(angle, true), dirToTarget);    // Calculate angle to target

            if (angleToTarget < viewAngle / 2f) //turns a full angle like 90° into a cone from -45deg to +45deg.
            {
                float distToTarget = Vector3.Distance(origin, target.transform.position);

                if (canSeeThroughWalls || !Physics2D.Raycast(origin, dirToTarget, distToTarget, obstacleMask))
                {
                    tempTargets.Add(target.transform); // add target as visible obj
                }
            }
        }

        // !!NOT WORKING YET. Sort manually to ensure Kieran comes before DASH
        if (PlayerAlert.Kieran != null && tempTargets.Contains(PlayerAlert.Kieran))
            visibleTargets.Add(PlayerAlert.Kieran);

        if (PlayerAlert.DASH != null && tempTargets.Contains(PlayerAlert.DASH))
            visibleTargets.Add(PlayerAlert.DASH);
    }

    // Set the cone position 
    public void SetOrigin(Vector3 newOrigin)
    {
        origin = newOrigin;
        transform.position = newOrigin;
    }

    // Set cone direction
    public void SetAimDirection(Vector3 aimDir)
    {
        angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;    //Mathf.Atan2: Calculates the angle (in radians) between the positive X-axis and the aimDir vector.
                                                                    //Mathf.Rad2Deg: Converts the angle from radians to degrees (Unity works in degrees for most rotations).
    }

    // getting a direction vector from a given angle
    private Vector3 DirFromAngle(float angleDeg, bool global)
    {
        if (!global) angleDeg += transform.eulerAngles.z;   //Adjust angle for local rotation
        float rad = angleDeg * Mathf.Deg2Rad;               //Convert to radians
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)); //Return direction vector
    }

    // returns a list of the mesh's vertices converted into world space positions.
    // Use to generate fog-of-war effects in player controller
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
