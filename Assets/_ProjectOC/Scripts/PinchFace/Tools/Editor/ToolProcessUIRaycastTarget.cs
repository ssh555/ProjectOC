using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ToolProcessUIRaycastTarget
{
    [MenuItem("Tools/Asset Process/Remove Raycast Target")]
    public static void RemoveRaycastTarget()
    {        
        // ��ȡ��ǰѡ�е�UI����
        RectTransform selectedUI = Selection.activeTransform as RectTransform;
        if (selectedUI == null)
        {
            Debug.LogWarning("No UI object selected!");
            return;
        }

        // ��ѡ��UI����������Ӷ����Raycast Target����Ϊfalse
        ToggleRaycastTargets(selectedUI, false);
    }
    
    // �ݹ����UI�����������Ӷ��󣬲������ǵ�Raycast Target��������Ϊָ����ֵ
    private static void ToggleRaycastTargets(RectTransform uiTransform, bool value)
    {
        // ��ȡ��ǰUI�����ϵ�����Graphic���,����Inactivge��
        Graphic[] graphics = uiTransform.GetComponentsInChildren<Graphic>(true);

        // ����ÿ��Graphic�����Raycast Target����
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = value;
        }
    }
}
