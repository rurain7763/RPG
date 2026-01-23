using System.Collections.Generic;
using UnityEngine;

public class UILine
{
    internal UILineRenderer renderer;
    internal Line2D line;
    internal bool isDirty;
    internal List<PooledObject> segments;

    public void AddPoints(params Vector2[] points)
    {
        line.AddPoints(points);
        isDirty = true;
    }

    public void AddPointsFromScreen(params Vector2[] screenPoints)
    {
        Vector2[] localPoints = new Vector2[screenPoints.Length];
        for (int i = 0; i < screenPoints.Length; i++)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(renderer.rectTransform, screenPoints[i], renderer.canvas.worldCamera, out var localPos);
            localPoints[i] = localPos;
        }
        line.AddPoints(localPoints);
        isDirty = true;
    }

    public void Clear()
    {
        line.Clear();
        isDirty = true;
    }
}

[RequireComponent(typeof(PooledObjectPool))]
public class UILineRenderer : MonoBehaviour
{
    [SerializeField] private List<Line2D> lines;

    internal Canvas canvas;
    internal RectTransform rectTransform;

    private PooledObjectPool pool;
    private List<UILine> lineUIs = new();

    private void Awake()
    {
        canvas = Helper.GetComponentInHighestAncestor<Canvas>(gameObject);
        rectTransform = GetComponent<RectTransform>();

        pool = GetComponent<PooledObjectPool>();
        pool.Initialize();

        for (int i = 0; i < lines.Count; i++)
        {
            var activeLine = new UILine
            {
                line = lines[i],
                isDirty = true,
                segments = new List<PooledObject>()
            };

            lineUIs.Add(activeLine);
        }
    }

    private void LateUpdate()
    {
        for (int i = 0; i < lineUIs.Count; i++)
        {
            var line = lineUIs[i];

            if (!line.isDirty)
            {
                continue;
            }

            DrawLine(line, i);

            line.isDirty = false;
        }
    }

    private void OnDestroy()
    {
        pool.Cleanup();
    }

    public UILine AddLine(params Vector2[] points)
    {
        var line2D = new Line2D(points);
        lines.Add(line2D);

        var activeLine = new UILine
        {
            renderer = this,
            line = line2D,
            isDirty = true,
            segments = new List<PooledObject>()
        };

        lineUIs.Add(activeLine);

        return activeLine;
    }

    public UILine AddLineFromScreen(params Vector2[] screenPoints)
    {
        var newLine = AddLine();
        newLine.AddPointsFromScreen(screenPoints);
        return newLine;
    }

    public void RemoveLine(UILine line)
    {
        int index = lineUIs.IndexOf(line);

        if (index == -1)
        {
            Logger.Warn("UILineRenderer: Attempted to remove a line that does not exist.");
            return;
        }

        foreach (var segment in line.segments)
        {
            pool.ReleaseObject(segment);
        }
        line.segments.Clear();

        lineUIs.RemoveAt(index);
        lines.RemoveAt(index);
    }

    private void DrawLine(UILine line, int lineIndex)
    {
        if (line.line.Points.Length < 2)
        {
            Logger.Warn("UILineRenderer: Line draw needs at least two points.");
            return;
        }

        List<PooledObject> segments = line.segments;

        for (int i = 0; i < line.line.Points.Length - 1; i++)
        {
            Vector2 start = line.line.Points[i];
            Vector2 end = line.line.Points[i + 1];

            Vector2 direction = end - start;
            float length = direction.magnitude;

            Vector2 center = (start + end) / 2;

            PooledObject segment = null;
            if (i >= segments.Count)
            {
                segment = pool.GetObject();
                segments.Add(segment);
            }
            else
            {
                segment = segments[i];
            }

            var segmentRectTransform = segment.GetComponent<RectTransform>();

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            segmentRectTransform.anchorMin = rectTransform.pivot;
            segmentRectTransform.anchorMax = rectTransform.pivot;
            segmentRectTransform.anchoredPosition = center;
            segmentRectTransform.sizeDelta = new Vector2(length, segmentRectTransform.sizeDelta.y);
            segmentRectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }

        int needCount = line.line.Points.Length - 1;
        for (int i = needCount; i < segments.Count; i++)
        {
            pool.ReleaseObject(segments[i]);
        }
        segments.RemoveRange(needCount, segments.Count - needCount);
    }

    public void Refresh()
    {
        for (int i = 0; i < lineUIs.Count; i++)
        {
            lineUIs[i].isDirty = true;
        }
    }

    public void ClearLines()
    {
        for (int i = 0; i < lineUIs.Count; i++)
        {
            var line = lineUIs[i];

            foreach (var segment in line.segments)
            {
                pool.ReleaseObject(segment);
            }
            line.segments.Clear();
        }

        lineUIs.Clear();
        lines.Clear();
    }
}