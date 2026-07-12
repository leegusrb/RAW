using System;
using UnityEngine;

[Serializable]
public class SkillSlotAssignment
{
	[SerializeField] private SkillSlotKey slotKey;
	[SerializeField] private SkillDefinition skill;

	public SkillSlotKey SlotKey => slotKey;
	public SkillDefinition Skill => skill;
}
