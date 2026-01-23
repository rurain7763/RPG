using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(MonoBehaviour), true)]
public class AutoBinderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var targetBehaviour = target as MonoBehaviour;
        if (targetBehaviour == null)
        {
            return;
        }

        if (GUILayout.Button("Auto Bind"))
        {
            AutoBind(targetBehaviour);
            EditorUtility.SetDirty(targetBehaviour);
        }
    }

    private void AutoBind(MonoBehaviour behaviour)
    {
        var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        int successCount = 0, failCount = 0;
        foreach (var field in fields) 
        {
            var attr = field.GetCustomAttribute<ReferenceAttribute>();
            if (attr == null)
            {
                continue;
            }

            Transform child = behaviour.transform.Find(attr.Path);
            if (child == null)
            {
                if (attr.Required)
                {
                    Debug.LogWarning($"[AutoBinder] Required field '{field.Name}' not found at path '{attr.Path}' in '{behaviour.name}'.");
                    failCount++;
                }
                continue;
            }

            if (field.FieldType == typeof(GameObject))
            {
                field.SetValue(behaviour, child.gameObject);
            }
            else if (field.FieldType.IsArray)
            {
                System.Type elementType = field.FieldType.GetElementType();

                var components = child.GetComponentsInChildren(elementType, true);

                var array = System.Array.CreateInstance(elementType, components.Length);
                for (int i = 0; i < components.Length; i++)
                {
                    array.SetValue(components[i], i);
                }

                field.SetValue(behaviour, array);
            }
            else
            {
                var component = child.GetComponent(field.FieldType);
                if (component == null)
                {
                    if (attr.Required)
                    {
                        Debug.LogWarning($"[AutoBinder] Required component of type '{field.FieldType}' not found on '{child.name}' for field '{field.Name}' in '{behaviour.name}'.");
                        failCount++;
                    }
                    continue;
                }

                field.SetValue(behaviour, component);
            }

            successCount++;
        }

        Debug.Log($"[AutoBinder] Binding complete for '{behaviour.name}'. Success: {successCount}, Failures: {failCount}.");
    }
}