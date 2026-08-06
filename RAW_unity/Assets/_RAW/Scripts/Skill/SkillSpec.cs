using UnityEngine;

public enum CastType
{
    bar,
    area,
    target
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "RAW/Data/Skill")]
public class SkillSpec : ScriptableObject
{
	[Header("Server Validation")]

	[SerializeField]
	private string skillId;

	[SerializeField, Min(0)]
	private int manaCost;

	[SerializeField, Min(0f)]
	private float cooldownSeconds;

	[Header("Casting")]

    //public GameObject skillView;
    //public string skillName;
    //public string iconDirectory;
    //public string description;
    public CastType castType;
    public TargettingSkillTarget targettingSkillTarget;
    //public string dealType;
    //public int maxLevel;
    //public Vector2 radius;
	[Min(0f)]
    public float range;
	[Min(0f)]
    public float size;
    public GameObject skillPrefab;
    public float moveSpeed = 5f;
    public AnimationClip animationClip;
    public float preDelay;
    public float postDelay;
    public float remainTime;
    public float damage;
    //public string animType; //attack1,2,3, skill1,2,3
    //public float consumeMana;
    //public float delay;
    //public float duration;
    //public float coolDown;

    //public float dealSync;

    //public float flatDeal;
    //public float dealIncreasePerSkillLevel;
    //public float dealIncreasePerPower;

    //public float flatHeal;
    //public float healIncreasePerSkillLevel;
    //public float healIncreasePerPower;

    //public float flatShield;
    //public float shieldIncreasePerSkillLevel;
    //public float shieldIncreasePerPower;

    //public float flatPower;
    //public float powerIncreasePerSkillLevel;
    //public float powerIncreasePerPower;

	public string SkillId => skillId;
	public int ManaCost => manaCost;
	public float CooldownSeconds => cooldownSeconds;
}
