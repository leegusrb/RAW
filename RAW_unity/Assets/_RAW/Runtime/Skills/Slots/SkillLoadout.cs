using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SkillLoadout : MonoBehaviour
{
	[SerializeField] private SkillSlotAssignment[] slots;

	private readonly Dictionary<SkillSlotKey, SkillDefinition> skillBySlot = new Dictionary<SkillSlotKey, SkillDefinition>();

	private void Awake()
	{
		RebuildCache();
	}

#if UNITY_EDITOR
	private void OnValidate()
	{
		RebuildCache();
	}
#endif

	public bool TryGetSkill(SkillSlotKey slotKey, out SkillDefinition skill)
	{
		if (skillBySlot.Count == 0)
			RebuildCache();

		return skillBySlot.TryGetValue(slotKey, out skill) && skill != null;
	}

	private void RebuildCache()
	{
		skillBySlot.Clear();

		if (slots == null)
			return;

		foreach (SkillSlotAssignment slot in slots)
		{
			if (slot == null)
				continue;
			
			if (slot.Skill == null)
				continue;

			if (skillBySlot.ContainsKey(slot.SlotKey))
			{
				Debug.LogWarning($"Duplicate skill slot assignment: {slot.SlotKey}", this);
				continue;
			}

			skillBySlot.Add(slot.SlotKey, slot.Skill);
		}
	}
}
