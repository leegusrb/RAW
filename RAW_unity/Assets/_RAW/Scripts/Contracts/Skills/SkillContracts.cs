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
public struct SkillCastStartedEvent
{
	public ulong casterObjectId;
	public ulong castId;
	public string skillId;

	public SkillTargetInfo targetInfo;

	// 서버 네트워크 시간 기준
	public double startedAt;
	public double executeAt;
}

[Serializable]
public struct SkillCastCommittedEvent
{
	public ulong casterObjectId;
	public ulong castId;
	public string skillId;

	public SkillTargetInfo targetInfo;
	public Vector3 spawnPosition;

	// 서버 네트워크 시간 기준
	public double executedAt;
}

[Serializable]
public struct SkillCastCancelledEvent
{
	public ulong casterObjectId;
	public ulong castId;
	public SkillUseRejectionReason reason;
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
