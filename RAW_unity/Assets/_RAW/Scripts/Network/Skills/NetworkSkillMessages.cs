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

	internal struct NetworkSkillCastStartedEvent : INetworkSerializable
	{
		public ulong CastId;
		public FixedString64Bytes SkillId;
		public NetworkSkillTargetInfo TargetInfo;

		public double StartedAt;
		public double ExecuteAt;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref CastId);
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref TargetInfo);
			serializer.SerializeValue(ref StartedAt);
			serializer.SerializeValue(ref ExecuteAt);
		}
	}

	internal struct NetworkSkillCastCommittedEvent : INetworkSerializable
	{
		public ulong CastId;
		public FixedString64Bytes SkillId;
		public NetworkSkillTargetInfo TargetInfo;

		public Vector3 SpawnPosition;
		public double ExecutedAt;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref CastId);
			serializer.SerializeValue(ref SkillId);
			serializer.SerializeValue(ref TargetInfo);
			serializer.SerializeValue(ref SpawnPosition);
			serializer.SerializeValue(ref ExecutedAt);
		}
	}

	internal struct NetworkSkillCastCancelledEvent : INetworkSerializable
	{
		public ulong CastId;
		public SkillUseRejectionReason Reason;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref CastId);
			serializer.SerializeValue(ref Reason);
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