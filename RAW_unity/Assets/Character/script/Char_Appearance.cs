using CustomDict;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;


public class Char_Appearance : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    private SpriteRenderer back;
    [SerializeField]    
    private SpriteRenderer body;
    [SerializeField]
    private SpriteRenderer bodyCloth;
    [SerializeField]
    private SpriteRenderer bodyArmor;
    [SerializeField]
    private SpriteRenderer hair;
    [SerializeField]
    private SpriteRenderer head;
    [SerializeField]
    private SpriteRenderer faceHair;
    [SerializeField]
    private SpriteRenderer rightEyeBack;
    [SerializeField]
    private SpriteRenderer rightEyeFront;
    [SerializeField]
    private SpriteRenderer leftEyeBack;
    [SerializeField]
    private SpriteRenderer leftEyeFront;
    [SerializeField]
    private SpriteRenderer helmet1;
    [SerializeField]
    private SpriteRenderer helmet2;
    [SerializeField]
    private SpriteRenderer leftArm;
    [SerializeField]
    private SpriteRenderer leftArmCloth;
    [SerializeField]
    private SpriteRenderer leftShoulder;    
    [SerializeField]
    private SpriteRenderer leftWeapon;
    [SerializeField]
    private SpriteRenderer leftShield;
    [SerializeField]
    private SpriteRenderer rightArm;
    [SerializeField]
    private SpriteRenderer rightArmCloth;
    [SerializeField]
    private SpriteRenderer rightShoulder;
    [SerializeField]
    private SpriteRenderer rightWeapon;
    [SerializeField]
    private SpriteRenderer rightShield;
    [SerializeField]
    private SpriteRenderer leftFoot;
    [SerializeField]
    private SpriteRenderer leftFootCloth;
    [SerializeField]
    private SpriteRenderer rightFoot;
    [SerializeField]
    private SpriteRenderer rightFootCloth;

    
    public SerializeDictCurrentEquipment currentEquipmentDict;

    
    public SerializeDictEquipmentSpriteRenderer equipmentSpriteRenderer;


    void Start()
    {

    }

    public void GetCurrentEquipment(EquipmentSlot slot, string itemName)
    {
        currentEquipmentDict.Add(slot, itemName);
    }

    public void SetAppearance()
    {
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (currentEquipmentDict.ContainsKey(slot))
            {
                switch (slot.ToString())
                {                    
                    case "Cloth":
                        SetCloth(currentEquipmentDict[slot]);
                        break;
                    case "Armor":
                        SetArmor(currentEquipmentDict[slot]);
                        break;
                    case "Pant":
                        SetPant(currentEquipmentDict[slot]);
                        break;
                    case "Eye":
                        SetEye(currentEquipmentDict[slot]);
                        break;                    
                    default:
                        SetSprite(equipmentSpriteRenderer[slot], DataBase.Instance.equipmentAddress + currentEquipmentDict[slot]);
                        break;
                }
            }
        }
    }


    void SetSprite(SpriteRenderer renderer, string address)
    {
        Addressables.LoadAssetAsync<Sprite>(address).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                renderer.sprite = handle.Result;
            }
            else
            {
                Debug.LogError($"Failed to load sprite at address: {address}");
            }

        };
    }
    

    
    void SetCloth(string address)
    {
        //List<string> _multipleSpriteParts = new List<string>();
        //_multipleSpriteParts.Add(DataBase.Instance.equipmentAddress + clothAddress + "[Body]");
        //_multipleSpriteParts.Add(DataBase.Instance.equipmentAddress + clothAddress + "[Left]");
        //_multipleSpriteParts.Add(DataBase.Instance.equipmentAddress + clothAddress + "[Right]");

        SetSprite(equipmentSpriteRenderer[EquipmentSlot.BodyCloth], DataBase.Instance.equipmentAddress + address + "[Body]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.LeftArmCloth], DataBase.Instance.equipmentAddress + address + "[Left]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.RightArmCloth], DataBase.Instance.equipmentAddress + address + "[Right]");
        
    }
    void SetArmor(string address)
    {
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.BodyArmor], DataBase.Instance.equipmentAddress + address + "[Body]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.LeftShoulder], DataBase.Instance.equipmentAddress + address + "[Left]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.RightShoulder], DataBase.Instance.equipmentAddress + address + "[Right]");
    }
    void SetPant(string address)
    {
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.LeftFootCloth], DataBase.Instance.equipmentAddress + address + "[Left]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.RightFootCloth], DataBase.Instance.equipmentAddress + address + "[Right]");
    }
    void SetEye(string address)
    {
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.LeftEyeBack], DataBase.Instance.equipmentAddress + address + "[Back]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.LeftEyeFront], DataBase.Instance.equipmentAddress + address + "[Front]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.RightEyeBack], DataBase.Instance.equipmentAddress + address + "[Back]");
        SetSprite(equipmentSpriteRenderer[EquipmentSlot.RightEyeFront], DataBase.Instance.equipmentAddress + address + "[Front]");
    }
    void SetHairEyEColor()
    {

    }
    

}
