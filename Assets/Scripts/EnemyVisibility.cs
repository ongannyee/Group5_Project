using System.Collections.Generic;
using UnityEngine;

public class EnemyVisibility : MonoBehaviour
{
    public static bool IsInFOV(FieldOfView fov, Transform target)
    {
        Vector3 dirToTarget = (target.position - fov.transform.position).normalized;
        float distance = Vector3.Distance(fov.transform.position, target.position);

        if (distance > fov.viewRadius) return false;
        float angle = Vector3.Angle(fov.transform.right, dirToTarget);
        if (angle > fov.viewAngle / 2f) return false;

        // Check for line of sight (obstacle blocking)
        RaycastHit2D hit = Physics2D.Raycast(fov.transform.position, dirToTarget, distance, fov.obstacleMask);
        return !hit;
    }

    public static bool CanSeePlayerCone(Transform enemyTransform, Transform cone, LayerMask obstacleMask)
    {
        Vector3 dir = (cone.position - enemyTransform.position).normalized;
        float distance = Vector3.Distance(enemyTransform.position, cone.position);

        RaycastHit2D hit = Physics2D.Raycast(enemyTransform.position, dir, distance, obstacleMask);
        return !hit;
    }
}
