using UnityEngine;

public readonly struct SkillExecutionContext
{
	public SkillSpec Skill { get; }
	public Vector3 SpawnPosition { get; }
	public Vector3 DestinationPosition { get; }
	public Vector3 LocalScale { get; }
	public SkillTarget Target { get; }

	public SkillExecutionContext(SkillSpec skill, Vector3 spawnPosition, Vector3 destinationPosition, Vector3 localScale, SkillTarget target)
	{
		Skill = skill;
		SpawnPosition = spawnPosition;
		DestinationPosition = destinationPosition;
		LocalScale = localScale;
		Target = target;
	}
}
