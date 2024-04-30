using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    public class ChangeBoneWeightPinchSetting :MonoBehaviour, IPinchSettingComp
    {
        public int Index { get; }
        public List<BoneWeightData> BoneWeightDatas;
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

        [System.Serializable]
        public class BoneWeightData
        {
            public BoneWeightChangeType boneWeightChangeType;
            public BoneWeightType boneWeightType;
            public Vector2 scaleValueRange;
            public float currentScaleValue;
            [Space(20)]
            public Vector2 offsetValueRange;
            public float currentOffsetValue;

            public BoneWeightData()
            {
                boneWeightChangeType = BoneWeightChangeType.Scale;
                scaleValueRange = new Vector2(0.5f, 1.5f);
                currentScaleValue = 1f;

                offsetValueRange = new Vector2(0.5f, 1.5f);
                currentOffsetValue = 1f;
            }
        }
        
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
