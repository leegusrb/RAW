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
    SkillDefinition skill;
    Transform caster;
    Vector2 moveDirection;
    float traveled;

    public void Init(SkillDefinition skillDefinition, Transform casterTransform, SkillCastContext context)
    {
        skill = skillDefinition;
        caster = casterTransform;
        transform.position = context.position;
        transform.rotation = Quaternion.Euler(0f, 0f, context.rotationZ);
        moveDirection = context.direction;

        if (skill.castType == CastType.Bar)
            transform.localScale = context.visualScale;
        else if (skill.castType == CastType.Area)
            transform.localScale = Vector3.one * skill.size;
    }

    void Update()
    {
        if (skill == null || skill.castType != CastType.Bar)
            return;

        float step = skill.moveSpeed * Time.deltaTime;
        transform.position += (Vector3)(moveDirection * step);
        traveled += step;

        if (traveled >= skill.range)
            Destroy(gameObject);
    }
}
