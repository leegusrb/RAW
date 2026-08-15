using System;
using System.Collections.Generic;

namespace RAW.Persistence
{
	[Serializable]
	public sealed class PlayerInventorySlotData
	{
		public int slotIndex;
		public string itemId;
		public int count;

		public PlayerInventorySlotData DeepCopy()
		{
			return new PlayerInventorySlotData
			{
				slotIndex = slotIndex,
				itemId = itemId,
				count = count
			};
		}
	}

	[Serializable]
	public sealed class PlayerEquipmentSlotData
	{
		public EquipmentSlot slot;
		public string itemId;

		public PlayerEquipmentSlotData DeepCopy()
		{
			return new PlayerEquipmentSlotData
			{
				slot = slot,
				itemId = itemId
			};
		}
	}

	[Serializable]
	public sealed class PlayerSkillSlotData
	{
		public SkillSlot slot;
		public string skillId;

		public PlayerSkillSlotData DeepCopy()
		{
			return new PlayerSkillSlotData
			{
				slot = slot,
				skillId = skillId
			};
		}
	}

	[Serializable]
	public sealed class PlayerPersistentData
	{
		public string userId;

		public int healthPoint = 100;
		public int manaPoint = 100;

		public int inventoryCapacity = 10;

		public List<PlayerInventorySlotData> inventory = new List<PlayerInventorySlotData>();
		public List<PlayerEquipmentSlotData> equipment = new List<PlayerEquipmentSlotData>();
		public List<PlayerSkillSlotData> skillLoadout = new List<PlayerSkillSlotData>();

		public static PlayerPersistentData CreateDefault(string userId)
		{
			return new PlayerPersistentData
			{
				userId = userId,
				healthPoint = 100,
				manaPoint = 100,
				inventoryCapacity = 10
			};
		}

		public PlayerPersistentData DeepCopy()
		{
			PlayerPersistentData copy =
				new PlayerPersistentData
				{
					userId = userId,
					healthPoint = healthPoint,
					manaPoint = manaPoint,
					inventoryCapacity = inventoryCapacity
				};

			if (inventory != null)
			{
				for (int i = 0; i < inventory.Count; i++)
				{
					PlayerInventorySlotData item = inventory[i];

					if (item != null)
						copy.inventory.Add(item.DeepCopy());
				}
			}

			if (equipment != null)
			{
				for (int i = 0; i < equipment.Count; i++)
				{
					PlayerEquipmentSlotData item = equipment[i];

					if (item != null)
						copy.equipment.Add(item.DeepCopy());
				}
			}

			if (skillLoadout != null)
			{
				for (int i = 0; i < skillLoadout.Count; i++)
				{
					PlayerSkillSlotData item = skillLoadout[i];

					if (item != null)
						copy.skillLoadout.Add(item.DeepCopy());
				}
			}

			return copy;
		}
	}
}