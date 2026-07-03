using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class SkillDefinitionValidationTests
{
    private const string SkillSearchFolder = "Assets/_RAW/Data/ScriptableObjects/Skills";

    [Test]
    public void SkillDefinitions_HaveUniqueSkillIds()
    {
        SkillDefinition[] skills = LoadAllAssets<SkillDefinition>(SkillSearchFolder);

        var skillById = new Dictionary<string, SkillDefinition>();

        foreach (SkillDefinition skill in skills)
        {
            string skillId = GetString(skill, "skillId");

            if (string.IsNullOrWhiteSpace(skillId))
                continue;

            if (skillById.TryGetValue(skillId, out SkillDefinition duplicate))
            {
                Assert.Fail(
                    $"Duplicate SkillDefinition skillId: {skillId}\n" +
                    $"First: {AssetDatabase.GetAssetPath(duplicate)}\n" +
                    $"Second: {AssetDatabase.GetAssetPath(skill)}"
                );
            }

            skillById.Add(skillId, skill);
        }
    }

    [Test]
    public void SkillDefinitions_HaveRequiredFields()
    {
        SkillDefinition[] skills = LoadAllAssets<SkillDefinition>(SkillSearchFolder);

        foreach (SkillDefinition skill in skills)
        {
            string path = AssetDatabase.GetAssetPath(skill);

            string skillId = GetString(skill, "skillId");
            string displayName = GetString(skill, "displayName");

            float range = GetFloat(skill, "range");
            float size = GetFloat(skill, "size");
            float cooldown = GetFloat(skill, "cooldown");
            float manaCost = GetFloat(skill, "manaCost");

            Assert.IsFalse(string.IsNullOrWhiteSpace(skillId), $"{path} has empty skillId.");
            Assert.IsFalse(string.IsNullOrWhiteSpace(displayName), $"{path} has empty displayName.");

            Assert.GreaterOrEqual(range, 0f, $"{path} has negative range.");
            Assert.GreaterOrEqual(size, 0f, $"{path} has negative size.");
            Assert.GreaterOrEqual(cooldown, 0f, $"{path} has negative cooldown.");
            Assert.GreaterOrEqual(manaCost, 0f, $"{path} has negative manaCost.");
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

    private static float GetFloat(Object target, string propertyName)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        Assert.IsNotNull(property, $"{target.name} does not have property: {propertyName}");
        return property.floatValue;
    }
}