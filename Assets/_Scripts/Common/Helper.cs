using Codice.Client.Common.Connection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Helper
{
    public static readonly float HalfPI = Mathf.PI / 2f;

    public static bool ElapsedEqual(float a, float b, float threshold = 0.001f)
    {
        return Mathf.Abs(a - b) < threshold;
    }

    public static IEnumerator WaitUntilAnimationFinished(Animator animator, string stateName, int layerIndex = 0)
    {
        yield return new WaitUntil(() =>
        {
            return animator.IsInTransition(layerIndex) == false;
        });

        yield return new WaitUntil(() =>
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.IsName(stateName) && stateInfo.normalizedTime >= 1f;
        });
    }

    public static IEnumerator WaitUntilAnimationFinished(Animator animator, int stateHash, int layerIndex = 0)
    {
        yield return new WaitUntil(() =>
        {
            return animator.IsInTransition(layerIndex) == false;
        });

        yield return new WaitUntil(() =>
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
            return stateInfo.shortNameHash == stateHash && stateInfo.normalizedTime >= 1f;
        });
    }

    public static bool IsAnimationStateFinished(Animator animator, int stateHash, int layerIndex = 0)
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (!stateInfo.loop)
        {
            return stateInfo.shortNameHash == stateHash && stateInfo.normalizedTime >= 1f;
        }
        else
        {
            return false;
        }
    }

    public static int GetAnimationLoopCount(Animator animator, int stateHash, int layerIndex = 0)
    {
        var stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (stateInfo.shortNameHash == stateHash)
        {
            return (int)stateInfo.normalizedTime;
        }
        else
        {
            return 0;
        }
    }

    public static Color GetColorByHexString(string hexString)
    {
        if (ColorUtility.TryParseHtmlString(hexString, out var color))
        {
            return color;
        }
        else
        {
            throw new ArgumentException($"Invalid hex color string: {hexString}");
        }
    }

    public static string GetColorHexString(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }

    public static string MakeColoredString(string hexString, string content)
    {
        return $"<color={hexString}>{content}</color>";
    }

    public static string MakeColoredString(Color color, string content)
    {
        string hexString = GetColorHexString(color);
        return MakeColoredString(hexString, content);
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    public static void SortAtLast<T>(IList<T> list, Comparison<T> comparison)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            if (comparison(list[i], list[i - 1]) < 0)
            {
                var temp = list[i];
                list[i] = list[i - 1];
                list[i - 1] = temp;
            }
            else
            {
                break;
            }
        }
    }

    public static int NextRepeat(int current, int max)
    {
        return (current + 1) % max;
    }

    public static int PreviousRepeat(int current, int max)
    {
        return (current - 1 + max) % max;
    }

    public static T GetComponentInHighestAncestor<T>(GameObject gameObject) where T : Component
    {
        T highestAncestorComponent = null;
        Transform current = gameObject.transform;

        while (current != null)
        {
            T component = current.GetComponent<T>();
            if (component != null)
            {
                highestAncestorComponent = component;
            }
            current = current.parent;
        }
        return highestAncestorComponent;
    }

    public static bool HasComponent<T>(GameObject gameObject) where T : Component
    {
        return gameObject.GetComponent<T>() != null;
    }

    public static bool HasComponentInChildren<T>(GameObject gameObject) where T : Component
    {
        return gameObject.GetComponentInChildren<T>() != null;
    }

    public static Vector2[] CalcTrajectoryPoints2D(Vector2 direction, float throwPower, float gravity, int numPoints, float timeStep)
    {
        Vector2 velocity = direction * throwPower;

        Vector2[] points = new Vector2[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            float t = i * timeStep;
            float x = velocity.x * t;
            float y = velocity.y * t + 0.5f * gravity * t * t;

            points[i] = new Vector2(x, y);
        }

        return points;
    }

    public static Vector2 CalcArcVelocity2D(Vector2 from, Vector2 to, float preferredHeight, float gravity)
    {
        float deltaY = to.y - from.y;
        float deltaX = to.x - from.x;

        float absGravity = Mathf.Abs(gravity);
        float actualHeight = Mathf.Max(preferredHeight, deltaY + 0.1f);

        float timeToApex = Mathf.Sqrt(2 * preferredHeight / absGravity);
        float timeFromApex = Mathf.Sqrt(2 * (preferredHeight - deltaY) / absGravity);

        float totalTime = timeToApex + timeFromApex;

        return new Vector2(deltaX / totalTime, Mathf.Sqrt(2 * absGravity * preferredHeight));
    }

    public static T GetRandomWeightedItem<T>(IEnumerable<T> items) where T : IWeightedItem
    {
        int totalWeight = 0;
        foreach (var item in items)
        {
            totalWeight += item.Weight;
        }
        return GetRandomWeightedItem(items, totalWeight);
    }

    public static T GetRandomWeightedItem<T>(IEnumerable<T> items, int totalWeight) where T : IWeightedItem
    {
        int randomWeight = UnityEngine.Random.Range(0, totalWeight);
        int currentWeight = 0;
        foreach (var item in items)
        {
            currentWeight += item.Weight;
            if (randomWeight < currentWeight)
            {
                return item;
            }
        }
        throw new InvalidOperationException("No items available to select.");
    }

    public static void EachDirectionsOnArc2D(Vector2 centerDir, float anglePerElement, float maxArcAngle, int elementCount, Action<int, Vector2> action)
    {
        float totalArcAngle = Mathf.Min(anglePerElement * (elementCount - 1), maxArcAngle);

        float baseAngle = Mathf.Atan2(centerDir.y, centerDir.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle - (totalArcAngle / 2.0f);
        float stepAngle = (elementCount > 1) ? (totalArcAngle / (elementCount - 1)) : 0.0f;

        for (int i = 0; i < elementCount; i++)
        {
            float currentAngle = startAngle + (stepAngle * i);
            float toRad = currentAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(toRad), Mathf.Sin(toRad));

            action.Invoke(i, dir);
        }
    }

    public static void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit(); 
#endif
    }
}