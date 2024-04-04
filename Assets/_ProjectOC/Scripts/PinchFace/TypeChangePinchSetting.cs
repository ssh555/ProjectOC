using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeChangePinchSetting : IPinchSettingComp
{
    public int Index { get; }
    public IPinchSettingComp.IPinchPartData PinchData { get; }

    public class TypeChangePinchSettingData : IPinchSettingComp.IPinchPartData 
    {
        public void SaveData()
        {
            throw new System.NotImplementedException();
        }

        public void LoadData()
        {
            throw new System.NotImplementedException();
        }
    }
}
