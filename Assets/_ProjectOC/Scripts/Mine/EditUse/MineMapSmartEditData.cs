using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using UnityEngine;

namespace MineSystem
{
    public class MineSmallMapEditData : ScriptableObject
    {
        //地形部分
        public int index;
        //考虑固定成16:9
        public int width, height;
        public bool[,] gridData;
        public Texture smallMapTex;
        public List<SingleTypeMineData> mineDatas = new List<SingleTypeMineData>();
        
        

        public class SingleTypeMineData
        {
            public string MineralMapID;
            public List<Vector2> position = new List<Vector2>();
        }
        public MineSmallMapEditData()
        {
            index = -1;
            width = 16;
            height = 9;
            gridData = new bool[width, height];
            smallMapTex = null;
        }
        
    }
}

