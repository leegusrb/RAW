using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
	public event Action MoveRequested;
	public event Action StopRequested;
	public event Action PrimaryActionRequested;
	public event Action<SkillSlotKey> SkillSlotRequested;

	public Vector2 PointerScreenPosition => pointerPositionAction != null ? pointerPositionAction.ReadValue<Vector2>() : Vector2.zero;

	private InputAction moveToPointerAction;
	private InputAction primaryAction;
	private InputAction stopAction;
	private InputAction skillQAction;
	private InputAction skillWAction;
	private InputAction skillEAction;
	private InputAction skillRAction;
	private InputAction pointerPositionAction;

	private void Awake()
	{
		InputActionMap playerMap = InputSystem.actions.FindActionMap("Player", true);

		moveToPointerAction = playerMap.FindAction("MoveToPointer", true);
		primaryAction = playerMap.FindAction("PrimaryAction", true);
		stopAction = playerMap.FindAction("Stop", true);
		skillQAction = playerMap.FindAction("SkillQ", true);
		skillWAction = playerMap.FindAction("SkillW", true);
		skillEAction = playerMap.FindAction("SkillE", true);
		skillRAction = playerMap.FindAction("SkillR", true);
		pointerPositionAction = playerMap.FindAction("PointerPosition", true);
	}

	private void OnEnable()
	{
		moveToPointerAction.performed += OnMoveToPointer;
		primaryAction.performed += OnPrimaryAction;
		stopAction.performed += OnStop;
		skillQAction.performed += OnSkillQ;
		skillWAction.performed += OnSkillW;
		skillEAction.performed += OnSkillE;
		skillRAction.performed += OnSkillR;
	}

	private void OnDisable()
	{
		moveToPointerAction.performed -= OnMoveToPointer;
		primaryAction.performed -= OnPrimaryAction;
		stopAction.performed -= OnStop;
		skillQAction.performed -= OnSkillQ;
		skillWAction.performed -= OnSkillW;
		skillEAction.performed -= OnSkillE;
		skillRAction.performed -= OnSkillR;
	}

	private void OnMoveToPointer(InputAction.CallbackContext context) => MoveRequested?.Invoke();
	private void OnPrimaryAction(InputAction.CallbackContext context) => PrimaryActionRequested?.Invoke();
	private void OnStop(InputAction.CallbackContext context) => StopRequested?.Invoke();
	private void OnSkillQ(InputAction.CallbackContext context) => SkillSlotRequested?.Invoke(SkillSlotKey.Q);
	private void OnSkillW(InputAction.CallbackContext context) => SkillSlotRequested?.Invoke(SkillSlotKey.W);
	private void OnSkillE(InputAction.CallbackContext context) => SkillSlotRequested?.Invoke(SkillSlotKey.E);
	private void OnSkillR(InputAction.CallbackContext context) => SkillSlotRequested?.Invoke(SkillSlotKey.R);
}
