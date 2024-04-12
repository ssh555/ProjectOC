using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTypePinchSetting : MonoBehaviour,IPinchSettingComp
    {
        public int Index { get; }
        public void LoadData()
        {
            throw new System.NotImplementedException();
        }

        public  void GenerateUI()
        {
            //Éú³ÉUI
            
        }
        
        private void Awake()
        {
            this.enabled = false;
        }
        
    }
}
