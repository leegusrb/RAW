using UnityEngine;

public class SkillObject : MonoBehaviour
{
    private SkillSpec spec;
    private Enemy targetEnemy;
    private bool hasAppliedDamage;

    public void Initialize(SkillSpec skillSpec, Enemy skillTargetEnemy)
    {
        spec = skillSpec;
        targetEnemy = skillTargetEnemy;
        Destroy(gameObject, spec.remainTime);

        if (spec.castType == CastType.target)
            ApplyDamageToTarget();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (spec == null || spec.castType == CastType.target || hasAppliedDamage)
            return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null)
            return;

        enemy.TakeDamage(spec.damage);
        hasAppliedDamage = true;
    }

    private void ApplyDamageToTarget()
    {
        if (targetEnemy == null)
        {
            Debug.LogError("타겟형 스킬에 대상 적이 없습니다.", this);
            return;
        }

        targetEnemy.TakeDamage(spec.damage);
        hasAppliedDamage = true;
    }
}
