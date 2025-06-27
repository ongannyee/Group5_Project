using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<InventorySlot> containerSlots = new List<InventorySlot>(4); // 4 slots
    public Transform interactionPoint;
    public float interactionRadius = 1.5f;
    private bool wasPlayerNearby = false;

    private bool isPlayerNearby = false;
    private GameObject playerInRange;

    void Awake()
    {
        // Initialize empty slots
        for (int i = 0; i < 5; i++)
        {
            containerSlots.Add(new InventorySlot());
        }
    }

    void Update()
    {
        CheckPlayerInRange();

        // Trigger only when player enters range
        if (isPlayerNearby && !wasPlayerNearby)
        {
            InspectPromptManager.Instance.ShowPromptContainer();
        }
        // Trigger only when player leaves range
        else if (!isPlayerNearby && wasPlayerNearby)
        {
            ContainerUI.Instance.CloseContainer();
            InspectPromptManager.Instance.HidePromptContainer();
        }

        // Interaction input
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.R))
        {
            bool isKieran = playerInRange.CompareTag("Kieran");
            ContainerUI.Instance.OpenContainer(this, isKieran);
        }

        // Update the previous state
        wasPlayerNearby = isPlayerNearby;
    }

    void CheckPlayerInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(interactionPoint.position, interactionRadius);
        isPlayerNearby = false;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Kieran"))
            {
                isPlayerNearby = true;
                playerInRange = col.gameObject;
                return;
            }
        }

        playerInRange = null;
    }

    public void RemoveItem(int index)
    {
        containerSlots[index].Clear();
    }

    public InventorySlot GetSlot(int index)
    {
        return containerSlots[index];
    }
}
