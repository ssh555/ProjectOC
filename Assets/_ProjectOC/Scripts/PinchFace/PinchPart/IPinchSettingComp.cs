using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public interface IPinchSettingComp 
    {
        public int Index { get; }
        //PinchPartType2
        public void LoadData(){}

        //����UIʱ˳�����ButtonAction����ȡ���Data
        public void GenerateUI()
        {
            
        }
    }
}