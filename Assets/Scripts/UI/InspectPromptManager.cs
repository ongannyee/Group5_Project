using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectPromptManager : MonoBehaviour
{
    public static InspectPromptManager Instance;
    public GameObject promptUI_container;
    public GameObject promptUI_guard;
    public GameObject promptUI_goal;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        promptUI_container.SetActive(false);
        promptUI_guard.SetActive(false);
        promptUI_goal.SetActive(false);
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
}
