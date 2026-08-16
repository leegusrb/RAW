using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[RequireComponent(typeof(NetworkObject))]
	[RequireComponent(typeof(Char_Inventory))]
	public class NetworkEquipmentSync : NetworkBehaviour
	{
		[SerializeField] private Char_Inventory inventory;
		[SerializeField] private EquipmentCatalog equipmentCatalog;

		private NetworkList<NetworkEquipmentEntry> equipmentList;

		private bool isApplyingNetworkState;
		private bool applyQueued;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();

			equipmentList = 
				new NetworkList<NetworkEquipmentEntry>(
					null,
					NetworkVariableReadPermission.Everyone,
					NetworkVariableWritePermission.Server
				);
		}

		public override void OnNetworkSpawn()
		{
			if (inventory == null)
			{
				Debug.LogError("Char_Inventory를 찾을 수 없습니다.", this);

				enabled = false;
				return;
			}

			if (equipmentCatalog == null)
			{
				Debug.LogError("EquipmentCatalog가 연결되지 않았습니다.", this);

				enabled = false;
				return;
			}

			equipmentList.OnListChanged += HandleNetworkEquipmentChanged;
			inventory.OnEquipmentChanged += HandleLocalEquipmentChanged;

			if (IsServer)
				WriteEquipmentToNetworkState();
			else
				ApplyNetworkStateToEquipment();
		}

		public override void OnNetworkDespawn()
		{
			equipmentList.OnListChanged -= HandleNetworkEquipmentChanged;

			if (inventory != null)
				inventory.OnEquipmentChanged -= HandleLocalEquipmentChanged;
		}

		private void CacheComponents()
		{
			if (inventory == null)
				inventory = GetComponent<Char_Inventory>();
		}

		public bool TryGetRegisteredSlot(string itemId, out EquipmentSlot equipmentSlot)
		{
			if (equipmentCatalog == null)
			{
				equipmentSlot = default;
				return false;
			}

			return equipmentCatalog.TryGetSlot(itemId, out equipmentSlot);
		}

		public void RequestEquip(EquipmentSlot requestedSlot, string itemId)
		{
			if (!IsSpawned)
			{
				Debug.LogWarning("NetworkObject가 Spawn되지 않아 장비를 요청할 수 없습니다.", this);
				return;
			}

			if (!IsOwner)
			{
				Debug.LogWarning("자신이 소유한 캐릭터의 장비만 변경할 수 있습니다.", this);
				return;
			}

			if (string.IsNullOrEmpty(itemId))
			{
				Debug.LogWarning("장비 Item ID가 비어 있습니다.", this);
				return;
			}

			// FixedString64Bytes가 안전하게 저장할 수 있도록 제한합니다.
			// 현재 장비 ID는 영문, 숫자, 기호만 사용합니다.
			if (itemId.Length > 60)
			{
				Debug.LogWarning($"장비 Item ID가 너무 깁니다: {itemId.Length}", this);
				return;
			}

			if (IsServer)
			{
				TryEquipOnServer(requestedSlot, itemId);
			}
			else
			{
				RequestEquipRpc(requestedSlot, new FixedString64Bytes(itemId));
			}
		}

		public void RequestUnequip(EquipmentSlot requestedSlot)
		{
			if (!IsSpawned)
			{
				Debug.LogWarning("NetworkObject가 Spawn되지 않아 장비 해제를 요청할 수 없습니다.", this);
				return;
			}

			if (!IsOwner)
			{
				Debug.LogWarning("자신이 소유한 캐릭터의 장비만 해제할 수 있습니다.", this);
				return;
			}

			if (IsServer)
			{
				TryUnequipOnServer(requestedSlot);
			}
			else
			{
				RequestUnequipRpc(requestedSlot);
			}
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestEquipRpc(EquipmentSlot requestedSlot, FixedString64Bytes itemId)
		{
			TryEquipOnServer(requestedSlot, itemId.ToString());
		}

		[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
		private void RequestUnequipRpc(EquipmentSlot requestedSlot)
		{
			TryUnequipOnServer(requestedSlot);
		}

		private bool TryEquipOnServer(EquipmentSlot requestedSlot, string itemId)
		{
			if (!IsServer)
				return false;

			if (inventory == null || equipmentCatalog == null)
				return false;

			if (string.IsNullOrEmpty(itemId))
				return false;

			// 서버 카탈로그에 등록된 장비인지,
			// 해당 슬롯에 들어갈 수 있는 장비인지 확인합니다.
			if (!equipmentCatalog.IsValidForSlot(itemId, requestedSlot))
			{
				Debug.LogWarning($"장비 요청 거절: 등록되지 않았거나 슬롯이 일치하지 않습니다. OwnerClientId={OwnerClientId}, Slot={requestedSlot}, ItemId={itemId}", this);
				return false;
			}

			// 이미 같은 장비를 착용 중이면 성공한 것으로 처리하되
			// 네트워크 목록을 다시 작성하지 않습니다.
			if (inventory.TryGetEquippedItemId(requestedSlot, out string currentItemId) && string.Equals(currentItemId, itemId, StringComparison.Ordinal))
			{
				return true;
			}

			if (!inventory.HasItem(itemId))
			{
				Debug.LogWarning($"장비 요청 거절: 인벤토리에 없는 아이템입니다. OwnerClientId={OwnerClientId}, ItemId={itemId}", this);
				return false;
			}

			inventory.SetEquipped(requestedSlot, itemId);

			Debug.Log($"장비 요청 승인: OwnerClientId={OwnerClientId}, Slot={requestedSlot}, ItemId={itemId}", this);

			return true;
		}

		private bool TryUnequipOnServer(EquipmentSlot requestedSlot)
		{
			if (!IsServer)
				return false;

			if (inventory == null || equipmentCatalog == null)
				return false;

			if (!inventory.TryGetEquippedItemId(requestedSlot, out string currentItemId))
			{
				// 이미 비어있는 슬롯은 성공으로 취급합니다.
				return true;
			}

			if (!equipmentCatalog.IsValidForSlot(currentItemId, requestedSlot))
			{
				Debug.LogWarning($"장비 해제 요청 거절: 현재 장비와 슬롯 정보가 올바르지 않습니다. OwnerClientId={OwnerClientId}, Slot={requestedSlot}, ItemId={currentItemId}", this);
				return false;
			}

			inventory.Unequip(requestedSlot);

			Debug.Log($"장비 해제 승인: OwnerClientId={OwnerClientId}, Slot={requestedSlot}", this);

			return true;
		}

		private void HandleLocalEquipmentChanged()
		{
			if (isApplyingNetworkState)
				return;

			if (IsServer)
			{
				WriteEquipmentToNetworkState();
			}
			else
			{
				// Client가 로컬 상태를 임의로 바꿨다면
				// 다음 프레임에 서버 상태로 복구합니다.
				applyQueued = true;
			}
		}

		private void WriteEquipmentToNetworkState()
		{
			if (!IsServer || inventory == null)
				return;

			equipmentList.Clear();

			foreach (KeyValuePair<EquipmentSlot, string> pair in inventory.EquippedItems)
			{
				if (string.IsNullOrEmpty(pair.Value))
					continue;

				if (!equipmentCatalog.IsValidForSlot(pair.Value, pair.Key))
				{
					Debug.LogWarning($"등록되지 않았거나 슬롯이 잘못된 장비입니다. Slot={pair.Key}. ItemId={pair.Value}", this);

					continue;
				}

				equipmentList.Add(
					new NetworkEquipmentEntry(
						pair.Key,
						pair.Value
					)
				);
			}
		}

		private void HandleNetworkEquipmentChanged(NetworkListEvent<NetworkEquipmentEntry> changeEvent)
		{
			if (!IsServer)
				applyQueued = true;
		}

		private void LateUpdate()
		{
			if (!applyQueued || !IsSpawned || IsServer)
				return;

			applyQueued = false;
			ApplyNetworkStateToEquipment();
		}

		private void ApplyNetworkStateToEquipment()
		{
			if (inventory == null)
				return;

			Dictionary<EquipmentSlot, string> snapshot = new Dictionary<EquipmentSlot, string>();

			for (int i = 0; i < equipmentList.Count; i++)
			{
				NetworkEquipmentEntry entry = equipmentList[i];

				snapshot[entry.Slot] = entry.ItemId.ToString();
			}

			isApplyingNetworkState = true;

			try
			{
				inventory.ReplaceEquipment(snapshot);
			}
			finally
			{
				isApplyingNetworkState = false;
			}
		}
		
		#if UNITY_EDITOR

		[ContextMenu("Test - Request Equip Hair 2")]
		private void TestRequestEquipHair2()
		{
			if (!CanRunOwnerRequestTest())
				return;

			RequestEquip(
				EquipmentSlot.Hair,
				"Hair_2.png"
			);
		}

		[ContextMenu("Test - Request Hair 2 As Armor")]
		private void TestRequestHair2AsArmor()
		{
			if (!CanRunOwnerRequestTest())
				return;

			RequestEquip(
				EquipmentSlot.Armor,
				"Hair_2.png"
			);
		}

		[ContextMenu("Test - Request Invalid Equipment")]
		private void TestRequestInvalidEquipment()
		{
			if (!CanRunOwnerRequestTest())
				return;

			RequestEquip(
				EquipmentSlot.Hair,
				"Invalid_Item.png"
			);
		}

		[ContextMenu("Test - Request Unequip Hair")]
		private void TestRequestUnequipHair()
		{
			if (!CanRunOwnerRequestTest())
				return;

			RequestUnequip(EquipmentSlot.Hair);
		}

		private bool CanRunOwnerRequestTest()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning(
					"Play Mode에서만 테스트할 수 있습니다.",
					this
				);

				return false;
			}

			if (!IsSpawned)
			{
				Debug.LogWarning(
					"NetworkObject가 Spawn되지 않았습니다.",
					this
				);

				return false;
			}

			if (!IsOwner)
			{
				Debug.LogWarning(
					"소유 중인 NetworkPlayer에서 실행해야 합니다.",
					this
				);

				return false;
			}

			return inventory != null &&
				equipmentCatalog != null;
		}

		#endif
	}
}
