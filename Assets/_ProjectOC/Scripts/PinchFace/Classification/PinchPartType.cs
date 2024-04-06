using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace ProjectOC.PinchFace
{
    [CreateAssetMenu(fileName = "NewPinchPartType", menuName = "ML/PinchFace")]
    public class PinchPartType : ScriptableObject
    {
        public PinchPartType1 pinchPartType1;
        public PinchPartType2 pinchPartType2;
        public PinchPartType3 pinchPartType3;
        public bool couldNaked;//�Ƿ������©��Ӱ���Ƿ����� �ղ�����ť

        private void InitType(string id)
        {
            string[] classification = id.Split('-');
            pinchPartType1 = (PinchPartType1)Enum.Parse(typeof(PinchPartType1), classification[0]);
            pinchPartType2 = (PinchPartType2)Enum.Parse(typeof(PinchPartType2), classification[1]);
            pinchPartType3 = (PinchPartType3)Enum.Parse(typeof(PinchPartType3), classification[2]);
            if (classification[3] == "0")
            {
                couldNaked = false;
            }
            else if (classification[3] == "1")
            {
                couldNaked = true;
            }
            else
                Debug.LogWarning($"classification[3] type warning{classification[3]}");
        }
#if UNITY_EDITOR
        [Button("�����ʲ�����������"), PropertyOrder(-1)]
        private void ChangeAssetName()
        {
            InitType(this.name);
        }
#endif
    }   

}
