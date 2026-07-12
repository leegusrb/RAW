using UnityEngine;

public class SkillIndicatorPresenter : MonoBehaviour
{
	[Header("Skill Indicators")]
	[SerializeField] private GameObject skillAreaIndicator;
	[SerializeField] private GameObject skillTargetingIndicator;
	[SerializeField] private GameObject skillBarIndicator;
	[SerializeField] private GameObject skillRangeIndicator;

	public Vector3 AreaPosition => skillAreaIndicator != null ? skillAreaIndicator.transform.position : Vector3.zero;
	public Vector3 TargetingPosition => skillTargetingIndicator != null ? skillTargetingIndicator.transform.position : Vector3.zero;
	public float BarRotationZ => skillBarIndicator != null ? skillBarIndicator.transform.eulerAngles.z : 0f;
	public Vector2 BarDirection => skillBarIndicator != null ? skillBarIndicator.transform.right : Vector2.right;
	public Vector3 BarLossyScale => skillBarIndicator != null ? skillBarIndicator.transform.lossyScale : Vector3.one;

	public void HideAll()
	{
		if (skillAreaIndicator != null)
			skillAreaIndicator.SetActive(false);

		if (skillTargetingIndicator != null)
			skillTargetingIndicator.SetActive(false);

		if (skillBarIndicator != null)
			skillBarIndicator.SetActive(false);

		if (skillRangeIndicator != null)
			skillRangeIndicator.SetActive(false);
	}

	public void ShowRange(float range)
	{
		if (skillRangeIndicator == null)
			return;

		skillRangeIndicator.transform.localScale = new Vector2(range, range);
		skillRangeIndicator.SetActive(true);
	}

	public void ShowArea(float size)
	{
		if (skillAreaIndicator == null)
			return;

		skillAreaIndicator.transform.localScale = new Vector2(size, size);
		skillAreaIndicator.SetActive(true);
	}

	public void ShowTargeting()
	{
		if (skillTargetingIndicator == null)
			return;

		skillTargetingIndicator.SetActive(true);
	}

	public void ShowBar(float range, float size)
	{
		if (skillBarIndicator == null)
			return;

		skillBarIndicator.transform.localScale = new Vector2(range, size);
		skillBarIndicator.SetActive(true);
	}

	public void SetAreaPosition(Vector3 position)
	{
		if (skillAreaIndicator == null)
			return;

		skillAreaIndicator.transform.position = position;
	}

	public void SetTargetingPosition(Vector3 position)
	{
		if (skillTargetingIndicator == null)
			return;

		skillTargetingIndicator.transform.position = position;
	}

	public void SetTargetingValid(bool isValid)
	{
		if (skillTargetingIndicator == null)
			return;

		skillTargetingIndicator.transform.GetChild(1).gameObject.SetActive(isValid);
	}

	public void SetBarRotation(Quaternion rotation)
	{
		if (skillBarIndicator == null)
			return;

		skillBarIndicator.transform.rotation = rotation;
	}

	public void SetBarScale(Vector2 scale)
	{
		if (skillBarIndicator == null)
			return;

		skillBarIndicator.transform.localScale = scale;
	}
}
