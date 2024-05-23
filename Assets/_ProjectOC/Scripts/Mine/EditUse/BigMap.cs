using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectOC.MineSystem
{
    public class BigMap : MonoBehaviour
    {
        [HideInInspector]
        public List<TileMap> tileMaps;
        public MineBigMapEditData BigMapEditDatas;
        public List<MineSmallMapEditData> SmallMapEditDatas = new List<MineSmallMapEditData>();
        public float mineToTileScale = 0.2f;

        //�½���ˢ�� ����
        public string mineID = "Mineral_Ti_1";
        public Sprite mineTex;

        public Color dataTileColor = new Color(44, 46, 47);
        public Color emptyTileColor = new Color(161, 162, 166);

        public List<Color> bigMapRegionColor = new List<Color>();
    }
}