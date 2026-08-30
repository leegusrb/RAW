using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	internal struct NetworkSkillTargetInfo : INetworkSerializeByMemcpy
	{
		public Vector3 Direction;
		public Vector3 TargetPosition;
		public ulong TargetObjectId;
	}

	internal struct NetworkSkillUseRequest : INetworkSerializable
	{
		public FixedString64Bytes SkillId;
		public NetworkSkillTargetInfo TargetInfo;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref TargetInfo);
		}
	}

	internal struct NetworkSkillUseRejectedEvent : INetworkSerializable
	{
		public FixedString64Bytes SkillId;
		public SkillUseRejectionReason Reason;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref Reason);
		}
	}

	internal struct NetworkSkillCastEvent : INetworkSerializable
	{
		public FixedString64Bytes SkillId;
		public NetworkSkillTargetInfo TargetInfo;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref TargetInfo);
		}
	}

	internal struct NetworkSkillHitEvent : INetworkSerializable
	{

		public FixedString64Bytes SkillId;
		public ulong TargetObjectId;
		public int Damage;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref TargetObjectId);
			serializer.SerializeValue(ref Damage);
		}
	}
}