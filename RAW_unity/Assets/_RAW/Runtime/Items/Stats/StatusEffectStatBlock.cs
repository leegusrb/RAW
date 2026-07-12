using System;
using UnityEngine;

[Serializable]
public class StatusEffectStatBlock
{
	[SerializeField] private StatusEffectType statusEffectType;
	[SerializeField] private float applyChance;
	[SerializeField] private float durationBonus;
	[SerializeField] private float effectPowerBonus;

	public StatusEffectType StatusEffectType => statusEffectType;
	public float ApplyChance => applyChance;
	public float DurationBonus => durationBonus;
	public float EffectffectPowerBonus => effectPowerBonus;
}
