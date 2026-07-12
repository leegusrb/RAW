using UnityEngine;

[CreateAssetMenu(menuName = "RAW/Inventory/Inventory Config")]
public class InventoryConfig : ScriptableObject
{
	[SerializeField] private int maxInventoryCapacity = 20;
	[SerializeField] private int defaultInventoryCapacity = 10;

	public int MaxInventoryCapacity => maxInventoryCapacity;
	public int DefaultInventoryCapacity	=> defaultInventoryCapacity;
}
