using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    [Header("Regular Movement Noise")]
    public float baseNoiseRadius;         // Radius guards can hear noise while moving
    public float maxNoiseRadius;
    public float noiseCooldown = 0.5f;     // Cooldown between each noise pulse
    public LayerMask enemyLayer;           // Detect guards within radius
    public bool emitNoise = false;         // Set true when character is moving
    private float noiseTimer = 0f;
    private Rigidbody2D rb;

    [Header("One-Time Pulse (Play Noise Skill)")]
    public float pulseRadius = 10f;         // Radius of the noise pulse
    public float pulseDuration = 3f;       // Duration of the noise pulse effect

    public float pulseTimer = 0f;
    private bool isPulsing = false;

    public GameObject sdSprite;
    DASHController dashController;
    [SerializeField] GameObject dc;

    void Awake()
    {
        dashController = dc.GetComponent<DASHController>();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float speed = rb.velocity.magnitude;
        float noiseRadius = Mathf.Lerp(baseNoiseRadius, maxNoiseRadius, speed/12f);
        
        // Regular movement-based noise emission
        if (emitNoise)
        {
            noiseTimer -= Time.deltaTime;
            if (noiseTimer <= 0f)
            {
                Debug.Log($"Speed: {speed}, Lerp factor: {speed/12f}, NoiseRadius: {noiseRadius}");
                EmitNoise(noiseRadius);
                noiseTimer = noiseCooldown;
            }
        }

        // If Play Noise skill is active, emit pulse every frame for pulseDuration
        if (isPulsing)
        {
            pulseTimer -= Time.deltaTime;
            if (pulseTimer > 0f)
            {
                EmitNoise(pulseRadius);
            }
            else
            {
                isPulsing = false;
                sdSprite.SetActive(false);
                dashController.startNoiseTimer();
            }
        }
    }


    // Emits a noise pulse in the specified radius, alerting nearby guards.
    void EmitNoise(float radius)
    {
        Collider2D[] guards = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var guard in guards)
        {
            EnemyMovement em = guard.GetComponent<EnemyMovement>();
            if (em != null)
                em.HeardNoise(transform.position); // Notify guard to investigate noise
        }
    }


    // Called by DASH's Play Noise skill to start emitting pulse noise.
    public void EmitOneTimePulse()
    {
        isPulsing = true;
        pulseTimer = pulseDuration;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, baseNoiseRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxNoiseRadius);
    }
}
