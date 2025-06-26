/* using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateChange : MonoBehaviour
{
    public GameObject crouch;
    public GameObject walk;
    public GameObject run;

    public int ss = 0;
    // Start is called before the first frame update
    void Start()
    {
        crouch.SetActive(true);
        walk.SetActive(false);
        run.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            ss = (ss + 1) % 3;
            switch (ss)
            {
                case 0: run.SetActive(false); crouch.SetActive(true); break;
                case 1: crouch.SetActive(false); walk.SetActive(true); break;
                case 2: walk.SetActive(false); run.SetActive(true); break;
            }
        }
    }
}
 */