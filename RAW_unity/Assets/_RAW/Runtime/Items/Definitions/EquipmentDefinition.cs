using UnityEngine;

[CreateAssetMenu(menuName = "RAW/Items/Equipment Definition")]
public class EquipmentDefinition : ItemDefinition
{
	[Header("Equipment")]
    [SerializeField] private EquipmentSlot equipmentSlot;

	[Header("Stats")]
	[SerializeField] private EquipmentStatBlock stats = new EquipmentStatBlock();

	public EquipmentSlot EquipmentSlot => equipmentSlot;
	public EquipmentStatBlock Stats => stats;
}
