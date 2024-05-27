using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public interface IPinchSettingComp 
    {
        //PinchPartType2
        public void LoadData(CharacterModelPinch _modelPinch);
        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch);
    }
}