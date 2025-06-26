using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject loseUI;
    public GameObject winUI;
    public GameObject pauseUI;

    public static bool isPaused;
    private bool gameEnded = false;

    private void Awake()
    {
        pauseUI.SetActive(false);
        loseUI.SetActive(false);
        winUI.SetActive(false);
        if (Instance == null)
            Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){
            if(isPaused){
                ResumeGame();
            }
            else{
                PauseGame();
            }
        }
    }

    public void TriggerGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (loseUI != null)
            loseUI.SetActive(true);
            isPaused = true;

        Debug.Log("Game Over!");
        Time.timeScale = 0f;
    }

    public void TriggerVictory()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (winUI != null)
            winUI.SetActive(true);
            isPaused = true;

            Debug.Log("You Win!");
            UnlockNewLevel();
        Time.timeScale = 0f;
    }

    public void PauseGame()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void toMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exitted.");
        Application.Quit();
    }

    void UnlockNewLevel()
    {
        if(SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex")){
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevel", PlayerPrefs.GetInt("UnlockedLevel", 1) + 1);
            PlayerPrefs.Save();
        }
    }
}
