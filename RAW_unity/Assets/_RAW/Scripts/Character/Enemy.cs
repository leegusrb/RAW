using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector2 hitPoint;
    [SerializeField]
    private Transform hitPositionObject;

    [SerializeField]
    private EnemySpec spec;

    [SerializeField]
    private Char_State characterState;

    private void Awake()
    {
        hitPoint = hitPositionObject.position;

        if (characterState == null)
            characterState = GetComponent<Char_State>();
    }
    void Start()
    {
        if (characterState == null || spec == null)
        {
            Debug.LogError("Enemy state or spec is not assigned.", this);
            return;
        }

        characterState.InitializeHealth(spec.maxHealthPoint);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public void TakeDamage(float damage)
    {
        if (characterState == null)
        {
            Debug.LogError("Enemy에 Char_State가 연결되어 있지 않습니다.", this);
            return;
        }

        characterState.TakeDamage(damage);
    }

    private void Death()
    {
        gameObject.SetActive(false);
    }
}
