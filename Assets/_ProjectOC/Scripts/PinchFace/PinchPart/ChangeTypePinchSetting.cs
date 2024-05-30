using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        
        //用List<PinchComp> 更好，而不是用PinchPart
        public void ApplyType(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch,PinchPart _pinchPart)
        {
            int IsInit = 1;
            if (typeIndex != -1)
            {
                if (_type2 == PinchPartType2.HairFront)
                {
                    IsInit = 2;
                    _modelPinch.ChangeType(PinchPartType3.HB_HairBack, typeIndex).Completed += ChangeTypeCallBack;
                }
                _modelPinch.ChangeType(_type3, typeIndex).Completed += ChangeTypeCallBack;;
            }

            void ChangeTypeCallBack(AsyncOperationHandle<GameObject> _handle)
            {
                IsInit--;
                if (IsInit == 0)
                {
                    _pinchPart.ApplyTypeCallBack(_modelPinch);
                }
            }
        }
        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
        }
    }
}
