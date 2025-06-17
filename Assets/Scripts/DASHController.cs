using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DASHController : MonoBehaviour
{
    // 1. DASH Movement
    public Rigidbody2D theRB;
    public float moveSpeed = 5;
    public Vector2 movement;

    // 2. Vision System
    public FieldOfView fov;
    public FogOfWar fogOfWar;
    public float circularRadius = 4f;
    public LayerMask obstacleMask; // Assign "Obstacles" layer in Inspector
    public bool canSeeThroughWalls = true; // Can be toggled by future skills

    // 3. DASH Skills
    // Camo Mode
    public bool isInCammo = false;
    private float cammoCooldown = 10f;
    private float cammoCooldownTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private bool canMove = true;

    // Play Noise
    private float noiseCooldown = 10f;
    private float noiseCooldownTimer = 0f;
    private NoiseEmitter noiseEmitter;  

    //Connect to Router
    public LayerMask routerLayer;

    void Start()
    {
        noiseEmitter = GetComponent<NoiseEmitter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // --- Skill Cooldowns ---
        if (cammoCooldownTimer > 0) cammoCooldownTimer -= Time.deltaTime;
        if (noiseCooldownTimer > 0) noiseCooldownTimer -= Time.deltaTime;
        
        // 1.1 Movement input
        if (canMove)
        {
            movement.x = Input.GetAxisRaw("Horizontal");
            movement.y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            movement = Vector2.zero;
        }

        // 1.2 Rotation toward mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 1.3 Noise
        bool isMoving = movement.sqrMagnitude > 0.01f;
        GetComponent<NoiseEmitter>().emitNoise = isMoving;

        // 2. Set full-circle field of view
        if (fov != null)
        {
            fov.SetOrigin(transform.position);
            fov.viewAngle = 360f; // full circle for DASH
            fov.canSeeThroughWalls = canSeeThroughWalls;
        }

/*         // !!NEED REVISED LOGIC 4. Reveal vision using full circle mesh and optionally reveal through walls 
        if (fogOfWar != null && fov != null)
        {
            fogOfWar.RevealConeMesh(fov.GetWorldVertices());

            if (canSeeThroughWalls)
            {
                fogOfWar.Reveal(transform.position, circularRadius);    
                
            }
            else
            {
                fogOfWar.RevealCircularBlocked(transform.position, circularRadius, obstacleMask);
            }
        } */

        // 3. Skill Input
        if (Input.GetKeyDown(KeyCode.Alpha1)) TryCammoMode();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryPlayNoise();
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryConnectToRouter();
    }

    void FixedUpdate()
    {
        // 1.3 Move
        //theRB.MovePosition(theRB.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
        theRB.velocity = movement.normalized * moveSpeed;
    }

    // 3. DASH SKILLS
    // === Skill 1: Cammo Mode ===
    void TryCammoMode()
    {
        if (cammoCooldownTimer > 0 && !isInCammo) 
        {
            Debug.Log("Cooldown: " + cammoCooldownTimer);
            return;
        }

        if (!isInCammo)
        {
            // Enter Cammo
            isInCammo = true;
            canMove = false;
            noiseEmitter.emitNoise = false;
            spriteRenderer.color = new Color(1, 1, 1, 0.3f); // translucent effect
            cammoCooldownTimer = cammoCooldown;
            //need handle timer
            int dashCamoLayer = LayerMask.NameToLayer("Camo");
            gameObject.layer = dashCamoLayer;
        }
        else
        {
            // Exit Cammo manually
            isInCammo = false;
            canMove = true;
            spriteRenderer.color = new Color(0, 0, 1, 1f);
            int dashCamoLayer = LayerMask.NameToLayer("DASH");
            gameObject.layer = dashCamoLayer;
        }
    }

    // === Skill 2: Play Noise ===
    void TryPlayNoise()
    {
        if (noiseCooldownTimer > 0)
        {
            Debug.Log("Cooldown: " + noiseCooldownTimer);
            return;
        }

        noiseEmitter.EmitOneTimePulse(); // Custom method, will create next
        noiseCooldownTimer = noiseCooldown;
    }

    // === Skill 3: Connect to Router ===
    void TryConnectToRouter()
    {
        Router[] routers = FindObjectsOfType<Router>();
        foreach (var router in routers)
        {
            if (router.IsInRange(transform.position))
            {
                router.Activate(); // or your activation logic
                break;
            }
        }
    }

    // Z3ra Hacking Abilities helper foo
    public void EnableThermalSweep(float duration)
    {
        StartCoroutine(ThermalSweepCoroutine(duration));
    }

    IEnumerator ThermalSweepCoroutine(float duration)
    {
        canSeeThroughWalls = true;
        yield return new WaitForSeconds(duration);
        canSeeThroughWalls = false;
    }
}
