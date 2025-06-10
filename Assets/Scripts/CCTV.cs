using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCTV : MonoBehaviour
{
    public FieldOfView fov;
    public bool isFlickering = false;
    private float flickerTimer;

    public GameObject alertIndicator;
    [SerializeField] private Transform spriteTransform;

    public bool isRotate = true;
    public float rotateSpeed = 20f;
    public float rotateAngle = 45f; // total swing angle (e.g., -45 to +45)
    private float currentAngle = 0f;
    private float direction = 1f;
    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Show alert if player visible
        alertIndicator.SetActive(fov.visibleTargets.Count > 0);

        // Rotating back and forth
        if (isRotate)
        {
            currentAngle += direction * rotateSpeed * Time.deltaTime;
            if (Mathf.Abs(currentAngle) > rotateAngle)
            {
                direction *= -1f;
                currentAngle = Mathf.Clamp(currentAngle, -rotateAngle, rotateAngle);
            }

            transform.rotation = startRotation * Quaternion.Euler(0, 0, currentAngle);
        }

        fov.SetOrigin(transform.position);
        fov.SetAimDirection(transform.right);

        if (!isFlickering)
            CheckForPlayer();
        else
        {
            flickerTimer -= Time.deltaTime;
            if (flickerTimer <= 0f)
                StopFlicker();
        }
    }

    void CheckForPlayer()
    {
        if (fov.visibleTargets.Count > 0)
        {
            Transform highestPriorityTarget = PlayerAlert.GetVisiblePlayerToChase(fov.visibleTargets);
            if (highestPriorityTarget != null)
                Alarm(highestPriorityTarget);
        }
    }

    void Alarm(Transform target)
    {
        Debug.Log("CCTV triggered alarm on " + target.name);
        EnemyMovement[] allGuards = FindObjectsOfType<EnemyMovement>();
        foreach (var guard in allGuards)
        {
            guard.SetTarget(target); // Immediately forces chase
        }
    }

    public void StartFlicker(float duration)
    {
        isFlickering = true;
        flickerTimer = duration;
        fov.enabled = false;
    }

    public void StopFlicker()
    {
        isFlickering = false;
        fov.enabled = true;
    }

    public void Flicker(float duration)
    {
        StartCoroutine(FlickerCoroutine(duration));
    }

    IEnumerator FlickerCoroutine(float duration)
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(true);
    }
}
