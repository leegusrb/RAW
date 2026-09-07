using UnityEngine;

public class CharacterState : MonoBehaviour
{
	[Header("Status")]
	
    [SerializeField] private int healthPoint = 100;
    [SerializeField] private int manaPoint = 100;
    [SerializeField] private int maxHealthPoint = 100;
    [SerializeField] private int maxManaPoint = 100;

	[Header("Movement")]
    public float moveSpeed;
    public bool isMovable = true;

	[Header("UI")]    
	
	[SerializeField]
    private HealthManager healthBar;

    private bool isMoving;
    private bool isActivatingSkill;

	public int HP
	{
		get => healthPoint;
		set
		{
			healthPoint = Mathf.Max(0, value);
			UpdateHealthBar();
		}
	}

	public int MP
	{
		get => manaPoint;
		set => manaPoint = Mathf.Max(0, value);
	}

    public int MaxHealth
    {
        get => maxHealthPoint;
        private set => maxHealthPoint = Mathf.Max(0, value);
    }

    public int CurrentHealth => healthPoint;

    public bool IsMoving
    {
        get => isMoving;
        set => isMoving = value;
    }

    public bool IsActivatingSkill
    {
        get => isActivatingSkill;
        set => isActivatingSkill = value;
    }

    public bool IsMovable => isMovable && !isActivatingSkill;

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
			return;
		
		int damageAmount = Mathf.CeilToInt(damage);

		HP = Mathf.Max(0, HP - damageAmount);

		if (HP == 0)
			Death();
    }

    public void InitializeHealth(float maxHealth)
    {
        MaxHealth = Mathf.RoundToInt(maxHealth);
        HP = MaxHealth;
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
            healthBar.SetHealth(CurrentHealth, MaxHealth);
    }

    private void Death()
    {

    }
}
