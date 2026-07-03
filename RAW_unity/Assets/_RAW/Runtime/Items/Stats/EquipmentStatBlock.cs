using System;
using UnityEngine;

[Serializable]
public class EquipmentStatBlock
{
	[Header("AD Line")]
	[SerializeField] private int attackPower;
	[SerializeField] private float attackSpeed;

	[Header("AP Line")]
	[SerializeField] private int abilityPower;
	[SerializeField] private float cooldownReduction;

	[Header("Critical Line")]
	[SerializeField] private float criticalChance;
	[SerializeField] private float criticalDamage;

	[Header("Mana Line")]
	[SerializeField] private int maxMana;
	[SerializeField] private float manaRegen;
	[SerializeField] private float manaCostReduction;

	[Header("Status Effect Line")]
	[SerializeField] private StatusEffectStatBlock statusEffectStats;

	public int AttackPower => attackPower;
	public float AttackSpped => attackSpeed;

	public int AbilityPower => abilityPower;
	public float CooldownReduction => cooldownReduction;

	public float CriticalChance => criticalChance;
	public float CriticalDamage => criticalDamage;

	public int MaxMana => maxMana;
	public float ManaRegen => manaRegen;
	public float ManaCostReduction => manaCostReduction;

	public StatusEffectStatBlock StatusEffectStats => statusEffectStats;
}
