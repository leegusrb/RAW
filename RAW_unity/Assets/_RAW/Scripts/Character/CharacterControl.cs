using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterState))]
public class CharacterControl : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask monsterLayer;

    [SerializeField] private GameObject targetPointer;
    [SerializeField] private CharacterState characterState;
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterSkillCastingController skillCastingController;

    private Vector2 targetPos;
    private readonly float pointingDuration = 0.25f;
    private Coroutine targetPointing;

    private readonly float obstacleAvoidDistance = 0.1f;
    private Vector2 obstacleAvoidDirection;
    private bool isFollowingWall;

    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    private void Awake()
    {
        if (characterState == null)
            characterState = GetComponent<CharacterState>();

        if (skillCastingController == null &&
            !TryGetComponent(out skillCastingController))
        {
            Debug.LogError("CharacterSkillCastingController 컴포넌트를 찾을 수 없습니다.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        targetPos = transform.position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            skillCastingController.CancelCasting();
            SetTargetPos();
        }

        if (TryGetPressedSkillSlot(out KeyMapping pressedSkillSlot))
            skillCastingController.BeginTargeting(pressedSkillSlot);

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            skillCastingController.TryConfirmCasting(mouseWorldPosition);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            skillCastingController.CancelCasting();
            StopMoving();
        }

        if (characterState.IsMovable)
            MoveCharacter();
        else
            StopMoving();
    }

    public void SetMoveDestination(Vector2 position)
    {
        targetPos = position;
    }

    public void StopMoving()
    {
        targetPos = transform.position;

        if (characterState.IsMoving)
        {
            characterState.IsMoving = false;

            if (animator != null)
                animator.SetBool(IsMovingHash, false);
        }

        isFollowingWall = false;
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

    private void SetTargetPos()
    {
        if (Camera.main == null)
            return;

        Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, groundLayer);

        if (hit.collider == null)
            return;

        SetMoveDestination(hit.point);

        if (targetPointing != null)
            StopCoroutine(targetPointing);

        targetPointing = StartCoroutine(PointingTarget());
    }

    private void MoveCharacter()
    {
        Vector2 currentPosition = transform.position;
        Vector2 targetDirection = (targetPos - currentPosition).normalized;
        float targetDistance = Vector2.Distance(currentPosition, targetPos);

        if (targetDistance <= 0.001f)
        {
            StopMoving();
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            currentPosition,
            targetDirection,
            targetDistance,
            obstacleLayer
        );

        if (CheckObstacle(hit, currentPosition))
        {
            if (isFollowingWall)
            {
                DoMove(obstacleAvoidDirection);
            }
            else
            {
                CalculateObstacleAvoidDirection(hit);
                isFollowingWall = true;
            }

            return;
        }

        isFollowingWall = false;
        DoMove(targetDirection);
    }

    private bool CheckObstacle(RaycastHit2D hit, Vector2 currentPosition)
    {
        if (hit.collider == null)
            return false;

        float obstacleDistance = Vector2.Distance(currentPosition, hit.point);
        return obstacleDistance < obstacleAvoidDistance;
    }

    private void DoMove(Vector2 targetDirection)
    {
        if (!characterState.IsMoving)
        {
            characterState.IsMoving = true;

            if (animator != null)
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
        scale.x = direction.x > 0f ? -xMagnitude : xMagnitude;
        transform.localScale = scale;
    }

    private void CalculateObstacleAvoidDirection(RaycastHit2D hit)
    {
        Vector2 normal = hit.normal;
        Vector2 tangentClockwise = new Vector2(-normal.y, normal.x);
        Vector2 tangentCounterClockwise = new Vector2(normal.y, -normal.x);
        Vector2 fromHitToTarget = (targetPos - hit.point).normalized;

        float clockwiseDot = Vector2.Dot(fromHitToTarget, tangentClockwise);
        float counterClockwiseDot = Vector2.Dot(fromHitToTarget, tangentCounterClockwise);

        obstacleAvoidDirection = (
            clockwiseDot > counterClockwiseDot
                ? tangentClockwise
                : tangentCounterClockwise
        ).normalized;

        Debug.DrawRay(transform.position, obstacleAvoidDirection, Color.blue);
    }

    private IEnumerator PointingTarget()
    {
        if (targetPointer == null)
        {
            Debug.LogWarning("목표 지점 포인터가 연결되지 않았습니다.", this);
            yield break;
        }

        targetPointer.SetActive(true);
        targetPointer.transform.position = targetPos;

        float halfDuration = pointingDuration / 2f;
        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            float value = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            targetPointer.transform.localScale = Vector3.one * value;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            float value = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
            elapsed += Time.deltaTime;
            targetPointer.transform.localScale = Vector3.one * value;
            yield return null;
        }

        targetPointer.SetActive(false);
        targetPointing = null;
    }
}
