using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillCastEvent : INetworkSerializable
	{
		public ulong CasterObjectId;
		public FixedString64Bytes SkillId;
		public Vector3 CasterPosition;
		public NetworkSkillTargetInfo TargetInfo;
		public uint RequestSequence;
		public double CastServerTime;

		public NetworkSkillCastEvent(
			ulong casterObjectId,
			FixedString64Bytes skillId,
			Vector3 casterPosition,
			NetworkSkillTargetInfo targetInfo,
			uint requestSequence,
			double castServerTime
		)
		{
			CasterObjectId = casterObjectId;
			SkillId = skillId;
			CasterPosition = casterPosition;
			TargetInfo = targetInfo;
			RequestSequence = requestSequence;
			CastServerTime = castServerTime;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref CasterObjectId);
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref CasterPosition);
			serializer.SerializeValue(ref TargetInfo);
			serializer.SerializeValue(ref RequestSequence);
			serializer.SerializeValue(ref CastServerTime);
		}
	}
}
