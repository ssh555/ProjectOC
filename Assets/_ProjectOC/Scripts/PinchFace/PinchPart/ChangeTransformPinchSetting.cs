using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTransformPinchSetting : MonoBehaviour, IPinchSettingComp
    {
        //����������
        public PinchTransfType.TransformType TransformType;

        //����
        private void Awake()
        {
            this.enabled = false;
        }
    }
}
