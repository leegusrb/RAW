using System;
using CustomDict;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public string itemId;
    public int count;

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || count <= 0;

    public void Clear()
    {
        itemId = null;
        count = 0;
    }

    public void Set(string id, int amount)
    {
        itemId = id;
        count = Mathf.Max(0, amount);
    }
}

public class CharacterInventory : MonoBehaviour
{
	[SerializeField] private InventoryConfig inventoryConfig;
    [SerializeField] private int currentInventoryCapacity = 10;

    public int CurrentInventoryCapacity => currentInventoryCapacity;

	public int MaxInventotyCapacity
	{
		get
		{
			if (inventoryConfig == null)
				return currentInventoryCapacity;

			return inventoryConfig.MaxInventotyCapacity;
		}
	}

    [SerializeField]
    private InventorySlot[] inventorySlots;    
    

    [SerializeField]
    private CustomDictCurrentEquipment equippedItems = new CustomDictCurrentEquipment();
    public CustomDictCurrentEquipment EquippedItems => equippedItems;

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;

	private void Awake()
	{
		ClampInventoryCapacity();
	}

	#if UNITY_EDITOR
	private void OnValidate()
	{
		ClampInventoryCapacity();
	}
	#endif

	private void ClampInventoryCapacity()
	{
		if (inventoryConfig == null)
			return;

		currentInventoryCapacity = Mathf.Clamp(
			currentInventoryCapacity,
			0,
			inventoryConfig.MaxInventotyCapacity
		);
	}

	public bool IsSlotUsable(int index)
    {
        return index >= 0 && index < currentInventoryCapacity;
    }

    public InventorySlot GetInventorySlot(int index)
    {
        if (inventorySlots == null || index < 0 || index >= inventorySlots.Length)
            return null;
        return inventorySlots[index];
    }

    public bool IsInventorySlotEmpty(int index)
    {
        var slot = GetInventorySlot(index);
        return slot == null || slot.IsEmpty;
    }

    public bool SetInventorySlot(int index, string itemId, int count)
    {
        if (!IsSlotUsable(index))
            return false;

        var slot = GetInventorySlot(index);
        if (slot == null)
            return false;

        if (string.IsNullOrEmpty(itemId) || count <= 0)
            slot.Clear();
        else
            slot.Set(itemId, count);

        OnInventoryChanged?.Invoke();
        return true;
    }

    public void ClearInventorySlot(int index)
    {
        SetInventorySlot(index, null, 0);
    }

    public bool TryGetEquippedItemId(EquipmentSlot slot, out string itemId)
    {
        if (equippedItems.TryGetValue(slot, out itemId) && !string.IsNullOrEmpty(itemId))
            return true;

        itemId = null;
        return false;
    }

    public void SetEquipped(EquipmentSlot slot, string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            if (equippedItems.ContainsKey(slot))
                equippedItems.Remove(slot);
        }
        else
        {
            if (equippedItems.ContainsKey(slot))
                equippedItems[slot] = itemId;
            else
                equippedItems.Add(slot, itemId);
        }

        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipmentSlot slot)
    {
        SetEquipped(slot, null);
    }

    public void ClearAllEquipment()
    {
        equippedItems.Clear();
        equippedItems.SyncInspectorFromDictionary();
        OnEquipmentChanged?.Invoke();
    }
}
