using UnityEngine;

public class SkillTargetingController : MonoBehaviour
{
	[Header("Layers")]
	[SerializeField] private LayerMask playerLayer;
	[SerializeField] private LayerMask monsterLayer;

	[Header("Presenter")]
	[SerializeField] private SkillIndicatorPresenter indicatorPresenter;

	private SkillDefinition currentCastingSkill;
	private bool isIndicatingSkill;

	public bool IsIndicatingSkill => isIndicatingSkill;
	public SkillDefinition CurrentSkill => currentCastingSkill;

	private void Awake()
	{
		if (indicatorPresenter == null)
			indicatorPresenter = GetComponent<SkillIndicatorPresenter>();
	}

	private void Start()
	{
		Cancel();
	}

	public void BeginIndicate(SkillDefinition skill)
	{
		Cancel();

		if (skill == null)
		{
			Debug.LogWarning("SkillDefinition is null.", this);
			return;
		}

		if (indicatorPresenter == null)
		{
			Debug.LogWarning("SkillIndicatorPresenter is not assigned.", this);
			return;
		}

		currentCastingSkill = skill;

		switch (currentCastingSkill.castType)
		{
			case CastType.Bar:
				indicatorPresenter.ShowBar(currentCastingSkill.range, currentCastingSkill.size);
				break;

			case CastType.Target:
				indicatorPresenter.ShowTargeting();
				break;

			case CastType.Area:
				indicatorPresenter.ShowArea(currentCastingSkill.size);
				break;

			default:
				Debug.LogError("Wrong skill cast type.");
				return;
		}

		indicatorPresenter.ShowRange(currentCastingSkill.range);
		isIndicatingSkill = true;
	}

	public void Cancel()
	{
		indicatorPresenter?.HideAll();
		isIndicatingSkill = false;
		currentCastingSkill = null;
	}

	public bool ActivateCurrentSkill()
	{
		if (!TryBuildCastContext(out SkillCastContext context))
			return false;

		if (currentCastingSkill.skillPrefab == null)
		{
			Debug.LogWarning($"Skill prefab is not assigned: {currentCastingSkill.name}", this);
			Cancel();
			return false;
		}

		GameObject skillObject = Instantiate(
			currentCastingSkill.skillPrefab,
			context.position,
			Quaternion.Euler(0f, 0f, context.rotationZ)
		);

		if (skillObject.TryGetComponent(out SkillObject skillObjectComponent))
		{
			skillObjectComponent.Init(currentCastingSkill, transform, context);
		}
		else
		{
			ApplyFallbackTransform(skillObject.transform, context);
		}

		Cancel();
		return true;
	}

	public bool TryBuildCastContext(out SkillCastContext context)
	{
		context = default;

		if (!isIndicatingSkill || currentCastingSkill == null || indicatorPresenter == null)
			return false;

		switch (currentCastingSkill.castType)
		{
			case CastType.Bar:
				context.position = transform.position;
				context.rotationZ = indicatorPresenter.BarRotationZ;
				context.direction = indicatorPresenter.BarDirection;
				context.visualScale = indicatorPresenter.BarLossyScale;
				return true;

			case CastType.Area:
				context.position = indicatorPresenter.AreaPosition;
				context.rotationZ = 0f;
				context.direction = Vector2.zero;
				context.visualScale = Vector3.one * currentCastingSkill.size;
				return true;

			case CastType.Target:
				context.position = indicatorPresenter.TargetingPosition;
				context.rotationZ = 0f;
				context.direction = Vector2.zero;
				context.visualScale = Vector3.one;
				return true;

			default:
				Debug.LogError("Wrong skill cast type.", this);
				return false;
		}
	}

	public void Tick(Vector2 pointerScreenPosition)
	{
		if (!isIndicatingSkill)
			return;

		if (currentCastingSkill == null)
			return;

		Vector3 mouseWorldPosition = GetMouseWorldPosition(pointerScreenPosition);

		switch (currentCastingSkill.castType)
		{
			case CastType.Bar:
				IndicateBarType(mouseWorldPosition);
				break;

			case CastType.Target:
				IndicateTargetingType(mouseWorldPosition);
				break;

			case CastType.Area:
				IndicateAreaType(mouseWorldPosition);
				break;

			default:
				Debug.LogError("Wrong skill cast type.");
				break;
		}
	}

	private Vector3 GetMouseWorldPosition(Vector2 pointerScreenPosition)
	{
		if (Camera.main == null)
		{
			Debug.LogError("Camera.main is null.");
			return transform.position;
		}

		Vector3 worldPosition = Camera.main.ScreenToWorldPoint(
			new Vector3(pointerScreenPosition.x, pointerScreenPosition.y, 0f)
		);

		return new Vector3(worldPosition.x, worldPosition.y, -1f);
	}

	private void IndicateBarType(Vector3 mouseWorldPosition)
	{
		Vector2 target = transform.position;

		float anglePi = Mathf.Atan2(
			mouseWorldPosition.y - target.y,
			mouseWorldPosition.x - target.x
		);

		float angleRad = anglePi * Mathf.Rad2Deg;

		if (transform.localScale.x > 0)
			angleRad -= 180f;

		indicatorPresenter.SetBarRotation(
			Quaternion.AngleAxis(angleRad, Vector3.forward)
		);

		float a = 1f;
		float b = 0.5f;

		float dx = mouseWorldPosition.x - target.x;
		float dy = mouseWorldPosition.y - target.y;

		if (Mathf.Abs(dx) < 0.0001f)
			dx = dx < 0f ? -0.0001f : 0.0001f;

		float slope = dy / dx;

		float t = Mathf.Atan((slope * a) / b);
		float xIntersect = target.x + a * Mathf.Cos(t);
		float yIntersect = target.y + b * Mathf.Sin(t);

		float ratio = Mathf.Sqrt(
			(xIntersect - target.x) * (xIntersect - target.x) +
			(yIntersect - target.y) * (yIntersect - target.y)
		);

		float scaledX = currentCastingSkill.range * ratio;

		indicatorPresenter.SetBarScale(
			new Vector2(scaledX, currentCastingSkill.size)
		);
	}

	private void IndicateTargetingType(Vector3 mouseWorldPosition)
	{
		indicatorPresenter.SetTargetingPosition(mouseWorldPosition);

		LayerMask mask = 0;

		if (currentCastingSkill.targetAlly)
			mask |= playerLayer;

		if (currentCastingSkill.targetEnemy)
			mask |= monsterLayer;

		Collider2D hitObject = Physics2D.OverlapPoint(mouseWorldPosition, mask);

		indicatorPresenter.SetTargetingValid(hitObject != null);
	}

	private void IndicateAreaType(Vector3 mouseWorldPosition)
	{
		indicatorPresenter.SetAreaPosition(mouseWorldPosition);
	}

	private void ApplyFallbackTransform(Transform skillTransform, SkillCastContext context)
	{
		if (skillTransform == null)
			return;

		if (currentCastingSkill.castType == CastType.Bar)
			skillTransform.localScale = context.visualScale;
		else if (currentCastingSkill.castType == CastType.Area)
			skillTransform.localScale = Vector3.one * currentCastingSkill.size;
	}
}
