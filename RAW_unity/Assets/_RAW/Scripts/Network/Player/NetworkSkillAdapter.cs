using UnityEngine;

namespace RAW.Network
{
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

		public bool TryGetSkillForSlot(SkillSlot slot, out SkillSpec skill)
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

		public SkillUseDispatchResult RequestUseSkill(SkillSlot slot)
		{
			if (networkSkillController == null)
			{
				Debug.LogError("스킬을 요청할 NetworkSkillController가 없습니다.", this);
				return SkillUseDispatchResult.DispatchFailed;
			}

			networkSkillController.RequestUseSkill(slot);

			return SkillUseDispatchResult.Dispatched;
		}
	}
	
}
