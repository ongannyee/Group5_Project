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

    public FogOfWar fogOfWar;
    public LayerMask obstacleMask;

    private KieranController kieranController;
    private DASHController dashController;

    void Start()
    {
        kieranController = kieran.GetComponent<KieranController>();
        dashController = dash.GetComponent<DASHController>();
        kieran.SetActive(true);
        dash.SetActive(false); // DASH is not in the world yet
        SetActiveCharacter(kieran);
    }

    void Update()
    {
        // Deploy DASH
        if (Input.GetKeyDown(KeyCode.Q))
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

    void LateUpdate()
    {
        if (fogOfWar == null || kieran == null || dash == null) return;

/*         List<List<Vector3>> coneMeshes = new List<List<Vector3>>();
        List<Vector3> circularCenters = new List<Vector3>();
        List<float> circularRadii = new List<float>();
        List<bool> seesThroughWalls = new List<bool>(); */

        // --- Always include Kieran if active
        if (kieran.activeSelf)
        {
            fogOfWar.RevealConeMesh(kieranController.fov.GetWorldVertices());
            fogOfWar.RevealCircularBlocked(kieran.transform.position, kieranController.circularRadius, LayerMask.GetMask("Obstacles"));
        }

        // --- Include DASH if active
        if (dash.activeSelf)
        {
            //fogOfWar.RevealConeMesh(dashController.fov.GetWorldVertices());

            if (dashController.canSeeThroughWalls)
            {
                //fogOfWar.Reveal(dash.transform.position, dashController.circularRadius);    
                fogOfWar.RevealCircularBlocked(dash.transform.position, dashController.circularRadius, LayerMask.GetMask("Default"));
                
            }
            else
            {
                fogOfWar.RevealCircularBlocked(dash.transform.position, dashController.circularRadius, obstacleMask);
            }
        }

        // --- Reveal vision per character
/*         for (int i = 0; i < coneMeshes.Count; i++)
        {
            //fogOfWar.RevealConeMesh(coneMeshes[i]);

            if (seesThroughWalls[i])
            {
                fogOfWar.Reveal(circularCenters[i], circularRadii[i]);
            }
            else
            {
                fogOfWar.RevealCircularBlocked(circularCenters[i], circularRadii[i], obstacleMask);
            }
        } */
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