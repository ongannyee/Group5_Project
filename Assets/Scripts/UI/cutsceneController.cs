using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class cutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneSlide
    {
        public Sprite image;
        [TextArea(3, 5)]
        public string[] dialogueLines;
    }

    public CutsceneSlide[] slides;
    private int counter = 0;

    public Image cutsceneImage;
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.03f;

    private int currentSlideIndex = 0;
    private int currentLineIndex = 0;

    private bool isTyping = false;
    private bool textFullyDisplayed = false;

    private Coroutine typingCoroutine;


    [Header("Audio")]
    public AudioClip textWritingSound;
    public AudioClip backgroundMusic;

    public AudioSource typingAudioSource;
    public AudioSource musicAudioSource;

    void Start()
    {
        if (musicAudioSource && backgroundMusic)
        {
            musicAudioSource.clip = backgroundMusic;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        ShowCurrentSlide();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                // Skip to full line
                StopCoroutine(typingCoroutine);
                dialogueText.text = slides[currentSlideIndex].dialogueLines[currentLineIndex];
                isTyping = false;
                textFullyDisplayed = true;
            }
            else if (textFullyDisplayed)
            {
                GoToNextLineOrSlide();
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Level1");
        }
    }

    void ShowCurrentSlide()
    {
        if (currentSlideIndex < slides.Length)
        {
            cutsceneImage.sprite = slides[currentSlideIndex].image;
            currentLineIndex = 0;
            ShowCurrentLine(true);
        }
        else
        {
            EndCutscene();
        }
    }

    void ShowCurrentLine(bool clear)
    {
        if(clear)
        {
            dialogueText.text = "";
        }
        typingCoroutine = StartCoroutine(TypeLine(slides[currentSlideIndex].dialogueLines[currentLineIndex]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        textFullyDisplayed = false;

        foreach (char letter in line)
        {
            dialogueText.text += letter;

            if (typingAudioSource && textWritingSound)
            {
                typingAudioSource.PlayOneShot(textWritingSound);
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        textFullyDisplayed = true;
    }

    void GoToNextLineOrSlide()
    {
        currentLineIndex++;
        if (currentLineIndex < slides[currentSlideIndex].dialogueLines.Length)
        {
            ShowCurrentLine(false);
        }
        else
        {
            currentSlideIndex++;
            ShowCurrentSlide();
        }
    }

    void EndCutscene()
    {
        dialogueText.text = "";
        cutsceneImage.enabled = false;
        enabled = false;
        Debug.Log("Cutscene ended.");

        // Load next scene
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level1");
    }
}
