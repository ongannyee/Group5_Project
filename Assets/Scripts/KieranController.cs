using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KieranController : MonoBehaviour
{
    //1. Player Movement
    public Rigidbody2D theRB;
    public float normalSpeed = 5f;
    public float fastSpeed = 8f;
    public float fasterSpeed = 12f;
    private float moveSpeed;
    private int speedState = 0; // 0: normal, 1: fast, 2: faster
    public Vector2 movement;

    //2. Cone Vision and circular vision
    public FieldOfView fov;
    public FogOfWar fogOfWar;
    public float circularRadius = 2f;
    public float peripheralRadius = 2f; // Adjustable in Inspector

    // 3. Hacking and item system
    public GameObject z3raHackingUI;        // UI panel reference
    public bool isLockedByScript = false;   // Unable to move while lockpicking
    public Z3raHackingManager z3ra;

    // 4. Reach Goal
    private bool isNearGoal = false;

    void Start()
    {
        moveSpeed = normalSpeed; // Start at normal speed

        // Ensure Z3ra hacking UI is initially hidden
        if (z3raHackingUI != null)
            z3raHackingUI.SetActive(false);
    }

    void Update()
    {
        HandleMovementInput();
        HandleRotation();
        HandleSpeedToggle();
        HandleNoiseEmission();
        HandleVisionReveal();
        HandlePlayerAbilities();
        LockPickingOfficeDoor();
        returnReplica();
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
    }

    //1.2 Rotate to face the mouse cursor
    void HandleRotation()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    //1.3 Toggle move speed with spacebar
    void HandleSpeedToggle()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            speedState = (speedState + 1) % 3;
            switch (speedState)
            {
                case 0: moveSpeed = normalSpeed; break;
                case 1: moveSpeed = fastSpeed; break;
                case 2: moveSpeed = fasterSpeed; break;
            }
            Debug.Log("Speed state: " + speedState + " | Speed: " + moveSpeed);
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
        }
        else
        {
            // Items (menu is closed)
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseItem(1);
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
                    door.TryLockpick(this);
                    break;
                }
            }
        }
    }

    void returnReplica()
    {
        if (isNearGoal && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("You Win!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            isNearGoal = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Goal"))
        {
            isNearGoal = false;
        }
    }
}
