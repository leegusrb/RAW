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



public class DataBase : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public int EquipmnetSlotSize = Enum.GetValues(typeof(EquipmentSlot)).Length;
    public EquipmentSlot equipmnetSlot;
    public string equipmentAddress = "Assets/DataBase/Equipment/";

    public static DataBase Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
