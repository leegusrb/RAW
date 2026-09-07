using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private EnemySpec spec;

    [SerializeField]
    private CharacterState characterState;

    private void Awake()
    {
        if (characterState == null)
            characterState = GetComponent<CharacterState>();
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

    private void Death()
    {
        gameObject.SetActive(false);
    }
}
