using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerUI : MonoBehaviour
{
    public static ContainerUI Instance;

    public GameObject containerPanel;                      // Root UI panel (enable/disable)
    public List<ContainerSlotUI> slotUIs;                  // Drag/drop 5 UI slot objects here

    private Container currentContainer;
    private bool isKieran;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        containerPanel.SetActive(false);                   // Hide UI initially
    }

    public void OpenContainer(Container container, bool isKieranPlayer)
    {
        currentContainer = container;
        isKieran = isKieranPlayer;

        containerPanel.SetActive(true);
        UpdateUI();
    }

    public void CloseContainer()
    {
        containerPanel.SetActive(false);
        currentContainer = null;
    }

    public void UpdateUI()
    {
        for (int i = 0; i < slotUIs.Count; i++)
        {
            InventorySlot dataSlot = currentContainer.GetSlot(i);
            slotUIs[i].SetSlot(dataSlot, isKieran, i);
        }
    }

    public void OnTakeButtonClicked(int index)
    {
        if (!isKieran || currentContainer == null) return;

        InventorySlot slot = currentContainer.GetSlot(index);
        if (slot.IsEmpty) return;

        if (InventoryManager.Instance.AddItem(slot.item, slot.quantity))
        {
            Debug.Log($"Taken: {slot.item.itemName}");
            currentContainer.RemoveItem(index);         
            UpdateUI();
            InventoryUI.Instance.RefreshUI();           
        }
    }
}
