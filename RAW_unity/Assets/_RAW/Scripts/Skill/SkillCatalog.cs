using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillCatalog", menuName = "RAW/Data/Skill Catalog")]
public class SkillCatalog : ScriptableObject
{
    [SerializeField]
	private List<SkillSpec> skills = new List<SkillSpec>();

	public bool TryGetSkill(string skillId, out SkillSpec skill)
	{
		if (!string.IsNullOrEmpty(skillId))
		{
			for (int i = 0; i < skills.Count; i++)
			{
				SkillSpec candidate = skills[i];

				if (candidate == null)
					continue;

				if (string.Equals(candidate.SkillId, skillId, System.StringComparison.Ordinal))
				{
					skill = candidate;
					return true;
				}
			}
		}

		skill = null;
		return false;
	}

	private void OnValidate()
	{
		HashSet<string> knownSkillIds = new HashSet<string>(StringComparer.Ordinal);

		for (int i = 0; i < skills.Count; i++)
		{
			SkillSpec skill = skills[i];

			if (skill == null)
			{
				Debug.LogError($"SkillCatalog의 {i}번 항목이 비어 있습니다.", this);
				continue;
			}

			if (string.IsNullOrEmpty(skill.SkillId))
			{
				Debug.LogError($"Skill ID가 비어 있습니다: {skill.name}", this);
				continue;
			}

			if (!knownSkillIds.Add(skill.SkillId))
			{
				Debug.LogError($"중복된 Skill ID가 있습니다: {skill.SkillId}", this);
			}
		}
	}
}
