using CustomDict;
using UnityEngine;

public class DataBase : MonoBehaviour
{
    //public int EquipmnetSlotSize = Enum.GetValues(typeof(EquipmentSlot)).Length;
    public string equipmentAddress = "Assets/DataBase/Equipment/";    
    //public CustomDictKeyMap KeyMap = new CustomDictKeyMap();
    public CustomDictSkill mySkillKeyMap = new CustomDictSkill();

    public static DataBase Instance;

    public int maxInventoryCapacity = 20;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
