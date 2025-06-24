using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InspectPromptManager : MonoBehaviour
{
    public static InspectPromptManager Instance;
    public GameObject promptUI; // Assign in Inspector

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        promptUI.SetActive(false);
    }

    public void ShowPrompt()
    {
        promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        promptUI.SetActive(false);
    }
}
