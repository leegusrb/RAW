using UnityEngine;

public class ItemDefinition : ScriptableObject
{
	[Header("Identity")]
    [SerializeField] private string itemId;
	[SerializeField] private string displayName;

	[TextArea]
	[SerializeField] private string description;

	[Header("Classification")]
	[SerializeField] private ItemLine itemLine;
	[SerializeField] private ItemRarity rarity;

	[Header("Presentation")]
	[SerializeField] private Sprite icon;

	[Header("Inventory")]
	[SerializeField] private int maxStack = 1;

	public string ItemId => itemId;
	public string DisplayName => displayName;
	public string Description => description;
	public ItemLine ItemLine => itemLine;
	public ItemRarity Rarity => rarity;
	public Sprite Icon => icon;
	public int MaxStack => maxStack;
}
