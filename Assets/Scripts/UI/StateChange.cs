/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateChange : MonoBehaviour
{
    KieranController kieranController;
    DASHController dashController;
    CharacterManager characterManager;


    [SerializeField] GameObject kieran;
    [SerializeField] GameObject dash;
    [SerializeField] GameObject cm;
    public GameObject crouch;
    public GameObject walk;
    public GameObject run;
    public GameObject motor;
    public GameObject camo;

    // Start is called before the first frame update

    void Awake()
    {
        kieranController = kieran.GetComponent<KieranController>();
        dashController = dash.GetComponent<DASHController>();
        characterManager = cm.GetComponent<CharacterManager>();
    }
    void Start()
    {
        crouch.SetActive(true);
        walk.SetActive(false);
        run.SetActive(false);
        motor.SetActive(false);
        camo.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Alpha1))
        {
            crouch.SetActive(false);
            walk.SetActive(false);
            run.SetActive(false);
            motor.SetActive(false);
            camo.SetActive(false);
            if(characterManager.isControllingKieran == true){
                switch (kieranController.speedState)
                {
                    case 0: crouch.SetActive(true); break;
                    case 1: walk.SetActive(true); break;
                    case 2: run.SetActive(true); break;
                }
            }
            else{
                if(dashController.isInCammo == false){
                    motor.SetActive(true);
                }
                else{
                    camo.SetActive(true);
                }
            }
        }
    }
}
 */