using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Consumable,
    KeyItem,
    OverrideChip,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public ItemType type;
    public int maxStack = 1; // >1 for consumables like darts
}

[CreateAssetMenu(fileName = "OverrideChip", menuName = "Items/Override Chip")]
public class OverrideChipData : Item
{
    public int level;  // Level 1, 2, or 3
}