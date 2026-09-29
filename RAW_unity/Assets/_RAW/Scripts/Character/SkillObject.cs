using System;
using UnityEngine;

public class SkillObject : MonoBehaviour
{
    private SkillSpec spec;
    private SkillTarget target;
    private Vector3 destinationPosition;
    private bool hasAppliedDamage;

	private Action<SkillTarget, int> applyDamage;

    public void Initialize(
        SkillSpec skillSpec,
        Vector3 skillDestinationPosition,
        SkillTarget skillTarget,
		Action<SkillTarget, int> damageApplier
    )
    {
        spec = skillSpec;
        target = skillTarget;
        destinationPosition = skillDestinationPosition;
		applyDamage = damageApplier;

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

        SkillTarget skillTarget = other.GetComponentInParent<SkillTarget>();
        
		ApplyDamageOnce(skillTarget);
    }

    private void ApplyDamageToTarget()
    {
        if (target == null)
        {
            Debug.LogError("타겟형 스킬에 대상이 없습니다.", this);
            return;
        }

        ApplyDamageOnce(target);
    }

	private void ApplyDamageOnce(SkillTarget skillTarget)
	{
		if (skillTarget == null || hasAppliedDamage)
			return;

		hasAppliedDamage = true;

		if (spec.damage <= 0f)
			return;

		int damageAmount = Mathf.CeilToInt(spec.damage);

		applyDamage?.Invoke(skillTarget, damageAmount);
	}
}
