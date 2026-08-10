namespace RAW.Contracts.Skills
{
	public enum SkillFailureReason
	{
		None = 0,

		InvalidRequest = 1,
		DuplicateRequest = 2,
		NotOwner = 3,

		SkillNotFound = 10,
		InvalidState = 11,
		InsufficientMana = 12,
		CooldownActive = 13,

		InvalidTarget = 20,
		TargetDead = 21,
		OutOfRange = 22,
		Blocked = 23
	}
}