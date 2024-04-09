using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public interface IPinchSettingComp
    {
        int Index { get; }
        public void LoadData();

        //����UIʱ˳�����ButtonAction����ȡ���Data
        public void GenerateUI();
    }
}