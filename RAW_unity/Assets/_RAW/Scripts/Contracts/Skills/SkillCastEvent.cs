using System;
using UnityEngine;

[Serializable]
public class SkillCastEvent
{
    public long casterObjectId;
	public string skillId;

	public Vector3 casterPosition;
	public SkillTargetInfo targetInfo;

	public uint requestSequence;
	public double castServerTime;
}
