using ML.Engine.Manager;
using ML.Engine.Utility;
using ProjectOC.MineSystem;
using ProjectOC.ProNodeNS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ProjectOC.Order.OrderManager;

namespace ProjectOC.MainInteract
{
    [System.Serializable]
    public sealed class MainInteractManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        public enum FunctionEnum
        {
            Bag = 0,
            Build,
            TechTree,
            ProManage,
            Order,
            MyClan,
            Friend,
            Option
        }

        #region Base
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// 单例管理
        /// </summary>
        public static MainInteractManager Instance { get { return instance; } }

        private static MainInteractManager instance;

        public void Init()
        {
            LoadTableData();
        }

        public void OnRegister()
        {
            if (instance == null)
            {
                instance = this;
                Init();
            }
        }

        public void OnUnregister()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        #endregion

        #region Load
        /// <summary>
        /// 主交互轮盘表数据
        /// </summary>
        [System.Serializable]
        public struct MainInteractRingTableData
        {
            public string ID;
            public int Sort;
            public string Name;
            public string Icon;
            public FunctionEnum Function;
        }
        public ML.Engine.ABResources.ABJsonAssetProcessor<MainInteractRingTableData[]> ABJAProcessor;
        private Dictionary<string, MainInteractRingTableData> mainInteractRingTableDataDic = new ();
        public List<MainInteractRingTableData> mainInteractRingTableDatas { get
            {
                var list = mainInteractRingTableDataDic.Values.ToList();
                list.Sort((a, b) => { return a.Sort.CompareTo(b.Sort); });
                return list;
            } }
        private void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<MainInteractRingTableData[]>("OCTableData", "MainInteractRing", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.mainInteractRingTableDataDic.Add(data.ID, data);
                }
            }, "主交互轮盘数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        #endregion
    }
}


