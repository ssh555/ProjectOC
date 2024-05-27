using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTexturePinchSetting :MonoBehaviour, IPinchSettingComp
    {
        public int textureIndex = -1;

        
        private void Awake()
        {
            textureIndex = -1;
            this.enabled = false;
        }

        public void LoadData(CharacterModelPinch _modelPinch)
        {
        }

        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
            if (textureIndex != -1)
            {
                _modelPinch.ChangeTexture(_type3,textureIndex);
            }
        }
    }
}
