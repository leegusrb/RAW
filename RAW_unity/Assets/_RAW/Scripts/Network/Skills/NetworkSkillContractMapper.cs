using System.Text;
using Unity.Collections;

namespace RAW.Network
{
	internal static class NetworkSkillContractMapper
	{
		public static bool TryToNetwork(
			SkillUseRequest request,
			out NetworkSkillUseRequest result
		)
		{
			result = default;

			if (request == null || string.IsNullOrWhiteSpace(request.skillId))
				return false;

			int skillIdByteCount = Encoding.UTF8.GetByteCount(request.skillId);

			if (skillIdByteCount > FixedString64Bytes.UTF8MaxLengthInBytes)
				return false;

			result = new NetworkSkillUseRequest
			{
				SkillId = new FixedString64Bytes(request.skillId),
				TargetInfo = ToNetworkTarget(request.target)
			};

			return true;
		}

		public static SkillUseRejectedEvent ToContract(NetworkSkillUseRejectedEvent networkEvent)
		{
			return new SkillUseRejectedEvent
			{
				skillId = networkEvent.SkillId.ToString(),
				reason = networkEvent.Reason
			};
		}

		public static SkillCastEvent ToContract(NetworkSkillCastEvent networkEvent, ulong casterObjectId)
		{
			return new SkillCastEvent
			{
				casterObjectId = casterObjectId,
				skillId = networkEvent.SkillId.ToString(),
				targetInfo = ToContractTarget(networkEvent.TargetInfo)
			};
		}

		public static SkillHitEvent ToContract(NetworkSkillHitEvent networkEvent, ulong casterObjectId)
		{
			return new SkillHitEvent
			{
				skillId = networkEvent.SkillId.ToString(),
				casterObjectId = casterObjectId,
				targetObjectId = networkEvent.TargetObjectId,
				damage = networkEvent.Damage,
			};
		}

		private static NetworkSkillTargetInfo ToNetworkTarget(SkillTargetInfo target)
		{
			return new NetworkSkillTargetInfo
			{
				Direction = target.direction,
				TargetPosition = target.targetPosition,
				TargetObjectId = target.targetObjectId
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
