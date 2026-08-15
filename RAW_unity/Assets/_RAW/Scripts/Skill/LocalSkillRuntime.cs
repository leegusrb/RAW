using UnityEngine;

[DisallowMultipleComponent]
public class LocalSkillRuntime :
	MonoBehaviour,
	ISkillRuntime
{
    public bool TryGetSkillForSlot(SkillSlot slot, out SkillSpec skill)
	{
		skill = null;

		if (DataBase.Instance == null)
		{
			Debug.LogError("로컬 스킬을 조회할 DataBase가 없습니다.", this);
			return false;
		}

		if (DataBase.Instance.mySkillKeyMap == null)
		{
			Debug.LogError("DataBase의 스킬 키맵이 없습니다.", this);
			return false;
		}

		string skillKey = slot.ToString().ToLowerInvariant();

		return DataBase.Instance.mySkillKeyMap.TryGetValue(skillKey, out skill) && skill != null;
	}

	public double GetRemainingCooldown(string skillId)
	{
		return 0d;
	}

	public SkillUseDispatchResult RequestUseSkill(SkillSlot slot)
	{
		if (!TryGetSkillForSlot(slot, out _))
			return SkillUseDispatchResult.DispatchFailed;

		return SkillUseDispatchResult.ExecuteLocally;
	}
}
