using UnityEngine;

public class AlarmManager : MonoBehaviour
{
    public static AlarmManager Instance;
    public AudioClip alarmClip;
    private AudioSource audioSource;
    private bool alarmTriggered = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    public void TriggerAlarm()
    {
        if (!alarmTriggered && alarmClip != null)
        {
            audioSource.clip = alarmClip;
            audioSource.Play();
            alarmTriggered = true;
        }
    }

    public void StopAlarm()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            alarmTriggered = false;
        }
    }
} 