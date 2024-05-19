using System;
using System.Collections;
using System.Collections.Generic;
using OpenCover.Framework.Model;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MineSystem
{
    public class MineSmallMapEditData : ScriptableObject
    {
        //���β���
        public int index;
        //���ǹ̶���16:9
        public int width, height;
        [ShowInInspector]
        public bool[,] gridData;
        public Texture smallMapTex;
        //Dictionart����һ��
        [ShowInInspector]
        public Dictionary<MineBigMapEditData.MineBrushData,List<Vector2>>  smallMapMineData;

        public MineSmallMapEditData()
        {
            index = -1;
            width = 16;
            height = 9;
            if (gridData == null)
            {
                Debug.Log("��������GridData");
                gridData = new bool[width, height];
            }
            smallMapTex = null;
            smallMapMineData = new Dictionary<MineBigMapEditData.MineBrushData, List<Vector2>>();
        }
        
    }
}

