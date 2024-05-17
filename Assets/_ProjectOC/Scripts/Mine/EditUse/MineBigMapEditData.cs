using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMineBigMapEditData", menuName = "OC/Mine/MineBigMapEditData")]
public class MineBigMapEditData : ScriptableObject
{
    //± À¢≤ø∑÷
    public List<MineBrushData> MineBrushDatas = new List<MineBrushData>();
    public Texture bigMapTex;
    
    [Serializable]
    public class MineBrushData
    {
        public string mineID;
        public Texture mineIcon;
            
        public BrushData brushData;
        public MineBrushData(string _mineID,Texture _tex)
        {
            mineID = _mineID;
            mineIcon = _tex;
            brushData = new BrushData();
        }
        public MineBrushData():this("Mineral_Ti_1", null)
        {}
    }
    [Serializable]
    public class BrushData
    {
        public float brushSize;
        public float brushHard;
        public int brushDensity;

        public float brushSizeMin = 0f, brushSizeMax = 10f;
        public float brushHardMin = 0f, brushHardMax = 10f;
        public int brushDensityMin = 0, brushDensityMax = 20;
        public BrushData()
        {
            brushSize = 3f;
            brushHard = 0f;
            brushDensity = 5;
        }
    }
}
