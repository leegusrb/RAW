using UnityEngine;

public class Skill_Warrior_Q : MonoBehaviour
{
    public float remainTime = 0.5f;
    private float remainTimer;

    void Update()
    {
        remainTimer += Time.deltaTime;
        if (remainTimer > remainTime)
            Destroy(gameObject);
    }
}
