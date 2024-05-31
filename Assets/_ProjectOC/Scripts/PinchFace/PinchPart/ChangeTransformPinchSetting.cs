using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTransformPinchSetting : MonoBehaviour, IPinchSettingComp
    {
        //±´Èû¶û¡¢Çò
        public PinchTransfType.TransformType TransformType;
        public Vector2 pinchRangeX,pinchBraidRangeY;
        [ReadOnly]
        public Vector2 curValue;
        //Ðý×ª·¶Î§
        
        
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
            _modelPinch.ChangeTransform(_type2,curValue);
        }
    }
}
