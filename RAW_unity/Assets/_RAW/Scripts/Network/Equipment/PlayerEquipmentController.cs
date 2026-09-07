using UnityEngine;

namespace RAW.Network
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CharacterInventory))]
	[RequireComponent(typeof(NetworkEquipmentSync))]
	public class PlayerEquipmentController : MonoBehaviour
	{
		[SerializeField] private CharacterInventory inventory;
		[SerializeField] private NetworkEquipmentSync networkEquipmentSync;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();
		}

		private void CacheComponents()
		{
			if (inventory == null)
				inventory = GetComponent<CharacterInventory>();

			if (networkEquipmentSync == null)
				networkEquipmentSync = GetComponent<NetworkEquipmentSync>();
		}

		public bool TryRequestEquipInventorySlot(int inventorySlotIndex)
		{
			if (!CanSendOwnerRequest())
				return false;

			if (!inventory.IsSlotUsable(inventorySlotIndex))
			{
				Debug.LogWarning($"사용할 수 없는 인벤토리 슬롯입니다: {inventorySlotIndex}", this);
				return false;
			}

			InventorySlot inventorySlot = inventory.GetInventorySlot(inventorySlotIndex);

			if (inventorySlot == null || inventorySlot.IsEmpty)
			{
				Debug.LogWarning($"비어 있는 인벤토리 슬롯입니다: {inventorySlotIndex}", this);
				return false;
			}

			if (!networkEquipmentSync.TryGetRegisteredSlot(inventorySlot.itemId, out EquipmentSlot equipmentSlot))
			{
				Debug.LogWarning($"장비 카탈로그에 등록되지 않은 아이템입니다: {inventorySlot.itemId}", this);
				return false;
			}

			networkEquipmentSync.RequestEquip(equipmentSlot, inventorySlot.itemId);

			return true;
		}

		public bool TryRequestUnequip(EquipmentSlot equipmentSlot)
		{
			if (!CanSendOwnerRequest())
				return false;

			if (!inventory.TryGetEquippedItemId(equipmentSlot, out string currentItemId))
			{
				Debug.LogWarning($"비어 있는 장비 슬롯입니다: {equipmentSlot}", this);
				return false;
			}

			if (!networkEquipmentSync.TryGetRegisteredSlot(currentItemId, out EquipmentSlot registeredSlot))
			{
				Debug.LogWarning($"장비 카탈로그에 등록되지 않은 아이템입니다: {currentItemId}", this);
				return false;
			}

			if (registeredSlot != equipmentSlot)
			{
				Debug.LogWarning($"현재 장비와 슬롯이 일치하지 않습니다. Slot={equipmentSlot}, ItemId={currentItemId}", this);

				return false;
			}

			networkEquipmentSync.RequestUnequip(equipmentSlot);

			return true;
		}

		private bool CanSendOwnerRequest()
		{
			if (inventory == null || networkEquipmentSync == null)
			{
				Debug.LogError("장비 조작에 필요한 컴포넌트가 없습니다.", this);
				return false;
			}

			if (!networkEquipmentSync.IsSpawned)
			{
				Debug.LogWarning("NetworkPlayer가 아직 Spawn되지 않았습니다.", this);
				return false;
			}

			if (!networkEquipmentSync.IsOwner)
			{
				Debug.LogWarning("자신이 소유한 캐릭터에서만 장비를 조작할 수 있습니다.", this);
				return false;
			}

			return true;
		}

#if UNITY_EDITOR

		[ContextMenu("Test - Equip Inventory Slot 0")]
		private void TestEquipInventorySlot0()
		{
			bool requestSent =
				TryRequestEquipInventorySlot(0);

			Debug.Log(
				$"0번 인벤토리 슬롯 장착 요청: " +
				$"{requestSent}",
				this
			);
		}

		[ContextMenu("Test - Unequip Hair")]
		private void TestUnequipHair()
		{
			bool requestSent =
				TryRequestUnequip(EquipmentSlot.Hair);

			Debug.Log(
				$"Hair 슬롯 해제 요청: {requestSent}",
				this
			);
		}

#endif
	}
}

