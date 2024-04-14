using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeColorPinchSetting : MonoBehaviour,IPinchSettingComp
    {
        [System.Flags]
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
        [NonSerialized] //固定不需要参与存档，不需要序列化存储
        public ColorChangeType colorChangeType;
        public int Index { get; } = -1;
        //需要存储
        public float param1,param2; //渐变程度，渐变高度
        public Color color1, color2;

        public void LoadData()
        {
            throw new System.NotImplementedException();
        }

        public void GenerateUI()
        {
            throw new System.NotImplementedException();
        }

        private void Awake()
        {
            this.enabled = false;
        }
    }
}
