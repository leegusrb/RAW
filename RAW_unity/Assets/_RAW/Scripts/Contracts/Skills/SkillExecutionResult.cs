using System;

namespace RAW.Contracts.Skills
{
	[Serializable]
	public struct SkillExecutionResult
	{
		public ulong ReqeustId;

		public SkillFailureReason FailureReason;
		
		public ulong TargetEntityId;
		public SkillEffectType EffectType;
		public int Amount;

		public int ManaCost;
		public float CooldownSeconds;

		public bool Succeeded => FailureReason == SkillFailureReason.None;

		public static SkillExecutionResult Rejected(
			ulong requestId,
			SkillFailureReason failureReason,
			ulong targetEntityId = 0)
		{
			if (failureReason == SkillFailureReason.None)
				failureReason = SkillFailureReason.InvalidRequest;

			return new SkillExecutionResult
			{
				ReqeustId = requestId,
				FailureReason = failureReason,

				TargetEntityId = targetEntityId,
				EffectType = SkillEffectType.None,
				Amount = 0,

				ManaCost = 0,
				CooldownSeconds = 0f
			};
		}


		public static SkillExecutionResult Success(
			ulong requestId,
			ulong targetEntityId,
			SkillEffectType effectType,
			int amount,
			int manaCost,
			float cooldownSeconds
		)
		{
			return new SkillExecutionResult
			{
				ReqeustId = requestId,
				FailureReason = SkillFailureReason.None,

				TargetEntityId = targetEntityId,
				EffectType = effectType,
				Amount = Math.Max(0, amount),

				ManaCost = Math.Max(0, manaCost),
				CooldownSeconds = Math.Max(0f, cooldownSeconds)
			};
		}
	}
}