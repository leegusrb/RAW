public interface ISkillRuntime
{
    bool TryGetSkillForSlot(SkillSlot slot, out SkillSpec skill);

	double GetRemainingCooldown(string skillId);

	SkillUseDispatchResult RequestUseSkill(SkillSlot slot);
}
