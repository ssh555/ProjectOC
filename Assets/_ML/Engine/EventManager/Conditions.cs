using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
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

        private string CheckBagItemGetText(string s)
        {
            Debug.Log("CheckBagItemGetText " + s);
            return s;
        }

        public bool CheckBuild(List<string> p1, List<int> p2, List<float> p3)
        {
            Debug.Log("CheckBuild ");
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

        private string CheckBuildGetText(string s)
        {
            Debug.Log("CheckBuildGetText " + s);
            return s;
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

        private string CheckWorkerEMCurrentGetText(string s)
        {
            Debug.Log("CheckWorkerEMCurrentGetText " + s);
            return s;
        }

        public bool Condition_CheckBuild_Bed_1()
        {
            return true;
        }
        public bool Condition_CheckBuild_SeedPlot_1()
        {
            return true;
        }
        public bool Condition_CheckBuild_LifeDiversion_1()
        {
            return true;
        }
    }
}





