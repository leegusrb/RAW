using System;
using Unity.Collections;
using Unity.Netcode;

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
}
