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
    public AudioClip loseSound;
    public AudioClip winSound;
    private AudioSource audioSource;

    public static bool isPaused;
    private bool gameEnded = false;

    private void Awake()
    {
        pauseUI.SetActive(false);
        loseUI.SetActive(false);
        winUI.SetActive(false);
        if (Instance == null)
            Instance = this;
        audioSource = GetComponent<AudioSource>();
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

        if (Input.GetKeyDown(KeyCode.P)) // Press 'P' to reset (for testing)
        {
            ResetProgress();
        }
        if (Input.GetKeyDown(KeyCode.O)) // Press 'P' to reset (for testing)
        {
            UnlockAllLevel();
        }
    }

    public void TriggerGameOver()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (loseUI != null)
            loseUI.SetActive(true);
            isPaused = true;

        if (loseSound != null && audioSource != null)
            audioSource.PlayOneShot(loseSound);

        Debug.Log("Game Over!");
        Time.timeScale = 0f;
        // Stop alarm sound
        if (AlarmManager.Instance != null)
        {
            AlarmManager.Instance.StopAlarm();
        }
    }

    public void TriggerVictory()
    {
        if (gameEnded) return;

        gameEnded = true;
        if (winUI != null)
            winUI.SetActive(true);
            isPaused = true;

        if (winSound != null && audioSource != null)
            audioSource.PlayOneShot(winSound);

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

    public void ResetProgress()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1);  // Reset to Level 1
        PlayerPrefs.SetInt("ReachedIndex", 1);   // Reset highest reached level
        PlayerPrefs.Save();                     // Save changes immediately
        Debug.Log("Progress reset to Level 1!");
    }

    public void UnlockAllLevel()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 5);  // Reset to Level 1
        PlayerPrefs.SetInt("ReachedIndex", 5);   // Reset highest reached level
        PlayerPrefs.Save();                     // Save changes immediately
        Debug.Log("Progress open to Level 5!");
    }
}
