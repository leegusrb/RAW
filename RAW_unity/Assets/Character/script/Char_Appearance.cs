using CustomDict;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Char_Appearance : MonoBehaviour
{
    [SerializeField]
    private Char_Inventory inventory;

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

    public CustomDictEquipmentSpriteRenderer equipmentSpriteRenderer;
    public CustomDictBodyColor bodyColor;

    CustomDictCurrentEquipment EquippedItems => inventory.EquippedItems;

    void OnEnable()
    {
        inventory.OnEquipmentChanged += SetAppearance;
    }

    void OnDisable()
    {
        inventory.OnEquipmentChanged -= SetAppearance;
    }

    void Start()
    {
        //SetAppearance();
    }

    public void SetAppearance()
    {
        var equipped = EquippedItems;
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (!equipped.ContainsKey(slot))
                continue;

            string itemId = equipped[slot];
            switch (slot.ToString())
            {
                case "Cloth":
                    SetCloth(itemId);
                    break;
                case "Armor":
                    SetArmor(itemId);
                    break;
                case "Pant":
                    SetPant(itemId);
                    break;
                case "Eye":
                    SetEye(itemId);
                    break;
                case "Hair":
                case "FaceHair":
                    SetSprite(equipmentSpriteRenderer[slot], DataBase.Instance.equipmentAddress + itemId);
                    SetHairColor(slot, bodyColor[slot]);
                    break;
                default:
                    SetSprite(equipmentSpriteRenderer[slot], DataBase.Instance.equipmentAddress + itemId);
                    break;
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
        equipmentSpriteRenderer[EquipmentSlot.LeftEyeFront].color = bodyColor[EquipmentSlot.LeftEyeBack];
        equipmentSpriteRenderer[EquipmentSlot.RightEyeFront].color = bodyColor[EquipmentSlot.RightEyeBack];
    }
    void SetHairColor(EquipmentSlot slot, UnityEngine.Color color)
    {
        equipmentSpriteRenderer[slot].color = color;
    }
    

}
