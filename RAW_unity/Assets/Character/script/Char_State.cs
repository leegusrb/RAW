using UnityEngine;

public class Char_State : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int healthPoint;
    private int manaPoint;
    private bool isWalking;
    private bool isAttacking;

    public bool isMovable;

    public float moveSpeed;

    public int HP
    {
        get {  return healthPoint; }
        set { healthPoint = value; }
    }

    public bool IsWalking
    {
        get { return isWalking; }
        set { isWalking = value; }

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
