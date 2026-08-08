using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentCatalogEntry
{
	[SerializeField] private string itemId;
	[SerializeField] private EquipmentSlot slot;

	public string ItemId => itemId;
	public EquipmentSlot Slot => slot;
}

[CreateAssetMenu(fileName = "EquipmentCatalog", menuName = "RAW/Data/Equipment Catalog")]
public class EquipmentCatalog : ScriptableObject
{
    [SerializeField]
	private List<EquipmentCatalogEntry> entries = new List<EquipmentCatalogEntry>();

	public bool TryGetSlot(string itemId, out EquipmentSlot slot)
	{
		if (!string.IsNullOrEmpty(itemId))
		{
			for (int i = 0; i < entries.Count; i++)
			{
				EquipmentCatalogEntry entry = entries[i];

				if (entry == null)
					continue;

				if (string.Equals(entry.ItemId, itemId, StringComparison.Ordinal))
				{
					slot = entry.Slot;
					return true;
				}
			}
		}

		slot = default;
		return false;
	}

	public bool IsValidForSlot(string itemId, EquipmentSlot requestedSlot)
	{
		if (!TryGetSlot(itemId, out EquipmentSlot registeredSlot))
			return false;

		return registeredSlot == requestedSlot;
	}

	private void OnValidate()
	{
		HashSet<string> knownItemIds = new HashSet<string>(StringComparer.Ordinal);

		for (int i = 0; i < entries.Count; i++)
		{
			EquipmentCatalogEntry entry = entries[i];

			if (entry == null)
			{
				Debug.LogError($"EquipmentCatalog의 {i}번 항목이 비어 있습니다.", this);

				continue;
			}

			if (string.IsNullOrEmpty(entry.ItemId))
			{
				Debug.LogError($"EquipmentCatalog의 {i}번 Item ID가 비어 있습니다.", this);

				continue;
			}

			if (!knownItemIds.Add(entry.ItemId))
			{
				Debug.LogError($"중복된 장비 ID가 있습니다: {entry.ItemId}", this);
			}
		}
	}
}
