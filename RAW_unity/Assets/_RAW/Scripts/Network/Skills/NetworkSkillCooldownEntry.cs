using System;
using Unity.Collections;
using Unity.Netcode;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillCooldownEntry :
		INetworkSerializable,
		IEquatable<NetworkSkillCooldownEntry>
	{
		public FixedString64Bytes SkillId;
		public double CooldownEndTime;

		public NetworkSkillCooldownEntry(string skillId, double cooldownEndTime)
		{
			SkillId = new FixedString64Bytes(skillId);
			CooldownEndTime = cooldownEndTime;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref CooldownEndTime);
		}

		public bool Equals(NetworkSkillCooldownEntry other)
		{
			return SkillId.Equals(other.SkillId) &&
				CooldownEndTime.Equals(other.CooldownEndTime);
		}
	}
}
