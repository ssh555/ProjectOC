using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTypePinchSetting : MonoBehaviour,IPinchSettingComp
    {
        public int typeIndex = -1;

        public enum SingleOrDouble
        {
            Double,
            Left,
            Right
        }
        public SingleOrDouble distributionMode  = SingleOrDouble.Double;
        
        
        private void Awake()
        {
            typeIndex = -1;
            this.enabled = false;
        }

        public void LoadData(CharacterModelPinch _modelPinch)
        {
        }

        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
            if (typeIndex != -1)
            {
                if (_type2 == PinchPartType2.HairFront)
                {
                    _modelPinch.ChangeType(PinchPartType3.HD_Dai, typeIndex);
                    _modelPinch.ChangeType(PinchPartType3.HB_HairBack, typeIndex);
                }
                _modelPinch.ChangeType(_type3, typeIndex);
            }
        }
    }
}
