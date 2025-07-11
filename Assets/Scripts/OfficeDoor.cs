using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfficeDoor : MonoBehaviour
{
    public Animator animator;

    public bool isLocked = true;
    public float lockpickDuration = 7f;
    private Coroutine lockpickRoutine;
    private bool isBeingLockpicked = false;
    private KieranController kieran;

    private GameObject kieranObj;
    private Vector2 kieranPosition;
    public float lockpickRadius = 3f; // Use world units (not 300f pixels)
    private bool hasShownPrompt = false;

    void Start()
    {
        if (kieranObj == null)
        {
            kieranObj = GameObject.Find("Kieran");
            if (kieranObj == null)
            {
                Debug.LogError("Kieran not found!");
            }
            else
            {
                Debug.Log("Found");
            }
        }
    }



    void Update()
    {
        if (kieranObj == null) return;

        kieranPosition = kieranObj.transform.position;

        if (IsInRange(kieranPosition))
        {
            if (!hasShownPrompt)
            {
                hasShownPrompt = true;
                InspectPromptManager.Instance.ShowPromptOfficeDoor();
            }
        }
        else
        {
            if (hasShownPrompt)
            {
                hasShownPrompt = false;
                InspectPromptManager.Instance.HidePromptOfficeDoor();
            }
        }
    }



    public bool IsInRange(Vector2 unitPosition)
    {
        return Vector2.Distance(transform.position, unitPosition) <= lockpickRadius;
    }


    public void TryLockpick(KieranController kieranRef)
    {
        if (!isLocked || isBeingLockpicked)
        {
            CancelLockpick(kieranRef);
            return;
        }

        kieran = kieranRef;
        lockpickRoutine = StartCoroutine(LockpickRoutine());
    }

    IEnumerator LockpickRoutine()
    {
        isBeingLockpicked = true;
        kieran.isLockedByScript = true;
        Debug.Log("Lockpicking started...");

        float timer = 0f;
        while (timer < lockpickDuration)
        {
            if (!isBeingLockpicked) yield break; // In case cancelled
            timer += Time.deltaTime;
            Debug.Log(timer);
            yield return null;
        }

        UnlockDoor();
    }

    public void CancelLockpick(KieranController kieranRef)
    {
        if (isBeingLockpicked)
        {
            Debug.Log("Lockpicking cancelled.");
            if (lockpickRoutine != null)
                StopCoroutine(lockpickRoutine);
            isBeingLockpicked = false;
            kieranRef.isLockedByScript = false;
        }
    }

    public void UnlockDoor()
    {
        animator.SetBool("interaction", false);
        isLocked = false;
        isBeingLockpicked = false;
        if (kieran != null) kieran.isLockedByScript = false;
        Debug.Log("Door unlocked!");
        gameObject.SetActive(false); // Hide the door
    }

    public void UnlockInstantly()
    {
        //StopAllCoroutines();
/*         isLocked = false;
        isBeingLockpicked = true; */
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, lockpickRadius);
}

}
