using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class KieranController : MonoBehaviour
{
    public Animator animator;
    OPCounter opCounter;
    public GameObject opc;

    //1. Player Movement
    public Rigidbody2D theRB;
    public float normalSpeed = 5f;
    public float fastSpeed = 8f;
    public float fasterSpeed = 12f;
    private float moveSpeed;
    public int speedState = 1; // 0: normal, 1: fast, 2: faster
    public Vector2 movement;

    //2. Cone Vision and circular vision
    public FieldOfView fov;
    public FogOfWar fogOfWar;
    public float circularRadius = 2f;
    //public float peripheralRadius = 2f; // Adjustable in Inspector

    // 3. Hacking and item system
    public GameObject z3raHackingUI;        // UI panel reference
    public bool isLockedByScript = false;   // Unable to move while lockpicking
    public Z3raHackingManager z3ra;

    // 4. Reach Goal
    private bool isNearGoal = false;
    // Assign a UI Panel with "You Win!" in inspector

    // 5. Items
    public GameObject sleepingDartPrefab; // assign in Inspector
    public Transform dartSpawnPoint;      // empty child transform in front of Kieran
    [SerializeField] private Transform dartPosition;
    //public int dartQuantity = 3;

    // 6. Actions on sleeping guard
    //6.1 drag guard
    public float dragRadius = 1.5f;
    private EnemyMovement draggingGuard = null;
    //6.2 wear uniform
    public bool isDisguised = false;
    public GuardType disguisedAs = GuardType.None;
    public float disguiseRadius = 1.5f;

    void Awake()
    {
        opCounter = opc.GetComponent<OPCounter>();
    }

    void Start()
    {
        moveSpeed = fastSpeed; // Start at normal speed

        // Ensure Z3ra hacking UI is initially hidden
        if (z3raHackingUI != null)
            z3raHackingUI.SetActive(false);
    }

    void Update()
    {
        if(!PauseMenu.isPaused){
            HandleMovementInput();
            HandleRotation();
            HandleSpeedToggle();
            HandleNoiseEmission();
            HandleVisionReveal();
            HandlePlayerAbilities();
            LockPickingOfficeDoor();
            HandleDisguise();
            HandleGuardDragging();
            returnToGoals();
            // Item related
            UpdateDartRotation();
        }
    }

    void FixedUpdate()
    {
        //1.4 Player movement
        if (isLockedByScript) return;   //Unable movement while lockpicking
        //theRB.MovePosition(theRB.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        theRB.velocity = movement.normalized * moveSpeed;
    }

    // 1.1 Get movement input
    void HandleMovementInput()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        //animation
        bool isMoving;

        if(movement.x != 0 || movement.y != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
        animator.SetBool("isMoving", isMoving);
    }

    //1.2 Rotate to face the mouse cursor
    void HandleRotation()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 90);
    }

    //1.3 Toggle move speed with spacebar
    void HandleSpeedToggle()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            speedState = (speedState + 1) % 3;
            switch (speedState)
            {
                case 0: moveSpeed = normalSpeed; break;
                case 1: moveSpeed = fastSpeed; break;
                case 2: moveSpeed = fasterSpeed; break;
            }
            Debug.Log("Speed state: " + speedState + " | Speed: " + moveSpeed);
            animator.SetInteger("speedState", speedState);
        }
    }

    //1.4 Noise
    void HandleNoiseEmission()
    {
        bool isMoving = movement.sqrMagnitude > 0.01f;
        GetComponent<NoiseEmitter>().emitNoise = isMoving;
    }

    //2.1 Cone Vision and Circular vision direction
    void HandleVisionReveal()
    {
        if (fov == null || fogOfWar == null) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDir = (mouseWorld - transform.position).normalized;

        fov.SetOrigin(transform.position);
        fov.SetAimDirection(aimDir);

        //fogOfWar.RevealConeMesh(fov.GetWorldVertices());
        //fogOfWar.RevealCircularBlocked(transform.position, circularRadius, LayerMask.GetMask("Obstacles"));
    }

    // 3. Z3ra Hacking Abilities and Items System
    void HandlePlayerAbilities()
    {
        // Press E to open Z3ra hacking menu
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleZ3raHackingMenu();
        }

        if (z3raHackingUI.activeSelf)
        {
            // Z3ra abilities (E menu is open)
            if (Input.GetKeyDown(KeyCode.Alpha1)) z3ra.TryJamComms();
            if (Input.GetKeyDown(KeyCode.Alpha2)) z3ra.TryThermalSweep();
            if (Input.GetKeyDown(KeyCode.Alpha3)) z3ra.TryRouterPing();
            if (Input.GetKeyDown(KeyCode.Alpha4)) z3ra.TryFlickerSurveillance();
            if (Input.GetKeyDown(KeyCode.Alpha5)) z3ra.TryPowerSurge();
            if (Input.GetKeyDown(KeyCode.Alpha6)) z3ra.TryHackDoor();
            opCounter.counterUpdate();
        }
        else
        {
            // Items (menu is closed)
            if (Input.GetKeyDown(KeyCode.Alpha1)) 
            {
                TryShootDart();
                InventoryManager.Instance.UseItem(1);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) UseItem(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UseItem(3);
        }
    }

    void ToggleZ3raHackingMenu()
    {
        if (z3raHackingUI != null)
        {
            bool isActive = z3raHackingUI.activeSelf;
            z3raHackingUI.SetActive(!isActive);
            Debug.Log("Z3ra Hacking Menu: " + (!isActive ? "Opened" : "Closed"));
        }
    } 

    void UseItem(int slot)
    {
        Debug.Log("Use item slot " + slot + " (functionality coming in next cycle)");
    }

/*     void TryShootDart()
    {
        if(dartQuantity>0)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - dartSpawnPoint.position).normalized;

            GameObject dart = Instantiate(sleepingDartPrefab, dartSpawnPoint.position, Quaternion.identity);
            dart.GetComponent<SleepingDart>().Launch(direction);
            dartQuantity--;
        }
        else
        {Debug.Log("No Sleeping Dart left.");}
    } */

    void TryShootDart()
    {
        // Check if player has at least 1 Sleeping Dart in inventory
        for (int i = 0; i < 5; i++)
        {
            InventorySlot slot = InventoryManager.Instance.GetSlot(i);
            if (slot.item != null && slot.item.itemName == "Sleeping Dart")
            {
                // Consume dart
                InventoryManager.Instance.UseItem(i);

                // Fire towards mouse direction
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (mouseWorldPos - (Vector2)dartSpawnPoint.position).normalized;

                GameObject dart = Instantiate(sleepingDartPrefab, dartSpawnPoint.position, Quaternion.identity);
                dart.GetComponent<SleepingDart>().Launch(direction);
                return;
            }
        }

        Debug.Log("No Sleeping Dart available");
    }

    private void UpdateDartRotation()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = mouseWorldPos - dartPosition.position;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        dartPosition.rotation = Quaternion.Euler(0f, 0f, angle);
    }


    void HandleGuardDragging()
    {
        if (Input.GetMouseButton(0)) // Hold LMB to drag
        {
            if (draggingGuard == null)
            {
                // Try to find a sleeping guard nearby
                Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, dragRadius);
                foreach (var col in nearby)
                {
                    EnemyMovement guard = col.GetComponent<EnemyMovement>();
                    if (guard != null && guard.IsSleeping() && !guard.isBeingDragged)
                    {
                        draggingGuard = guard;
                        guard.isBeingDragged = true;
                        guard.draggedBy = transform;
                        break;
                    }
                }
            }
        }
        else
        {
            if (draggingGuard != null)
            {
                draggingGuard.isBeingDragged = false;
                draggingGuard.draggedBy = null;
                draggingGuard = null;
            }
        }
    }

    void HandleDisguise()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isDisguised)
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, disguiseRadius);
            foreach (var col in nearby)
            {
                EnemyMovement guard = col.GetComponent<EnemyMovement>();
                if (guard != null && guard.IsSleeping())
                {
                    isDisguised = true;
                    disguisedAs = guard.guardType;
                    Debug.Log("Kieran disguised as " + disguisedAs);
                    break;
                }
            }
        }
    }

    void LockPickingOfficeDoor()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var c in nearby)
            {
                OfficeDoor door = c.GetComponentInParent<OfficeDoor>();
                if (door != null)
                {
                    animator.SetBool("interaction", true);
                    door.TryLockpick(this);
                    break;
                }
            }
        }
    }

    void returnToGoals()
    {
        if (isNearGoal && Input.GetKeyDown(KeyCode.F))
        {
            // Check if any guard is still chasing or alarmed
            EnemyMovement[] allGuards = FindObjectsOfType<EnemyMovement>();
            bool anyChasing = allGuards.Any(g => 
                g.currentState == EnemyMovement.State.Chasing || 
                g.currentState == EnemyMovement.State.Alarmed);

            if (anyChasing)
            {
                Debug.Log("Cannot return yet! Guards are chasing you.");
                return;
            }

            // All clear, you win
            GameManager.Instance.TriggerVictory();
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            isNearGoal = true;
            InspectPromptManager.Instance.ShowPromptGoal();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            isNearGoal = false;
            InspectPromptManager.Instance.HidePromptGoal();
        }
    }
}
