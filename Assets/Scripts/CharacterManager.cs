using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Swapping control between Kieran and DASH
public class CharacterManager : MonoBehaviour
{
    public GameObject kieran;
    public GameObject dash;
    public FollowCamera followCamera;

    private bool isControllingKieran = true;
    private bool dashDeployed = false;

    void Start()
    {
        kieran.SetActive(true);
        dash.SetActive(false); // DASH is not in the world yet
        SetActiveCharacter(kieran);
    }

    void Update()
    {
        // Deploy DASH
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!dashDeployed)
            {
                DeployDash();
            }
            else if (isControllingKieran && IsOverlapping(kieran, dash))
            {
                WithdrawDash();
            }
        }

        // Swap control if both are deployed
        if (Input.GetKeyDown(KeyCode.Tab) && dashDeployed)
        {
            SwapControl();
        }
    }

    void DeployDash()
    {
        dash.transform.position = kieran.transform.position;
        dash.SetActive(true);
        dashDeployed = true;
        SetActiveCharacter(dash);
        isControllingKieran = false;
    }

    void WithdrawDash()
    {
        dash.SetActive(false);
        dashDeployed = false;
        SetActiveCharacter(kieran);
        isControllingKieran = true;
    }

    void SwapControl()
    {
        if (isControllingKieran)
        {
            SetActiveCharacter(dash);
        }
        else
        {
            SetActiveCharacter(kieran);
        }
        isControllingKieran = !isControllingKieran;
    }

    void SetActiveCharacter(GameObject character)
    {
        kieran.GetComponent<KieranController>().enabled = (character == kieran);
        dash.GetComponent<DASHController>().enabled = (character == dash);
        followCamera.SetFollowTarget(character);
    }

    bool IsOverlapping(GameObject objA, GameObject objB)
    {
        Collider2D colA = objA.GetComponent<Collider2D>();
        Collider2D colB = objB.GetComponent<Collider2D>();

        if (colA == null || colB == null) return false;

        return colA.bounds.Intersects(colB.bounds);
    }
}