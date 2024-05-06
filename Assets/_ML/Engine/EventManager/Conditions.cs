using System;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.BuildingSystem;
using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.Utilities;
using UnityEngine;

namespace ML.Engine.Event
{
    public sealed partial class FunctionLiabrary : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        public bool Condition_CheckBagItem_Water_1(List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CureRecover ");
            Debug.Log("p1 ");
            foreach (var item in p1)
            {
                Debug.Log($"{item} ");
            }
            Debug.Log("p2 ");
            foreach (var item in p2)
            {
                Debug.Log($"{item} ");
            }
            Debug.Log("p3 ");
            foreach (var item in p3)
            {
                Debug.Log($"{item} ");
            }
            return true;
        }

        public bool Condition_CheckBagItem_Bed_1(List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CureRecover ");
            Debug.Log("p1 ");
            foreach (var item in p1)
            {
                Debug.Log($"{item} ");
            }
            Debug.Log("p2 ");
            foreach (var item in p2)
            {
                Debug.Log($"{item} ");
            }
            Debug.Log("p3 ");
            foreach (var item in p3)
            {
                Debug.Log($"{item} ");
            }
            return true;
        }

        public bool Condition_CheckWorkerEMCurrent_1(List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CureRecover ");
            Debug.Log("p1 ");
            foreach (var item in p1)
            {
                Debug.Log($"{item} ");
            }
            Debug.Log("p2 ");
            foreach (var item in p2)
            {
                Debug.Log($"{item} ");
            }
            Debug.Log("p3 ");
            foreach (var item in p3)
            {
                Debug.Log($"{item} ");
            }
            return true;
        }

        public bool CheckBuild(List<string> p1, List<int> p2, List<float> p3)
        {
            if (p1.IsNullOrEmpty() || p2.IsNullOrEmpty() || p3.IsNullOrEmpty())
            {
                Debug.LogError("[TabelData Error]CheckBuild");
            }
            //Build_Interact_LifeDiversion_1
            string buildingTypeStr = p1[0].Split("_")[2];
            BuildingCategory2 buildingType =  (BuildingCategory2)Enum.Parse(typeof(Color), buildingTypeStr);
            int currentCount = BuildingManager.Instance.GetBuildingCount(buildingType);

            return currentCount >= p2[0];

        }
    }
}





