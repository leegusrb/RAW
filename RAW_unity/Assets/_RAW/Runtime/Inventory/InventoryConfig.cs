using UnityEngine;

[CreateAssetMenu(menuName = "RAW/Inventory/Inventory Config")]
public class InventoryConfig : ScriptableObject
{
	[SerializeField] private int maxInventotyCapacity = 20;
	[SerializeField] private int defaultInventoryCapacity = 10;

	public int MaxInventotyCapacity => maxInventotyCapacity;
	public int DefaultInventoryCapacity	=> defaultInventoryCapacity;
}
