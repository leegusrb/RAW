using CustomDict;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Char_Appearance : MonoBehaviour
{
    [SerializeField]
    private Char_Inventory inventory;
	
	[SerializeField]
	private AddressableAssetService addressableAssetService;

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

	private readonly Dictionary<SpriteRenderer, AsyncOperationHandle<Sprite>> activeSpriteHandles = new Dictionary<SpriteRenderer, AsyncOperationHandle<Sprite>>();
	private readonly Dictionary<SpriteRenderer, AsyncOperationHandle<Sprite>> pendingSpriteHandles = new Dictionary<SpriteRenderer, AsyncOperationHandle<Sprite>>();
	private readonly Dictionary<SpriteRenderer, int> spriteLoadVersions = new Dictionary<SpriteRenderer, int>();

    CustomDictCurrentEquipment EquippedItems => inventory != null ? inventory.EquippedItems : null;

	private void Awake()
	{
		if (addressableAssetService == null)
			addressableAssetService = AddressableAssetService.Instance;

		if (addressableAssetService == null)
			addressableAssetService = FindFirstObjectByType<AddressableAssetService>();
	}

	void OnEnable()
    {
        if (inventory != null)
            inventory.OnEquipmentChanged += SetAppearance;
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnEquipmentChanged -= SetAppearance;
    }

	private void OnDestroy()
	{
		ReleaseAllSpriteHandles();
	}

    public void SetAppearance()
    {
        if (inventory == null || EquippedItems == null || DataBase.Instance == null)
            return;

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
		if (renderer == null)
		{
			Debug.LogWarning("SetSprite failed. SpriteRenderer is null.");
			return;
		}

		if (string.IsNullOrEmpty(address))
		{
			ClearSprite(renderer);
			return;
		}

		if (addressableAssetService == null)
		{
			Debug.LogError("AddressableAssetService is not assigned.");
			return;
		}

		int loadVersion = IncreaseLoadVersion(renderer);

		ReleasePendingSprite(renderer);

		AsyncOperationHandle<Sprite> handle = addressableAssetService.LoadSprite(address);
		pendingSpriteHandles[renderer] = handle;

		handle.Completed += completedHandle =>
		{
			if (!IsLatestLoadRequest(renderer, loadVersion))
			{
				ReleaseHandleIfValid(completedHandle);
				return;
			}

			pendingSpriteHandles.Remove(renderer);

			if (completedHandle.Status == AsyncOperationStatus.Succeeded)
			{
				ReleaseActiveSprite(renderer);

				renderer.sprite = completedHandle.Result;
				activeSpriteHandles[renderer] = completedHandle;
			}
			else
			{
				ReleaseHandleIfValid(completedHandle);
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
    
	private int IncreaseLoadVersion(SpriteRenderer renderer)
	{
		if (!spriteLoadVersions.TryGetValue(renderer, out int version))
			version = 0;

		version++;
		spriteLoadVersions[renderer] = version;

		return version;
	}

	private bool IsLatestLoadRequest(SpriteRenderer renderer, int loadVersion)
	{
		if (renderer == null)
			return false;

		if (!spriteLoadVersions.TryGetValue(renderer, out int currentVersion))
			return false;

		return currentVersion == loadVersion;
	}

	private void ClearSprite(SpriteRenderer renderer)
	{
		if (renderer == null)
			return;

		IncreaseLoadVersion(renderer);

		ReleasePendingSprite(renderer);
		ReleaseActiveSprite(renderer);

		renderer.sprite = null;
	}

	private void ReleasePendingSprite(SpriteRenderer renderer)
	{
		if (renderer == null)
			return;

		if (!pendingSpriteHandles.TryGetValue(renderer, out AsyncOperationHandle<Sprite> handle))
			return;

		ReleaseHandleIfValid(handle);
		pendingSpriteHandles.Remove(renderer);
	}

	private void ReleaseActiveSprite(SpriteRenderer renderer)
	{
		if (renderer == null)
			return;

		if (!activeSpriteHandles.TryGetValue(renderer, out AsyncOperationHandle<Sprite> handle))
			return;

		ReleaseHandleIfValid(handle);
		activeSpriteHandles.Remove(renderer);
	}

	private void ReleaseAllSpriteHandles()
	{
		foreach (var pair in pendingSpriteHandles)
		{
			ReleaseHandleIfValid(pair.Value);
		}

		pendingSpriteHandles.Clear();

		foreach (var pair in activeSpriteHandles)
		{
			ReleaseHandleIfValid(pair.Value);
		}

		activeSpriteHandles.Clear();

		spriteLoadVersions.Clear();
	}

	private void ReleaseHandleIfValid(AsyncOperationHandle<Sprite> handle)
	{
		if (addressableAssetService != null)
		{
			addressableAssetService.ReleaseSprite(handle);
		}
	}
}
