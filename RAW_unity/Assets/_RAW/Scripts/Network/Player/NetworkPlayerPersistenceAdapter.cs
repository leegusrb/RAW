using System;
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
	public sealed class NetworkPlayerPersistenceAdapter : NetworkBehaviour
	{
		[SerializeField] private NetworkCharacterState characterState;
		[SerializeField] private NetworkInventorySync inventorySync;
		[SerializeField] private NetworkEquipmentSync equipmentSync;

		private PlayerPersistentData persistentDataBaseline;

		public event Action<ulong, PlayerPersistentData> PersistentStateCaptureBeforeDespawn;
		public event Action<ulong, string> PersistentStateCaptureFailedBeforeDespawn;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();
		}

		public override void OnNetworkPreDespawn()
		{
			if (!IsServer)
				return;

			if (persistentDataBaseline == null)
			{
				PersistentStateCaptureFailedBeforeDespawn?.Invoke(
					OwnerClientId,
					"스냅샷의 기준이 될 플레이어 데이터가 없습니다."
				);

				return;
			}

			if (!TryCapturePersistentStateOnServer(
				persistentDataBaseline,
				out PlayerPersistentData snapshot,
				out string error
			))
			{
				PersistentStateCaptureFailedBeforeDespawn?.Invoke(
					OwnerClientId,
					error
				);

				return;
			}

			persistentDataBaseline = snapshot.DeepCopy();

			PersistentStateCaptureBeforeDespawn?.Invoke(
				OwnerClientId,
				snapshot
			);
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

			persistentDataBaseline = playerData.DeepCopy();

			error = null;
			return true;
		}

		public bool TryCapturePersistentStateOnServer(
			PlayerPersistentData existingData,
			out PlayerPersistentData snapshot,
			out string error
		)
		{
			snapshot = null;

			CacheComponents();

			if (existingData == null)
			{
				error = "스냅샷의 기준이 될 플레이어 데이터가 없습니다.";
				return false;
			}

			if (characterState == null ||
				inventorySync == null ||
				equipmentSync == null)
			{
				error = "영속 상태 추출에 필요한 컴포넌트가 없습니다.";
				return false;
			}

			PlayerPersistentData candidate = existingData.DeepCopy();

			if (!characterState.TryWritePersistentStateOnServer(candidate))
			{
				error = "HP/MP 상태 추출에 실패했습니다.";
				return false;
			}

			if (!inventorySync.TryWritePersistentStateOnServer(candidate))
			{
				error = "인벤토리 상태 추출에 실패했습니다.";
				return false;
			}

			if (!equipmentSync.TryWritePersistentStateOnServer(candidate))
			{
				error = "장비 상태 추출에 실패했습니다.";
				return false;
			}

			if (!PlayerPersistentDataValidator.TryValidate(candidate, out error))
			{
				return false;
			}

			snapshot = candidate;
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
