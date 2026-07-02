using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[SerializeField] private PlayerInputReader inputReader;
	[SerializeField] private ClickMoveController clickMoveController;
	[SerializeField] private SkillTargetingController skillTargetingController;
	[SerializeField] private SkillLoadout skillLoadout;
	
	private void Awake()
	{
		if (inputReader == null)
			inputReader = GetComponent<PlayerInputReader>();

		if (clickMoveController == null)
			clickMoveController = GetComponent<ClickMoveController>();

		if (skillTargetingController == null)
			skillTargetingController = GetComponent<SkillTargetingController>();

		if (skillLoadout == null)
			skillLoadout = GetComponent<SkillLoadout>();
	}

	private void OnEnable()
	{
		if (inputReader == null)
			return;

		inputReader.MoveRequested += HandleMoveRequested;
		inputReader.StopRequested += HandleStopRequested;
		inputReader.SkillSlotRequested += HandleSkillSlotRequested;
		inputReader.PrimaryActionRequested += HandlePrimaryActionRequested;
	}

	private void OnDisable()
	{
		if (inputReader == null)
			return;

		inputReader.MoveRequested -= HandleMoveRequested;
		inputReader.StopRequested -= HandleStopRequested;
		inputReader.SkillSlotRequested -= HandleSkillSlotRequested;
		inputReader.PrimaryActionRequested -= HandlePrimaryActionRequested;
	}

	void Update()
    {
		clickMoveController?.TickMove();
		skillTargetingController?.Tick();
    }

	private void HandleMoveRequested()
	{
		clickMoveController?.SetTargetByMouse();

		if (skillTargetingController != null && skillTargetingController.IsIndicatingSkill)
		{
			skillTargetingController.Cancel();
		}
	}

	private void HandleStopRequested()
	{
		clickMoveController?.StopMove();

		if (skillTargetingController != null && skillTargetingController.IsIndicatingSkill)
		{
			skillTargetingController.Cancel();
		}
	}

	private void HandleSkillSlotRequested(SkillSlotKey slotKey)
	{
		if (skillLoadout == null)
		{
			Debug.LogWarning("SkillLoadout is no assigned.", this);
			return;
		}

		if (!skillLoadout.TryGetSkill(slotKey, out SkillDefinition skill))
		{
			Debug.LogWarning($"Skill is not assigned to slot: {slotKey}", this);
			return;
		}

		skillTargetingController?.BeginIndicate(skill);
	}

	private void HandlePrimaryActionRequested()
	{
		if (skillTargetingController == null)
			return;

		if (!skillTargetingController.IsIndicatingSkill)
			return;

		// TODO: 실제 스킬 시전 연결은 SkillCastController 추가 단계에서 처리한다.
	}
}
