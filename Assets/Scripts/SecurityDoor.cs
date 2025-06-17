using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecurityDoor : MonoBehaviour
{
    public int requiredKeycardLevel = 1;
    public GameObject doorVisual; // the sprite/animation part
    public Collider2D doorCollider;

    private bool isOpen = false;

    void Start()
    {
        if (doorCollider == null)
            doorCollider = GetComponent<Collider2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!isOpen && other.CompareTag("Kieran") && Input.GetKeyDown(KeyCode.R))
        {
            if (HasKeycard(requiredKeycardLevel))
                OpenDoor();
            else
                Debug.Log("Need Keycard Level " + requiredKeycardLevel);
        }
    }

    bool HasKeycard(int level)
    {
        for (int i = 0; i < InventoryManager.Instance.inventorySlots.Length; i++)
        {
            InventorySlot slot = InventoryManager.Instance.GetSlot(i);
            if (!slot.IsEmpty && slot.item is KeycardItem keycard && keycard.keycardLevel >= level)
            {
                return true;
            }
        }
        return false;
    }

    void OpenDoor()
    {
        isOpen = true;
        doorCollider.enabled = false;
        if (doorVisual != null)
            doorVisual.SetActive(false); // optionally disable sprite/visual
        Debug.Log("Door Opened!");
    }
}
