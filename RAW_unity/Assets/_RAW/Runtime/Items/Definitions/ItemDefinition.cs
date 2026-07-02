using UnityEngine;

public class ItemDefinition : ScriptableObject
{
    public string itemId;
	public string displayName;
	[TextArea]
	public string description;

	public ItemLine itemLine;
	public ItemRarity rarity;

	public Sprite icon;
	public int maxStack = 1;
}
