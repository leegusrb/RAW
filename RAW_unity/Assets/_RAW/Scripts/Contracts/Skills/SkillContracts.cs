using System;
using UnityEngine;

[Serializable]
public class SkillUseRequest
{
    public string skillId;
	public SkillTargetInfo target;
}

[Serializable]
public struct SkillTargetInfo
{
	public Vector3 direction;
	public Vector3 targetPosition;
	public ulong targetObjectId;
}

[Serializable]
public struct SkillCastEvent
{
    public ulong casterObjectId;
	public string skillId;

	public SkillTargetInfo targetInfo;
}

[Serializable]
public struct SkillHitEvent
{
	public string skillId;

	public ulong casterObjectId;
	public ulong targetObjectId;

	public int damage;
}

[Serializable]
public struct SkillUseRejectedEvent
{
	public string skillId;
	public SkillUseRejectionReason reason;
}

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
