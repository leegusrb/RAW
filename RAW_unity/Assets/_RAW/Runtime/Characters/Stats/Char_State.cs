using System;
using UnityEngine;

public class Char_State : MonoBehaviour
{
    [SerializeField] private int healthPoint = 100;
    [SerializeField] private int manaPoint = 100;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool isMovable = true;

    public int HP
    {
        get => healthPoint;
        set => healthPoint = Mathf.Max(0, value);
    }

	public int MP
	{
		get => manaPoint;
		set => manaPoint = Mathf.Max(0, value);
	}

	public float MoveSpeed => moveSpeed;
	public bool IsMovable => isMovable;

	public void SetMovable(bool value)
	{
		isMovable = value;
	}
}
