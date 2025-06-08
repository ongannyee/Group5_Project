using System.Collections.Generic;
using UnityEngine;

//checks whether a target is within a Field of View (FOV) cone. Used in  
public class EnemyVisibility : MonoBehaviour
{
    // Return True if target is visible
    public static bool IsInFOV(FieldOfView fov, Transform target)
    {
        Vector3 dirToTarget = (target.position - fov.transform.position).normalized;    // Computes the direction vector from the FOV origin to the target
        float distance = Vector3.Distance(fov.transform.position, target.position);     // Measures how far the target is from the FOV origin
        if (distance > fov.viewRadius) return false;                                    // Not visible if the target is outside the view radius

        float angle = Vector3.Angle(fov.transform.right, dirToTarget);                  // Calculates the angle between the FOV's right direction and the direction to the target
        if (angle > fov.viewAngle / 2f) return false;                                   // Not visible if the target is outside the FOV cone angle

        RaycastHit2D hit = Physics2D.Raycast(fov.transform.position, dirToTarget, distance, fov.obstacleMask);  // Check for line of sight (obstacle blocking)
        return !hit;    // Returns true if no obstacle is hit, means the target is in range, within angle, and not blocked — so it's visible
    }

    // checks whether an enemy can "see" a player’s vision cone
    public static bool CanSeePlayerCone(Transform enemyTransform, Transform cone, LayerMask obstacleMask)
    {
        Vector3 dir = (cone.position - enemyTransform.position).normalized;                             // Calculates the direction from the enemy to the cone
        float distance = Vector3.Distance(enemyTransform.position, cone.position);                      // Measures the straight-line distance between the enemy and the cone

        RaycastHit2D hit = Physics2D.Raycast(enemyTransform.position, dir, distance, obstacleMask);
        return !hit;
    }
}
