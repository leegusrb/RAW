using UnityEngine;

public static class SkillExecutor
{
    public static void ExecutePrepared(SkillExecutionContext context)
	{
		GameObject skillObject = Object.Instantiate(
			context.Skill.skillPrefab,
			context.SpawnPosition,
			Quaternion.identity
		);

		skillObject.transform.localScale = context.LocalScale;

		SkillObject skillObjectComponent = skillObject.GetComponent<SkillObject>();

		skillObjectComponent.Initialize(
			context.Skill,
			context.DestinationPosition,
			context.Target
		);
	}
}
