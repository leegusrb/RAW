using System;
using Unity.Collections;
using Unity.Netcode;

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
}
