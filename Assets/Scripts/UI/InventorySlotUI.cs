using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    public void SetSlot(Item item, int quantity)
    {
        if (item == null)
        {
            itemIcon.enabled = false;
            quantityText.text = "";
        }
        else
        {
            itemIcon.enabled = true;
            itemIcon.sprite = item.icon;
            quantityText.text = item.maxStack > 1 ? quantity.ToString() : "";
        }
    }
}
