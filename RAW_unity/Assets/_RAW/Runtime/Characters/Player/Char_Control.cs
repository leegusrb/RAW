using UnityEngine;

public class Char_Control : MonoBehaviour
{
	[SerializeField] private ClickMoveController clickMoveController;
	[SerializeField] private SkillTargetingController skillTargetingController;
	[SerializeField] private SkillLoadout skillLoadout;
	
	private void Awake()
	{
		if (clickMoveController == null)
			clickMoveController = GetComponent<ClickMoveController>();

		if (skillTargetingController == null)
			skillTargetingController = GetComponent<SkillTargetingController>();

		if (skillLoadout == null)
			skillLoadout = GetComponent<SkillLoadout>();
	}

    void Update()
    {
		HandleMovementInput();
		HandleSkillInput();
		
		clickMoveController?.TickMove();
		skillTargetingController?.Tick();
    }

	private void HandleMovementInput()
	{
        if (Input.GetMouseButtonDown(1))
        {
			clickMoveController?.SetTargetByMouse();

			if (skillTargetingController != null && skillTargetingController.IsIndicatingSkill)
			{
				skillTargetingController.Cancel();
			}        
        }

		if (Input.GetKeyDown(KeyCode.S))
		{
			clickMoveController?.StopMove();

			if (skillTargetingController != null && skillTargetingController.IsIndicatingSkill)
			{
				skillTargetingController.Cancel();
			}
		}
	}

	private void HandleSkillInput()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			BeginSkillIndicator(SkillSlotKey.Q);
		}
		else if (Input.GetKeyDown(KeyCode.W))
		{
			BeginSkillIndicator(SkillSlotKey.W);
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			BeginSkillIndicator(SkillSlotKey.E);
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			BeginSkillIndicator(SkillSlotKey.R);
		}

		if (Input.GetMouseButtonDown(0))
		{
			if (skillTargetingController != null && skillTargetingController.IsIndicatingSkill)
			{
				// TODO: 실제 스킬 시전 연결
			}
		}
	}

	private void BeginSkillIndicator(SkillSlotKey slotKey)
	{
		if (skillLoadout == null)
		{
			Debug.LogWarning("SkillLoadout is not assigned.", this);
			return;
		}

		if (!skillLoadout.TryGetSkill(slotKey, out SkillDefinition skill))
		{
			Debug.LogWarning($"Skill is not assigned to slot: {slotKey}", this);
			return;
		}

		skillTargetingController?.BeginIndicate(skill);
	}
}
