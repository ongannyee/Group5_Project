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

    void Start()
    {
        moveSpeed = normalSpeed; // Start at normal speed
    }

    void Update()
    {
        //1.1 Get movement input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        //1.2 Rotate to face the mouse cursor
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        //1.3 Toggle move speed with spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            speedState = (speedState + 1) % 3;

            switch (speedState)
            {
                case 0:
                    moveSpeed = normalSpeed;
                    break;
                case 1:
                    moveSpeed = fastSpeed;
                    break;
                case 2:
                    moveSpeed = fasterSpeed;
                    break;
            }

            Debug.Log("Speed state: " + speedState + " | Speed: " + moveSpeed);
        }

        //2.1 Cone Vision
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDir = (mouseWorld - transform.position).normalized;

        fov.SetOrigin(transform.position);
        fov.SetAimDirection(aimDir);

        if (fogOfWar != null && fov != null)
        {
            fogOfWar.RevealConeMesh(fov.GetWorldVertices());
            // Reveal a small circle around player for peripheral awareness
            fogOfWar.Reveal(transform.position, circularRadius); // radius = 1.5 units

        }
    }

    void FixedUpdate()
    {
        //1.4 Player movement
        theRB.MovePosition(theRB.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
