// EnemyMovement.cs
using System.Collections;
using UnityEngine;

public enum GuardType
{
    None,
    TypeA,
    TypeB,
    TypeC,
    TypeD
    // Extendable
}

public class EnemyMovement : MonoBehaviour
{
    public Animator animator;
    // Enemy patrolling, chasing initialization
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    private int currentPointIndex = 0;
    private float investigateTimer = 0f;
    private float maxInvestigateTimer;
    private bool isChecked = false;

    private Transform target;
    public FieldOfView fov;

    // State of the enemy
    public enum State { Patrolling, Investigating, Chasing,  Alarmed, Sleep}   // 5 states
    public State currentState = State.Patrolling;              
    private Vector3 investigateTarget;
    private bool alarmed = false;           // If this guard is alarmed
    private float alarmDelay = 3f;          // Time to pause before chasing after seeing a player
    private float alarmTimer = 0f;
    private Transform alarmedTarget;        // Target after alarm delay
    private bool isSleeping = false;
    public Vector2 movement;
    private SpriteRenderer sr;
    public float captureRadius = 0.5f;

    // UI being shown on enemy corresponding to the state of enemy
    public GameObject alertIndicator;
    //[SerializeField] private Transform spriteTransform;

    // Z3ra Hacking Abilities helper variables
    private bool commsDisabled = false;
    private float originalViewAngle;
    private float originalViewDistance;

    // Actions on Sleeping Guard
    [HideInInspector] public bool isBeingDragged = false;
    [HideInInspector] public Transform draggedBy = null;
    public GuardType guardType = GuardType.TypeA;

    public AudioClip walkingClip;
    private AudioSource audioSource;
    private bool wasMoving = false;

    public AudioClip communicationClip;
    private AudioSource commAudioSource;
    public float commMinInterval = 8f;
    public float commMaxInterval = 20f;
    public float commHearRadius = 20f;
    private float commTimer = 0f;

    void Start()
    {
        //fov = GetComponentInChildren<FieldOfView>();
        target = null;

        if (fov != null)
        {
            //originalViewAngle = fov.viewAngle;
            originalViewDistance = fov.viewRadius;
        }
        sr = GetComponent<SpriteRenderer>();

        // Walking sound setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Communication sound setup
        commAudioSource = gameObject.AddComponent<AudioSource>();
        commAudioSource.loop = false;
        commAudioSource.playOnAwake = false;
        ResetCommTimer();
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
                CheckForPlayer(); 
                InvestigateSleepingGuard();
                break;

            case State.Investigating:
                Investigate();
                CheckForPlayer();
                break;

            case State.Alarmed:
                animator.SetBool("isInvestigate", true);
                Alarmed(); // wait before chasing
                break;

            case State.Chasing:
                animator.SetBool("isInvestigate", false);
                Chase();
                break;

            case State.Sleep:
                animator.SetTrigger("sleep");
                Sleep();
                break;
        }

        //sleeping guard setup
        if (currentState == State.Sleep && isBeingDragged && draggedBy != null)
        {
            transform.position = Vector3.Lerp(transform.position, draggedBy.position + new Vector3(-0.5f, -0.5f, 0f), 10f * Time.deltaTime);
        }

        // Setup alert indicator
        if (alertIndicator.activeSelf)
        {
            // Offset in local space, e.g. (x = right, y = up)
            Vector3 offset = new Vector3(1.5f, 2.5f, 0f);                       // alert indicator position
            alertIndicator.transform.position = transform.position + offset;
            alertIndicator.transform.rotation = Quaternion.identity;            // Keeps the icon upright
        }

        // Capture DASH or Kieran consider lose
        if (currentState == State.Chasing || currentState == State.Alarmed)
        {
            CheckCapture();
        }

        // Walking sound logic
        bool isMoving = false;
        float pitch = 1.0f;
        switch (currentState)
        {
            case State.Patrolling:
                isMoving = movement.sqrMagnitude > 0.01f;
                pitch = 1.0f; // normal
                break;
            case State.Investigating:
                isMoving = movement.sqrMagnitude > 0.01f;
                pitch = 1.1f; // slightly faster
                break;
            case State.Chasing:
            case State.Alarmed:
                isMoving = movement.sqrMagnitude > 0.01f;
                pitch = 1.25f; // fastest
                break;
            default:
                isMoving = false;
                break;
        }
        if (isMoving && !wasMoving)
        {
            if (walkingClip != null)
            {
                audioSource.clip = walkingClip;
                audioSource.pitch = pitch;
                audioSource.Play();
            }
        }
        else if (!isMoving && wasMoving)
        {
            audioSource.Stop();
        }
        // If already moving, update pitch if state changed
        if (isMoving && audioSource.isPlaying)
        {
            audioSource.pitch = pitch;
        }
        wasMoving = isMoving;

        // Guard communication sound logic
        commTimer -= Time.deltaTime;
        if (commTimer <= 0f)
        {
            Transform kieran = PlayerAlert.Kieran;
            if (kieran != null && Vector2.Distance(transform.position, kieran.position) <= commHearRadius)
            {
                if (communicationClip != null)
                {
                    commAudioSource.PlayOneShot(communicationClip);
                }
            }
            ResetCommTimer();
        }
    }

    private void ResetCommTimer()
    {
        commTimer = Random.Range(commMinInterval, commMaxInterval);
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
        if (!isSleeping)
        {
            float distance = Vector2.Distance(transform.position, investigateTarget);

            // If close enough, stop and rotate left/right
            if (distance < 0.2f)
            {
                movement = Vector2.zero;

                // Rotate left and right like inspecting
                float angle = Mathf.PingPong(Time.time * 60f, 90f) - 45f; // -45 to 45 degrees
                transform.rotation = Quaternion.Euler(0f, 0f, angle-90f);

                // Update FOV direction based on angle
                float radians = angle * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
                fov.SetAimDirection(direction);
            }
            else
            {
                // Move toward sleeping guard
                MoveTowards(investigateTarget, patrolSpeed);
            }

            investigateTimer += Time.deltaTime;

            if (investigateTimer >= maxInvestigateTimer)
            {
                investigateTimer = 0f;
                currentState = State.Patrolling;
            }
        }
    }

    void Chase()
    {
        if (target != null && !isSleeping)
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

    void Sleep()
    {
        movement = Vector2.zero;
        fov.viewRadius = 0;
        FieldOfView fieldofview = GetComponent<FieldOfView>();
        if (fieldofview != null) 
        {
            fieldofview.enabled = false;
        }
        int guardSleepingLayer = LayerMask.NameToLayer("Guard-Sleeping");
        gameObject.layer = guardSleepingLayer;

        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        bool kieranFound = false;

        foreach (var collider in nearby)
        {
            KieranController kieran = collider.GetComponent<KieranController>();
            if (kieran != null)
            {
                kieranFound = true;
                break; // Exit loop once we find Kieran
            }
        }

        if (kieranFound)
        {
            InspectPromptManager.Instance.ShowPromptGuard();
        }
        else
        {
            InspectPromptManager.Instance.HidePromptGuard();
        }
    }

    void MoveTowards(Vector3 position, float speed)
    {
        Vector3 dir = (position - transform.position).normalized;
        
        // Rotate to face the target
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90); 
        }

        transform.position += dir * speed * Time.deltaTime;

        // Update Field of View origin and aim
        fov.SetOrigin(transform.position);
        fov.SetAimDirection(dir);
    }

    //Checks if any player is in the field of view, prioritize Kieran
    void CheckForPlayer()
    {
        foreach (Transform t in fov.visibleTargets)
        {
            KieranController kieran = t.GetComponent<KieranController>();
            if (kieran != null)
            {
                float dist = Vector2.Distance(transform.position, kieran.transform.position);
                bool overlapping = dist < 0.5f;

                if (kieran.isDisguised && kieran.disguisedAs == this.guardType && !overlapping)
                {
                    // Kieran is disguised and not overlapping - skip detection
                    continue;
                }

                // Not disguised or overlapping - trigger alarm
                if (!alarmed && currentState != State.Alarmed)
                {
                    alarmTimer = 0f;
                    alarmedTarget = kieran.transform;
                    currentState = State.Alarmed;
                    Debug.Log(name + " saw disguised or undisguised Kieran.");
                }
                else if (alarmed)
                {
                    target = kieran.transform;
                    currentState = State.Chasing;
                }
                return;
            }

            // Detect DASH normally
            if (t.name.Contains("DASH"))
            {
                if (!alarmed)
                {
                    alarmTimer = 0f;
                    alarmedTarget = t;
                    currentState = State.Alarmed;
                }
                else
                {
                    target = t;
                    currentState = State.Chasing;
                }
                return;
            }
        }
    }

    // Sees if any player cone is visible
    void CheckForPlayerCone()
    {
        Collider2D[] cones = Physics2D.OverlapCircleAll(transform.position, fov.viewRadius);
        int visionLayer = LayerMask.NameToLayer("Vision");
        foreach (var cone in cones)
        {
            if (cone.gameObject.layer == visionLayer && IsInLineOfSight(cone.transform))
            {
                FieldOfView vision = cone.GetComponent<FieldOfView>();
                if (vision != null && vision.ownerType != guardType) // Ignore same type
                {
                    investigateTarget = cone.transform.position;
                    maxInvestigateTimer = 3f;
                    currentState = State.Investigating;
                    break;
                }
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

        // Play alarm sound globally
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.TriggerAlarm();
        }

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
        if(currentState != State.Sleep)
        {
            alarmed = true;
            currentState = State.Chasing;
            target = t;
        }
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

    public void InvestigateSleepingGuard()
    {
        if (currentState == State.Sleep) return;

        Collider2D[] sleepingGuards = Physics2D.OverlapCircleAll(transform.position, fov.viewRadius);

        foreach (var sleepingGuardCollider in sleepingGuards)
        {
            EnemyMovement guardScript = sleepingGuardCollider.GetComponent<EnemyMovement>();

            if (guardScript != null && guardScript.IsSleeping() && IsInLineOfSight(guardScript.transform))
            {
                if (!guardScript.isChecked)
                {
                    currentState = State.Investigating;
                    investigateTarget = guardScript.transform.position;
                    maxInvestigateTimer = 10f;
                    guardScript.isChecked = true;
                    break;
                }
            }
        }
    }

    public void GoToSleep()
    {
        if (isSleeping) return;
        currentState = State.Sleep;
        isSleeping = true;
    }

    public bool IsSleeping()
    {
        return isSleeping;
    }

    void CheckCapture()
    {
        if (PlayerAlert.Kieran != null)
        {
            float distToKieran = Vector2.Distance(transform.position, PlayerAlert.Kieran.position);
            if (distToKieran <= captureRadius)
            {
                Debug.Log("Captured Kieran!");
                GameManager.Instance.TriggerGameOver();
                return;
            }
        }

        if (PlayerAlert.DASH != null)
        {
            float distToDASH = Vector2.Distance(transform.position, PlayerAlert.DASH.position);
            if (distToDASH <= captureRadius)
            {
                Debug.Log("Captured DASH!");
                GameManager.Instance.TriggerGameOver();
                return;
            }
        }
    }

}
