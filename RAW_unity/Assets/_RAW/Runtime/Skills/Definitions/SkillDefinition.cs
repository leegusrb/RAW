using UnityEngine;

[CreateAssetMenu(menuName = "RAW/Skills/Skill Definition")]
public class SkillDefinition : ScriptableObject
{
    public string skillId;
	public string displayName;
	[TextArea]
	public string description;

	public SkillSchool school;
	public CastType castType;

	public bool targetEnemy;
	public bool targetAlly;

	public float range;
	public float size;
	public float cooldown;
	public float manaCost;

	public GameObject skillPrefab;
	public float moveSpeed = 5f;
	public AnimationClip animationClip;
}
