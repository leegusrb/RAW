// TODO:
// 임시 전역 데이터 컨테이너.
// 추후 SkillSlotDatabase, EquipmentDatabase, InventoryConfig로 분리한다.

using UnityEngine;

public class DataBase : MonoBehaviour
{
    public string equipmentAddress = "Assets/DataBase/Equipment/";    

    public static DataBase Instance;

    public int maxInventoryCapacity = 20;

    private void Awake()
    {
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;

		if (transform.parent != null)
		{
			Debug.LogError(
				"DataBase must be placed on a root GameObject. " +
				"Move this GameObject to the top level of the Hierarchy."
			);
			return;
		}

		DontDestroyOnLoad(gameObject);
    }
}
