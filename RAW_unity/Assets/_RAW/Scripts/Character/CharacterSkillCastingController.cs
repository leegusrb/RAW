using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterControl))]
[RequireComponent(typeof(CharacterState))]
[RequireComponent(typeof(CharacterAnimation))]
public class CharacterSkillCastingController : MonoBehaviour
{
    private sealed class SkillCastContext
    {
        public SkillSpec Skill;
        public float RangeRadius;
        public Vector2 TargetPosition;
        public Vector2 CastDirection;
        public SkillTarget Target;
    }

    [SerializeField] private CharacterControl characterControl;
    [SerializeField] private CharacterState characterState;
    [SerializeField] private CharacterAnimation characterAnimation;

    [SerializeField] private GameObject skillAreaIndicator;
    [SerializeField] private GameObject skillTargetingIndicator;
    [SerializeField] private GameObject skillBarIndicator;
    [SerializeField] private GameObject skillRangeIndicator;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private TargettingSkillIndicator targetIndicator;

    private ISkillRuntime skillRuntime;

    private SkillSpec currentCastingSkill;
    private bool isIndicatingSkill;

    private Coroutine currentActivatingSkillCoroutine;
    private float currentCastingSkillRangeRadius;
    private SkillCastContext pendingSkillCast;

    public bool IsTargeting => isIndicatingSkill;
    public bool IsSkillPending => pendingSkillCast != null;

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
            IndicateSkill(currentCastingSkill.castType);

        if (pendingSkillCast != null)
            UpdatePendingSkillCast();
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
                skillBarIndicator.SetActive(true);
                break;

            case CastType.target:
                targetIndicator.target = currentCastingSkill.targettingSkillTarget;
                skillTargetingIndicator.SetActive(true);
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

        SkillCastContext castContext = CreateSkillCastContext(
            mouseWorldPosition,
            currentCastingSkill,
            currentCastingSkillRangeRadius
        );

        if (castContext == null)
            return false;

        CancelPendingSkillCast();
        HideIndicator();

        if (RequiresRangeCheck(castContext.Skill) &&
            !IsInsideRange(transform.position, castContext.TargetPosition, castContext.RangeRadius))
        {
            pendingSkillCast = castContext;
            characterControl.SetMoveDestination(castContext.TargetPosition);
            return true;
        }

        RequestSkill(castContext);
        return true;
    }

    public void CancelCasting()
    {
        CancelPendingSkillCast();
        StopActivatingSkill();
        HideIndicator();
    }

    private void IndicateSkill(CastType currentCastingSkillCastType)
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        switch (currentCastingSkillCastType)
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
        Vector2 castDirection = GetBarCastDirection(center, mousePosition);
        float angleDegrees = Mathf.Atan2(
            castDirection.y,
            castDirection.x
        ) * Mathf.Rad2Deg;

        if (transform.localScale.x > 0f)
            angleDegrees -= 180f;

        skillBarIndicator.transform.rotation = Quaternion.AngleAxis(
            angleDegrees,
            Vector3.forward
        );

        float ratio = Vector2.Distance(
            center,
            GetEllipseIntersection(center, center + castDirection, 1f)
        );

        skillBarIndicator.transform.localScale = new Vector2(
            currentCastingSkill.range * ratio,
            currentCastingSkill.size
        );
    }

    private void RequestSkill(SkillCastContext castContext)
    {
        if (castContext == null || castContext.Skill == null)
            return;

        SkillSpec skill = castContext.Skill;
        double remainingCooldown = skillRuntime.GetRemainingCooldown(skill.SkillId);

        if (remainingCooldown > 0d)
        {
            Debug.LogWarning($"{skill.name} 스킬은 쿨다운 중입니다. 남은 시간={remainingCooldown:F2}", this);
            return;
        }

        SkillUseRequest skillUseRequest = new SkillUseRequest
        {
            skillId = skill.SkillId,
            target = new SkillTargetInfo
            {
                direction = castContext.CastDirection,
                targetPosition = castContext.TargetPosition
            }
        };

        SkillUseRequestResult requestResult = skillRuntime.RequestUseSkill(skillUseRequest);

        switch (requestResult)
        {
            case SkillUseRequestResult.ExecuteLocally:
                ActivateSkill(castContext);
                return;

            case SkillUseRequestResult.HandleByRuntime:
                return;

            case SkillUseRequestResult.Rejected:
            default:
                Debug.LogWarning($"{skill.name} 스킬 요청이 거절되었습니다.", this);
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

    private void ActivateSkill(SkillCastContext castContext)
    {
        StopActivatingSkill();

        currentActivatingSkillCoroutine = StartCoroutine(
            ActivateSkillCoroutine(castContext)
        );
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

    private SkillCastContext CreateSkillCastContext(
        Vector2 mousePosition,
        SkillSpec skill,
        float rangeRadius
    )
    {
        SkillCastContext castContext = new SkillCastContext
        {
            Skill = skill,
            RangeRadius = rangeRadius,
            TargetPosition = transform.position
        };

        switch (skill.castType)
        {
            case CastType.target:
                TargettingSkillIndicator indicator =
                    skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

                if (indicator == null || indicator.targettingTarget == null)
                    return null;

                castContext.Target = indicator.targettingTarget;
                RefreshTargetPosition(castContext);
                break;

            case CastType.area:
                castContext.TargetPosition = mousePosition;
                break;

            case CastType.bar:
                castContext.TargetPosition = mousePosition;
                castContext.CastDirection = GetBarCastDirection(
                    transform.position,
                    mousePosition
                );
                break;
        }

        return castContext;
    }

    private void UpdatePendingSkillCast()
    {
        SkillCastContext castContext = pendingSkillCast;

        if (!RefreshTargetPosition(castContext))
        {
            CancelPendingSkillCast();
            return;
        }

        if (!IsInsideRange(
            transform.position,
            castContext.TargetPosition,
            castContext.RangeRadius
        ))
        {
            characterControl.SetMoveDestination(castContext.TargetPosition);
            return;
        }

        pendingSkillCast = null;
        characterControl.StopMoving();
        RequestSkill(castContext);
    }

    private bool RefreshTargetPosition(SkillCastContext castContext)
    {
        if (castContext.Skill.castType != CastType.target)
            return true;

        if (castContext.Target == null)
            return false;

        castContext.TargetPosition = castContext.Target.HitPosition;

        return true;
    }

    private void CancelPendingSkillCast()
    {
        if (pendingSkillCast == null)
            return;

        pendingSkillCast = null;
        characterControl.StopMoving();
    }

    private static bool RequiresRangeCheck(SkillSpec skill)
    {
        return skill.castType == CastType.target || skill.castType == CastType.area;
    }

    private IEnumerator ActivateSkillCoroutine(SkillCastContext castContext)
    {
        SkillSpec skill = castContext.Skill;

        RefreshTargetPosition(castContext);

        characterState.IsActivatingSkill = true;
        FlipTowards(castContext.TargetPosition);

        characterAnimation.PlaySkill(skill);

        yield return new WaitForSeconds(skill.preDelay);

        GetSkillObjectPositions(
            castContext,
            out Vector3 spawnPosition,
            out Vector3 destinationPosition
        );

        skillRuntime.CreateSkillObject(
            skillSpec: skill,
            spawnPosition: spawnPosition,
            destinationPosition: destinationPosition,
            skillObjectLocalScale: new Vector3(
                transform.localScale.x < 0f ? -1f : 1f,
                1f,
                1f
            ),
            skillTarget: castContext.Target
        );

        yield return new WaitForSeconds(skill.postDelay);

        characterState.IsActivatingSkill = false;
        currentActivatingSkillCoroutine = null;
    }

    private void GetSkillObjectPositions(
        SkillCastContext castContext,
        out Vector3 spawnPosition,
        out Vector3 destinationPosition
    )
    {
        if (castContext.Skill.castType == CastType.bar)
        {
            spawnPosition = projectileSpawnPoint.position;
            Vector2 destination = GetRayEllipseIntersection(
                spawnPosition,
                castContext.CastDirection,
                transform.position,
                castContext.RangeRadius
            );
            destinationPosition = new Vector3(
                destination.x,
                destination.y,
                spawnPosition.z
            );
            return;
        }

        spawnPosition = castContext.TargetPosition;
        destinationPosition = spawnPosition;
    }

    private Vector2 GetBarCastDirection(Vector2 center, Vector2 target)
    {
        Vector2 direction = target - center;
        if (direction.sqrMagnitude > Mathf.Epsilon)
            return direction.normalized;

        return transform.localScale.x < 0f
            ? Vector2.right
            : Vector2.left;
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

        float semiMinorAxis = semiMajorAxis * 0.5f;
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
