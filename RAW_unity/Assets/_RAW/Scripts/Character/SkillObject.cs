using UnityEngine;

public class SkillObject : MonoBehaviour
{
    private SkillSpec spec;
    private Enemy targetEnemy;
    private Vector3 destinationPosition;
    private bool hasAppliedDamage;

    public void Initialize(
        SkillSpec skillSpec,
        Vector3 skillDestinationPosition,
        Enemy skillTargetEnemy
    )
    {
        spec = skillSpec;
        targetEnemy = skillTargetEnemy;
        destinationPosition = skillDestinationPosition;

        if (spec.castType == CastType.bar)
        {
            RotateTowardsDestination();
        }
        else
        {
            Destroy(gameObject, spec.remainTime);
        }

        if (spec.castType == CastType.target)
            ApplyDamageToTarget();
    }

    private void Update()
    {
        if (spec == null || spec.castType != CastType.bar)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destinationPosition,
            spec.moveSpeed * Time.deltaTime
        );

        if (transform.position == destinationPosition)
            Destroy(gameObject);
    }

    private void RotateTowardsDestination()
    {
        Vector2 direction = destinationPosition - transform.position;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        float angleDegrees = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 현재 Bar 스킬 이펙트 스프라이트의 기본 진행 방향은 왼쪽이다.
        if (transform.localScale.x > 0f)
            angleDegrees -= 180f;

        transform.rotation = Quaternion.AngleAxis(angleDegrees, Vector3.forward);
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
