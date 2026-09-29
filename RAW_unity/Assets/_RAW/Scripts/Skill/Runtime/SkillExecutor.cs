using System;
using UnityEngine;

public static class SkillExecutor
{
    public static void ExecutePrepared(
		SkillExecutionContext context,
		Action<SkillTarget, int> applyDamage
	)
	{
		GameObject skillObject = UnityEngine.Object.Instantiate(
			context.Skill.skillPrefab,
			context.SpawnPosition,
			Quaternion.identity
		);

		skillObject.transform.localScale = context.LocalScale;

		SkillObject skillObjectComponent = skillObject.GetComponent<SkillObject>();

		skillObjectComponent.Initialize(
			context.Skill,
			context.DestinationPosition,
			context.Target,
			applyDamage
		);
	}
}
