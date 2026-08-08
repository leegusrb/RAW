public interface ISkillRuntime
{
    bool TryGetSkillForSlot(KeyMapping slot, out SkillSpec skill);

	double GetRemainingCooldown(string skillId);

	void RequestUseSkill(KeyMapping slot);
}
