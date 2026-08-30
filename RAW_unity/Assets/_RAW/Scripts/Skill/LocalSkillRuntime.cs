using UnityEngine;

[DisallowMultipleComponent]
public class LocalSkillRuntime :
	MonoBehaviour,
	ISkillRuntime
{
    public bool TryGetSkillForSlot(KeyMapping slot, out SkillSpec skill)
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

	public SkillUseRequestResult RequestUseSkill(string skillId)
	{
		if (string.IsNullOrWhiteSpace(skillId))
			return SkillUseRequestResult.Rejected;

		return SkillUseRequestResult.ExecuteLocally;
	}

	public void CreateSkillObject(
		SkillSpec skillSpec,
		Vector3 spawnPosition,
		Vector3 destinationPosition,
		Vector3 skillObjectLocalScale,
		SkillTarget skillTarget
	)
	{
		if (skillSpec == null)
		{
			Debug.LogError("생성할 스킬 정보가 없습니다.", this);
			return;
		}

		if (skillSpec.skillPrefab == null)
		{
			Debug.LogError(
				$"{skillSpec.name} 스킬에 프리팹이 연결되지 않았습니다.",
				skillSpec
			);
			return;
		}

		GameObject skillObject = Instantiate(
			skillSpec.skillPrefab,
			spawnPosition,
			Quaternion.identity
		);

		skillObject.transform.localScale = skillObjectLocalScale;

		if (!skillObject.TryGetComponent(out SkillObject skillObjectComponent))
		{
			Debug.LogError(
				$"{skillSpec.name} 프리팹에 SkillObject가 없습니다.",
				skillObject
			);
			Destroy(skillObject);
			return;
		}

		skillObjectComponent.Initialize(
			skillSpec,
			destinationPosition,
			skillTarget
		);
	}
}
