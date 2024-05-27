using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTransformPinchSetting : MonoBehaviour, IPinchSettingComp
    {
        //±´Èû¶û¡¢Çò
        public PinchTransfType.TransformType TransformType;

        //Êý¾Ý
        private void Awake()
        {
            this.enabled = false;
        }

        public void LoadData(CharacterModelPinch _modelPinch)
        {
        }

        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
        }
    }
}
