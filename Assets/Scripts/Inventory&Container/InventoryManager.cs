using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    OPCounter opCounter;
    public GameObject opc;

    public InventorySlot[] inventorySlots = new InventorySlot[5];

    // Initiating items
    public Item sleepingDartItem;

    private void Awake()
    {
        opCounter = opc.GetComponent<OPCounter>();

        if (Instance == null) Instance = this;

        // Initialize empty slots
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i] = new InventorySlot();
        }
    }

    private void Start()
    {
        initiateItem();
    }

    // Add item to inventory
    public bool AddItem(Item newItem, int amount = 1)
    {
        // If item is an Override Chip, apply OP and do not store
        if (newItem.type == ItemType.OverrideChip && newItem is OverrideChipData chip)
        {
            int opAmount = 0;

            switch (chip.level)
            {
                case 1: opAmount = 100; break;
                case 2: opAmount = 300; break;
                case 3: opAmount = 600; break;
            }

            Z3raHackingManager hackingManager = FindObjectOfType<Z3raHackingManager>();
            if (hackingManager != null)
            {
                hackingManager.AddOverridePoints(opAmount);
                opCounter.counterUpdate();
            }
            else
            {
                Debug.LogWarning("Z3raHackingManager not found in scene.");
            }

            return true; // Successfully 'used' chip
        }

        // Regular logic for darts, keycards, etc.
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].item == newItem && newItem.maxStack > 1)
            {
                inventorySlots[i].quantity += amount;
                if (InventoryUI.Instance != null)
                {
                    InventoryUI.Instance.RefreshUI();
                }

                return true;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                inventorySlots[i].item = newItem;
                inventorySlots[i].quantity = amount;
                if (InventoryUI.Instance != null)
                {
                    InventoryUI.Instance.RefreshUI();
                }
                return true;
            }
        }

        Debug.Log("Inventory Full!");
        return false;
    }

    // Use item by slot index (called from input)
    public void UseItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventorySlots.Length) return;

        InventorySlot slot = inventorySlots[slotIndex];
        if (slot.IsEmpty) return;

        Debug.Log($"Used: {slot.item.itemName}");

        // Handle item effect here (custom per item)
        if (slot.item.type == ItemType.Consumable)
        {
            slot.quantity--;
            if (slot.quantity <= 0)
                slot.Clear();

            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.RefreshUI();
            }
        }

        // For KeyItem, it stays (or triggers scene logic)
    }

    public InventorySlot GetSlot(int index)
    {
        return inventorySlots[index];
    }

    private void initiateItem()
    {
        if (sleepingDartItem != null)
        {
            AddItem(sleepingDartItem, 5); // Add 5 darts on start

            if (InventoryUI.Instance != null)
            {
                InventoryUI.Instance.RefreshUI();
                Debug.Log("Refresh UI");
            }
        }
        else
        {
            Debug.LogWarning("Sleeping Dart Item not assigned in InventoryManager.");
        }
    }
}
