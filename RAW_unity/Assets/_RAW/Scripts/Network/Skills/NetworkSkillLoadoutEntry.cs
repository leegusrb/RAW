using System;
using Unity.Collections;
using Unity.Netcode;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillLoadoutEntry :
		INetworkSerializable,
		IEquatable<NetworkSkillLoadoutEntry>
	{
		public KeyMapping Slot;
		public FixedString64Bytes SkillId;

		public NetworkSkillLoadoutEntry(KeyMapping slot, string skillId)
		{
			Slot = slot;
			SkillId = new FixedString64Bytes(skillId);
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			int slotValue = (int)Slot;

			serializer.SerializeValue(ref slotValue);
			serializer.SerializeValue(ref SkillId);

			if (serializer.IsReader)
				Slot = (KeyMapping)slotValue;
		}

		public bool Equals(NetworkSkillLoadoutEntry other)
		{
			return Slot == other.Slot && SkillId.Equals(other.SkillId);
		}
	}
}
