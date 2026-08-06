using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkInventoryEntry :
		INetworkSerializable,
		IEquatable<NetworkInventoryEntry>
	{
		public int SlotIndex;
		public FixedString64Bytes ItemId;
		public int Count;

		public NetworkInventoryEntry(int slotIndex, string itemId, int count)
		{
			SlotIndex = slotIndex;
			ItemId = new FixedString64Bytes(itemId);
			Count = count;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SlotIndex);
			serializer.SerializeValue(ref ItemId);
			serializer.SerializeValue(ref Count);
		}

		public bool Equals(NetworkInventoryEntry other)
		{
			return SlotIndex == other.SlotIndex &&
				ItemId.Equals(other.ItemId) &&
				Count == other.Count;
		}
	}

	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkObject))]
	[RequireComponent(typeof(Char_Inventory))]
	public class NetworkInventorySync : NetworkBehaviour
	{
		[SerializeField] private Char_Inventory inventory;

		private readonly NetworkVariable<int> inventoryCapacity =
			new NetworkVariable<int>(
				0,
				NetworkVariableReadPermission.Owner,
				NetworkVariableWritePermission.Server
			);

		private NetworkList<NetworkInventoryEntry> inventoryList;

		private bool isApplyingNetworkState;
		private bool applyQueued;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();

			inventoryList =
				new NetworkList<NetworkInventoryEntry>(
					null,
					NetworkVariableReadPermission.Owner,
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

			inventoryCapacity.OnValueChanged += HandleInventoryCapacityChanged;
			inventoryList.OnListChanged += HandleNetworkInventoryChanged;
			inventory.OnInventoryChanged += HandleLocalInventoryChanged;

			if (IsServer)
			{
				WriteInventoryToNetworkState();
			}
			else if (IsOwner)
			{
				ApplyNetworkStateToInventory();
			}
		}

		public override void OnNetworkDespawn()
		{
			inventoryCapacity.OnValueChanged -= HandleInventoryCapacityChanged;
			inventoryList.OnListChanged -= HandleNetworkInventoryChanged;

			if (inventory != null)
				inventory.OnInventoryChanged -= HandleLocalInventoryChanged;
		}

		private void CacheComponents()
		{
			if (inventory == null)
				inventory = GetComponent<Char_Inventory>();
		}

		private void HandleLocalInventoryChanged()
		{
			if (isApplyingNetworkState)
				return;

			if (IsServer)
			{
				WriteInventoryToNetworkState();
			}
			else if (IsOwner)
			{
				// 소유 클라이언트가 로컬 인벤토리를
				// 임의로 바꾼 경우 서버 상태로 복구합니다.
				applyQueued = true;
			}
		}

		private void HandleInventoryCapacityChanged(int previousValue, int newValue)
		{
			if (IsOwner && !IsServer)
				applyQueued = true;
		}

		private void HandleNetworkInventoryChanged(NetworkListEvent<NetworkInventoryEntry> changeEvent)
		{
			if (IsOwner && !IsServer)
				applyQueued = true;
		}

		private void LateUpdate()
		{
			if (!applyQueued || !IsSpawned || IsServer || !IsOwner)
				return;

			applyQueued = false;

			ApplyNetworkStateToInventory();
		}

		private void WriteInventoryToNetworkState()
		{
			if (!IsServer || inventory == null)
				return;

			inventoryCapacity.Value = inventory.CurrentInventoryCapacity;

			inventoryList.Clear();

			for (int i = 0; i < inventory.CurrentInventoryCapacity; i++)
			{
				InventorySlot slot = inventory.GetInventorySlot(i);

				if (slot == null || slot.IsEmpty)
					continue;

				inventoryList.Add(new NetworkInventoryEntry(i, slot.itemId, slot.count));
			}
		}

		private void ApplyNetworkStateToInventory()
		{
			if (inventory == null)
				return;

			Dictionary<int, InventorySlot> snapshot = new Dictionary<int, InventorySlot>();

			for (int i = 0; i < inventoryList.Count; i++)
			{
				NetworkInventoryEntry entry = inventoryList[i];

				if (entry.SlotIndex < 0 ||
					entry.SlotIndex >= inventoryCapacity.Value ||
					entry.Count <= 0 ||
					entry.ItemId.IsEmpty)
				{
					continue;
				}

				InventorySlot slot = new InventorySlot();

				slot.Set(
					entry.ItemId.ToString(),
					entry.Count
				);

				snapshot[entry.SlotIndex] = slot;
			}

			isApplyingNetworkState = true;

			try
			{
				inventory.ReplaceInventory(
					inventoryCapacity.Value,
					snapshot
				);
			}
			finally
			{
				isApplyingNetworkState = false;
			}
		}

#if UNITY_EDITOR

		[ContextMenu("Test - Set Slot 0 Hair 2")]
		private void TestSetSlot0Hair2()
		{
			if (!CanRunServerTest())
				return;

			bool succeeded = inventory.SetInventorySlot(
				0,
				"Hair_2.png",
				1
			);

			Debug.Log(
				$"서버 인벤토리 테스트: " +
				$"성공={succeeded}, " +
				$"Hair_2 개수={inventory.GetItemCount("Hair_2.png")}",
				this
			);
		}

		[ContextMenu("Test - Clear Slot 0")]
		private void TestClearSlot0()
		{
			if (!CanRunServerTest())
				return;

			inventory.ClearInventorySlot(0);

			Debug.Log(
				"서버 인벤토리 0번 슬롯을 비웠습니다.",
				this
			);
		}

		[ContextMenu("Test - Try Local Inventory Tamper")]
		private void TestTryLocalInventoryTamper()
		{
			if (!Application.isPlaying ||
				!IsSpawned ||
				!IsOwner ||
				IsServer)
			{
				Debug.LogWarning(
					"소유 Client의 NetworkPlayer에서 실행해야 합니다.",
					this
				);

				return;
			}

			inventory.SetInventorySlot(
				0,
				"Invalid_Client_Item.png",
				999
			);

			Debug.Log(
				"로컬 인벤토리 변조를 시도했습니다. " +
				"다음 프레임에 서버 상태로 복구되어야 합니다.",
				this
			);
		}

		private bool CanRunServerTest()
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

