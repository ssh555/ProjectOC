using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
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
        public void ApplyType(CharacterModelPinch _modelPinch,PinchPart.PinchPartData _pinchPartData)
        {
            int IsInit = 1;
            if (typeIndex != -1)
            {
                if (_pinchPartData.PinchPartType2 == PinchPartType2.HairFront)
                {
                    IsInit = 3;
                    _modelPinch.ChangeType(PinchPartType3.HB_HairBack, typeIndex).Completed += ChangeTypeCallBack;
                    _modelPinch.ChangeType(PinchPartType3.HB_HairBraid, 0).Completed += ChangeTypeCallBack;
                }
                _modelPinch.ChangeType(_pinchPartData.PinchPartType3, typeIndex).Completed += ChangeTypeCallBack;;
            }
            else
            {
                ChangeTypeCallBack(new AsyncOperationHandle<GameObject>());
            }

            void ChangeTypeCallBack(AsyncOperationHandle<GameObject> _handle)
            {
                IsInit--;
                if (IsInit == 0)
                {
                    ApplyOtherComp(_modelPinch, _pinchPartData);
                }
            }
        }
        
        public void ApplyOtherComp(CharacterModelPinch _modelPinch,PinchPart.PinchPartData _pinchPartData)
        {
            foreach (var _pinchSettingComp in _pinchPartData.pinchSettingComps)
            {
                _pinchSettingComp.Apply(_pinchPartData.PinchPartType2,_pinchPartData.PinchPartType3,_modelPinch);
            }
            // StartCoroutine(ExecuteAfterOneFrame(_modelPinch, _pinchPartData));
        }

        IEnumerator ExecuteAfterOneFrame(CharacterModelPinch _modelPinch,PinchPart.PinchPartData _pinchPartData)
        {
            yield return null;
            foreach (var _pinchSettingComp in _pinchPartData.pinchSettingComps)
            {
                _pinchSettingComp.Apply(_pinchPartData.PinchPartType2,_pinchPartData.PinchPartType3,_modelPinch);
            }
        }

        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
            
        }
    }
}
