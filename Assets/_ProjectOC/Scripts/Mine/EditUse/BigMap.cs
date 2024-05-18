using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MineSystem
{
    public class BigMap : MonoBehaviour
    {
        public List<TileMap> tileMaps;
        public MineBigMapEditData BigMapEditDatas;
        public List<MineSmallMapEditData> SmallMapEditDatas = new List<MineSmallMapEditData>();
        public float mineScale = 3f;

        //�½���ˢ�� ����
        public string mineID = "Mineral_Ti_1";
        public Sprite mineTex;

    }
}