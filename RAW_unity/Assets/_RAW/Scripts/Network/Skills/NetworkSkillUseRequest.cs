using System;
using Unity.Collections;
using Unity.Netcode;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillUseRequest : INetworkSerializable
	{
		public FixedString64Bytes SkillId;
		public uint RequestSequence;
		public NetworkSkillTargetInfo Target;

		public NetworkSkillUseRequest(string skillId, uint requestSequence, NetworkSkillTargetInfo target)
		{
			SkillId = new FixedString64Bytes(skillId);
			RequestSequence = requestSequence;
			Target = target;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref RequestSequence);
			serializer.SerializeValue(ref Target);
		}
	}
}
