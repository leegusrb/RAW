using System;
using UnityEngine;

namespace RAW.Network
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(NetworkSkillController))]
	public class NetworkSkillAdapter :
		MonoBehaviour,
		ISkillRuntime
	{
		[SerializeField]
		private NetworkSkillController networkSkillController;

		private void Reset()
		{
			CacheComponents();
		}

		private void Awake()
		{
			CacheComponents();

			if (networkSkillController == null)
			{
				Debug.LogError("NetworkSkillController가 연결되지 않았습니다.", this);
				enabled = false;
			}
		}

		private void CacheComponents()
		{
			if (networkSkillController == null)
				networkSkillController = GetComponent<NetworkSkillController>();
		}

		public bool TryGetSkillForSlot(KeyMapping slot, out SkillSpec skill)
		{
			if (networkSkillController == null)
			{
				skill = null;
				return false;
			}

			return networkSkillController.TryGetSkillForSlot(slot, out skill);
		}

		public double GetRemainingCooldown(string skillId)
		{
			if (networkSkillController == null)
				return 0d;

			return networkSkillController.GetRemainingCooldown(skillId);
		}

		public SkillUseRequestResult RequestUseSkill(string skillId)
		{
			if (networkSkillController == null)
			{
				Debug.LogError("스킬을 요청할 NetworkSkillController가 없습니다.", this);
				return SkillUseRequestResult.Rejected;
			}

			if (string.IsNullOrWhiteSpace(skillId))
				return SkillUseRequestResult.Rejected;

			foreach (KeyMapping slot in Enum.GetValues(typeof(KeyMapping)))
			{
				if (!networkSkillController.TryGetSkillForSlot(slot, out SkillSpec skill))
					continue;

				if (!string.Equals(skill.SkillId, skillId, StringComparison.Ordinal))
					continue;

				networkSkillController.RequestUseSkill(slot);
				return SkillUseRequestResult.HandleByRuntime;
			}

			Debug.LogWarning($"장착되지 않은 스킬입니다: {skillId}", this);
			return SkillUseRequestResult.Rejected;
		}
	}
	
}
