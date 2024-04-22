using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTransformPinchSetting : MonoBehaviour, IPinchSettingComp
    {
        public int Index { get; }
        public PinchTransfType.TransformType TransformType;
        
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
