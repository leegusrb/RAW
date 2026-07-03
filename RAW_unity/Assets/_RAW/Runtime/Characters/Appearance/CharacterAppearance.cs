using CustomDict;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterAppearance : MonoBehaviour
{
	[Header("Addressables")]
	[SerializeField] 
	private string equipmentAddressPrefix = "Assets/DataBase/Equipment/";

    [SerializeField]
    private CharacterInventory inventory;
	
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

    public CustomDictAppearanceSpriteRenderer appearanceSpriteRenderer;
    public CustomDictAppearanceColor appearanceColor;

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
        if (inventory == null || EquippedItems == null)
            return;

        var equipped = EquippedItems;
        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (!equipped.ContainsKey(slot))
                continue;

            string itemId = equipped[slot];
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
					SetSprite(appearanceSpriteRenderer[AppearancePart.Hair], GetEquipmentSpriteAddress(itemId));
					SetColor(AppearancePart.Hair);
					break;

                case EquipmentSlot.FaceHair:
					SetSprite(appearanceSpriteRenderer[AppearancePart.FaceHair], GetEquipmentSpriteAddress(itemId));
					SetColor(AppearancePart.FaceHair);
                    break;

				case EquipmentSlot.Helmet:
					SetSprite(appearanceSpriteRenderer[AppearancePart.Helmet1], GetEquipmentSpriteAddress(itemId));
					break;

				case EquipmentSlot.Weapon:
					SetSprite(appearanceSpriteRenderer[AppearancePart.RightWeapon], GetEquipmentSpriteAddress(itemId));
					break;

				case EquipmentSlot.Shield:
					SetSprite(appearanceSpriteRenderer[AppearancePart.LeftShield], GetEquipmentSpriteAddress(itemId));
					break;

				case EquipmentSlot.Back:
					SetSprite(appearanceSpriteRenderer[AppearancePart.Back], GetEquipmentSpriteAddress(itemId));
					break;

                default:
					Debug.LogWarning($"Unsupported equipment slot: {slot}", this);
                    break;
            }
        }
    }

	private bool TryGetRenderer(AppearancePart part, out SpriteRenderer renderer)
	{
		renderer = null;

		if (appearanceSpriteRenderer == null)
			return false;

		if (!appearanceSpriteRenderer.TryGetValue(part, out renderer))
		{
			Debug.LogWarning($"Appearance renderer is not registered: {part}", this);
			return false;
		}

		if (renderer == null)
		{
			Debug.LogError($"Appearance renderer is null: {part}", this);
			return false;
		}

		return true;
	}

	private void SetSprite(AppearancePart part, string address)
	{
		if (!TryGetRenderer(part, out SpriteRenderer renderer))
			return;

		SetSprite(renderer, address);
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
        //_multipleSpriteParts.Add(GetEquipmentSpriteAddress(clothAddress, "[Body]"));
        //_multipleSpriteParts.Add(GetEquipmentSpriteAddress(clothAddress, "[Left]"));
        //_multipleSpriteParts.Add(GetEquipmentSpriteAddress(clothAddress, "[Right]"));

        SetSprite(AppearancePart.BodyCloth, GetEquipmentSpriteAddress(address, "[Body]"));
        SetSprite(AppearancePart.LeftArmCloth, GetEquipmentSpriteAddress(address, "[Left]"));
        SetSprite(AppearancePart.RightArmCloth, GetEquipmentSpriteAddress(address, "[Right]"));
        
    }
    void SetArmor(string address)
    {
        SetSprite(AppearancePart.BodyArmor, GetEquipmentSpriteAddress(address, "[Body]"));
        SetSprite(AppearancePart.LeftShoulder, GetEquipmentSpriteAddress(address, "[Left]"));
        SetSprite(AppearancePart.RightShoulder, GetEquipmentSpriteAddress(address, "[Right]"));
    }
    void SetPant(string address)
    {
        SetSprite(AppearancePart.LeftFootCloth, GetEquipmentSpriteAddress(address, "[Left]"));
        SetSprite(AppearancePart.RightFootCloth, GetEquipmentSpriteAddress(address, "[Right]"));
    }
    void SetEye(string address)
    {
        SetSprite(AppearancePart.LeftEyeBack, GetEquipmentSpriteAddress(address, "[Back]"));
        SetSprite(AppearancePart.LeftEyeFront, GetEquipmentSpriteAddress(address, "[Front]"));
        SetSprite(AppearancePart.RightEyeBack, GetEquipmentSpriteAddress(address, "[Back]"));
        SetSprite(AppearancePart.RightEyeFront, GetEquipmentSpriteAddress(address, "[Front]"));

		SetColor(AppearancePart.LeftEyeFront);
		SetColor(AppearancePart.RightEyeFront);
    }
    void SetColor(AppearancePart part)
    {
		if (!appearanceSpriteRenderer.ContainsKey(part))
			return;

		if (!appearanceColor.ContainsKey(part))
			return;

		appearanceSpriteRenderer[part].color = appearanceColor[part];
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

	private string GetEquipmentSpriteAddress(string itemId)
	{
		if (string.IsNullOrEmpty(itemId))
			return string.Empty;

		return equipmentAddressPrefix + itemId;
	}

	private string GetEquipmentSpriteAddress(string itemId, string spriteName)
	{
		if (string.IsNullOrEmpty(itemId))
			return string.Empty;

		return equipmentAddressPrefix + itemId + spriteName;
	}
}
