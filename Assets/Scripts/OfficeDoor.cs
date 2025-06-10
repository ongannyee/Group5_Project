using System.Collections;
using UnityEngine;

public class OfficeDoor : MonoBehaviour
{
    public bool isLocked = true;
    public float lockpickDuration = 7f;
    private Coroutine lockpickRoutine;
    private bool isBeingLockpicked = false;
    private KieranController kieran;

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
        isLocked = false;
        isBeingLockpicked = false;
        if (kieran != null) kieran.isLockedByScript = false;
        Debug.Log("Door unlocked!");
        gameObject.SetActive(false); // Hide the door
    }

    public void UnlockInstantly()
    {
        StopAllCoroutines();
        isLocked = false;
        isBeingLockpicked = true;
        gameObject.SetActive(false);
    }
}
