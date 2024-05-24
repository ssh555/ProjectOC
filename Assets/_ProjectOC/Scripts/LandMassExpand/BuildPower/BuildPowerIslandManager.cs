using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using UnityEngine;


namespace ProjectOC.LandMassExpand
{
    [System.Serializable]
    public class BuildPowerIslandManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public List<ISupportPowerBPart> powerCores = new List<ISupportPowerBPart>();
        public List<BuildPowerSub> powerSubs = new List<BuildPowerSub>();
        public List<INeedPowerBpart> electAppliances = new List<INeedPowerBpart>();
        public int PowerCoreMaxCount = 1;


        #region Register

        public void OnRegister()
        {
            LoadTableData();
            //EnterIslandLevel(0);
        }
        
        

        #endregion
        
        #region 供电系统
        public bool CoverEachOther(IPowerBPart powerBPart1, IPowerBPart powerBPart2)
        {
            float r1 = powerBPart1.PowerSupportRange;
            float r2 = powerBPart2.PowerSupportRange;
            Vector3 pos1 = powerBPart1.transform.position;
            Vector3 pos2 = powerBPart2.transform.position;
            return CoverEachOther(r1, r2, pos1, pos2);
        }
        public bool CoverEachOther(IPowerBPart powerBPart1, float r2, Vector3 pos2)
        {
            float r1 = powerBPart1.PowerSupportRange;
            Vector3 pos1 = powerBPart1.transform.position;
            return CoverEachOther(r1, r2, pos1, pos2);
        }
        
        public bool CoverEachOther(float r1, float r2, Vector3 pos1, Vector3 pos2)
        {
            float distanceSquared = (pos1 - pos2).sqrMagnitude;
            float radiusSunSquared = (r1 + r2) * (r1 + r2);
            return radiusSunSquared >= distanceSquared;
        }
        #endregion



        #region 主岛升级

        private MainlandLevelTableData[] mainLandLevelDatas;
        public int currentLandLevel { get; private set; } = 0;
        public MainlandLevelTableData CurrentLandLevelData => mainLandLevelDatas[currentLandLevel];

        private string abPath = "OCTableData";
        private string islandLevelPath = "MainlandLevel";

        
        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<MainlandLevelTableData[]> ABJAProcessor = new ML.Engine.ABResources.
                ABJsonAssetProcessor<MainlandLevelTableData[]>(abPath, islandLevelPath,
                    (datas) =>
                    {
                        mainLandLevelDatas = new MainlandLevelTableData[datas.Length];
                        datas.CopyTo(mainLandLevelDatas,0);
                        EnterIslandLevel(0);
                    }, "岛屿升级");
        
            ABJAProcessor.StartLoadJsonAssetData();
        }


        public void IslandUpdate()
        {
            currentLandLevel++;
            EnterIslandLevel(currentLandLevel);
        }
        private void EnterIslandLevel(int _index)
        {
            foreach (var _event in mainLandLevelDatas[_index].Events)
            {
                GameManager.Instance.EventManager.ExecuteEvent(_event);
            }
        }

        #endregion
        
        
        
    }
}