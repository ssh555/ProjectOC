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
            [LabelText("��ɫ")]
            PureColor = 1 << 0,
            [LabelText("˫����ɫ")]
            DoubelMatColor = 1 << 1,  //������Ⱦ������ͷ
            [LabelText("��̬����")]
            GradientColorStatic = 1 << 2,
            [LabelText("��̬����")]
            GradientColorDynamic = 1 << 3
        }
        public ColorChangeType colorChangeType = ColorChangeType.PureColor;
        public ColorChangeType CurColorChangeType;
        //��Ҫ�洢
        public float param1,param2; //����̶ȣ�����߶�
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
            _modelPinch.ChangeColor(_type2,colors[0],0);
        }
    }
}
