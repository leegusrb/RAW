using System.Text;
using Unity.Collections;

namespace RAW.Network
{
	public static class NetworkSkillContractMapper
	{
		public static bool TryToNetwork(
			SkillUseRequest request,
			out NetworkSkillUseRequest networkRequest
		)
		{
			networkRequest = default;

			if (request == null || string.IsNullOrWhiteSpace(request.skillId))
				return false;

			int skillIdByteCount = Encoding.UTF8.GetByteCount(request.skillId);

			if (skillIdByteCount > FixedString64Bytes.UTF8MaxLengthInBytes)
				return false;

			NetworkSkillTargetInfo networkTarget = ToNetworkTarget(request.target);

			networkRequest =
				new NetworkSkillUseRequest(
					request.skillId,
					request.requestSequence,
					networkTarget
				);

			return true;
		}

		private static NetworkSkillTargetInfo ToNetworkTarget(SkillTargetInfo target)
		{
			if (target == null)
				return default;

			return new NetworkSkillTargetInfo(
				target.direction,
				target.targetPosition,
				target.targetObjectId
			);
		}

		public static SkillUseRejectedEvent ToContract(NetworkSkillUseRejectedEvent networkEvent)
		{
			return new SkillUseRejectedEvent
			{
				skillId = networkEvent.SkillId.ToString(),
				requestSequence = networkEvent.RequestSequence,
				reason = networkEvent.Reason
			};
		}

		public static SkillCastEvent ToContract(NetworkSkillCastEvent networkEvent)
		{
			return new SkillCastEvent
			{
				casterObjectId = networkEvent.CasterObjectId,
				skillId = networkEvent.SkillId.ToString(),
				casterPosition = networkEvent.CasterPosition,
				targetInfo = ToContractTarget(networkEvent.TargetInfo),
				requestSequence = networkEvent.RequestSequence,
				castServerTime = networkEvent.CastServerTime
			};
		}

		public static SkillHitEvent ToContract(NetworkSkillHitEvent networkEvent)
		{
			return new SkillHitEvent
			{
				casterObjectId = networkEvent.CasterObjectId,
				targetObjectId = networkEvent.TargetObjectId,
				skillId = networkEvent.SkillId.ToString(),
				damage = networkEvent.Damage,
				targetHpAfterHit = networkEvent.TargetHpAfterHit,
				hitPosition = networkEvent.HitPosition,
				requestSequence = networkEvent.RequestSequence,
				hitIndex = networkEvent.HitIndex,
				hitServerTime = networkEvent.HitServerTime
			};
		}

		private static SkillTargetInfo ToContractTarget(NetworkSkillTargetInfo networkTarget)
		{
			return new SkillTargetInfo
			{
				direction = networkTarget.Direction,
				targetPosition = networkTarget.TargetPosition,
				targetObjectId = networkTarget.TargetObjectId
			};
		}
	}
}
