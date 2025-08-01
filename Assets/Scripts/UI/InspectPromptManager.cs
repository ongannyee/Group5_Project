using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectPromptManager : MonoBehaviour
{
    public static InspectPromptManager Instance;
    public GameObject promptUI_container;
    public GameObject promptUI_guard;
    public GameObject promptUI_goal;
    public GameObject promptUI_router;
    public GameObject promptUI_door;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        promptUI_container.SetActive(false);
        promptUI_guard.SetActive(false);
        promptUI_goal.SetActive(false);
        promptUI_router.SetActive(false);
        promptUI_door.SetActive(false);
    }

    public void ShowPromptContainer()
    {
        promptUI_container.SetActive(true);

    }

    public void HidePromptContainer()
    {
        promptUI_container.SetActive(false);
    }

    public void ShowPromptGuard()
    {
        promptUI_guard.SetActive(true);
    }

    public void HidePromptGuard()
    {
        promptUI_guard.SetActive(false);
    }

    public void ShowPromptGoal()
    {
        promptUI_goal.SetActive(true);
    }

    public void HidePromptGoal()
    {
        promptUI_goal.SetActive(false);
    }

    public void ShowPromptRouter()
    {
        promptUI_router.SetActive(true);
    }

    public void HidePromptRouter()
    {
        promptUI_router.SetActive(false);
    }
    
    public void ShowPromptOfficeDoor()
    {
        Debug.Log("Calling ShowPromptOfficeDoor()");
        promptUI_door.SetActive(true);
    }

    public void HidePromptOfficeDoor()
    {
        promptUI_door.SetActive(false);
    }
}
