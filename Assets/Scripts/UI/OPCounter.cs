using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OPCounter : MonoBehaviour
{
    Z3raHackingManager z3raHackingManager;
    [SerializeField] GameObject zhm;
    public Text text;
    // Start is called before the first frame update
    void Awake()
    {
        z3raHackingManager = zhm.GetComponent<Z3raHackingManager>();
        text.text = z3raHackingManager.currentOP.ToString();
    }

    public void counterUpdate()
    {
        text.text = z3raHackingManager.currentOP.ToString();
    }
}
