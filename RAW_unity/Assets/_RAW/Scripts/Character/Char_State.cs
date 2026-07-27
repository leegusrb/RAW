using UnityEngine;

public class Char_State : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int healthPoint = 100;
    [SerializeField] private int manaPoint = 100;
    private bool isWalking;
    private bool isAttacking;

    public bool isMovable;

    public float moveSpeed;

    public int HP
    {
        get {  return healthPoint; }
        set { healthPoint = value; }
    }

	public int MP
	{
		get { return manaPoint; }
		set { manaPoint = value; }
	}

    public bool IsWalking
    {
        get { return isWalking; }
        set { isWalking = value; }

    }
}
