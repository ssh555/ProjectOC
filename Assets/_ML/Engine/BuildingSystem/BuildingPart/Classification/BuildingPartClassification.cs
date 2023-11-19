using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    [System.Serializable, LabelText("建筑物分类")]
    public struct BuildingPartClassification
    {
        [LabelText("一级分类-类别")]
        public BuildingCategory Category;

        [LabelText("二级分类-类型")]
        public BuildingType Type;

        [LabelText("三级分类-样式")]
        public BuildingStyle Style;

        [LabelText("四级分类-高度"), PropertyTooltip("单位cm, 用整数表示"), SerializeField]
        public short Height;

        public override bool Equals(object obj)
        {
            return this == (BuildingPartClassification)obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Category, Type, Style, Height);
        }
        public static bool operator ==(BuildingPartClassification A, BuildingPartClassification B)
        {
            return A.Category == B.Category && A.Type == B.Type && A.Style == B.Style && A.Height == B.Height;
        }

        public static bool operator !=(BuildingPartClassification A, BuildingPartClassification B)
        {
            return !(A == B);
        }

        public override string ToString()
        {
            return $"{Category}-{Type}-{Style}-{Height}";
        }
    }
}
