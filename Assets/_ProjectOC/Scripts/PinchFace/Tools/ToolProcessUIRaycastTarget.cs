using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ToolProcessUIRaycastTarget
{
    [MenuItem("Tools/Asset Process/Remove Raycast Target")]
    public static void RemoveRaycastTarget()
    {        
        // 获取当前选中的UI对象
        RectTransform selectedUI = Selection.activeTransform as RectTransform;
        if (selectedUI == null)
        {
            Debug.LogWarning("No UI object selected!");
            return;
        }

        // 将选定UI对象的所有子对象的Raycast Target设置为false
        ToggleRaycastTargets(selectedUI, false);
    }
    
    // 递归遍历UI对象及其所有子对象，并将它们的Raycast Target属性设置为指定的值
    private static void ToggleRaycastTargets(RectTransform uiTransform, bool value)
    {
        // 获取当前UI对象上的所有Graphic组件,加上Inactivge的
        Graphic[] graphics = uiTransform.GetComponentsInChildren<Graphic>(true);

        // 设置每个Graphic组件的Raycast Target属性
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = value;
        }
    }
}
