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

        //生成UI时顺便加入ButtonAction，读取相关Data
        public void GenerateUI()
        {
            
        }
    }
}