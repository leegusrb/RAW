using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class ItemDefinitionValidationTests
{
    private const string ItemSearchFolder = "Assets/_RAW/Data/ScriptableObjects/Items";

    [Test]
    public void ItemDefinitions_HaveUniqueItemIds()
    {
        ItemDefinition[] items = LoadAllAssets<ItemDefinition>(ItemSearchFolder);

        var itemById = new Dictionary<string, ItemDefinition>();

        foreach (ItemDefinition item in items)
        {
            string itemId = GetString(item, "itemId");

            if (string.IsNullOrWhiteSpace(itemId))
                continue;

            if (itemById.TryGetValue(itemId, out ItemDefinition duplicate))
            {
                Assert.Fail(
                    $"Duplicate ItemDefinition itemId: {itemId}\n" +
                    $"First: {AssetDatabase.GetAssetPath(duplicate)}\n" +
                    $"Second: {AssetDatabase.GetAssetPath(item)}"
                );
            }

            itemById.Add(itemId, item);
        }
    }

    [Test]
    public void ItemDefinitions_HaveRequiredFields()
    {
        ItemDefinition[] items = LoadAllAssets<ItemDefinition>(ItemSearchFolder);

        foreach (ItemDefinition item in items)
        {
            string path = AssetDatabase.GetAssetPath(item);

            string itemId = GetString(item, "itemId");
            string displayName = GetString(item, "displayName");
            int maxStack = GetInt(item, "maxStack");

            Assert.IsFalse(string.IsNullOrWhiteSpace(itemId), $"{path} has empty itemId.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(displayName), $"{path} has empty displayName.");
            Assert.GreaterOrEqual(maxStack, 1, $"{path} has invalid maxStack.");
        }
    }

    [Test]
    public void EquipmentDefinitions_HaveRequiredFields()
    {
        EquipmentDefinition[] equipments = LoadAllAssets<EquipmentDefinition>(ItemSearchFolder);

        foreach (EquipmentDefinition equipment in equipments)
        {
            string path = AssetDatabase.GetAssetPath(equipment);

            string itemId = GetString(equipment, "itemId");
            string displayName = GetString(equipment, "displayName");
            int maxStack = GetInt(equipment, "maxStack");

            SerializedObject serializedObject = new SerializedObject(equipment);

            SerializedProperty equipmentSlot = serializedObject.FindProperty("equipmentSlot");
            SerializedProperty stats = serializedObject.FindProperty("stats");

            Assert.IsFalse(string.IsNullOrWhiteSpace(itemId), $"{path} has empty itemId.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(displayName), $"{path} has empty displayName.");

            Assert.IsNotNull(equipmentSlot, $"{path} does not have equipmentSlot property.");
            Assert.IsNotNull(stats, $"{path} does not have stats property.");

            Assert.AreEqual(1, maxStack, $"{path} is equipment, so maxStack should be 1.");
        }
    }

    private static T[] LoadAllAssets<T>(string folder) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder });
        var assets = new List<T>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                assets.Add(asset);
        }

        return assets.ToArray();
    }

    private static string GetString(Object target, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        Assert.IsNotNull(property, $"{target.name} does not have property: {propertyName}");
        return property.stringValue;
    }

    private static int GetInt(Object target, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        Assert.IsNotNull(property, $"{target.name} does not have property: {propertyName}");
        return property.intValue;
    }
}