using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
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
    }
}





