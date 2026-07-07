using UnityEngine;

public struct SkillCastContext
{
    public Vector3 position;
    public Vector2 direction;
    public float rotationZ;
    public Vector3 visualScale;
}

public class SkillObject : MonoBehaviour
{
    SkillSpec skill;
    Transform caster;
    Vector2 moveDirection;
    float traveled;

    public void Init(SkillSpec skillSpec, Transform casterTransform, SkillCastContext context)
    {
        skill = skillSpec;
        caster = casterTransform;
        transform.position = context.position;
        transform.rotation = Quaternion.Euler(0f, 0f, context.rotationZ);
        moveDirection = context.direction;

        if (skill.castType == CastType.bar)
            transform.localScale = context.visualScale;
        else if (skill.castType == CastType.area)
            transform.localScale = Vector3.one * skill.size;
    }

    void Update()
    {
        if (skill == null || skill.castType != CastType.bar)
            return;

        float step = skill.moveSpeed * Time.deltaTime;
        transform.position += (Vector3)(moveDirection * step);
        traveled += step;

        if (traveled >= skill.range)
            Destroy(gameObject);
    }
}
