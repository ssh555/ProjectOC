using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MineSystem
{
    public class BigMap : MonoBehaviour
    {
        [HideInInspector]
        public List<TileMap> tileMaps;
        public MineBigMapEditData BigMapEditDatas;
        public List<MineSmallMapEditData> SmallMapEditDatas = new List<MineSmallMapEditData>();
        public float mineToTileScale = 0.2f;

        //新建笔刷项 数据
        public string mineID = "Mineral_Ti_1";
        public Sprite mineTex;

        public Color dataTileColor,emptyTileColor;
    }
}