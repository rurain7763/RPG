using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILineRenderer))]
public class UILineRendererEditor : Editor
{
    private UILineRenderer renderer;
    private RectTransform rectTransform;

    private bool isEditMode = false;

    private void OnEnable()
    {
        renderer = (UILineRenderer)target;
        rectTransform = renderer.GetComponent<RectTransform>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        if (isEditMode)
        {
            if (GUILayout.Button("Exit Edit Mode", GUILayout.Height(30)))
            {
                isEditMode = false;
            }
        }
        else
        {
            if (GUILayout.Button("Enter Edit Mode", GUILayout.Height(30)))
            {
                isEditMode = true;
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Refresh All Lines", GUILayout.Height(30)))
        {
            renderer.Refresh();
        }
    }

    private void OnSceneGUI()
    {
        if (renderer == null || rectTransform == null || !isEditMode)
        {
            return;
        }

        serializedObject.Update();
        SerializedProperty linesProp = serializedObject.FindProperty("lines");

        for (int i = 0; i < linesProp.arraySize; i++)
        {
            DrawLineEditor(linesProp.GetArrayElementAtIndex(i), i);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLineEditor(SerializedProperty lineProp, int index)
    {
        SerializedProperty pointsProp = lineProp.FindPropertyRelative("points");
        if (pointsProp == null || pointsProp.arraySize < 2) return;

        Handles.color = Color.cyan;

        for (int i = 0; i < pointsProp.arraySize; i++)
        {
            SerializedProperty pProp = pointsProp.GetArrayElementAtIndex(i);
            Vector2 localPos = pProp.vector2Value;

            Vector3 worldPos = rectTransform.TransformPoint(localPos);

            float size = HandleUtility.GetHandleSize(worldPos) * 0.05f;
            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.FreeMoveHandle(worldPos, size, Vector3.zero, Handles.DotHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                pProp.vector2Value = rectTransform.InverseTransformPoint(newWorldPos);
            }

            if (i < pointsProp.arraySize - 1)
            {
                Vector2 nextLocal = pointsProp.GetArrayElementAtIndex(i + 1).vector2Value;
                Vector3 nextWorld = rectTransform.TransformPoint(nextLocal);
                Handles.DrawLine(worldPos, nextWorld, 2f);
            }
        }
    }
}