using UnityEngine;

public interface ISkillRuntime
{
    bool TryGetSkillForSlot(KeyMapping slot, out SkillSpec skill);

	double GetRemainingCooldown(string skillId);

	SkillUseRequestResult RequestUseSkill(string skillId);

	void CreateSkillObject(
		SkillSpec skillSpec,
		Vector3 spawnPosition,
		Vector3 destinationPosition,
		Vector3 skillObjectLocalScale,
		Enemy skillTargetEnemy
	);
}
