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
            [LabelText("双材质色")]
            DoubelMatColor = 1 << 1,  //例如挑染、阴阳头
            [LabelText("动态渐变")]
            GradientColorStatic = 1 << 2,
            [LabelText("静态渐变")]
            GradientColorDynamic = 1 << 3
        }
        public ColorChangeType colorChangeType = ColorChangeType.PureColor;
        public ColorChangeType CurColorChangeType;
        //需要存储
        public float param1,param2; //渐变程度，渐变高度
        public Color color1, color2;

        
        private void Awake()
        {
            this.enabled = false;
        }
    }
}
