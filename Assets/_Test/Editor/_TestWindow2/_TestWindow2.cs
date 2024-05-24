using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class _TestWindow2 : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    [MenuItem("Window/UI Toolkit/_TestWindow2")]
    public static void ShowExample()
    {
        _TestWindow2 wnd = GetWindow<_TestWindow2>();
        wnd.titleContent = new GUIContent("_TestWindow2");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
    }
}
