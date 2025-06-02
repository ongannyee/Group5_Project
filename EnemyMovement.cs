using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float obstacleCheckCircleRadius;
    [SerializeField] private float obstacleCheckDistance;
    [SerializeField] private LayerMask obstacleLayerMask;

    private Rigidbody2D rigidbody;
    private PlayerAlert playerAlert;
    private Vector2 targetDirection;
    private float changeDirectionCoooldown;
    private RaycastHit2D[] obstacleCollisions;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        playerAlert = GetComponent<PlayerAlert>();
        targetDirection = transform.up;
        obstacleCollisions = new RaycastHit2D[10];
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
    }

    private void UpdateTargetDirection()
    {
        HandleRandomDirectionChange();
        HandleObstacles();
        HandlePlayerTargeting();
    }

    private void HandleRandomDirectionChange()
    {
        changeDirectionCoooldown -= Time.deltaTime;

        if(changeDirectionCoooldown <= 0)
        {
            float angleChange = Random.Range(-90f, 90f);
            Quaternion rotation = Quaternion.AngleAxis(angleChange, transform.forward);
            targetDirection = rotation * targetDirection;

            //changeDirectionCoooldown = Random.Range(1f, 5f);
        }
    }

    private void HandlePlayerTargeting()
    {
        if (playerAlert.AwareOfPlayer)
        {
            targetDirection = playerAlert.DirectionToPlayer;
        }
    }
    
    private void HandleObstacles()
    {
        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(obstacleLayerMask);

        int numberOfCollisions = Physics2D.CircleCast(transform.position, obstacleCheckCircleRadius, transform.up, contactFilter, obstacleCollisions, obstacleCheckDistance);

        for(int i = 0; i < numberOfCollisions; i++)
        {
            var obstacleCollision = obstacleCollisions[i];

            if (obstacleCollision.collider.gameObject == gameObject)
            {
                continue;
            }

            targetDirection = obstacleCollision.normal;
            break;
        }
    }

    private void RotateTowardsTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, targetDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        rigidbody.SetRotation(rotation);
    }

    private void SetVelocity()
    {
        rigidbody.velocity = transform.up * speed;
    }
}
