using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using UnityEditor;
using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    [System.Serializable, LabelText("建筑物分类")]
    public struct BuildingPartClassification
    {
        [LabelText("一级分类-Category1"), OnValueChanged("OnCategory1Changed")]
        public BuildingCategory1 Category1;

        [LabelText("二级分类-Category2"), ValueDropdown("GetCategory2Values", AppendNextDrawer = true, DisableGUIInAppendedDrawer = true)]
        public BuildingCategory2 Category2;

        [LabelText("三级分类-Category3")]
        public BuildingCategory3 Category3;

        [LabelText("四级分类-Category4"), PropertyTooltip("单位cm, 用整数表示"), SerializeField]
        public short Category4;

        public BuildingPartClassification(string id)
        {
            string[] classification = id.Split('_');
            Category1 = (BuildingCategory1)Enum.Parse(typeof(BuildingCategory1), classification[0]);
            Category2 = (BuildingCategory2)Enum.Parse(typeof(BuildingCategory2), classification[1]);
            Category3 = (BuildingCategory3)Enum.Parse(typeof(BuildingCategory3), classification[2]);
            Category4 = short.Parse(classification[3]);
        }

        public override bool Equals(object obj)
        {
            return this == (BuildingPartClassification)obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Category1, Category2, Category3, Category4);
        }
        public static bool operator ==(BuildingPartClassification A, BuildingPartClassification B)
        {
            return A.Category1 == B.Category1 && A.Category2 == B.Category2 && A.Category3 == B.Category3 && A.Category4 == B.Category4;
        }

        public static bool operator !=(BuildingPartClassification A, BuildingPartClassification B)
        {
            return !(A == B);
        }

        public override string ToString()
        {
            return $"{Category1}_{Category2}_{Category3}_{Category4}";
        }



        private ValueDropdownList<int> GetCategory2Values()
        {
            var values = new ValueDropdownList<int>();
            foreach (var value in Enum.GetValues(typeof(BuildingCategory2)))
            {
                var fieldInfo = typeof(BuildingCategory2).GetField(value.ToString());
                var attributes = fieldInfo.GetCustomAttributes(typeof(LabelTextAttribute), false);
                var labelText = ((LabelTextAttribute)attributes[0]).Text;

                if ((int)value >= (int)Category1 * 100 && (int)value < ((int)Category1 + 1) * 100)
                {
                    values.Add(labelText, (int)value);
                }
            }
            return values;
        }

        private void OnCategory1Changed()
        {
            int min = (int)Category1 * 100;
            int max = ((int)Category1 + 1) * 100;
            BuildingCategory2 closestValue = BuildingCategory2.None;
            int closestDifference = int.MaxValue;

            foreach (var value in Enum.GetValues(typeof(BuildingCategory2)))
            {
                int intValue = (int)value;
                if (intValue >= min && intValue < max)
                {
                    int difference = Math.Abs(intValue - min);
                    if (difference < closestDifference)
                    {
                        closestDifference = difference;
                        closestValue = (BuildingCategory2)value;
                    }
                }
            }

            Category2 = closestValue;

        }


    }
}
