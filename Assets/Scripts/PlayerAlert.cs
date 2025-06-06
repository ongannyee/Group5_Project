using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAlert : MonoBehaviour
{
    public bool AwareOfPlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }

    [SerializeField] private float playerAlertDistance;

    private Transform kieran;
    private Transform dash;

    private void Awake()
    {
        // Find both players in the scene
        kieran = FindObjectOfType<KieranController>().transform;
        dash = FindObjectOfType<DASHController>().transform; // Replace with your actual DASH controller name
    }

    private void Update()
    {
        AwareOfPlayer = false;

        Vector2 enemyPos = transform.position;
        Vector2 directionToKieran = (Vector2)kieran.position - enemyPos;
        Vector2 directionToDash = (Vector2)dash.position - enemyPos;

        float distanceToKieran = directionToKieran.magnitude;
        float distanceToDash = directionToDash.magnitude;

        // Check who is in range
        bool kieranInRange = distanceToKieran <= playerAlertDistance;
        bool dashInRange = distanceToDash <= playerAlertDistance;

        if (kieranInRange && dashInRange)
        {
            // Choose the closest one
            if (distanceToKieran <= distanceToDash)
            {
                DirectionToPlayer = directionToKieran.normalized;
            }
            else
            {
                DirectionToPlayer = directionToDash.normalized;
            }

            AwareOfPlayer = true;
        }
        else if (kieranInRange)
        {
            DirectionToPlayer = directionToKieran.normalized;
            AwareOfPlayer = true;
        }
        else if (dashInRange)
        {
            DirectionToPlayer = directionToDash.normalized;
            AwareOfPlayer = true;
        }
    }
}
