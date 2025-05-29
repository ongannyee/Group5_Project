using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KieranController : MonoBehaviour
{
    //1. Player Movement
    public Rigidbody2D theRB;
    public float moveSpeed = 5;
    public Vector2 movement;

    //2. Cone Vision
    public FieldOfView fov;
    
    void Start()
    {
        
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

        //2.1 Cone Vision
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 aimDir = (mouseWorld - transform.position).normalized;

        fov.SetOrigin(transform.position);
        fov.SetAimDirection(aimDir);
    }

    void FixedUpdate()
    {
        //1.3 Player movement
        theRB.MovePosition(theRB.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
