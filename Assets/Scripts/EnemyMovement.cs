// EnemyMovement.cs
using System.Collections;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // Enemy patrolling, chasing initialization
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    private int currentPointIndex = 0;

    private Transform target;
    private FieldOfView fieldOfView;

    // State of the enemy
    private enum State { Patrolling, Investigating, Chasing }   // 3 states
    private State currentState = State.Patrolling;              
    private Vector3 investigateTarget;

    // UI being shown on enemy corresponding to the state of enemy
    public GameObject alertIndicator;
    [SerializeField] private Transform spriteTransform;

    void Start()
    {
        fieldOfView = GetComponentInChildren<FieldOfView>();
        target = null;
    }

    void Update()
    {
        // Show alert only if chasing
        alertIndicator.SetActive(currentState == State.Chasing);

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                CheckForPlayerCone();
                CheckForPlayer(); // Add this here to allow detection while patrolling
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
            Vector3 offset = new Vector3(1.0f, 2.5f, 0f);                       // alert indicator position
            alertIndicator.transform.position = transform.position + offset;
            alertIndicator.transform.rotation = Quaternion.identity;            // Keeps the icon upright
        }

    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform point = patrolPoints[currentPointIndex];                      // Predefined patrolling point

        MoveTowards(point.position, patrolSpeed);                               // Move the the patrol point with patrol speed

        if (Vector3.Distance(transform.position, point.position) <= 0.1f)       // Move to next patrol point
        {
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
        
        // Rotate to face the target
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle); 
        }

        transform.position += dir * speed * Time.deltaTime;

        // Update Field of View origin and aim
        fieldOfView.SetOrigin(transform.position);
        fieldOfView.SetAimDirection(dir);
    }

    //Checks if any player is in the field of view, prioritize Kieran
    void CheckForPlayer()
    {
        if (fieldOfView.visibleTargets.Count > 0)
        {
            Transform highestPriorityTarget = fieldOfView.visibleTargets[0];

            // Always update target if it's different (i.e. switch to Kieran if seen)
            if (target != highestPriorityTarget)
            {
                target = highestPriorityTarget;
                currentState = State.Chasing;
                Debug.Log("Switched target to: " + target.name);
            }
            else if (currentState != State.Chasing)
            {
                currentState = State.Chasing;
                Debug.Log("Started chasing: " + target.name);
            }
        }
        else
        {
            target = null;
            if (currentState == State.Chasing)
            {
                currentState = State.Patrolling;
                Debug.Log("Lost sight of target. Returning to patrol.");
            }
        }
    }

    //Sees if any player cone is visible
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

    // Checks if a target is blocked by walls
    bool IsInLineOfSight(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dir.magnitude, fieldOfView.obstacleMask);
        return hit.collider == null;
    }
}
