using UnityEngine;

public class Char_Control : MonoBehaviour
{
	[SerializeField] private ClickMoveController clickMoveController;
	[SerializeField] private SkillTargetingController skillTargetingController;
	
	private void Awake()
	{
		if (clickMoveController == null)
			clickMoveController = GetComponent<ClickMoveController>();

		if (skillTargetingController == null)
			skillTargetingController = GetComponent<SkillTargetingController>();
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
			BeginSkillIndicator("q");
		}
		else if (Input.GetKeyDown(KeyCode.W))
		{
			BeginSkillIndicator("w");
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			BeginSkillIndicator("e");
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			BeginSkillIndicator("r");
		}

		if (Input.GetMouseButton(0))
		{
			if (skillTargetingController != null && skillTargetingController.IsIndicatingSkill)
			{
				// TODO:
				// 여기서 실제 스킬 로직 연결
			}
		}
	}

	private void BeginSkillIndicator(string inputKey)
	{
		skillTargetingController?.BeginIndicate(inputKey);
	}
}
