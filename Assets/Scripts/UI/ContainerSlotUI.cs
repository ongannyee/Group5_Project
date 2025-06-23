using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContainerSlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public Button takeButton;

    private int index;

    public void SetSlot(InventorySlot slot, bool canTake, int slotIndex)
    {
        index = slotIndex;

        if (slot.IsEmpty)
        {
            itemIcon.enabled = false;
            quantityText.text = "";
            takeButton.interactable = false;
        }
        else
        {
            itemIcon.enabled = true;
            itemIcon.sprite = slot.item.icon;
            quantityText.text = slot.item.maxStack > 1 ? slot.quantity.ToString() : "";
            takeButton.interactable = canTake;
        }
    }

    public void OnTakePressed()
    {
        ContainerUI.Instance.OnTakeButtonClicked(index);
        Debug.Log("Get Item.");
    }
}
