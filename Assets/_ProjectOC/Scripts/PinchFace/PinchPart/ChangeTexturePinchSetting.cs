using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTexturePinchSetting :MonoBehaviour, IPinchSettingComp
    {
        public int textureIndex;

        
        private void Awake()
        {
            this.enabled = false;
        }
    }
}
