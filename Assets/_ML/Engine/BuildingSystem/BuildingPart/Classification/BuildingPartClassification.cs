using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    [System.Serializable, LabelText("���������")]
    public struct BuildingPartClassification
    {
        [LabelText("һ������-���")]
        public BuildingCategory Category;

        [LabelText("��������-����")]
        public BuildingType Type;

        [LabelText("��������-��ʽ")]
        public BuildingStyle Style;

        [LabelText("�ļ�����-�߶�"), PropertyTooltip("��λcm, ��������ʾ"), SerializeField]
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
