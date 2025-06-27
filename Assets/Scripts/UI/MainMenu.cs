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
        Time.timeScale = 1f;
        string levelName = "Level" + levelId;
        SceneManager.LoadScene(levelName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Game Exitted");
        Application.Quit();
    }
}
