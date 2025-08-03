using CustomDict;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Char_Control : MonoBehaviour
{
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private LayerMask obstacleLayer;
    [SerializeField]
    private GameObject targetPointer;
    [SerializeField]
    private float pointingDuration;


    private Vector2 targetPos;
    [SerializeField]
    private float obstacleAvoidDistance;
    private Coroutine targetPointing;
    [SerializeField]
    private bool isFollowingWall = false;
    private Vector2 obstacleAvoidDirection;

    [SerializeField]
    private Char_State characterState;

    [SerializeField]
    private Animator animator;

    private bool isMoving;


    [SerializeField]
    private GameObject skillAreaIndicator;
    [SerializeField]
    private GameObject skillTargetingIndicator;
    [SerializeField]
    private GameObject skillBarIndicator;
    [SerializeField]
    private GameObject skillRangeIndicator;

    SkillSpec currentCastingSkill;
    private GameObject currentSkillIndicator;


    private bool isIndicatingSkill;
    void Start()
    {
        HideIndicator();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            SetTargetPos();
        }
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.A))
        {
            string now_input_key = Input.inputString.ToLower();
            ShowIndicator(now_input_key);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (isIndicatingSkill)
            {

            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StopMove();
            if (isIndicatingSkill)
            {
                HideIndicator();
            }
        }
        if (characterState.isMovable == true)
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

    }

    void ShowIndicator(string now_input_key)
    {
        HideIndicator();
        currentCastingSkill = DataBase.Instance.mySkill[now_input_key];
        if (currentCastingSkill.castType == "bar")
        {
            currentSkillIndicator = skillBarIndicator;
        }
        else if (currentCastingSkill.castType == "target")
        {
            currentSkillIndicator = skillTargetingIndicator;
        }
        else if (currentCastingSkill.castType == "area")
        {
            currentSkillIndicator = skillAreaIndicator;
        }
        currentSkillIndicator.transform.localScale = currentCastingSkill.range;        
        currentSkillIndicator.SetActive(true);
        skillRangeIndicator.SetActive(true);
        isIndicatingSkill = true;         
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
        if(isMoving == false)
        {
            isMoving = true;
            animator.SetBool("isMoving", true);
        }
        transform.position += (Vector3)(targetDirection * characterState.moveSpeed * Time.deltaTime);
        if (targetDirection.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    void StopMove()
    {
        targetPos = transform.position;
        if (isMoving == true)
        {
            isMoving = false;
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

    //void OnDrawGizmos()
    //{
    //    if (targetPos == null) return;
    //    Gizmos.color = Color.red;
    //    Vector2 dir = ((Vector2)targetPos - (Vector2)transform.position).normalized;
    //    Gizmos.DrawRay(transform.position, dir * Vector2.Distance(transform.position, targetPos));
    //}
}
