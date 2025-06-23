using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<InventorySlot> containerSlots = new List<InventorySlot>(4); // 4 slots
    public Transform interactionPoint;
    public float interactionRadius = 1.5f;

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

        if (isPlayerNearby && Input.GetKeyDown(KeyCode.R))
        {
            bool isKieran = playerInRange.CompareTag("Kieran");
            Debug.Log(isKieran);
            ContainerUI.Instance.OpenContainer(this, isKieran);
        }
        else if(!isPlayerNearby)
        {
            ContainerUI.Instance.CloseContainer();
        }
    }

    void CheckPlayerInRange()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(interactionPoint.position, interactionRadius);
        isPlayerNearby = false;

        foreach (var col in colliders)
        {
            if (col.CompareTag("Kieran") || col.CompareTag("DASH"))
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
