using System;
using UnityEngine;

[Serializable]
public class SkillCastEvent
{
    public ulong casterObjectId;
	public string skillId;

	public Vector3 casterPosition;
	public SkillTargetInfo targetInfo;

	public uint requestSequence;
	public double castServerTime;
}
