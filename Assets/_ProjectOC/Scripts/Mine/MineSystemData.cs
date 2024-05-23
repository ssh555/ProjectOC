using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.MineSystem.MineSystemData;
using static ProjectOC.Order.OrderManager;
/// <summary>
/// �ɿ�ϵͳ����
/// </summary>
namespace ProjectOC.MineSystem
{
    public class MineSystemData
    {
        public const int MAPDEPTH = 3;

        /// <summary>
        /// ���ͼ�浵����
        /// </summary>
        public class IslandMapData
        {
            [LabelText("���ͼ����"), ReadOnly, ShowInInspector]
            public int MAPDEPTH = MineSystemData.MAPDEPTH;
            [LabelText("���ͼ���ű���"), ReadOnly, ShowInInspector]
            public float GridScale;
            [LabelText("�����б�"), ReadOnly, ShowInInspector]
            public List<MapRegionData> MapRegionDatas;
            [LabelText("���ͼ��ͼ���������"), ReadOnly, ShowInInspector]
            public bool[] isUnlockIslandMap = new bool[MineSystemData.MAPDEPTH];
            [LabelText("��������"), ReadOnly, ShowInInspector]
            public MainIslandData mainIslandData;
            [LabelText("С��ͼ����"), ReadOnly, ShowInInspector]
            public Dictionary<string, MineralMapData> mineralMapDatas;
        }

        /// <summary>
        /// ��������
        /// </summary>
        public class MapRegionData
        {
            [LabelText("����ID"), ReadOnly, ShowInInspector]
            public string MapRegionID;
            [LabelText("�Ƿ�Ϊ�ϰ���"), ReadOnly, ShowInInspector]
            public bool IsBlock;
            [LabelText("�����ͼ���������"), ReadOnly, ShowInInspector]
            public bool[] isUnlockLayer;
            [LabelText("С��ͼ����"), ReadOnly, ShowInInspector]
            public string[] mineralDataID;

            public MapRegionData(string mapRegionID, bool isBlock)
            {
                MapRegionID = mapRegionID;
                IsBlock = isBlock;
                this.isUnlockLayer = new bool[MineSystemData.MAPDEPTH];
                this.mineralDataID = new string[MineSystemData.MAPDEPTH];
            }
        }

        /// <summary>
        /// С��ͼ���ݣ��ɿ��ͼ���ݣ�
        /// </summary>
        public class MineralMapData
        {
            [LabelText("�ɿ��ͼID"), ReadOnly, ShowInInspector]
            public string MineralMapID;
/*            [LabelText("�ɿ��ͼԤ�����ʲ�·��"), ReadOnly, ShowInInspector]
            public string PrefabPath;*/
            [LabelText("�ɿ��ͼ�еĿ�������"), ReadOnly, ShowInInspector]
            public List<MineData> MineDatas;

            public MineralMapData(string mineralMapID, List<MineData> mineDatas)
            {
                MineralMapID = mineralMapID;
                MineDatas = mineDatas;
            }
        }

        /// <summary>
        /// ������������
        /// </summary>
        public class MineData
        {
            [LabelText("����ID"), ReadOnly, ShowInInspector]
            public string MineID;
            [LabelText("����λ��"), ReadOnly, ShowInInspector]
            public Vector2 position;
            [LabelText("ʣ�࿪�ɴ���"), ReadOnly, ShowInInspector]
            public int RemainMineNum;

            public MineData(string mineralMapID, Vector2 position, int remainMineNum)
            {
                MineID = mineralMapID;
                this.position = position;
                RemainMineNum = remainMineNum;
            }
        }

        /// <summary>
        /// ��������
        /// </summary>
        public class MainIslandData
        {
            [LabelText("�ƶ��ٶ�"), ReadOnly, ShowInInspector]
            private float moveSpeed = 1;
            [LabelText("����λ��"), ReadOnly, ShowInInspector]
            private Vector2 curPos;
            public Vector2 CurPos { get { return curPos; } set { curPos = value; } }
            [LabelText("Ŀ��λ��"), ReadOnly, ShowInInspector]
            private Vector2 targetPos;
            public Vector2 TargetPos { get { return targetPos; } set {  targetPos = value; } }
            [LabelText("�Ƿ����ƶ�"), ReadOnly, ShowInInspector]
            private bool isMoving;
            public bool IsMoving { get { return isMoving; } set { isMoving = value; OnisMovingChanged?.Invoke(value); } }
            public event Action<bool> OnisMovingChanged;
            [LabelText("��ǰ���ڵĵ�ͼ��"), ReadOnly, ShowInInspector]
            private int curMineLayer;
            [LabelText("��ǰ���ڵĴ��ͼ����ID"), ReadOnly, ShowInInspector]
            private string curMapRegionID;

            private Vector2 lastPos;
            public Vector2 LastPos { get { return lastPos; } }
            public Vector2 MovingDir { get { return (targetPos - curPos).normalized; } }
            private bool isPause;
            public bool IsPause { get { return isPause; } set { isPause = value; } }
            public MainIslandData()
            {
                lastPos = curPos;
                isPause = false;
            }
            
            public bool isReachTarget { 
            get {
                    if(isPause) return false;
                    if (Vector2.Distance(curPos, targetPos)<1f)
                    {
                        IsMoving = false;
                        return true;
                    }
                    if(IsMoving)
                    {
                        lastPos = curPos;
                        curPos += moveSpeed * (targetPos - curPos).normalized;
                    }
                        
                    return false;
                }
            }

            public void Reset()
            {
                curPos = lastPos;
                targetPos = curPos;
                IsMoving = false;
            }
            
        }

        /// <summary>
        /// ���������
        /// </summary>
        [System.Serializable]
        public struct MineralTableData
        {
            public string ID;
            public string Icon;
            public List<ML.Engine.InventorySystem.Formula> MineEff;
            public int MineNum;
        }
    }
}


