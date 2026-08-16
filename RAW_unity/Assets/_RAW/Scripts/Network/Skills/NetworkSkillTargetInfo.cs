using System;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network
{
	[Serializable]
	public struct NetworkSkillTargetInfo : INetworkSerializable
	{
		public Vector3 Direction;
		public Vector3 TargetPosition;
		public ulong TargetObjectId;

		public NetworkSkillTargetInfo(Vector3 direction, Vector3 targetPosition, ulong targetObjectId)
		{
			Direction = direction;
			TargetPosition = targetPosition;
			TargetObjectId = targetObjectId;
		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer)
			where T : IReaderWriter
		{
			serializer.SerializeValue(ref Direction);
			serializer.SerializeValue(ref TargetPosition);
			serializer.SerializeValue(ref TargetObjectId);
		}
	}
}