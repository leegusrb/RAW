using System;
using System.Collections.Generic;

namespace RAW.Persistence
{
	public static class PlayerPersistentDataValidator
	{
		public static bool TryValidate(
			PlayerPersistentData playerData,
			out string error
		)
		{
			if (playerData == null)
			{
				error = "플레이어 데이터가 없습니다.";
				return false;
			}

			if (string.IsNullOrWhiteSpace(playerData.userId))
			{
				error = "플레이어 데이터의 UserId가 비어 있습니다.";
				return false;
			}

			if (playerData.healthPoint < 0)
			{
				error = "HP가 음수입니다.";
				return false;
			}

			if (playerData.manaPoint < 0)
			{
				error = "MP가 음수입니다.";
				return false;
			}

			if (playerData.inventoryCapacity < 0)
			{
				error = "인벤토리 용량이 음수입니다.";
				return false;
			}

			HashSet<int> inventorySlotIndices = new();
			HashSet<string> inventoryItemIds = new(StringComparer.Ordinal);

			if (playerData.inventory != null)
			{
				for (int i = 0; i < playerData.inventory.Count; i++)
				{
					PlayerInventorySlotData slot = playerData.inventory[i];

					if (slot == null)
					{
						error = $"인벤토리 {i}번 데이터가 null입니다.";
						return false;
					}

					if (slot.slotIndex < 0 || slot.slotIndex >= playerData.inventoryCapacity)
					{
						error = $"인벤토리 슬롯 범위가 잘못되었습니다: {slot.slotIndex}";
						return false;
					}

					if (!inventorySlotIndices.Add(slot.slotIndex))
					{
						error = $"인벤토리 슬롯이 중복되었습니다: {slot.slotIndex}";
						return false;
					}

					if (string.IsNullOrWhiteSpace(slot.itemId))
					{
						error = $"인벤토리 {slot.slotIndex}번 ItemId가 비어있습니다.";
						return false;
					}

					if (slot.count <= 0)
					{
						error = $"인벤토리 아이템 수량이 올바르지 않습니다: {slot.count}";
						return false;
					}

					inventoryItemIds.Add(slot.itemId);
				}
			}

			HashSet<EquipmentSlot> equipmentSlots = new();

			if (playerData.equipment != null)
			{
				for (int i = 0; i < playerData.equipment.Count; i++)
				{
					PlayerEquipmentSlotData equipment = playerData.equipment[i];

					if (equipment == null)
					{
						error = $"장비 {i}번 데이터가 null입니다.";
						return false;
					}

					if (!Enum.IsDefined(typeof(EquipmentSlot), equipment.slot))
					{
						error = $"유효하지 않은 장비 슬롯입니다: {equipment.slot}";
						return false;
					}

					if (!equipmentSlots.Add(equipment.slot))
					{
						error = $"장비 슬롯이 중복되었습니다: {equipment.slot}";
						return false;
					}

					if (string.IsNullOrWhiteSpace(equipment.itemId))
					{
						error = $"장비 ItemId가 비어 있습니다: {equipment.slot}";
						return false;
					}

					if (!inventoryItemIds.Contains(equipment.itemId))
					{
						error = $"인벤토리에 없는 장비입니다. Slot={equipment.slot}, ItemId={equipment.itemId}";
						return false;
					}
				}
			}

			error = null;
			return true;
		}
	}
}
