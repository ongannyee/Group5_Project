using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button[] buttons;
    public AudioClip backgroundMusic;
    public AudioClip clickClip;
    private AudioSource audioSource;

    private void Awake()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Background music logic
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        if (backgroundMusic != null && (!audioSource.isPlaying || audioSource.clip != backgroundMusic))
        {
            audioSource.clip = backgroundMusic;
            audioSource.Play();
        }

        for(int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
            buttons[i].onClick.AddListener(PlayClickSound);
        }
        
        for(int i = 0; i < unlockedLevel; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public void PlayClickSound()
    {
        if (clickClip != null)
        {
            audioSource.PlayOneShot(clickClip);
        }
    }

    public void LevelSelection()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LevelSelection");
    }

    public void OpenLevel(int levelId)
    {
        // Ensure normal time scale (in case paused)
        Time.timeScale = 1f;

        // Load "StartingCutscene" only if Level1 is selected
        if (levelId == 1)
        {
            SceneManager.LoadScene("StartingCutscene");
            return; // Exit early to avoid double-loading
        }

        // Construct level name (e.g., "Level2", "Level3")
        string levelName = "Level" + levelId;

        // Check if the scene exists before loading
        if (Application.CanStreamedLevelBeLoaded(levelName))
        {
            SceneManager.LoadScene(levelName);
        }
        else
        {
            Debug.LogError("Scene not found: " + levelName);
            // Fallback: Load menu or a default level
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exited");
        Application.Quit();
    }
}
