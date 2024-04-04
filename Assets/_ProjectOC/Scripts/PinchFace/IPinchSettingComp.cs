using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPinchSettingComp
{
    int Index { get; }
    IPinchPartData PinchData { get; }
    public interface IPinchPartData
    {
        public void SaveData();
        public void LoadData();
    }
}
