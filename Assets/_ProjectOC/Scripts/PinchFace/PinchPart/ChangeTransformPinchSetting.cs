using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTransformPinchSetting : MonoBehaviour, IPinchSettingComp
    {
        //����������
        public PinchTransfType.TransformType TransformType;

        //����
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
