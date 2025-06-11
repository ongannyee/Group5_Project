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
    private float investigateTimer = 0f;

    private Transform target;
    public FieldOfView fov;

    // State of the enemy
    private enum State { Patrolling, Investigating, Chasing,  Alarmed}   // 4 states
    private State currentState = State.Patrolling;              
    private Vector3 investigateTarget;
    private bool alarmed = false;           // If this guard is alarmed
    private float alarmDelay = 3f;          // Time to pause before chasing after seeing a player
    private float alarmTimer = 0f;
    private Transform alarmedTarget;        // Target after alarm delay

    // UI being shown on enemy corresponding to the state of enemy
    public GameObject alertIndicator;
    //[SerializeField] private Transform spriteTransform;

    // Z3ra Hacking Abilities helper variables
    private bool commsDisabled = false;
    private float originalViewAngle;
    private float originalViewDistance;

    void Start()
    {
        //fov = GetComponentInChildren<FieldOfView>();
        target = null;

        if (fov != null)
        {
            //originalViewAngle = fov.viewAngle;
            originalViewDistance = fov.viewRadius;
        }
    }

    void Update()
    {
        // Show alert only if chasing
        alertIndicator.SetActive(currentState == State.Chasing || currentState == State.Alarmed);

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

            case State.Alarmed:
                Alarmed(); // wait before chasing
                break;

            case State.Chasing:
                Chase();
                break;
        }

        if (alertIndicator.activeSelf)
        {
            // Offset in local space, e.g. (x = right, y = up)
            Vector3 offset = new Vector3(1.5f, 2.5f, 0f);                       // alert indicator position
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
        investigateTimer += Time.deltaTime;

        if (Vector2.Distance(transform.position, investigateTarget) < 0.2f || investigateTimer >= 3f)
        {
            investigateTimer = 0f;
            currentState = State.Patrolling;
        }
    }

    void Chase()
    {
        if (target != null)
        {
            MoveTowards(target.position, chaseSpeed);

            if (!fov.visibleTargets.Contains(target))
            {
                if (!alarmed)
                {
                    currentState = State.Patrolling;
                    target = null;
                    Debug.Log(name + " lost target, returning to patrol.");
                }
                // else: stay in chase mode even without visibility
            }
        }
        else
        {
            if (!alarmed)
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
        fov.SetOrigin(transform.position);
        fov.SetAimDirection(dir);
    }

    //!!NOT WORKING YET. Checks if any player is in the field of view, prioritize Kieran
    void CheckForPlayer()
    {
        if (fov.visibleTargets.Count > 0)
        {
            Transform priorityTarget = PlayerAlert.GetVisiblePlayerToChase(fov.visibleTargets);

            // If not already chasing or alarmed
            if (!alarmed && currentState != State.Alarmed)
            {
                alarmTimer = 0f;
                alarmedTarget = priorityTarget;
                currentState = State.Alarmed;
                Debug.Log(name + " saw " + priorityTarget.name + ", starting alarm delay!");
            }
            else if (alarmed)
            {
                // Already alarmed, just update target
                target = priorityTarget;
                currentState = State.Chasing;
            }
        }
    }

    // Sees if any player cone is visible
    void CheckForPlayerCone()
    {
        Collider2D[] cones = Physics2D.OverlapCircleAll(transform.position, fov.viewRadius);

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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dir.magnitude, fov.obstacleMask);
        return hit.collider == null;
    }

    // Called by NoiseEmitter on Kieran or DASH
    public void HeardNoise(Vector3 noisePos)
    {
        if (currentState == State.Chasing || currentState == State.Alarmed || alarmed)  //ignore noise when chasing/alarmed
            return;

        investigateTarget = noisePos;
        investigateTimer = 0f; // Reset timer when moving to new noise
        currentState = State.Investigating;
        Debug.Log(name + " heard a noise and is investigating.");
    }

    void Alarmed()
    {
        alarmTimer += Time.deltaTime;

        if (alarmTimer >= alarmDelay && !commsDisabled)
        {
            EnemyMovement[] allGuards = FindObjectsOfType<EnemyMovement>();
            foreach (var guard in allGuards)
            {
                guard.SetTarget(alarmedTarget); 
            }
            Debug.Log(name + " is alarmed, all guards will will chase him permanently!"); 
        }
        else if (alarmTimer >= alarmDelay)
        {
            SetTarget(alarmedTarget);
            Debug.Log("JamComms break down the communication. The only guard who detects " + name + "will chase him permanently!"); 
        }
    }

    public void SetTarget(Transform t)
    {
        alarmed = true;
        currentState = State.Chasing;
        target = t;
    }

    // Z3ra skills
    public void DisableComms(float duration)
    {
        StartCoroutine(DisableCommsCoroutine(duration));
    }

    IEnumerator DisableCommsCoroutine(float duration)
    {
        commsDisabled = true;
        yield return new WaitForSeconds(duration);
        commsDisabled = false;
    }

/*     public bool AreCommsDisabled()
    {
        return commsDisabled;
    } */

    public void ReduceVisionTemporarily(float duration)
    {
        if (fov != null)
            StartCoroutine(ReduceVisionCoroutine(duration));
    }

    IEnumerator ReduceVisionCoroutine(float duration)
    {
        if (fov != null)
        {
            //fov.viewAngle *= 0.5f;
            fov.viewRadius *= 0.5f;
            Debug.Log("Enemy Vision is reduced!");
        }
        yield return new WaitForSeconds(duration);
        if (fov != null)
        {
            //fov.viewAngle = originalViewAngle;
            fov.viewRadius = originalViewDistance;
            Debug.Log("Enemy Vision is restored!");
        }
    }
}
