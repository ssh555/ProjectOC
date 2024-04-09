using System.Collections;
using System.Collections.Generic;
using ML.Engine.SaveSystem;
using ML.Engine.Utility;
using ProjectOC.PinchFace;
using UnityEngine;

public class PinchPart : MonoBehaviour,ISaveData
{
    public string SavePath { get; set; }
    public string SaveName { get; set; }
    public bool IsDirty { get; set; }
    public Version Version { get; set; }
    public object Clone()
    {
        throw new System.NotImplementedException();
    }

    public PinchPartType3 PinchPartType3;
    private List<IPinchSettingComp> _pinchSettingComps;

    //����_pinchSettingComps Buttons������
    public void GenerateUI()
    {
        
    }
}
