using RAW.Persistence;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
	[RequireComponent(typeof(NetworkCharacterState))]
	[RequireComponent(typeof(NetworkInventorySync))]
	[RequireComponent(typeof(NetworkEquipmentSync))]
	public sealed class NetworkPlayerPersistenceAdapter : MonoBehaviour
	{
		[SerializeField] private NetworkCharacterState characterState;
		[SerializeField] private NetworkInventorySync inventorySync;
		[SerializeField] private NetworkEquipmentSync equipmentSync;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();
		}

		public bool InitializePersistentStateOnServer(
			PlayerPersistentData playerData,
			out string error
		)
		{
			CacheComponents();

			if (!PlayerPersistentDataValidator.TryValidate(
				playerData,
				out error
			))
			{
				return false;
			}

			if (characterState == null ||
				inventorySync == null ||
				equipmentSync == null)
			{
				error = "영속 상태 적용에 필요한 컴포넌트가 없습니다.";
				return false;
			}

			if (!characterState.InitializePersistentStateOnServer(
				playerData.healthPoint,
				playerData.manaPoint
			))
			{
				error = "HP/MP 적용에 실패했습니다.";
				return false;
			}

			if (!inventorySync.InitializePersistentStateOnServer(
				playerData.inventoryCapacity,
				playerData.inventory
			))
			{
				error = "인벤토리 적용에 실패했습니다.";
				return false;
			}

			if (!equipmentSync.InitializePersistentStateOnServer(
				playerData.equipment
			))
			{
				error = "장비 적용에 실패했습니다.";
				return false;
			}

			error = null;
			return true;
		}

		private void CacheComponents()
		{
			if (characterState == null)
				characterState = GetComponent<NetworkCharacterState>();

			if (inventorySync == null)
				inventorySync = GetComponent<NetworkInventorySync>();

			if (equipmentSync == null)
				equipmentSync = GetComponent<NetworkEquipmentSync>();
		}
	}
}
