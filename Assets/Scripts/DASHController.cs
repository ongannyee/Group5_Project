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

    void Update()
    {
        // 1. Movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. Rotation toward mouse
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 3. Set full-circle field of view
        if (fov != null)
        {
            fov.SetOrigin(transform.position);
            fov.viewAngle = 360f; // full circle for DASH
            fov.canSeeThroughWalls = canSeeThroughWalls;
        }

        // !!NEED REVISED LOGIC 4. Reveal vision using full circle mesh and optionally reveal through walls 
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
        }
    }

    void FixedUpdate()
    {
        // 1.3 Move
        theRB.MovePosition(theRB.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
