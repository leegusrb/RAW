using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillHitEvent : INetworkSerializable
	{
		public ulong CasterObjectId;
		public ulong TargetObjectId;

		public FixedString64Bytes SkillId;

		public int Damage;
		public int TargetHpAfterHit;

		public Vector3 HitPosition;

		public uint RequestSequence;
		public ushort HitIndex;
		public double HitServerTime;

		public NetworkSkillHitEvent(
			ulong casterObjectId,
			ulong targetObjectId,
			FixedString64Bytes skillId,
			int damage,
			int targetHpAfterHit,
			Vector3 hitPosition,
			uint requestSequence,
			ushort hitIndex,
			double hitServerTime
		)
		{
			CasterObjectId = casterObjectId;
			TargetObjectId = targetObjectId;
			SkillId = skillId;
			Damage = damage;
			TargetHpAfterHit = targetHpAfterHit;
			HitPosition = hitPosition;
			RequestSequence = requestSequence;
			HitIndex = hitIndex;
			HitServerTime = hitServerTime;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref CasterObjectId);
			serializer.SerializeValue(ref TargetObjectId);
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref Damage);
			serializer.SerializeValue(ref TargetHpAfterHit);
			serializer.SerializeValue(ref HitPosition);
			serializer.SerializeValue(ref RequestSequence);
			serializer.SerializeValue(ref HitIndex);
			serializer.SerializeValue(ref HitServerTime);
		}
	}
}
