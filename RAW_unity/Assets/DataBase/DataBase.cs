using CustomDict;
using JetBrains.Annotations;
using System;
using UnityEngine;


public enum EquipmentSlot
{
    Back,
    Body,
    BodyCloth,
    BodyArmor,
    Hair,
    Head,
    FaceHair,
    RightEyeBack,
    RightEyeFront,
    LeftEyeBack,
    LeftEyeFront,
    Helmet1,
    Helmet2,
    LeftArm,
    LeftArmCloth,
    LeftShoulder,
    LeftWeapon,
    LeftShield,
    RightArm,
    RightArmCloth,
    RightShoulder,
    RightWeapon,
    RightShield,
    LeftFoot,
    LeftFootCloth,
    RightFoot,
    RightFootCloth,
    Cloth,
    Armor,
    Pant,
    Eye
}


public enum KeyMapping
{
    Q,
    W,
    E,
    R
}


public class DataBase : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public int EquipmnetSlotSize = Enum.GetValues(typeof(EquipmentSlot)).Length;
    public EquipmentSlot equipmnetSlot;
    public string equipmentAddress = "Assets/DataBase/Equipment/";
    public KeyMapping customKeyMap;
    public CustomDictKeyMap KeyMap = new CustomDictKeyMap();
    public CustomDictSkill mySkill = new CustomDictSkill();
    public static DataBase Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
