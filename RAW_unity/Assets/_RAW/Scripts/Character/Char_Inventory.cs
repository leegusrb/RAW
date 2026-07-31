using System;
using System.Collections.Generic;
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

public class Char_Inventory : MonoBehaviour
{
    [SerializeField]
    private int currentInventoryCapacity = 10;
    public int CurrentInventoryCapacity => currentInventoryCapacity;

    [SerializeField]
    private InventorySlot[] inventorySlots;    
    

    [SerializeField]
    private CustomDictCurrentEquipment equippedItems = new CustomDictCurrentEquipment();
    public CustomDictCurrentEquipment EquippedItems => equippedItems;

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;

	private void Awake()
	{
		EnsureInventoryInitialized();
	}

	private void OnValidate()
	{
		EnsureInventoryInitialized();
	}

	private void EnsureInventoryInitialized()
	{
		currentInventoryCapacity = Mathf.Max(0, currentInventoryCapacity);

		if (inventorySlots == null || inventorySlots.Length != currentInventoryCapacity)
		{
			InventorySlot[] resizedSlots = new InventorySlot[currentInventoryCapacity];

			if (inventorySlots != null)
			{
				int copyCount = Mathf.Min(inventorySlots.Length, resizedSlots.Length);

				for (int i = 0; i < copyCount; i++)
				{
					resizedSlots[i] = inventorySlots[i];
				}
			}

			inventorySlots = resizedSlots;
		}

		for (int i = 0; i < inventorySlots.Length; i++)
		{
			if (inventorySlots[i] == null)
				inventorySlots[i] = new InventorySlot();
		}
	}

    public bool IsSlotUsable(int index)
    {
        return index >= 0 && 
			   index < currentInventoryCapacity &&
			   inventorySlots != null &&
			   index < inventorySlots.Length;
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
		EnsureInventoryInitialized();

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

	public int GetItemCount(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
			return 0;

		EnsureInventoryInitialized();

		int totalCount = 0;

		for (int i = 0; i < currentInventoryCapacity; i++)
		{
			InventorySlot slot = inventorySlots[i];

			if (slot == null || slot.IsEmpty)
				continue;

			if (string.Equals(slot.itemId, itemId, StringComparison.Ordinal))
				totalCount += slot.count;
		}

		return totalCount;
	}

	public bool HasItem(string itemId, int requiredCount = 1)
	{
		if (requiredCount <= 0)
			return false;

		return GetItemCount(itemId) >= requiredCount;
	}

	// 클라이언트 인벤토리를 서버 스냅샷으로 한 번에 교체
	public void ReplaceInventory(int capacity, IReadOnlyDictionary<int, InventorySlot> items)
	{
		currentInventoryCapacity = Mathf.Max(0, capacity);

		EnsureInventoryInitialized();

		for (int i = 0; i < inventorySlots.Length; i++)
		{
			inventorySlots[i].Clear();
		}

		if (items != null)
		{
			foreach (KeyValuePair<int, InventorySlot> pair in items)
			{
				if (!IsSlotUsable(pair.Key))
					continue;

				InventorySlot sourceSlot = pair.Value;

				if (sourceSlot == null || sourceSlot.IsEmpty)
					continue;

				inventorySlots[pair.Key].Set(
					sourceSlot.itemId,
					sourceSlot.count
				);
			}
		}

		OnInventoryChanged?.Invoke();
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
			{
                equippedItems[slot] = itemId;
				equippedItems.SyncInspectorFromDictionary();
			}
            else
                equippedItems.Add(slot, itemId);
        }

        OnEquipmentChanged?.Invoke();
    }

	public void ReplaceEquipment(IReadOnlyDictionary<EquipmentSlot, string> equipment)
	{
		equippedItems.Clear();
		
		if (equipment != null)
		{
			foreach (KeyValuePair<EquipmentSlot, string> pair in equipment)
			{
				if (string.IsNullOrEmpty(pair.Value))
					continue;

				equippedItems[pair.Key] = pair.Value;
			}
		}

		equippedItems.SyncInspectorFromDictionary();
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
