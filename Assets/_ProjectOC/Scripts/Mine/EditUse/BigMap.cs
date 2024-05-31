#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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

        [ShowInInspector]
        public Dictionary<string, Sprite> mineBrushDatas = new Dictionary<string, Sprite>();
        //�½���ˢ�� ����
        public string mineID = "Mineral_Ti_1";
        public Sprite mineTex;

        public Color dataTileColor = new Color32(44, 46, 47,255);
        public Color emptyTileColor = new Color32(161, 162, 166,255);

        public List<Color> bigMapRegionColor = new List<Color>();
    }
}

#endif