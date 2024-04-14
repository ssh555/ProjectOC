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
            [LabelText("��ɫ")]
            PureColor = 1 << 0,
            [LabelText("˫����ɫ")]
            DoubelMatColor = 1 << 1,  //������Ⱦ������ͷ
            [LabelText("��̬����")]
            GradientColorStatic = 1 << 2,
            [LabelText("��̬����")]
            GradientColorDynamic = 1 << 3
        }
        [NonSerialized] //�̶�����Ҫ����浵������Ҫ���л��洢
        public ColorChangeType colorChangeType;
        public int Index { get; } = -1;
        //��Ҫ�洢
        public float param1,param2; //����̶ȣ�����߶�
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
