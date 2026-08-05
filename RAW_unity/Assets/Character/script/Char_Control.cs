using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

    SkillSpec currentCastingSkill;
    private bool isIndicatingSkill;
    private string currentCastingSkillKey;
    private Coroutine currentActivatingSkillCoroutine;
    Vector2 currentActivatingSkillTargetPosition;
    GameObject currentActivatingSKillTarget;
    float currentCastingSkillRangeRadius;
    
    void Start()
    {
        HideIndicator();
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            StopActivatingSkill();
            SetTargetPos();
            if (isIndicatingSkill)
                HideIndicator();
            
        }
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.A))
        {
            string now_input_key = Input.inputString.ToLower();
            ShowIndicator(now_input_key);            
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (isIndicatingSkill && IsPossibleToActivateSkill())
            {                
                SetSkillTarget(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                ActivateSkill();
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopActivatingSkill();
            StopMove();
            if (isIndicatingSkill)
            {
                HideIndicator();
            }
            
        }
        if (characterState.IsMovable == true)
        {
            MoveCharacter();
        }
        else
        {
            StopMove();
        }
        if (isIndicatingSkill)
        {
            IndicateSkill();
        }
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


    void ShowIndicator(string now_input_key)
    {
        HideIndicator();
        currentCastingSkill = DataBase.Instance.mySkillKeyMap[now_input_key];
        if (currentCastingSkill.castType == CastType.bar)
        {
            skillBarIndicator.transform.localScale = new Vector2(currentCastingSkill.range, currentCastingSkill.size);
            skillBarIndicator.SetActive(true);
        }
        else if (currentCastingSkill.castType == CastType.target)
        {
            skillTargetingIndicator.SetActive(true);
            skillTargetingIndicator.GetComponent<TargettingSkillIndicator>().target = currentCastingSkill.targettingSkillTarget;            
        }
        else if (currentCastingSkill.castType == CastType.area)
        {
            skillAreaIndicator.transform.localScale = new Vector2(currentCastingSkill.size, currentCastingSkill.size);
            skillAreaIndicator.SetActive(true);
        }
        skillRangeIndicator.transform.localScale = new Vector2(currentCastingSkill.range, currentCastingSkill.range);
        skillRangeIndicator.SetActive(true);
        currentCastingSkillRangeRadius = Vector2.Distance(skillRangeIndicator.transform.GetChild(0).position, skillRangeIndicator.transform.GetChild(1).position);
        isIndicatingSkill = true;
        currentCastingSkillKey = now_input_key;
    }

    void HideIndicator()
    {
        skillAreaIndicator.SetActive(false);
        skillTargetingIndicator.SetActive(false);
        skillBarIndicator.SetActive(false);
        skillRangeIndicator.SetActive(false);
        isIndicatingSkill = false;
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
            FlipTowards(targetPos);
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
        if(characterState.IsMoving == false)
        {
            characterState.IsMoving = true;
            animator.SetBool("isMoving", true);
        }
        transform.position += (Vector3)(targetDirection * characterState.moveSpeed * Time.deltaTime);        
    }

    void StopMove()
    {
        targetPos = transform.position;
        if (characterState.IsMoving == true)
        {
            characterState.IsMoving = false;
            animator.SetBool("isMoving", false);
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
        if (currentActivatingSKillTarget != null)
            currentActivatingSkillTarget = currentActivatingSKillTarget.GetComponent<Enemy>();

        currentActivatingSkillCoroutine = StartCoroutine(CoroutineActivateSkill(currentCastingSkill, currentCastingSkillRangeRadius, currentCastingSkillKey, currentActivatingSkillTarget));
        HideIndicator();
    }
    bool IsPossibleToActivateSkill()
    {
        // check mana << todo

        // check target
        bool isPossilbe = false;
        switch (currentCastingSkill.castType)
        {
            case CastType.target:
                if (skillTargetingIndicator.GetComponent<TargettingSkillIndicator>().targetableIndicator.activeSelf)                                    
                    isPossilbe = true;                
                else
                    isPossilbe = false;
                break;
            case CastType.area:
            case CastType.bar:
                isPossilbe = true;
                break;
        }

        return isPossilbe;
    }
    private void SetSkillTarget(Vector2 mousePos)
    {
        currentActivatingSkillTargetPosition = transform.position;
        currentActivatingSKillTarget = null;
        switch (currentCastingSkill.castType)
        {
            case CastType.target:
                currentActivatingSKillTarget = skillTargetingIndicator.GetComponent<TargettingSkillIndicator>().targettingTarget;
                currentActivatingSkillTargetPosition = currentActivatingSKillTarget.GetComponent<Enemy>().hitPoint;
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
