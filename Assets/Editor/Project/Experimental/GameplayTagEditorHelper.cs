using Experimental;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GameplayTagEditorHelper
{
    [MenuItem("Assets/Make GameplayTag child", false, priority = 500)]
    private static void MakeChildOfSelectedTag(MenuCommand command)
    {
        var parentTag = Selection.activeObject as GameplayTag;
        string parentTagPath = AssetDatabase.GetAssetPath(parentTag);
        string parentTagDirectory = Path.GetDirectoryName(parentTagPath);

        var childTag = ScriptableObject.CreateInstance<GameplayTag>();
        string childTagDefaultName = $"{parentTag.name}.";

        ProjectWindowUtil.CreateAsset(childTag, Path.Combine(parentTagDirectory, $"{childTagDefaultName}.asset"));
    }

    [MenuItem("Assets/Make GameplayTag child", true)]
    private static bool ValidateMakeChildOfSelectedTag(MenuCommand command)
    {
        return Selection.activeObject is GameplayTag;
    }
}