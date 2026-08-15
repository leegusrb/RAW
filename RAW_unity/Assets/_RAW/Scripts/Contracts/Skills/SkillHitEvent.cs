using System;
using UnityEngine;

[Serializable]
public class SkillHitEvent
{
	public ulong casterEntityId;
	public ulong targetEntityId;
	
	public string skillId;

	public int damage;
	public int targetHpAfterHit;

	public Vector3 hitPosition;

	public uint requestSequence;
	public ushort hitIndex;
	public double hitServerTime;
}
