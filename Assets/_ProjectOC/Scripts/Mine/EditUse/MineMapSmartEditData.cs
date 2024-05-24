#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.MineSystem
{
    public class MineSmallMapEditData : ScriptableObject
    {
        //地形部分
        public int index;
        //考虑固定成16:9
        public int width, height;


        public Texture smallMapTex;
        public bool[] gridData;
        public List<SingleMineData> mineData = new List<SingleMineData>();
        
        [System.Serializable]
        public class SingleMineData
        {
            public string MineID;
            public List<Vector2> MinePoses;
            public SingleMineData(string _id)
            {
                MineID = _id;
                MinePoses = new List<Vector2>();
            }
        }
        public MineSmallMapEditData()
        {
            index = -1;
            width = 16;
            height = 9;
            if (gridData == null)
            {
                gridData = new bool[width* height];
            }

            if (mineData == null)
            {
                mineData = new List<SingleMineData>();
            }
            smallMapTex = null;
        }
        
    }
}



#endif