using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkEquipmentEntry :
		INetworkSerializable,
		IEquatable<NetworkEquipmentEntry>
	{
		public EquipmentSlot Slot;
		public FixedString64Bytes ItemId;

		public NetworkEquipmentEntry(EquipmentSlot slot, string itemId)
		{
			Slot = slot;
			ItemId = new FixedString64Bytes(itemId);
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			int slotValue = (int)Slot;

			serializer.SerializeValue(ref slotValue);
			serializer.SerializeValue(ref ItemId);

			if (serializer.IsReader)
				Slot = (EquipmentSlot)slotValue;
		}

		public bool Equals(NetworkEquipmentEntry other)
		{
			return Slot == other.Slot && ItemId.Equals(other.ItemId);
		}
	}

	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
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
				WriteInventoryToNetworkList();
			else
				ApplyNetworkListToInventory();
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

		private void HandleLocalEquipmentChanged()
		{
			if (isApplyingNetworkState)
				return;

			if (IsServer)
			{
				WriteInventoryToNetworkList();
			}
			else
			{
				// Client가 로컬 상태를 임의로 바꿨다면
				// 다음 프레임에 서버 상태로 복구합니다.
				applyQueued = true;
			}
		}

		private void WriteInventoryToNetworkList()
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
			ApplyNetworkListToInventory();
		}

		private void ApplyNetworkListToInventory()
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

		[ContextMenu("Test - Equip Hair 1")]
		private void TestEquipHair1()
		{
			if (!CanRunContextTest())
				return;

			inventory.SetEquipped(
				EquipmentSlot.Hair,
				"Hair_1.png"
			);
		}

		[ContextMenu("Test - Equip Hair 2")]
		private void TestEquipHair2()
		{
			if (!CanRunContextTest())
				return;

			inventory.SetEquipped(
				EquipmentSlot.Hair,
				"Hair_2.png"
			);
		}

		[ContextMenu("Test - Unequip Hair")]
		private void TestUnequipHair()
		{
			if (!CanRunContextTest())
				return;

			inventory.Unequip(EquipmentSlot.Hair);
		}

		[ContextMenu("Test - Equip Armor 1")]
		private void TestEquipArmor1()
		{
			if (!CanRunContextTest())
				return;

			inventory.SetEquipped(
				EquipmentSlot.Armor,
				"Armor_1.png"
			);
		}

		[ContextMenu("Test - Clear Equipment")]
		private void TestClearEquipment()
		{
			if (!CanRunContextTest())
				return;

			inventory.ClearAllEquipment();
		}

		private bool CanRunContextTest()
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning(
					"Play Mode에서만 실행할 수 있습니다.",
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

			if (!IsServer)
			{
				Debug.LogWarning(
					"서버 또는 Host에서만 실행할 수 있습니다.",
					this
				);

				return false;
			}

			return inventory != null;
		}

		#endif
	}
}
