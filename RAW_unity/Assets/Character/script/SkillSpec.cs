using UnityEngine;

[CreateAssetMenu]
public class SkillSpec : ScriptableObject
{
    

    //public GameObject skillView;
    public string skillName;
    public string iconDirectory;
    public string description;
    public string castType;
    public string dealType;
    public int maxLevel;
    public Vector2 radius;
    public Vector2 range;

    public string animType; //attack1,2,3, skill1,2,3
    public float consumeMana;
    public float delay;
    public float duration;
    public float coolDown;

    public float dealSync;

    public float flatDeal;
    public float dealIncreasePerSkillLevel;
    public float dealIncreasePerPower;

    public float flatHeal;
    public float healIncreasePerSkillLevel;
    public float healIncreasePerPower;

    public float flatShield;
    public float shieldIncreasePerSkillLevel;
    public float shieldIncreasePerPower;

    public float flatPower;
    public float powerIncreasePerSkillLevel;
    public float powerIncreasePerPower;
}
