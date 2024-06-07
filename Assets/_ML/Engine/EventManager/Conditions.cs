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
        public bool CheckBagItem(List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CheckBagItem ");
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

        private string CheckBagItemGetText(string s,List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CheckBagItemGetText " + s);
            return s;
        }

        public bool CheckBuild(List<string> p1, List<int> p2, List<float> p3)
        {
            if (p1.IsNullOrEmpty() || p2.IsNullOrEmpty())
            {
                Debug.LogError("[TabelData Error] CheckBuild");
            }
            //Build_Interact_LifeDiversion_1
            string[] idSplit = p1[0].Split("_");
            string buildingTypeStr = idSplit[2];
            BuildingCategory2 buildingType = (BuildingCategory2)Enum.Parse(typeof(BuildingCategory2), buildingTypeStr);
            int _level = 0;
            if (idSplit.Length == 4)
            {
                _level = int.Parse(idSplit[3]);
            }
            int currentCount = BuildingManager.Instance.GetBuildingCount(buildingType, _level);

            
            return currentCount >= p2[0];
        }

        private string CheckBuildGetText(string s,List<string> p1, List<int> p2, List<float> p3)
        {
            string[] idSplit = p1[0].Split("_");
            string buildingTypeStr = idSplit[2];
            BuildingCategory2 buildingType = (BuildingCategory2)Enum.Parse(typeof(BuildingCategory2), buildingTypeStr);
            int _level = 0;
            if (idSplit.Length == 4)
            {
                _level = int.Parse(idSplit[3]);
            }
            int currentCount = BuildingManager.Instance.GetBuildingCount(buildingType, _level);
            
            string _conditionText = s.Replace("&S1",currentCount.ToString());
            return _conditionText;
        }
        public bool CheckWorkerEMCurrent(List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CheckWorkerEMCurrent ");
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


        private string CheckWorkerEMCurrentGetText(string s,List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CheckWorkerEMCurrentGetText " + s);
            return s;
        }

    }
}





