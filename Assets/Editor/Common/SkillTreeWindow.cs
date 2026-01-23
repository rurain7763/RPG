using UnityEditor;
using UnityEngine;

public class SkillTreeWindow : EditorWindow
{
    private SkillTreeBuilder builder;

    // 캔버스 이동(Pan)을 위한 오프셋
    private Vector2 panOffset = Vector2.zero;

    // 마우스 우클릭 위치
    private Vector2 contextClickPosition;

    // 노드 드래그 상태
    private bool isDraggingNode = false;
    private int draggedNodeIndex = -1;

    // 연결선 드래그 상태
    private bool isDrawingConnection = false;
    private int startNodeIndex = -1;
    private Vector2 dragMousePosition;

    // 노드의 최소 크기
    private readonly Vector2 MIN_NODE_SIZE = new Vector2(250, 160); // 노드 크기를 적절히 조정

    [MenuItem("Tools/Skill Tree Editor")]
    public static void ShowWindow()
    {
        GetWindow<SkillTreeWindow>("Skill Tree Editor");
    }

    private void OnGUI()
    {
        // 1. Builder 선택 필드
        builder = EditorGUILayout.ObjectField("Skill Tree Builder", builder, typeof(SkillTreeBuilder), false) as SkillTreeBuilder;
        if (builder == null)
        {
            EditorGUILayout.HelpBox("편집할 SkillTreeBuilder ScriptableObject를 선택하세요.", MessageType.Info);
            return;
        }

        // 2. GUI 영역 정의
        Rect fullRect = new Rect(0, 0, position.width, position.height);

        // 3. 배경 및 연결선 그리기
        DrawGrid(20, 0.2f, Color.gray);
        DrawGrid(100, 0.4f, Color.gray);
        DrawConnections();

        // 4. 노드 그리기 및 속성 처리
        ProcessNodes();

        // 5. 마우스/키보드 이벤트 처리 (드래그, 연결, 컨텍스트 메뉴)
        ProcessManualEvents(fullRect);

        // 6. 변경 사항 감지 및 갱신
        if (GUI.changed)
        {
            Repaint();
        }
    }

    // ----------------------------------------------------
    // Draw Methods (그리기 메소드)
    // ----------------------------------------------------

    private void DrawGrid(float spacing, float opacity, Color color)
    {
        Handles.BeginGUI();
        Handles.color = new Color(color.r, color.g, color.b, opacity);

        Vector2 newOffset = new Vector2(panOffset.x % spacing, panOffset.y % spacing);

        for (int i = 0; i < position.width / spacing; i++)
        {
            Handles.DrawLine(new Vector3(spacing * i + newOffset.x, 0), new Vector3(spacing * i + newOffset.x, position.height));
        }

        for (int j = 0; j < position.height / spacing; j++)
        {
            Handles.DrawLine(new Vector3(0, spacing * j + newOffset.y), new Vector3(position.width, spacing * j + newOffset.y));
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    private void DrawConnections()
    {
        if (builder.AllNodes.Count < 1) return;

        Handles.BeginGUI();

        for (int i = 0; i < builder.AllNodes.Count; i++)
        {
            var parentNode = builder.AllNodes[i];

            // 노드 중앙 위치 계산
            Vector3 startPos = parentNode.Position + panOffset + MIN_NODE_SIZE / 2f;

            // ChildIndices를 순회하며 연결선 그리기
            foreach (var childIndex in parentNode.ChildIndices)
            {
                if (childIndex >= 0 && childIndex < builder.AllNodes.Count)
                {
                    var childNode = builder.AllNodes[childIndex];
                    Vector3 endPos = childNode.Position + panOffset + MIN_NODE_SIZE / 2f;

                    Handles.color = Color.cyan;
                    Handles.DrawBezier(
                        startPos,
                        endPos,
                        startPos + Vector3.right * 50f,
                        endPos + Vector3.left * 50f,
                        Color.cyan,
                        null,
                        3f
                    );
                }
            }
        }

        // 연결 드래그 중인 선 그리기
        if (isDrawingConnection && startNodeIndex != -1)
        {
            Vector3 startPos = builder.AllNodes[startNodeIndex].Position + panOffset + MIN_NODE_SIZE / 2f;

            Handles.color = Color.yellow;
            Handles.DrawBezier(
                startPos,
                dragMousePosition,
                startPos + Vector3.right * 50f,
                dragMousePosition + Vector2.left * 50f,
                Color.yellow,
                null,
                3f
            );
        }

        Handles.EndGUI();
    }


    private void ProcessNodes()
    {
        if (builder == null) return;

        // SerializedObject를 사용하여 노드 속성을 수정하고 저장
        SerializedObject serializedObject = new SerializedObject(builder);
        SerializedProperty nodesProp = serializedObject.FindProperty(nameof(SkillTreeBuilder.AllNodes));
        if (nodesProp == null) return;

        Handles.BeginGUI();

        // 노드 배열 순회
        for (int i = 0; i < nodesProp.arraySize; i++)
        {
            SerializedProperty nodeProp = nodesProp.GetArrayElementAtIndex(i);
            SerializedProperty positionProp = nodeProp.FindPropertyRelative("Position");
            SerializedProperty skillProp = nodeProp.FindPropertyRelative("Skill");
            SerializedProperty certificateProp = nodeProp.FindPropertyRelative("Certificate");

            Rect screenRect = new Rect(positionProp.vector2Value + panOffset, MIN_NODE_SIZE);

            // 1. 노드 배경 그리기
            Color nodeColor = (i == draggedNodeIndex) ? new Color(0.3f, 0.3f, 0.4f, 1f) : new Color(0.2f, 0.2f, 0.2f, 1f);
            Handles.DrawSolidRectangleWithOutline(screenRect, nodeColor, Color.black);

            // 2. 노드 내용 (Inspector 필드) 그리기
            GUILayout.BeginArea(screenRect);

            // Skill Data 표시
            if (skillProp.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(skillProp, GUIContent.none);
            }
            else
            {
                SerializedObject skillObj = new SerializedObject(skillProp.objectReferenceValue);
                SerializedProperty displayName = skillObj.FindProperty("DisplayName");
                SerializedProperty icon = skillObj.FindProperty("Icon");

                EditorGUILayout.LabelField(displayName.stringValue, EditorStyles.boldLabel);
                EditorGUILayout.ObjectField(icon, typeof(Sprite), GUIContent.none, GUILayout.Width(64), GUILayout.Height(64));
                EditorGUILayout.PropertyField(certificateProp, true);
            }

            GUILayout.EndArea();
        }

        Handles.EndGUI();
        // SerializedProperty를 변경했으면 이 시점에서 적용
        serializedObject.ApplyModifiedProperties();
    }


    // ----------------------------------------------------
    // Event Methods (이벤트 처리 메소드)
    // ----------------------------------------------------

    private void ProcessManualEvents(Rect fullRect)
    {
        Event e = Event.current;
        contextClickPosition = e.mousePosition;

        // 1. 마우스 다운
        if (e.type == EventType.MouseDown)
        {
            draggedNodeIndex = GetNodeIndexAtPosition(e.mousePosition);

            if (e.button == 0) // 왼쪽 버튼 (노드 이동 드래그)
            {
                if (draggedNodeIndex != -1)
                {
                    isDraggingNode = true;
                    e.Use();
                }
            }
            else if (e.button == 1) // 오른쪽 버튼 (연결 드래그 시작 또는 컨텍스트 메뉴)
            {
                if (e.alt && draggedNodeIndex != -1)
                {
                    // 노드 위에서 우클릭 시, 연결 드래그 시작으로 간주 (Alt 키는 제외하고 기본 우클릭으로 연결 시작)
                    isDrawingConnection = true;
                    startNodeIndex = draggedNodeIndex;
                    e.Use();
                }
            }
        }

        // 2. 마우스 드래그
        else if (e.type == EventType.MouseDrag)
        {
            if (isDraggingNode && draggedNodeIndex != -1)
            {
                UpdateNodePosition(draggedNodeIndex, e.delta);
                GUI.changed = true;
                e.Use();
            }
            else if (isDrawingConnection)
            {
                dragMousePosition = e.mousePosition;
                GUI.changed = true;
                e.Use();
            }
            else if (e.button == 0 && !isDraggingNode && fullRect.Contains(e.mousePosition))
            {
                // 캔버스 팬 로직
                panOffset += e.delta;
                GUI.changed = true;
                e.Use();
            }
        }

        // 3. 마우스 업
        if (e.type == EventType.MouseUp)
        {
            if (e.button == 0) // 왼쪽 버튼 드래그 종료
            {
                isDraggingNode = false;
                draggedNodeIndex = -1;
                GUI.changed = true;
            }
            else if (e.button == 1) // 오른쪽 버튼 처리
            {
                if (isDrawingConnection)
                {
                    // A. 연결 드래그 종료: 연결 시도
                    int endNodeIndex = GetNodeIndexAtPosition(e.mousePosition);

                    if (endNodeIndex != -1 && startNodeIndex != endNodeIndex)
                    {
                        // builder의 ConnectNode 함수 호출
                        ConnectNodes(startNodeIndex, endNodeIndex);
                    }

                    isDrawingConnection = false;
                    startNodeIndex = -1;
                    GUI.changed = true;
                    e.Use();
                }
                else
                {
                    // B. 단순 오른쪽 클릭: 컨텍스트 메뉴 띄우기
                    ShowContextMenu();
                    e.Use();
                }
            }
        }
    }

    private void ShowContextMenu()
    {
        GenericMenu menu = new GenericMenu();
        int clickedNodeIndex = GetNodeIndexAtPosition(contextClickPosition);

        if (clickedNodeIndex != -1)
        {
            // 노드 관련 메뉴
            menu.AddItem(new GUIContent($"Remove Node ({clickedNodeIndex})"), false, () => RemoveNode(clickedNodeIndex));
            menu.AddItem(new GUIContent($"Disconnect All from Node ({clickedNodeIndex})"), false, () => DisconnectAllFromNode(clickedNodeIndex));
        }
        else
        {
            // 캔버스 관련 메뉴
            menu.AddItem(new GUIContent("Add Node"), false, OnClickAddNode);
        }

        menu.ShowAsContext();
    }

    // ----------------------------------------------------
    // Utility & Action Methods (유틸리티 및 액션 메소드)
    // ----------------------------------------------------

    /// <summary>
    /// 마우스 위치에 있는 노드의 인덱스를 반환합니다.
    /// </summary>
    private int GetNodeIndexAtPosition(Vector2 mousePosition)
    {
        if (builder == null) return -1;

        // 뒤에서부터 순회하여 가장 위에 있는 노드를 선택
        for (int i = builder.AllNodes.Count - 1; i >= 0; i--)
        {
            var nodeData = builder.AllNodes[i];

            // 노드 크기는 MIN_NODE_SIZE를 기준으로 임시 계산
            Rect screenRect = new Rect(nodeData.Position + panOffset, MIN_NODE_SIZE);

            if (screenRect.Contains(mousePosition))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// 노드 위치를 업데이트하고 Unity 직렬화 시스템에 기록합니다.
    /// </summary>
    private void UpdateNodePosition(int index, Vector2 delta)
    {
        if (builder == null || index < 0 || index >= builder.AllNodes.Count) return;

        SerializedObject serializedObject = new SerializedObject(builder);
        SerializedProperty nodesProp = serializedObject.FindProperty(nameof(SkillTreeBuilder.AllNodes));

        // 해당 노드의 Position 속성을 찾아 값을 변경
        SerializedProperty positionProp = nodesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Position");

        // 🌟 변경 전 Undo 기록 (SerializedObject는 변경 전 RecordObject를 필요로 하지 않지만, 안전을 위해)
        Undo.RecordObject(builder, "Move Node");

        positionProp.vector2Value += delta;

        // 변경 사항을 SerializedObject에 적용하고 파일에 저장
        serializedObject.ApplyModifiedProperties();

        // EditorUtility.SetDirty(builder); // ApplyModifiedProperties가 저장 처리를 포함합니다.
        GUI.changed = true;
    }

    /// <summary>
    /// 새로운 노드를 리스트에 추가합니다.
    /// </summary>
    private void OnClickAddNode()
    {
        if (builder == null) return;

        // SerializedProperty를 사용하여 배열에 요소를 추가해야 Undo/Redo가 가능함
        SerializedObject serializedObject = new SerializedObject(builder);
        SerializedProperty nodesProp = serializedObject.FindProperty(nameof(SkillTreeBuilder.AllNodes));

        // 배열에 빈 요소 추가
        nodesProp.InsertArrayElementAtIndex(nodesProp.arraySize);
        SerializedProperty newNodeProp = nodesProp.GetArrayElementAtIndex(nodesProp.arraySize - 1);

        // 초기 위치 설정
        newNodeProp.FindPropertyRelative("Position").vector2Value = contextClickPosition - panOffset;

        // List<int> 속성 (ParentIndices, ChildIndices)을 찾아서 명시적으로 Clear
        // *주의: FindPropertyRelative가 참조 타입 List를 제대로 초기화하지 못할 수 있으므로, 
        // Array의 ClearArray()를 호출하는 것이 안전합니다.
        newNodeProp.FindPropertyRelative("ChildIndices").ClearArray();
        newNodeProp.FindPropertyRelative("ParentIndices").ClearArray();

        // Skill과 UnlockConditions 배열 초기화 (필요하다면)
        newNodeProp.FindPropertyRelative("Skill").objectReferenceValue = null;
        SerializedProperty conditionsProp = newNodeProp.FindPropertyRelative("UnlockConditions");
        if (conditionsProp != null) conditionsProp.ClearArray();


        serializedObject.ApplyModifiedProperties();
        GUI.changed = true;
        Repaint();
    }

    /// <summary>
    /// 두 노드를 연결합니다. (builder의 ConnectNode 함수 활용)
    /// </summary>
    private void ConnectNodes(int parentIndex, int childIndex)
    {
        if (builder == null) return;

        // 🌟 builder의 메소드를 호출하기 전에 Undo 기록
        Undo.RecordObject(builder, "Connect Nodes");

        // builder의 로직을 사용하여 인덱스 연결
        builder.ConnectNode(parentIndex, childIndex);

        // builder 객체가 변경되었음을 명시적으로 알립니다.
        EditorUtility.SetDirty(builder);
        GUI.changed = true;
    }

    /// <summary>
    /// 노드를 제거하고 인덱스를 재정렬합니다. (builder의 RemoveNode 함수 활용)
    /// </summary>
    private void RemoveNode(int indexToRemove)
    {
        if (builder == null) return;

        // 🌟 builder의 메소드를 호출하기 전에 Undo 기록
        Undo.RecordObject(builder, "Remove Node");

        // builder의 로직을 사용하여 노드 제거 및 인덱스 재정렬
        builder.RemoveNode(indexToRemove);

        // builder 객체가 변경되었음을 명시적으로 알립니다.
        EditorUtility.SetDirty(builder);
        GUI.changed = true;
    }

    /// <summary>
    /// 노드와 관련된 모든 연결을 끊습니다.
    /// </summary>
    private void DisconnectAllFromNode(int index)
    {
        if (builder == null || index < 0 || index >= builder.AllNodes.Count) return;

        // 🌟 Undo 기록 시작
        Undo.RecordObject(builder, "Disconnect All from Node");

        var node = builder.AllNodes[index];

        // 1. 부모 노드들에서 자신의 인덱스 참조를 제거
        foreach (var parentIndex in node.ParentIndices.ToArray())
        {
            if (parentIndex >= 0 && parentIndex < builder.AllNodes.Count)
            {
                builder.AllNodes[parentIndex].ChildIndices.Remove(index);
            }
        }

        // 2. 자식 노드들에서 자신의 인덱스 참조를 제거
        foreach (var childIndex in node.ChildIndices.ToArray())
        {
            if (childIndex >= 0 && childIndex < builder.AllNodes.Count)
            {
                builder.AllNodes[childIndex].ParentIndices.Remove(index);
            }
        }

        // 3. 자신의 연결 목록 초기화
        node.ParentIndices.Clear();
        node.ChildIndices.Clear();

        // 변경 사항을 Unity에 알림
        EditorUtility.SetDirty(builder);
        GUI.changed = true;
    }
}