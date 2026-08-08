using System.Collections;
using UnityEngine;

public class Char_Control : MonoBehaviour{
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private LayerMask obstacleLayer;    
    [SerializeField]
    private LayerMask playerLayer;
    [SerializeField]
    private LayerMask monsterLayer;

    
    [SerializeField]
    private GameObject targetPointer;
    private Vector2 targetPos;
    private float pointingDuration = 0.25f;        
    private Coroutine targetPointing;

    private float obstacleAvoidDistance = 0.1f;
    private Vector2 obstacleAvoidDirection;
    private bool isFollowingWall = false;
    

    [SerializeField]
    private Char_State characterState;

    [SerializeField]
    private Animator animator;    


    [SerializeField]
    private GameObject skillAreaIndicator;
    [SerializeField]
    private GameObject skillTargetingIndicator;
    [SerializeField]
    private GameObject skillBarIndicator;
    [SerializeField]
    private GameObject skillRangeIndicator;

	private ISkillRuntime skillRuntime;

    private SkillSpec currentCastingSkill;
	private KeyMapping currentCastingSlot;
    private bool isIndicatingSkill;

    private string currentCastingSkillKey;
    private Coroutine currentActivatingSkillCoroutine;
    private Vector2 currentActivatingSkillTargetPosition;
    private GameObject currentActivatingSkillTargetObject;
    private float currentCastingSkillRangeRadius;

	private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

	private void Awake()
	{
		skillRuntime = GetComponent<ISkillRuntime>();

		if (skillRuntime == null)
			Debug.LogError("ISkillRuntime 구현체를 찾을 수 없습니다.", this);
	}
    
    void Start()
    {
        HideIndicator();
        targetPos = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            StopActivatingSkill();
            SetTargetPos();

            if (isIndicatingSkill)
                HideIndicator();
            
        }

		if (TryGetPressedSkillSlot(out KeyMapping pressedSkillSlot))
		{
			ShowIndicator(pressedSkillSlot);
		}

		if (Input.GetMouseButtonDown(0) && isIndicatingSkill && IsPossibleToActivateSkill())
		{
			SetSkillTarget(Camera.main.ScreenToWorldPoint(Input.mousePosition));
			RequestCurrentSkill();
		}

        if (Input.GetKeyDown(KeyCode.S))
        {
            StopActivatingSkill();
            StopMove();

            if (isIndicatingSkill)
                HideIndicator();
        }
		
        if (characterState != null && characterState.IsMovable)
            MoveCharacter();
        else
            StopMove();
		
        if (isIndicatingSkill)
            IndicateSkill();
    }

	private bool TryGetPressedSkillSlot(out KeyMapping skillSlot)
	{
		if (Input.GetKeyDown(KeyCode.Q))
		{
			skillSlot = KeyMapping.Q;
			return true;
		}

		if (Input.GetKeyDown(KeyCode.W))
		{
			skillSlot = KeyMapping.W;
			return true;
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			skillSlot = KeyMapping.E;
			return true;
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			skillSlot = KeyMapping.R;
			return true;
		}

		if (Input.GetKeyDown(KeyCode.A))
		{
			skillSlot = KeyMapping.A;
			return true;
		}

		skillSlot = default;
		return false;
	}

    void IndicateSkill()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos = new Vector3(mousePos.x, mousePos.y, -1);
        switch (currentCastingSkill.castType)
        {
            case CastType.bar:
                IndicateBarType(mousePos);
                break;
            case CastType.target:
                IndicateTargertingType(mousePos);
                break;
            case CastType.area:
                IndicateAreaType(mousePos);
                break;
            default:
                Debug.LogError("wrong skill cast type");
                break;
        }
    }

    void IndicateBarType(Vector3 mousePos)
    {
        //Vector2 target = skillRangeAreaBar.transform.position;
        Vector2 target = transform.position;
        float angle_pi = Mathf.Atan2(mousePos.y - target.y, mousePos.x - target.x);
        float angle_rad = angle_pi * Mathf.Rad2Deg;

        if (transform.localScale.x > 0)
            angle_rad -= 180;
        skillBarIndicator.transform.rotation = Quaternion.AngleAxis(angle_rad, Vector3.forward);

        //with cosine equation
        //float ratio = (float)(Mathf.Cos(2 * angle_pi) / 4 + 0.75);

        /*
         * with two dim equation
        angle_pi = Mathf.Abs(angle_pi) / Mathf.PI;
        float ratio = 2 * angle_pi * angle_pi - 2 * angle_pi + 1;

        */

        //with ellipse equation
        float a = 1f; // long axis
        float b = 0.5f; //short axis
        float slope = (mousePos.y - target.y) / (mousePos.x - target.x);
        float t = Mathf.Atan((slope * a) / b);
        float x_intersect = target.x + a * Mathf.Cos(t);
        float y_intersect = target.y + b * Mathf.Sin(t);
        float ratio = Mathf.Sqrt((x_intersect - target.x) * (x_intersect - target.x) + (y_intersect - target.y) * (y_intersect - target.y));


        float scaled_x = currentCastingSkill.range * ratio;

        skillBarIndicator.transform.localScale = new Vector2(scaled_x, currentCastingSkill.size);
    }

    void IndicateTargertingType(Vector3 mousePos)
    {
        skillTargetingIndicator.transform.position = mousePos;       
    }

    void IndicateAreaType(Vector3 mousePos)
    {
        skillAreaIndicator.transform.position = mousePos;
    }

    private void ShowIndicator(KeyMapping skillSlot)
    {
        HideIndicator();

		if (skillRuntime == null)
		{
			Debug.LogError("스킬 런타임이 연결되지 않았습니다.", this);
			return;
		}

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

		currentCastingSlot = skillSlot;
        currentCastingSkill = skill;
		currentCastingSkillKey = skillSlot.ToString().ToLowerInvariant();

		switch (currentCastingSkill.castType)
		{
			case CastType.bar:
				skillBarIndicator.transform.localScale = new Vector2(currentCastingSkill.range, currentCastingSkill.size);
				skillBarIndicator.SetActive(true);
				break;

			case CastType.target:
            	skillTargetingIndicator.SetActive(true);

				TargettingSkillIndicator targetIndicator = skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

				if (targetIndicator != null)
					targetIndicator.target = currentCastingSkill.targettingSkillTarget;

				break;

			case CastType.area:
				skillAreaIndicator.transform.localScale = new Vector2(currentCastingSkill.size, currentCastingSkill.size);
				skillAreaIndicator.SetActive(true);
				break;

			default:
				Debug.LogError($"지원하지 않는 스킬 CastType입니다: {currentCastingSkill.castType}", this);

				currentCastingSkill = null;
				return;

		}

        skillRangeIndicator.transform.localScale = new Vector2(currentCastingSkill.range, currentCastingSkill.range);
        skillRangeIndicator.SetActive(true);
        currentCastingSkillRangeRadius = Vector2.Distance(skillRangeIndicator.transform.GetChild(0).position, skillRangeIndicator.transform.GetChild(1).position);
        isIndicatingSkill = true;
    }

	private void RequestCurrentSkill()
	{
		if (!isIndicatingSkill || currentCastingSkill == null)
			return;

		if (skillRuntime == null)
		{
			Debug.LogError("스킬 런타임이 연결되지 않았습니다.", this);

			HideIndicator();
			return;
		}

		double remainingCooldown = skillRuntime.GetRemainingCooldown(currentCastingSkill.SkillId);

		if (remainingCooldown > 0d)
		{
			Debug.LogWarning($"{currentCastingSlot} 스킬은 쿨다운 중입니다. 남은 시간={remainingCooldown:F2}", this);

			HideIndicator();
			return;
		}

		SkillUseRequestResult requestResult = skillRuntime.RequestUseSkill(currentCastingSlot);

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
				Debug.LogWarning($"{currentCastingSlot} 스킬 요청이 거절되었습니다.", this);

				HideIndicator();
				return;
		}
	}

    void HideIndicator()
    {
        skillAreaIndicator.SetActive(false);
        skillTargetingIndicator.SetActive(false);
        skillBarIndicator.SetActive(false);
        skillRangeIndicator.SetActive(false);

        isIndicatingSkill = false;
		currentCastingSkill = null;
    }

    void SetTargetPos()
    {
        Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);

        if (hit.collider != null)
        {
            targetPos = hit.point;
            if (targetPointing != null)
            {
                StopCoroutine(targetPointing);
            }
            targetPointing = StartCoroutine(PointingTarget());
        }
    }

    void MoveCharacter()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetDirection = (targetPos - currentPosition).normalized;
        float targetDist = Vector2.Distance(currentPosition, targetPos);
        if (Vector2.Distance(targetPos, currentPosition) > 0.001f)
        {
            RaycastHit2D hit = Physics2D.Raycast(currentPosition, targetDirection, targetDist, obstacleLayer);
            bool isObstructed = CheckObstacle(hit, currentPosition);
            if (isObstructed)
            {
                if (isFollowingWall)
                {
                    DoMove(obstacleAvoidDirection);
                }
                else
                {
                    CalObstacleAvoidDirection(hit);
                    isFollowingWall = true;
                }
            }
            else
            {
                isFollowingWall = false;
                DoMove(targetDirection);
            }
        }
        else
        {
            StopMove();
        }
    }
    bool CheckObstacle(RaycastHit2D hit, Vector2 currentPosition)
    {        
        if (hit.collider != null)
        {
            float obstracleDistanc = Vector2.Distance(currentPosition, hit.point);
            if (obstracleDistanc < obstacleAvoidDistance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }

    }

    void DoMove(Vector2 targetDirection)
    {        
        if (!characterState.IsMoving)
        {
            characterState.IsMoving = true;
            animator.SetBool(IsMovingHash, true);
        }

        transform.position += (Vector3)(targetDirection * characterState.moveSpeed * Time.deltaTime);
        
		SetFacingByDirection(targetDirection);
    }

	private void SetFacingByDirection(Vector2 direction)
	{
		if (Mathf.Abs(direction.x) < 0.001f)
			return;

		Vector3 scale = transform.localScale;
		float xMagnitude = Mathf.Abs(scale.x);

		scale.x = direction.x > 0f
			? -xMagnitude
			: xMagnitude;

		transform.localScale = scale;
	}

    void StopMove()
    {
        targetPos = transform.position;

        if (characterState != null && characterState.IsMoving)
        {
            characterState.IsMoving = false;
            animator.SetBool(IsMovingHash, false);
        }

        isFollowingWall = false;
    }

    void CalObstacleAvoidDirection(RaycastHit2D hit)
    {
        Vector2 normal = hit.normal;
        // 법선에 수직인 접선 방향 2개 계산 (시계방향, 반시계방향)
        Vector2 tangentCW = new Vector2(-normal.y, normal.x);  // 시계 방향 접선
        Vector2 tangentCCW = new Vector2(normal.y, -normal.x); // 반시계 방향 접선

        // 목표 방향과 각 접선 방향의 내적 계산해서 가까운 쪽 선택
        Vector2 fromHitToTarget = ((Vector2)targetPos - hit.point).normalized;

        float dotCW = Vector2.Dot(fromHitToTarget, tangentCW);
        float dotCCW = Vector2.Dot(fromHitToTarget, tangentCCW);
        obstacleAvoidDirection = (dotCW > dotCCW ? tangentCW : tangentCCW).normalized;
        Debug.DrawRay(transform.position, obstacleAvoidDirection, Color.blue);
    }

    IEnumerator PointingTarget()
    {
        if (targetPointer == null)
        {
            Debug.LogWarning("목표 지점 포인터가 연결되지 않았습니다.", this);

            yield break;
        }

        targetPointer.SetActive(true);
        targetPointer.transform.position = targetPos;

        float halfDuration = pointingDuration / 2f;
        
        float t = 0f;
        while (t < halfDuration)
        {
            float value = Mathf.Lerp(0f, 1f, t / halfDuration);            
            t += Time.deltaTime;
            targetPointer.transform.localScale = Vector3.one * value;
            yield return null;
        }
        t = 0f;
        while (t < halfDuration)
        {
            float value = Mathf.Lerp(1f, 0f, t / halfDuration);            
            t += Time.deltaTime;
            targetPointer.transform.localScale = Vector3.one * value;
            yield return null;
        }

        yield return null;
        targetPointer.SetActive(false);
    }

    void ActivateSkill()
    {
        StopActivatingSkill();

        Enemy currentActivatingSkillTarget = null;

        if (currentActivatingSkillTargetObject != null)
            currentActivatingSkillTarget = currentActivatingSkillTargetObject.GetComponent<Enemy>();

        currentActivatingSkillCoroutine = StartCoroutine(CoroutineActivateSkill(currentCastingSkill, currentCastingSkillRangeRadius, currentCastingSkillKey, currentActivatingSkillTarget));
        HideIndicator();
    }

    private bool IsPossibleToActivateSkill()
    {
        // check mana << todo

        // check target
		if (currentCastingSkill == null)
			return false;

		if (currentCastingSkill.castType != CastType.target)
			return true;

		TargettingSkillIndicator indicator = skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

		return indicator != null && indicator.targettingTarget != null;
    }

    private void SetSkillTarget(Vector2 mousePos)
    {
        currentActivatingSkillTargetPosition = transform.position;
        currentActivatingSkillTargetObject = null;

        switch (currentCastingSkill.castType)
        {
            case CastType.target:
				TargettingSkillIndicator indicator = skillTargetingIndicator.GetComponent<TargettingSkillIndicator>();

				if (indicator == null || indicator.targettingTarget == null)
					return;
	
                currentActivatingSkillTargetObject = indicator.targettingTarget;

				Enemy enemy = currentActivatingSkillTargetObject.GetComponent<Enemy>();

				if (enemy != null)
                	currentActivatingSkillTargetPosition = enemy.hitPoint;

                break;
	
            case CastType.area:
                currentActivatingSkillTargetPosition = mousePos;
                break;

            case CastType.bar:
                currentActivatingSkillTargetPosition = transform.position;
                break;
        }
    }

    private IEnumerator CoroutineActivateSkill(SkillSpec currentActivatingSkill, float skillRangeRadius, string skillInputKey, Enemy currentActivatingSkillTarget)
    {
        
        if (currentActivatingSkill.castType == CastType.target || currentActivatingSkill.castType == CastType.area)
        {
            while (IsInsideRange(transform.position, currentActivatingSkillTargetPosition, skillRangeRadius) == false)
            {
                targetPos = currentActivatingSkillTargetPosition;
                yield return null;
            }
        }

        characterState.IsActivatingSkill = true;
        FlipTowards(currentActivatingSkillTargetPosition);
        animator.SetTrigger(skillInputKey);

        // 선딜
        yield return new WaitForSeconds(currentActivatingSkill.preDelay);

        // 발동: 스킬 오브젝트 생성
        GameObject skillObject = Instantiate(
            currentActivatingSkill.skillPrefab,
            GetSkillGeneratePosition(currentActivatingSkill.castType, currentActivatingSkillTargetPosition),
            Quaternion.identity
        );
        skillObject.transform.localScale = new Vector3(transform.localScale.x < 0 ? -1 : 1, 1, 1);

        SkillObject skillObjectComponent = skillObject.GetComponent<SkillObject>();
        if (skillObjectComponent != null)
            skillObjectComponent.Initialize(currentActivatingSkill, currentActivatingSkillTarget);

        // 후딜
        yield return new WaitForSeconds(currentActivatingSkill.postDelay);

        characterState.IsActivatingSkill = false;
        currentActivatingSkillCoroutine = null;
    }

    private Vector3 GetSkillGeneratePosition(CastType currentActivatingSkillCastType, Vector2 activatingMousePosition)
    {
        Vector3 position = transform.position;
        switch (currentActivatingSkillCastType)
        {
            case CastType.area:
            case CastType.target:
                position = activatingMousePosition;
                break;
            case CastType.bar:
                position = transform.position;
                break;           
        }
        return position;
    }

    private bool IsInsideRange(Vector2 center, Vector2 target, float SemiMajorAxisLength)
    {
        float SemiMinorAxisLength = SemiMajorAxisLength * 0.5f;

        Vector2 p = target - center;

        float value =
            (p.x * p.x) / (SemiMajorAxisLength * SemiMajorAxisLength) +
            (p.y * p.y) / (SemiMinorAxisLength * SemiMinorAxisLength);

        return value <= 1f;
    }

    private void StopActivatingSkill()
    {
        if (currentActivatingSkillCoroutine == null)
            return;

        StopCoroutine(currentActivatingSkillCoroutine);
        currentActivatingSkillCoroutine = null;
        characterState.IsActivatingSkill = false;
        targetPos = transform.position;
    }


    public static Vector2 GetEllipseIntersection(Vector2 center, Vector2 target, float a)
    {
        float b = a * 0.5f;

        Vector2 dir = target - center;
        dir.Normalize();

        float scale = 1.0f /
            Mathf.Sqrt(
                (dir.x * dir.x) / (a * a) +
                (dir.y * dir.y) / (b * b)
            );

        return center + dir * scale;
    }

    /// <summary>
    /// A와 C 사이 거리
    /// </summary>
    public static float GetDistanceToEllipse(Vector2 center, Vector2 target, float a)
    {
        Vector2 c = GetEllipseIntersection(center, target, a);
        return Vector2.Distance(target, c);
    }


    Vector3 GetBarIndicatorVisualScale()
    {
        return skillBarIndicator.transform.lossyScale;
    }

    //void OnDrawGizmos()
    //{
    //    if (targetPos == null) return;
    //    Gizmos.color = Color.red;
    //    Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
    //    Gizmos.DrawRay(transform.position, dir * Vector2.Distance(transform.position, targetPos));
    //}

    void FlipTowards(Vector3 position)
    {
        Vector3 scale = transform.localScale;

        if (position.x > transform.position.x)
            scale.x = -Mathf.Abs(scale.x);
        else if (position.x < transform.position.x)
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }


}
