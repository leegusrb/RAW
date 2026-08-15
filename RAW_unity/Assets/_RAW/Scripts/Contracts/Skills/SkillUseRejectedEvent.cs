using System;

[Serializable]
public class SkillUseRejectedEvent
{
	public string skillId;
	public uint requestSequence;
	public SkillUseRejectionReason reason;
}
