using System.Text;
using Unity.Collections;
using UnityEngine;

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
	}
}
