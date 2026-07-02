// TODO:
// 현재 런타임 SkillTargetingController와 Database.mySkillKeyMap이 사용하는 임시 스킬 정의.
// SkillDefinition 전환이 끝나면 제거한다.

using UnityEngine;

[CreateAssetMenu(menuName ="RAW/Skills/Skill Spec")]
public class SkillSpec : ScriptableObject
{
    //public GameObject skillView;
    //public string skillName;
    //public string iconDirectory;
    //public string description;
    public CastType castType;
    public bool targetEnemy;
    public bool targetAlly;
    //public string dealType;
    //public int maxLevel;
    //public Vector2 radius;
    public float range;
    public float size;
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
}
