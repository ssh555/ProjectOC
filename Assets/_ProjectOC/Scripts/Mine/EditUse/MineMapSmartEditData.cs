using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using UnityEngine;

namespace MineSystem
{
    public class MineSmallMapEditData : ScriptableObject
    {
        //���β���
        public int index;
        //���ǹ̶���16:9
        public int width, height;
        public bool[,] gridData;
        public Texture smallMapTex;
        //Dictionart����һ��
        public Dictionary<MineBigMapEditData.MineBrushData,List<Vector2>>  smallMapMineData = new Dictionary<MineBigMapEditData.MineBrushData, List<Vector2>>();

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

