using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestPos : MonoBehaviour
{
    public RectTransform testTrans;

    // 用于显示世界坐标的变量
    [HideInInspector]
    public Vector3 worldPosition;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (testTrans != null)
        {
            worldPosition = GetWorldPosition(testTrans);
        }
    }

    public Vector3 GetWorldPosition(RectTransform rectTransform)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform is null!");
            return Vector3.zero;
        }

        Vector3 worldPosition = rectTransform.localPosition;
        Transform parent = rectTransform.parent;

        while (parent != null)
        {
            RectTransform parentRectTransform = parent.GetComponent<RectTransform>();
            if (parentRectTransform != null)
            {
                Vector3 parentScale = parentRectTransform.localScale;
                worldPosition = Vector3.Scale(worldPosition, parentScale) + parentRectTransform.localPosition;
            }
            else
            {
                Debug.LogWarning("Parent does not have RectTransform component!");
            }

            parent = parent.parent;
        }

        return worldPosition;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestPos))]
public class TestPosEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestPos script = (TestPos)target;

        if (script.testTrans != null)
        {
            // 显示世界坐标
            EditorGUILayout.LabelField("World Position", script.worldPosition.ToString());
        }
        else
        {
            EditorGUILayout.LabelField("World Position", "N/A");
        }
    }
}
#endif