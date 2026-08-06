using UnityEngine;

public class Char_State : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float maxHealthPoint = 100f;
    private float maxManaPoint;
    private bool isMoving;
    private bool isActivatingSkill;

    private bool isMovable;

    public float moveSpeed;

    private float currentHealthPoint;
    private float currentManaPoint;

    [SerializeField]
    private HealthManager healthBar;
    public float MaxHealth
    {
        get {  return maxHealthPoint; }
        private set { maxHealthPoint = value; }
    }

    public float CurrentHealth
    {
        get { return currentHealthPoint; }
        private set { currentHealthPoint = value; }
    }

    public bool IsMoving
    {
        get { return isMoving; }
        set { isMoving = value; }

    }

    public bool IsActivatingSkill
    {
        get { return isActivatingSkill; }
        set { isActivatingSkill = value; }
    }

    public bool IsMovable
    {
        get { return isMovable; }
    }

    void Start()
    {
        InitializeHealth(MaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        isMovable = true;
        if (isActivatingSkill)
            isMovable = false;
    }

    public void TakeDamage(float damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0f);

        UpdateHealthBar();

        if (CurrentHealth <= 0f)
            Death();
    }

    public void InitializeHealth(float maxHealth)
    {
        MaxHealth = Mathf.Max(maxHealth, 0f);
        CurrentHealth = MaxHealth;
        UpdateHealthBar();
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
