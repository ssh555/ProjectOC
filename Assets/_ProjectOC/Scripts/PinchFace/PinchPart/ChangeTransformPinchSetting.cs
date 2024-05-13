using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTransformPinchSetting : MonoBehaviour, IPinchSettingComp
    {
        //±´Èû¶û¡¢Çò
        public PinchTransfType.TransformType TransformType;

        //Êý¾Ý
        private void Awake()
        {
            this.enabled = false;
        }
    }
}
