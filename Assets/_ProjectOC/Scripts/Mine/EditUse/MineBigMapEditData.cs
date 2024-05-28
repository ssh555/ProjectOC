#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MineSystem
{
    [CreateAssetMenu(fileName = "NewMineBigMapEditData", menuName = "OC/Mine/MineBigMapEditData")]
    public class MineBigMapEditData : ScriptableObject
    {
        //± À¢≤ø∑÷
        public List<MineBrushData> MineBrushDatas;
        public Texture bigMapTex;

        public MineBrushData IDToMineBrushData(string _id)
        {
            MineBrushData _res = null;
            foreach (var _singleBrushData in MineBrushDatas)
            {
                if (_singleBrushData.mineID == _id)
                {
                    return _singleBrushData;
                }
            }
            Debug.LogWarning($"Can't find brush{_id}");
            return _res;
        }
        
        [Serializable]
        public class MineBrushData
        {
            public string mineID;
            public Sprite mineIcon;
                
            public BrushData brushData;
            public MineBrushData(string _mineID,Sprite _tex)
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
        
}

#endif