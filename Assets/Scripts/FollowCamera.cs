using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] GameObject thingToFollow;

    public void SetFollowTarget(GameObject newTarget)
    {
        thingToFollow = newTarget;
    }

    void LateUpdate()
    {
        if (thingToFollow != null)
        {
            transform.position = thingToFollow.transform.position + new Vector3(0, 0, -10);
        }
    }
}
