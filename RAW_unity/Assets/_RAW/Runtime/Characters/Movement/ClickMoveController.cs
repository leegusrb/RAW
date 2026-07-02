using System.Collections;
using UnityEngine;

public class ClickMoveController : MonoBehaviour
{
	[Header("Layers")]
	[SerializeField] private LayerMask groundLayer;
	[SerializeField] private LayerMask obstacleLayer;

	[Header("Target Pointer")]
	[SerializeField] private GameObject targetPointer;
	[SerializeField] private float pointingDuration = 0.25f;

	[Header("Character")]
	[SerializeField] private Char_State characterState;
	[SerializeField] private Animator animator;

	[Header("Obstacle Avoidance")]
	[SerializeField] private float obstacleAvoidDistance = 0.1f;

	private Vector2 targetPos;
	private Coroutine targetPointing;

	private Vector2 obstacleAvoidDirection;
	private bool isFollowingWall;

	private bool isMoving;

	private void Awake()
	{
		if (characterState == null)
			characterState = GetComponent<Char_State>();

		if (animator == null)
			animator = GetComponentInChildren<Animator>();
	}

	private void Start()
	{
		targetPos = transform.position;
	}

	public void SetTargetByMouse()
	{
		if (Camera.main == null)
		{
			Debug.LogError("Camera.main is null.");
			return;
		}

		Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		Collider2D ground = Physics2D.OverlapPoint(mouseWorldPosition, groundLayer);

		if (ground == null)
			return;

		targetPos = mouseWorldPosition;

		if (targetPointing != null)
			StopCoroutine(targetPointing);

		targetPointing = StartCoroutine(PointingTarget());
	}

	public void TickMove()
	{
		if (characterState == null)
			return;

		if (characterState.IsMovable)
		{
			MoveCharacter();
		}
		else
		{
			StopMove();
		}
	}

	public void StopMove()
	{
		targetPos = transform.position;

		if (isMoving == true)
		{
			isMoving = false;

			if (animator != null)
				animator.SetBool("isMoving", false);
		}

		isFollowingWall = false;
	}

	private void MoveCharacter()
	{
		Vector2 currentPosition = transform.position;
		Vector2 targetDirection = (targetPos - currentPosition).normalized;
		float targetDistance = Vector2.Distance(currentPosition, targetPos);

		if (Vector2.Distance(targetPos, currentPosition) <= 0.001f)
		{
			StopMove();
			return;
		}

		RaycastHit2D hit = Physics2D.Raycast(
			currentPosition,
			targetDirection,
			targetDistance,
			obstacleLayer
		);

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

	private bool CheckObstacle(RaycastHit2D hit, Vector2 currentPosition)
	{
		if (hit.collider == null)
			return false;

		float obstacleDistance = Vector2.Distance(currentPosition, hit.point);

		return obstacleDistance < obstacleAvoidDistance;
	}

	private void DoMove(Vector2 targetDirection)
	{
		if (characterState == null)
			return;

		if (isMoving == false)
		{
			isMoving = true;

			if (animator != null)
				animator.SetBool("isMoving", true);
		}

		transform.position += (Vector3) (targetDirection * characterState.MoveSpeed * Time.deltaTime);

		if (targetDirection.x > 0)
		{
			transform.localScale = new Vector3(-1, 1, 1);
		}
		else
		{
			transform.localScale = new Vector3(1, 1, 1);
		}
	}

	private void CalObstacleAvoidDirection(RaycastHit2D hit)
	{
		Vector2 normal = hit.normal;

		Vector2 tangentCW = new Vector2(-normal.y, normal.x);
		Vector2 tangentCCW = new Vector2(normal.y, -normal.x);

		Vector2 fromHitToTarget = ((Vector2) targetPos - hit.point).normalized;

		float dotCW = Vector2.Dot(fromHitToTarget, tangentCW);
		float dotCCW = Vector2.Dot(fromHitToTarget, tangentCCW);

		obstacleAvoidDirection = (dotCW > dotCCW ? tangentCW : tangentCCW).normalized;

		Debug.DrawRay(transform.position, obstacleAvoidDirection, Color.blue);
	}

	private IEnumerator PointingTarget()
	{
		if (targetPointer == null)
			yield break;

		targetPointer.SetActive(true);
		targetPointer.transform.position = targetPos;

		float halfDuration = pointingDuration / 2;

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
}
