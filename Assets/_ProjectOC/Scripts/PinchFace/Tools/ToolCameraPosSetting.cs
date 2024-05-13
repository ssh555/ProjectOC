using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;


public class ToolCameraPosSetting : MonoBehaviour
{
    [SerializeField,InspectorName("����")]
    private Transform TargetTransf;

    [Button("����Ŀ��")]
    public void LookAtTarget()
    {
        if(TargetTransf == null)
            TargetTransf = this.transform.parent;
        this.transform.LookAt(TargetTransf);
    }

    [Button("�������,ɾ�����")]
    public void SettingOver()
    {
        // ���ȳ���ɾ��������   UniversalAdditionalCameraData ���
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
