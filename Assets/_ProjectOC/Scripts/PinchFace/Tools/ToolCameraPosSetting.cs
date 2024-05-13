using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class ToolCameraPosSetting : MonoBehaviour
{
    [SerializeField,InspectorName("看向")]
    private Transform TargetTransf;

    [Button("看向目标")]
    public void LookAtTarget()
    {
        if(TargetTransf == null)
            TargetTransf = this.transform.parent;
        this.transform.LookAt(TargetTransf);
    }

    [Button("设置完成,删除组件")]
    public void SettingOver()
    {
        // 首先尝试删除依赖项   UniversalAdditionalCameraData 组件
        UniversalAdditionalCameraData additionalCameraData = GetComponent<UniversalAdditionalCameraData>();
        if (additionalCameraData != null)
        {
            DestroyImmediate(additionalCameraData);
        }
        
        Component[] components = transform.GetComponents<Component>();
        foreach (var _comp in components)
        {
            if (_comp.GetType() != typeof(Transform))
            {
                DestroyImmediate(_comp);
            }
        }
    }
}
