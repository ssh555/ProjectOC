using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.Manager;
using System;
using System.Collections.Generic;

namespace ProjectOC.WorkerEchoNS
{
    public enum Category
    {
        None,
        Random,
        Cat,
        Deer,
        Fox,
        Rabbit,
        Dog,
        Seal,
    }

    [System.Serializable]
    public struct WorkerEchoTableData
    {
        public string ID;
        public Category Category;
        public List<Formula> Raw;
        public int TimeCost;
    }

    [System.Serializable]
    public sealed class WorkerEchoManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;
        /// <summary>
        /// 基础数据表
        /// </summary>
        private Dictionary<string, WorkerEchoTableData> EffectTableDict = new Dictionary<string, WorkerEchoTableData>();
        
        private Dictionary<string, WorkerEchoTableData> WorkerEchoTableDict = new Dictionary<string, WorkerEchoTableData>();
        public static ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableData[]> ABJAProcessor;
        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableData[]>("Json/TableData", "WorkerEcho", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.WorkerEchoTableDict.Add(data.ID, data);
                    }
                }, null, "隐兽共鸣表数据");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        public string GetRandomID()
        {
            List<string> list = new List<string>(WorkerEchoTableDict.Keys);
            System.Random random = new System.Random();
            int index = random.Next(WorkerEchoTableDict.Keys.Count-1);
            string ID = list[index];
            return ID;
        }
        public Category GetCategory(string id)
        {
            if (!this.WorkerEchoTableDict.ContainsKey(id))
            {
                return Category.None;
            }
            return this.WorkerEchoTableDict[id].Category;
        }
        public List<Formula> GetRaw(string id)
        {
            if (!this.WorkerEchoTableDict.ContainsKey(id))
            {
                return null;
            }
            return this.WorkerEchoTableDict[id].Raw;
        }

        public int GetTimeCost(string id)
        {
            if (!this.WorkerEchoTableDict.ContainsKey(id))
            {
                return 0;
            }
            return this.WorkerEchoTableDict[id].TimeCost;
        }

    }
}