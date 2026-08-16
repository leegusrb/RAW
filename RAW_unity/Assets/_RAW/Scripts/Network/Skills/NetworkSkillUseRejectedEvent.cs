using System;
using Unity.Collections;
using Unity.Netcode;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillUseRejectedEvent : INetworkSerializable
	{
		public FixedString64Bytes SkillId;
		public uint RequestSequence;
		public SkillUseRejectionReason Reason;

		public NetworkSkillUseRejectedEvent(FixedString64Bytes skillId, uint requestSequence, SkillUseRejectionReason reason)
		{
			SkillId = skillId;
			RequestSequence = requestSequence;
			Reason = reason;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref RequestSequence);

			int reasonValue = (int)Reason;

			serializer.SerializeValue(ref reasonValue);

			if (serializer.IsReader)
				Reason = (SkillUseRejectionReason)reasonValue;
		}
	}
}
