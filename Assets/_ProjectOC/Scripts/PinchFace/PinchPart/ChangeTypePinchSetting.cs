using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeTypePinchSetting : MonoBehaviour,IPinchSettingComp
    {
        public int Index { get; }

        public enum SingleOrDouble
        {
            Double,
            Left,
            Right
        }
        public SingleOrDouble distributionMode  = SingleOrDouble.Double;
        
        public void LoadData()
        {
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
