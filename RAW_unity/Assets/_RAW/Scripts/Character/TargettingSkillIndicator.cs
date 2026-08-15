using UnityEngine;



public class TargettingSkillIndicator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    public SkillTargetRelation target = SkillTargetRelation.None;
    public bool isTargetting;
    [SerializeField] private LayerMask currentTargetLayer;
    public GameObject targetableIndicator;
    public GameObject targettingTarget;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray, transform.forward, Mathf.Infinity, currentTargetLayer);

        if (hit.collider != null)
        {
            targetableIndicator.SetActive(true);
            targettingTarget = hit.collider.gameObject;
        }
        else
        {
            targetableIndicator.SetActive(false);
            targettingTarget = null;
        }
        SetLayer();
    }

    void SetLayer()
    {
        if (target == SkillTargetRelation.Enemy)
        {
            currentTargetLayer = LayerMask.GetMask("Enemy");
        }
        else if (target == SkillTargetRelation.Ally)
        {
            currentTargetLayer = LayerMask.GetMask("Ally");
        }
    }
}
