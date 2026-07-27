using CustomDict;
using System;
using System.Collections.Generic;
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

    CustomDictCurrentEquipment EquippedItems => inventory != null ? inventory.EquippedItems : null;

	private readonly Dictionary<SpriteRenderer, AsyncOperationHandle<Sprite>> activeSpriteHandles = new Dictionary<SpriteRenderer, AsyncOperationHandle<Sprite>>();
	private readonly Dictionary<SpriteRenderer, int> spriteLoadVersions = new Dictionary<SpriteRenderer, int>();
	private readonly List<AsyncOperationHandle<Sprite>> pendingSpriteHandles = new List<AsyncOperationHandle<Sprite>>();
	private bool isDestroyed;


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

    void Start()
    {
        // SetAppearance();
    }

	private void OnDestroy()
	{
		isDestroyed = true;

		foreach (AsyncOperationHandle<Sprite> handle in activeSpriteHandles.Values)
		{
			ReleaseHandle(handle);
		}

		activeSpriteHandles.Clear();

		List<AsyncOperationHandle<Sprite>> pendingCopy = 
			new List<AsyncOperationHandle<Sprite>>(pendingSpriteHandles);

		pendingSpriteHandles.Clear();

		foreach (AsyncOperationHandle<Sprite> handle in pendingCopy)
		{
			ReleaseHandle(handle);
		}
	}

	public void SetAppearance()
    {
        if (inventory == null || EquippedItems == null || DataBase.Instance == null)
            return;

		ClearEquipmentAppearance();

        foreach (KeyValuePair<EquipmentSlot, string> pair in EquippedItems)
        {
			EquipmentSlot slot = pair.Key;
			string itemId = pair.Value;

			if (string.IsNullOrEmpty(itemId))
				continue;

            switch (slot)
            {
                case EquipmentSlot.Cloth:
                    SetCloth(itemId);
                    break;

                case EquipmentSlot.Armor:
                    SetArmor(itemId);
                    break;

                case EquipmentSlot.Pant:
                    SetPant(itemId);
                    break;

                case EquipmentSlot.Eye:
                    SetEye(itemId);
                    break;

                case EquipmentSlot.Hair:
                case EquipmentSlot.FaceHair:
					SetSlotSprite(slot, DataBase.Instance.equipmentAddress + itemId);
                    SetHairColor(slot);
                    break;

                default:
					SetSlotSprite(slot, DataBase.Instance.equipmentAddress + itemId);
                    break;
            }
        }
    }


    void SetSprite(SpriteRenderer renderer, string address)
    {
		if (renderer == null)
			return;

			if (string.IsNullOrEmpty(address))
		{
			ClearSprite(renderer);
			return;
		}

		int requestVersion = IncreaseLoadVersion(renderer);

		AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);

		pendingSpriteHandles.Add(handle);

        handle.Completed += completedHandle =>
        {
			pendingSpriteHandles.Remove(completedHandle);

			if (isDestroyed)
				return;

			if (renderer == null)
			{
				ReleaseHandle(completedHandle);
				return;
			}

			if (!IsLatestLoadRequest(renderer, requestVersion))
			{
				ReleaseHandle(completedHandle);
				return;
			}

			if (completedHandle.Status != AsyncOperationStatus.Succeeded)
			{
				ReleaseHandle(completedHandle);

				Debug.LogError($"장비 Sprite 로딩 실패: {address}", this);

				return;
			}

			ReleaseActiveSprite(renderer);

			renderer.sprite = completedHandle.Result;

			activeSpriteHandles[renderer] = completedHandle;
        };
    }
    
	private int IncreaseLoadVersion(SpriteRenderer renderer)
	{
		if (!spriteLoadVersions.TryGetValue(renderer, out int version))
			version = 0;

		version++;
		spriteLoadVersions[renderer] = version;

		return version;
	}
	
	private bool IsLatestLoadRequest(SpriteRenderer renderer, int requestVersion)
	{
		return spriteLoadVersions.TryGetValue(renderer, out int currentVersion) && currentVersion == requestVersion;
	}

	private void ReleaseActiveSprite(SpriteRenderer renderer)
	{
		if (!activeSpriteHandles.TryGetValue(renderer, out AsyncOperationHandle<Sprite> handle))
			return;

		ReleaseHandle(handle);
		activeSpriteHandles.Remove(renderer);
	}

	private void ReleaseHandle(AsyncOperationHandle<Sprite> handle)
	{
		if (handle.IsValid())
			Addressables.Release(handle);
	}

	private void ClearSprite(SpriteRenderer renderer)
	{
		if (renderer == null)
			return;

		IncreaseLoadVersion(renderer);
		ReleaseActiveSprite(renderer);

		renderer.sprite = null;
	}

	private void ClearSlotSprite(EquipmentSlot slot)
	{
		if (!TryGetRenderer(slot, out SpriteRenderer renderer))
			return;

		ClearSprite(renderer);
	}

	private void ClearEquipmentAppearance()
	{
		ClearSlotSprite(EquipmentSlot.Back);

		ClearSlotSprite(EquipmentSlot.BodyCloth);
		ClearSlotSprite(EquipmentSlot.LeftArmCloth);
		ClearSlotSprite(EquipmentSlot.RightArmCloth);

		ClearSlotSprite(EquipmentSlot.BodyArmor);
		ClearSlotSprite(EquipmentSlot.LeftShoulder);
		ClearSlotSprite(EquipmentSlot.RightShoulder);

		ClearSlotSprite(EquipmentSlot.LeftFootCloth);
		ClearSlotSprite(EquipmentSlot.RightFootCloth);

		ClearSlotSprite(EquipmentSlot.Hair);
		ClearSlotSprite(EquipmentSlot.FaceHair);

		ClearSlotSprite(EquipmentSlot.LeftEyeBack);
		ClearSlotSprite(EquipmentSlot.LeftEyeFront);
		ClearSlotSprite(EquipmentSlot.RightEyeBack);
		ClearSlotSprite(EquipmentSlot.RightEyeFront);

		ClearSlotSprite(EquipmentSlot.Helmet1);
		ClearSlotSprite(EquipmentSlot.Helmet2);

		ClearSlotSprite(EquipmentSlot.LeftWeapon);
		ClearSlotSprite(EquipmentSlot.RightWeapon);
		ClearSlotSprite(EquipmentSlot.LeftShield);
		ClearSlotSprite(EquipmentSlot.RightShield);
	}
    
    void SetCloth(string itemId)
    {
        //List<string> _multipleSpriteParts = new List<string>();
        //_multipleSpriteParts.Add(DataBase.Instance.equipmentAddress + clothAddress + "[Body]");
        //_multipleSpriteParts.Add(DataBase.Instance.equipmentAddress + clothAddress + "[Left]");
        //_multipleSpriteParts.Add(DataBase.Instance.equipmentAddress + clothAddress + "[Right]");

		string prefix = DataBase.Instance.equipmentAddress;

		SetSlotSprite(EquipmentSlot.BodyCloth, prefix + itemId + "[Body]");
		SetSlotSprite(EquipmentSlot.LeftArmCloth, prefix + itemId + "[Left]");
		SetSlotSprite(EquipmentSlot.RightArmCloth, prefix + itemId + "[Right]");
    }
    void SetArmor(string itemId)
    {
		string prefix = DataBase.Instance.equipmentAddress;

		SetSlotSprite(EquipmentSlot.BodyArmor, prefix + itemId + "[Body]");
		SetSlotSprite(EquipmentSlot.LeftShoulder, prefix + itemId + "[Left]");
		SetSlotSprite(EquipmentSlot.RightShoulder, prefix + itemId + "[Right]");
    }
    void SetPant(string itemId)
    {
		string prefix = DataBase.Instance.equipmentAddress;

		SetSlotSprite(EquipmentSlot.LeftFootCloth, prefix + itemId + "[Left]");
		SetSlotSprite(EquipmentSlot.RightFootCloth, prefix + itemId + "[Right]");
    }
    void SetEye(string itemId)
    {
		string prefix = DataBase.Instance.equipmentAddress;

		SetSlotSprite(EquipmentSlot.LeftEyeBack, prefix + itemId + "[Back]");
		SetSlotSprite(EquipmentSlot.LeftEyeFront, prefix + itemId + "[Front]");
		SetSlotSprite(EquipmentSlot.RightEyeBack, prefix + itemId + "[Back]");
		SetSlotSprite(EquipmentSlot.RightEyeFront, prefix + itemId + "[Front]");

        equipmentSpriteRenderer[EquipmentSlot.LeftEyeFront].color = bodyColor[EquipmentSlot.LeftEyeBack];
        equipmentSpriteRenderer[EquipmentSlot.RightEyeFront].color = bodyColor[EquipmentSlot.RightEyeBack];
    }
    void SetHairColor(EquipmentSlot slot)
    {
		if (!TryGetRenderer(slot, out SpriteRenderer renderer))
			return;

		if (bodyColor == null || !bodyColor.TryGetValue(slot, out Color color))
			return;
		
		renderer.color = color;
    }
    
	private bool TryGetRenderer(EquipmentSlot slot, out SpriteRenderer renderer)
	{
		renderer = null;

		if (equipmentSpriteRenderer == null)
			return false;

		if (!equipmentSpriteRenderer.TryGetValue(slot, out renderer))
		{
			Debug.LogWarning($"장비 SpriteRenderer가 등록되지 않았습니다: {slot}", this);

			return false;
		}

		if (renderer == null)
		{
			Debug.LogWarning($"장비 SpriteRenderer가 null입니다: {slot}", this);

			return false;
		}

		return true;
	}

	private void SetSlotSprite(EquipmentSlot slot, string address)
	{
		if (!TryGetRenderer(slot, out SpriteRenderer renderer))
			return;

		SetSprite(renderer, address);
	}
}
