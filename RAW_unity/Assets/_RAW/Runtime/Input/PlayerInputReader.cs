using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInputReader : MonoBehaviour
{
	public event Action MoveRequested;
	public event Action StopRequested;
	public event Action PrimaryActionRequested;
	public event Action<SkillSlotKey> SkillSlotRequested;

	private void Update()
	{
		ReadMovementInput();
		ReadSkillInput();
		ReadActionInput();
	}

	private void ReadMovementInput()
	{
		if (Input.GetMouseButton(1))
		{
			MoveRequested?.Invoke();
		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			StopRequested?.Invoke();
		}
	}

	private void ReadSkillInput()
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			SkillSlotRequested?.Invoke(SkillSlotKey.Q);
		}
		else if (Input.GetKeyDown(KeyCode.W))
		{
			SkillSlotRequested?.Invoke(SkillSlotKey.W);
		}
		else if (Input.GetKeyDown(KeyCode.E))
		{
			SkillSlotRequested?.Invoke(SkillSlotKey.E);
		}
		else if (Input.GetKeyDown(KeyCode.R))
		{
			SkillSlotRequested?.Invoke(SkillSlotKey.R);
		}
	}

	private void ReadActionInput()
	{
		if (Input.GetMouseButtonDown(0))
		{
			PrimaryActionRequested?.Invoke();
		}
	}
}
