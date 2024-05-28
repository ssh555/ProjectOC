using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeColorPinchSetting : MonoBehaviour,IPinchSettingComp
    {
        [System.Flags,Serializable]
        public enum ColorChangeType
        {
            [LabelText("All")]
            All = int.MaxValue,
            [LabelText("None")]
            None = 0,
            [LabelText("纯色")]
            PureColor = 1 << 0,
            [LabelText("双材质色1")]
            DoubelMatColor = 1 << 1,  //例如挑染、阴阳头
            [LabelText("双材质色2")]
            DoubelMatColor2 = 1 << 2,  //例如挑染、阴阳头
            [LabelText("动态渐变")]
            GradientColorStatic = 1 << 3,
            [LabelText("静态渐变")]
            GradientColorDynamic = 1 << 4
        }
        
        public ColorChangeType colorChangeType = ColorChangeType.PureColor;
        public int CurColorChangeType = 0;
        //需要存储
        public float param1,param2; //渐变程度，渐变高度
        public Color[] colors = new Color[2];

        
        private void Awake()
        {
            
            this.enabled = false;
        }

        public void LoadData(CharacterModelPinch _modelPinch)
        {
        }

        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
            _modelPinch.ChangeColor(_type2,colors[0],0,CurColorChangeType);
            _modelPinch.ChangeColor(_type2,colors[1],1,CurColorChangeType);
        }
    }
}
