using UnityEngine;

[CreateAssetMenu(menuName = "RAW/Items/Equipment Definition")]
public class EquipmentDefinition : ScriptableObject
{
    public EquipmentSlot equipmentSlot;

	public int attackPower;
	public int abilityPower;
	public float attackSpeed;
	public float cooldownReduction;

	public float criticalChance;
	public float criticalDamage;

	public int maxMana;
	public float manaRegen;
	public float manaCostReduction;
}
