using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LevelSelection()
    {
        SceneManager.LoadScene("LevelSelection");
    }

    public void LevelOne()
    {
        SceneManager.LoadScene("SampleScene");
        //SceneManager.LoadScene("Level1");
    }

    public void LevelTwo()
    {
        SceneManager.LoadScene("Level2");
    }

    public void LevelThree()
    {
        SceneManager.LoadScene("Level3");
    }

    public void LevelFour()
    {
        SceneManager.LoadScene("Level4");
    }

    public void LevelFive()
    {
        SceneManager.LoadScene("Level5");
    }

    public void GoToSettingMenu()
    {
        SceneManager.LoadScene("SettingMenu");
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
