public enum SkillUseRejectionReason
{
	Unknown = 0,

	SkillNotFound = 1,
	SkillNotOwned = 2,
	SkillNotEquipped = 3,

	Cooldown = 10,
	NotEnoughResource = 11,

	InvalidState = 20,
	InvalidTarget = 21,
	OutOfRange = 22
}
