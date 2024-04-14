using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeBoneWeightPinchSetting :MonoBehaviour, IPinchSettingComp
    {
        public int Index { get; }
        public Vector2 scaleValueRange;
        public Vector2 offsetValueRange;
        [System.Flags]
        public enum BoneWeightChangeType
        {
            [LabelText("All")]
            All = int.MaxValue,
            [LabelText("None")]
            None = 0,
            [LabelText("Ëõ·Å")]
            Scale = 1 << 0,
            [LabelText("Æ«ÒÆ")]
            Offset = 1 << 1
        }
        public BoneWeightChangeType boneWeightChangeType;
        
        public float currentValue;
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
