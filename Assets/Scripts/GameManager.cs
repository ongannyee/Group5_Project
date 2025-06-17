using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject loseUI;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void TriggerGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (loseUI != null)
            loseUI.SetActive(true);

        Debug.Log("Game Over!");
        Time.timeScale = 0f;
    }
}
