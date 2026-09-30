using System;
using UnityEngine;

public static class SkillExecutor
{
    public static void ExecutePrepared(
		SkillExecutionContext context,
		Action<SkillTarget, float> applyDamage
	)
	{
		GameObject skillObject = UnityEngine.Object.Instantiate(
			context.Skill.skillPrefab,
			context.SpawnPosition,
			Quaternion.identity
		);

		skillObject.transform.localScale = context.LocalScale;

		SkillObject skillObjectComponent = skillObject.GetComponent<SkillObject>();

		skillObjectComponent.Initialize(
			context.Skill,
			context.DestinationPosition,
			context.Target,
			applyDamage
		);
	}
}

public readonly struct SkillExecutionContext
{
	public SkillSpec Skill { get; }
	public Vector3 SpawnPosition { get; }
	public Vector3 DestinationPosition { get; }
	public Vector3 LocalScale { get; }
	public SkillTarget Target { get; }

	public SkillExecutionContext(SkillSpec skill, Vector3 spawnPosition, Vector3 destinationPosition, Vector3 localScale, SkillTarget target)
	{
		Skill = skill;
		SpawnPosition = spawnPosition;
		DestinationPosition = destinationPosition;
		LocalScale = localScale;
		Target = target;
	}
}

public static class SkillGeometry
{
	public const float VerticalRangeRatio = 0.5f;

	public static float GetRangeRadius(
		Vector2 center,
		Vector2 boundary
	)
	{
		return Vector2.Distance(center, boundary);
	}

    public static bool IsInsideRange(Vector2 center, Vector2 target, float semiMajorAxis)
    {
        float semiMinorAxis = semiMajorAxis * VerticalRangeRatio;
        Vector2 offset = target - center;

        float value =
            (offset.x * offset.x) / (semiMajorAxis * semiMajorAxis) +
            (offset.y * offset.y) / (semiMinorAxis * semiMinorAxis);

        return value <= 1f;
    }

	public static Vector2 GetEllipseIntersection(Vector2 center, Vector2 target, float semiMajorAxis)
    {
        float semiMinorAxis = semiMajorAxis * VerticalRangeRatio;
        Vector2 direction = (target - center).normalized;

        float scale = 1f / Mathf.Sqrt(
            (direction.x * direction.x) / (semiMajorAxis * semiMajorAxis) +
            (direction.y * direction.y) / (semiMinorAxis * semiMinorAxis)
        );

        return center + direction * scale;
    }

	public static void GetSkillObjectPositions(
		CastType castType,
		Vector3 casterPosition,
		Vector3 projectileSpawnPosition,
		Vector2 castDirection,
		Vector2 targetPosition,
		float rangeRadius,
        out Vector3 spawnPosition,
        out Vector3 destinationPosition
    )
    {
        if (castType == CastType.bar)
        {
            spawnPosition = projectileSpawnPosition;

            Vector2 destination = GetRayEllipseIntersection(
                spawnPosition,
                castDirection,
                casterPosition,
                rangeRadius
            );

            destinationPosition = new Vector3(
                destination.x,
                destination.y,
                spawnPosition.z
            );

            return;
        }

        spawnPosition = targetPosition;
        destinationPosition = spawnPosition;
    }

	private static Vector2 GetRayEllipseIntersection(
        Vector2 rayOrigin,
        Vector2 rayDirection,
        Vector2 ellipseCenter,
        float semiMajorAxis
    )
    {
        if (semiMajorAxis <= 0f || rayDirection.sqrMagnitude <= Mathf.Epsilon)
            return rayOrigin;

        rayDirection.Normalize();

        float semiMinorAxis = semiMajorAxis * VerticalRangeRatio;
        Vector2 originOffset = rayOrigin - ellipseCenter;

        float coefficientA =
            (rayDirection.x * rayDirection.x) / (semiMajorAxis * semiMajorAxis) +
            (rayDirection.y * rayDirection.y) / (semiMinorAxis * semiMinorAxis);
        float coefficientB = 2f * (
            (originOffset.x * rayDirection.x) / (semiMajorAxis * semiMajorAxis) +
            (originOffset.y * rayDirection.y) / (semiMinorAxis * semiMinorAxis)
        );
        float coefficientC =
            (originOffset.x * originOffset.x) / (semiMajorAxis * semiMajorAxis) +
            (originOffset.y * originOffset.y) / (semiMinorAxis * semiMinorAxis) -
            1f;

        float discriminant =
            coefficientB * coefficientB - 4f * coefficientA * coefficientC;

        if (discriminant < 0f)
            return rayOrigin;

        float intersectionDistance =
            (-coefficientB + Mathf.Sqrt(discriminant)) / (2f * coefficientA);

        if (intersectionDistance <= 0f)
            return rayOrigin;

        return rayOrigin + rayDirection * intersectionDistance;
    }
}
