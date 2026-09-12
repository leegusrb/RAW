using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RAW.Network.Tests
{
	public class NetworkSkillCastContractTests
	{
		[Test]
		public void StartedEvent_RoundTripAndMap_PreservesFields()
		{
			var source = new NetworkSkillCastStartedEvent
			{
				CastId = 42,
				SkillId = new FixedString64Bytes("archer_example"),
				TargetInfo = new NetworkSkillTargetInfo
				{
					Direction = new Vector3(1f, 0f, 0f),
					TargetPosition = new Vector3(3f, 4f, 0f),
					TargetObjectId = 99
				},
				StartedAt = 100000.125,
				ExecuteAt = 100000.126
			};

			var received = RoundTrip(source);
			var result = NetworkSkillContractMapper.ToContract(received, 15);

			Assert.That(result.casterObjectId, Is.EqualTo(15UL));
			Assert.That(result.castId, Is.EqualTo(source.CastId));
			Assert.That(result.skillId, Is.EqualTo(source.SkillId.ToString()));
			Assert.That(result.targetInfo.direction,
				Is.EqualTo(source.TargetInfo.Direction));
			Assert.That(result.targetInfo.targetPosition,
				Is.EqualTo(source.TargetInfo.TargetPosition));
			Assert.That(result.targetInfo.targetObjectId,
				Is.EqualTo(source.TargetInfo.TargetObjectId));
			Assert.That(result.startedAt, Is.EqualTo(source.StartedAt));
			Assert.That(result.executeAt, Is.EqualTo(source.ExecuteAt));
		}

		[Test]
		public void CommittedEvent_RoundTripAndMap_PreservesFields()
		{
			var source = new NetworkSkillCastCommittedEvent
			{
				CastId = 43,
				SkillId = new FixedString64Bytes("archer_example"),
				TargetInfo = new NetworkSkillTargetInfo
				{
					Direction = new Vector3(0f, 1f, 0f),
					TargetPosition = new Vector3(5f, 6f, 0f),
					TargetObjectId = 100
				},
				SpawnPosition = new Vector3(1f, 2f, 0f),
				ExecutedAt = 100000.127
			};

			var received = RoundTrip(source);
			var result = NetworkSkillContractMapper.ToContract(received, 16);

			Assert.That(result.casterObjectId, Is.EqualTo(16UL));
			Assert.That(result.castId, Is.EqualTo(source.CastId));
			Assert.That(result.skillId, Is.EqualTo(source.SkillId.ToString()));
			Assert.That(result.targetInfo.direction,
				Is.EqualTo(source.TargetInfo.Direction));
			Assert.That(result.targetInfo.targetPosition,
				Is.EqualTo(source.TargetInfo.TargetPosition));
			Assert.That(result.targetInfo.targetObjectId,
				Is.EqualTo(source.TargetInfo.TargetObjectId));
			Assert.That(result.spawnPosition, Is.EqualTo(source.SpawnPosition));
			Assert.That(result.executedAt, Is.EqualTo(source.ExecutedAt));
		}

		[Test]
		public void CancelledEvent_RoundTripAndMap_PreservesFields()
		{
			var source = new NetworkSkillCastCancelledEvent
			{
				CastId = 44,
				Reason = SkillUseRejectionReason.OutOfRange
			};

			var received = RoundTrip(source);
			var result = NetworkSkillContractMapper.ToContract(received, 17);

			Assert.That(result.casterObjectId, Is.EqualTo(17UL));
			Assert.That(result.castId, Is.EqualTo(source.CastId));
			Assert.That(result.reason, Is.EqualTo(source.Reason));
		}

		private static T RoundTrip<T>(T source)
			where T : INetworkSerializable, new()
		{
			using var writer = new FastBufferWriter(256, Allocator.Temp);
			writer.WriteNetworkSerializable(source);

			using var reader = new FastBufferReader(writer, Allocator.Temp);
			reader.ReadNetworkSerializable<T>(out T result);

			return result;
		}
	}
}