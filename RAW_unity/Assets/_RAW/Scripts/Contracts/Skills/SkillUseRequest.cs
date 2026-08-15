using System;

[Serializable]
public class SkillUseRequest
{
    public string skillId;
	public uint requestSequence;
	public SkillTargetInfo target;
}
