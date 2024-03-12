using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.SaveSystem
{
    public class TestSaveData : ISaveData
    {
        public string dataString = "";
        public int dataInt = 0;
        public float dataFloat = 0f;

        public TestSaveData() 
        {
            this.SaveName = "TestSaveData";
            this.IsDirty = false;
        }

        public void AddToSaveSystem()
        {
            this.IsDirty = true;
            Manager.GameManager.Instance.SaveManager.SaveController.datas.Add(this);
        }
    }
}