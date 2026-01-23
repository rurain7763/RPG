using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[ExecuteInEditMode]
public class BoundedPreferredSize : MonoBehaviour, ILayoutElement, ILayoutIgnorer
{
    [SerializeField] private bool m_IgnoreLayout = false;
    [SerializeField] private float m_MinWidth = -1;
    [SerializeField] private float m_MinHeight = -1;
    [SerializeField] private float m_MaxWidth = -1;
    [SerializeField] private float m_MaxHeight = -1;
    [SerializeField] private int m_layoutPriority = 1;

    private ILayoutElement m_layoutElement;

    protected void Awake()
    {
        if (m_layoutElement == null)
        {
            m_layoutElement = gameObject.GetComponent<ILayoutElement>();
            if (m_layoutElement == null)
                Debug.LogError("gameObject " + gameObject.name + " is not ILayoutElement");
        }
    }

    public virtual void CalculateLayoutInputHorizontal() { }
    public virtual void CalculateLayoutInputVertical() { }
    public virtual float minWidth { get { return -1; } }
    public float preferredWidth
    {
        get
        {
            float orginalPreferredWidth = m_layoutElement.preferredWidth;
            if (orginalPreferredWidth > m_MaxWidth)
                return m_MaxWidth;
            if (orginalPreferredWidth < m_MinWidth)
                return m_MinWidth;
            return orginalPreferredWidth;
        }
    }
    public virtual float flexibleWidth { get { return -1; } }
    public virtual float minHeight { get { return -1; } }
    public float preferredHeight
    {
        get
        {
            float orginalPreferredHeight = m_layoutElement.preferredHeight;
            if (orginalPreferredHeight > m_MaxHeight)
                return m_MaxHeight;
            if (orginalPreferredHeight < m_MinHeight)
                return m_MinHeight;
            return orginalPreferredHeight;
        }
    }
    public virtual float flexibleHeight { get { return -1; } }
    public virtual int layoutPriority { get { return m_layoutPriority; } }
    public virtual bool ignoreLayout { get { return m_IgnoreLayout; } }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoundedPreferredSize))]
public class BoundedPreferredSizeEditor : Editor
{
    // override the editor to be able to show the public variables on the inspector.
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif