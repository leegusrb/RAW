using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterControl))]
[RequireComponent(typeof(CharacterState))]
[RequireComponent(typeof(CharacterAnimation))]
public class CharacterSkillCastingController : MonoBehaviour
{
    [SerializeField] private CharacterControl characterControl;
    [SerializeField] private CharacterState characterState;
    [SerializeField] private CharacterAnimation characterAnimation;

    [SerializeField] private GameObject skillAreaIndicator;
    [SerializeField] private GameObject skillTargetingIndicator;
    [SerializeField] private GameObject skillBarIndicator;
    [SerializeField] private GameObject skillRangeIndicator;

    private ISkillRuntime skillRuntime;

    private SkillSpec currentCastingSkill;
    private bool isIndicatingSkill;

    private Coroutine currentActivatingSkillCoroutine;
    private Vector2 currentActivatingSkillTargetPosition;
    private GameObject currentActivatingSkillTargetObject;
    private float currentCastingSkillRangeRadius;

    public bool IsTargeting => isIndicatingSkill;

    private void Awake()
    {
        CacheComponents();
        HideIndicator();

        if (skillRuntime == null)
        {
            Debug.LogError("ISkillRuntime 구현체를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (isIndicatingSkill)
            IndicateSkill();
    }

    private void CacheComponents()
    {
        if (characterControl == null)
            characterControl = GetComponent<CharacterControl>();

        if (characterState == null)
            characterState = GetComponent<CharacterState>();

        if (characterAnimation == null)
            characterAnimation = GetComponent<CharacterAnimation>();

        skillRuntime = GetComponent<ISkillRuntime>();
    }

    public void BeginTargeting(KeyMapping skillSlot)
    {
        HideIndicator();

        if (!enabled)
            return;

        if (!skillRuntime.TryGetSkillForSlot(skillSlot, out SkillSpec skill))
        {
            Debug.LogWarning($"{skillSlot} 슬롯에 등록된 스킬이 없습니다.", this);
            return;
        }

        double remainingCooldown = skillRuntime.GetRemainingCooldown(skill.SkillId);

        if (remainingCooldown > 0d)
        {
            Debug.LogWarning($"{skillSlot} 스킬은 쿨다운 중입니다. 남은 시간={remainingCooldown:F2}", this);
            return;
        }

        currentCastingSkill = skill;

        switch (currentCastingSkill.castType)
        {
            case CastType.bar:
                skillBarIndicator.transform.localScale = new Vector2(
                    currentCastingSkill.range,
                    currentCastingSkill.size
                );
                skillBarIndicator.SetActive(true);
                break;

            case CastType.target:
                skillTargetingIndicator.SetActive(true);

                TargettingSkillIndicator targetIndicator =
                    skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

                if (targetIndicator != null)
                    targetIndicator.target = currentCastingSkill.targettingSkillTarget;
                break;

            case CastType.area:
                skillAreaIndicator.transform.localScale = new Vector2(
                    currentCastingSkill.size,
                    currentCastingSkill.size
                );
                skillAreaIndicator.SetActive(true);
                break;

            default:
                Debug.LogError($"지원하지 않는 스킬 CastType입니다: {currentCastingSkill.castType}", this);
                currentCastingSkill = null;
                return;
        }

        skillRangeIndicator.transform.localScale = new Vector2(
            currentCastingSkill.range,
            currentCastingSkill.range
        );
        skillRangeIndicator.SetActive(true);

        currentCastingSkillRangeRadius = Vector2.Distance(
            skillRangeIndicator.transform.GetChild(0).position,
            skillRangeIndicator.transform.GetChild(1).position
        );

        isIndicatingSkill = true;
    }

    public bool TryConfirmCasting(Vector2 mouseWorldPosition)
    {
        if (!isIndicatingSkill || !IsPossibleToActivateSkill())
            return false;

        SetSkillTarget(mouseWorldPosition, currentCastingSkill);
        RequestCurrentSkill(currentCastingSkill);
        return true;
    }

    public void CancelCasting()
    {
        StopActivatingSkill();
        HideIndicator();
    }

    private void IndicateSkill()
    {
        if (currentCastingSkill == null || Camera.main == null)
            return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = -1f;

        switch (currentCastingSkill.castType)
        {
            case CastType.bar:
                IndicateBarType(mousePosition);
                break;

            case CastType.target:
                skillTargetingIndicator.transform.position = mousePosition;
                break;

            case CastType.area:
                skillAreaIndicator.transform.position = mousePosition;
                break;

            default:
                Debug.LogError("지원하지 않는 스킬 CastType입니다.", this);
                break;
        }
    }

    private void IndicateBarType(Vector3 mousePosition)
    {
        Vector2 center = transform.position;
        float angleRadians = Mathf.Atan2(
            mousePosition.y - center.y,
            mousePosition.x - center.x
        );
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        if (transform.localScale.x > 0f)
            angleDegrees -= 180f;

        skillBarIndicator.transform.rotation = Quaternion.AngleAxis(
            angleDegrees,
            Vector3.forward
        );

        const float semiMajorAxis = 1f;
        const float semiMinorAxis = 0.5f;
        float slope = (mousePosition.y - center.y) / (mousePosition.x - center.x);
        float ellipseAngle = Mathf.Atan((slope * semiMajorAxis) / semiMinorAxis);
        float intersectX = center.x + semiMajorAxis * Mathf.Cos(ellipseAngle);
        float intersectY = center.y + semiMinorAxis * Mathf.Sin(ellipseAngle);
        float ratio = Vector2.Distance(center, new Vector2(intersectX, intersectY));

        skillBarIndicator.transform.localScale = new Vector2(
            currentCastingSkill.range * ratio,
            currentCastingSkill.size
        );
    }

    private void RequestCurrentSkill(SkillSpec skill)
    {
        if (!isIndicatingSkill || skill == null)
            return;

        double remainingCooldown = skillRuntime.GetRemainingCooldown(skill.SkillId);

        if (remainingCooldown > 0d)
        {
            Debug.LogWarning($"{skill.name} 스킬은 쿨다운 중입니다. 남은 시간={remainingCooldown:F2}", this);
            HideIndicator();
            return;
        }

        SkillUseRequestResult requestResult = skillRuntime.RequestUseSkill(skill.SkillId);

        switch (requestResult)
        {
            case SkillUseRequestResult.ExecuteLocally:
                ActivateSkill();
                return;

            case SkillUseRequestResult.HandleByRuntime:
                HideIndicator();
                return;

            case SkillUseRequestResult.Rejected:
            default:
                Debug.LogWarning($"{skill.name} 스킬 요청이 거절되었습니다.", this);
                HideIndicator();
                return;
        }
    }

    private void HideIndicator()
    {
        SetIndicatorActive(skillAreaIndicator, false);
        SetIndicatorActive(skillTargetingIndicator, false);
        SetIndicatorActive(skillBarIndicator, false);
        SetIndicatorActive(skillRangeIndicator, false);

        isIndicatingSkill = false;
        currentCastingSkill = null;
    }

    private static void SetIndicatorActive(GameObject indicator, bool isActive)
    {
        if (indicator != null)
            indicator.SetActive(isActive);
    }

    private void ActivateSkill()
    {
        StopActivatingSkill();

        Enemy targetEnemy = null;

        if (currentActivatingSkillTargetObject != null)
            targetEnemy = currentActivatingSkillTargetObject.GetComponent<Enemy>();

        currentActivatingSkillCoroutine = StartCoroutine(
            ActivateSkillCoroutine(
                currentCastingSkill,
                currentCastingSkillRangeRadius,
                targetEnemy
            )
        );

        HideIndicator();
    }

    private bool IsPossibleToActivateSkill()
    {
        if (currentCastingSkill == null)
            return false;

        if (currentCastingSkill.castType != CastType.target)
            return true;

        TargettingSkillIndicator indicator =
            skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

        return indicator != null && indicator.targettingTarget != null;
    }

    private void SetSkillTarget(Vector2 mousePosition, SkillSpec skill)
    {
        currentActivatingSkillTargetPosition = transform.position;
        currentActivatingSkillTargetObject = null;

        switch (skill.castType)
        {
            case CastType.target:
                TargettingSkillIndicator indicator =
                    skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

                if (indicator == null || indicator.targettingTarget == null)
                    return;

                currentActivatingSkillTargetObject = indicator.targettingTarget;

                Enemy enemy = currentActivatingSkillTargetObject.GetComponent<Enemy>();

                if (enemy != null)
                    currentActivatingSkillTargetPosition = enemy.hitPoint;
                break;

            case CastType.area:
                currentActivatingSkillTargetPosition = mousePosition;
                break;

            case CastType.bar:
                currentActivatingSkillTargetPosition = transform.position;
                break;
        }
    }

    private IEnumerator ActivateSkillCoroutine(
        SkillSpec skill,
        float skillRangeRadius,
        Enemy targetEnemy
    )
    {
        if (skill.castType == CastType.target || skill.castType == CastType.area)
        {
            while (!IsInsideRange(
                transform.position,
                currentActivatingSkillTargetPosition,
                skillRangeRadius
            ))
            {
                characterControl.MoveTo(currentActivatingSkillTargetPosition);
                yield return null;
            }
        }

        characterState.IsActivatingSkill = true;
        FlipTowards(currentActivatingSkillTargetPosition);

        characterAnimation.PlaySkill(skill);

        yield return new WaitForSeconds(skill.preDelay);

        GameObject skillObject = Instantiate(
            skill.skillPrefab,
            GetSkillGeneratePosition(skill.castType, currentActivatingSkillTargetPosition),
            Quaternion.identity
        );

        skillObject.transform.localScale = new Vector3(
            transform.localScale.x < 0f ? -1f : 1f,
            1f,
            1f
        );

        SkillObject skillObjectComponent = skillObject.GetComponent<SkillObject>();

        if (skillObjectComponent != null)
            skillObjectComponent.Initialize(skill, targetEnemy);

        yield return new WaitForSeconds(skill.postDelay);

        characterState.IsActivatingSkill = false;
        currentActivatingSkillCoroutine = null;
    }

    private Vector3 GetSkillGeneratePosition(CastType castType, Vector2 targetPosition)
    {
        switch (castType)
        {
            case CastType.area:
            case CastType.target:
                return targetPosition;

            case CastType.bar:
            default:
                return transform.position;
        }
    }

    private static bool IsInsideRange(Vector2 center, Vector2 target, float semiMajorAxis)
    {
        float semiMinorAxis = semiMajorAxis * 0.5f;
        Vector2 offset = target - center;

        float value =
            (offset.x * offset.x) / (semiMajorAxis * semiMajorAxis) +
            (offset.y * offset.y) / (semiMinorAxis * semiMinorAxis);

        return value <= 1f;
    }

    private void StopActivatingSkill()
    {
        if (currentActivatingSkillCoroutine == null)
            return;

        StopCoroutine(currentActivatingSkillCoroutine);
        currentActivatingSkillCoroutine = null;

        characterState.IsActivatingSkill = false;

        characterControl.StopMoving();
    }

    public static Vector2 GetEllipseIntersection(Vector2 center, Vector2 target, float semiMajorAxis)
    {
        float semiMinorAxis = semiMajorAxis * 0.5f;
        Vector2 direction = (target - center).normalized;

        float scale = 1f / Mathf.Sqrt(
            (direction.x * direction.x) / (semiMajorAxis * semiMajorAxis) +
            (direction.y * direction.y) / (semiMinorAxis * semiMinorAxis)
        );

        return center + direction * scale;
    }

    public static float GetDistanceToEllipse(Vector2 center, Vector2 target, float semiMajorAxis)
    {
        Vector2 intersection = GetEllipseIntersection(center, target, semiMajorAxis);
        return Vector2.Distance(target, intersection);
    }

    private void FlipTowards(Vector3 position)
    {
        Vector3 scale = transform.localScale;

        if (position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else if (position.x < transform.position.x)
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }
}
