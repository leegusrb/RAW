public interface ISkillRuntime
{
    bool TryGetSkillForSlot(SkillSlot slot, out SkillSpec skill);

	double GetRemainingCooldown(string skillId);

	SkillUseHandlingResult RequestUseSkill(SkillSlot slot);
}
