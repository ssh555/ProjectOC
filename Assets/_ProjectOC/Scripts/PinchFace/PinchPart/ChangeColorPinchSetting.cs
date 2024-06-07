using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
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
            [LabelText("˫����ɫ1")]
            DoubelMatColor = 1 << 1,  //��Ⱦ���ڻ�ɫ
            [LabelText("˫����ɫ2")]
            DoubelMatColor2 = 1 << 2,  //����ͷ����ǰ���󷢶�Ҫ��ɫ��
            [LabelText("��̬����")]
            GradientColorStatic = 1 << 3,
            [LabelText("��̬����")]
            GradientColorDynamic = 1 << 4
        }
        
        public ColorChangeType colorChangeType = ColorChangeType.PureColor;
        public int CurColorChangeType = 0;
        //��Ҫ�洢
        public float smoothStepThreshold = 0.2f,smoothStrength = 0.2f; //����̶ȣ�����߶�
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
            //��û�г�ʼ��
            if (colors.Length == 0)
            {
                Color color1 = _modelPinch.GetType2Color(_type2, 0);
                if (color1 != Color.clear)
                {
                    colors[0] = color1;
                }
                else
                {
                    colors[0] = new Color(0.3f, 0.3f, 0.3f);
                }
                Color color2 = _modelPinch.GetType2Color(_type2, 1);
                if (color2 != Color.clear)
                {
                    colors[1] = color2;
                }
                else
                {
                    colors[1] = new Color(0.3f, 0.3f, 0.3f);
                }
            }
            
            
            _modelPinch.ChangeColor(_type2,colors[0],0);
            if (_type2 == PinchPartType2.HairBack || _type2 == PinchPartType2.HairFront)
            {
                _modelPinch.ChangeColor(_type2,colors[1],1);
                _modelPinch.ChangeColorType(_type2,CurColorChangeType,smoothStepThreshold,smoothStrength);
            }
        }
    }
}
