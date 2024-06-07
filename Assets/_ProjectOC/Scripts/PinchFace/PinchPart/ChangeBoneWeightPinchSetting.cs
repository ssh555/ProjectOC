using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.PinchFace
{
    [System.Serializable]
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
            public Vector3 currentScaleValue;
            [Space(20)]
            public Vector2 offsetValueRange;
            public Vector3 currentOffsetValue;
        }

        private void Awake()
        {
            foreach (var _boneWeightData in BoneWeightDatas)
            {
                _boneWeightData.currentScaleValue = Vector3.one;
                _boneWeightData.currentOffsetValue = Vector3.zero;
            }
            this.enabled = false;
        }

        public void LoadData(CharacterModelPinch _modelPinch)
        {
        }

        public void Apply(PinchPartType2 _type2,PinchPartType3 _type3,CharacterModelPinch _modelPinch)
        {
            foreach (var _boneWeightData in BoneWeightDatas)
            {
                _modelPinch.ChangeBoneScale(_boneWeightData.boneWeightType,_boneWeightData.currentScaleValue);
                if ((_boneWeightData.boneWeightChangeType & BoneWeightChangeType.Offset) != 0)
                {
                    _modelPinch.ChangeBoneScale(_boneWeightData.boneWeightType,_boneWeightData.currentOffsetValue,false);
                }
            }
        }
    }
}
