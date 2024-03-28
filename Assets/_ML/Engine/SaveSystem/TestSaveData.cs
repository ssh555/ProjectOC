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
        }

        public void AddToSaveSystem()
        {
#if UNITY_EDITOR
            this.dataInt = 1;
            this.IsDirty = true;
            Manager.GameManager.Instance.SaveManager.SaveController.SaveData(this, true);
#endif
        }
    }
}