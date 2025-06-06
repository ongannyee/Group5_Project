// EnemyMovement.cs
using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    private int currentPointIndex = 0;

    private Transform target;
    private FieldOfView fieldOfView;

    private enum State { Patrolling, Investigating, Chasing }
    private State currentState = State.Patrolling;
    private Vector3 investigateTarget;

    public GameObject alertIndicator;
    [SerializeField] private Transform spriteTransform;

    void Start()
    {
        fieldOfView = GetComponentInChildren<FieldOfView>();
        target = null;
    }

    void Update()
    {
        // 🔴 Show alert only if chasing
        alertIndicator.SetActive(currentState == State.Chasing);

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                CheckForPlayerCone();
                CheckForPlayer(); // << Add this here to allow detection while patrolling
                break;

            case State.Investigating:
                Investigate();
                CheckForPlayer();
                break;

            case State.Chasing:
                Chase();
                break;
        }

        if (alertIndicator.activeSelf)
        {
            // Offset in local space, e.g. (x = right, y = up)
            Vector3 offset = new Vector3(1.0f, 2.5f, 0f); // tweak these values to position top-right
            alertIndicator.transform.position = transform.position + offset;
            alertIndicator.transform.rotation = Quaternion.identity; // Keeps the icon upright
        }

    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform point = patrolPoints[currentPointIndex];
        Debug.Log($"Moving to patrol point {currentPointIndex}: {point.name}");

        MoveTowards(point.position, patrolSpeed);

        if (Vector3.Distance(transform.position, point.position) <= 0.1f)
        {
            Debug.Log($"Reached patrol point {currentPointIndex}");
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }

    }

    void Investigate()
    {
        MoveTowards(investigateTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, investigateTarget) < 0.2f)
        {
            currentState = State.Patrolling;
        }
    }

    void Chase()
    {
        if (target != null)
        {
            MoveTowards(target.position, chaseSpeed);

            if (!fieldOfView.visibleTargets.Contains(target))
            {
                currentState = State.Patrolling;
                target = null;
            }
        }
        else
        {
            currentState = State.Patrolling;
        }
    }

    void MoveTowards(Vector3 position, float speed)
    {
        Vector3 dir = (position - transform.position).normalized;
        
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle); // This rotates the whole guard
        }

        transform.position += dir * speed * Time.deltaTime;

        // Update Field of View origin and aim
        fieldOfView.SetOrigin(transform.position);
        fieldOfView.SetAimDirection(dir);
    }




    void CheckForPlayer()
    {
        if (fieldOfView.visibleTargets.Count > 0)
        {
            target = fieldOfView.visibleTargets[0];
            currentState = State.Chasing;
        }
    }

    void CheckForPlayerCone()
    {
        Collider2D[] cones = Physics2D.OverlapCircleAll(transform.position, fieldOfView.viewRadius);

        foreach (var cone in cones)
        {
            if (cone.CompareTag("PlayerVision") && IsInLineOfSight(cone.transform))
            {
                investigateTarget = cone.transform.position;
                currentState = State.Investigating;
                break;
            }
        }
    }

    bool IsInLineOfSight(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dir.magnitude, fieldOfView.obstacleMask);
        return hit.collider == null;
    }
}
