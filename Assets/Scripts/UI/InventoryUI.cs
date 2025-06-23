using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public InventorySlotUI[] slotUIs;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            InventorySlot invSlot = InventoryManager.Instance.GetSlot(i);
            slotUIs[i].SetSlot(invSlot.item, invSlot.quantity);
        }
    }
}
