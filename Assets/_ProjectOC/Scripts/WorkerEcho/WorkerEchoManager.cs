using ML.Engine.InventorySystem;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace ProjectOC.WorkerEchoNS
{
    public enum Category
    {
        None,
        [LabelText("随机")]
        Random,
        [LabelText("烹饪兽")]
        CookWorker,
        [LabelText("手工兽")]
        HandCraftWorker,
        [LabelText("精工兽")]
        IndustryWorker,
        [LabelText("术法兽")]
        MagicWorker,
        [LabelText("搬运兽")]
        TransportWorker,
        [LabelText("采集兽")]
        CollectWorker
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
        public void OnRegister()
        {
            LoadTableData();
        }

        /// <summary>
        /// 基础数据表
        /// </summary>
        private Dictionary<string, WorkerEchoTableData> EffectTableDict = new Dictionary<string, WorkerEchoTableData>();
        
        private Dictionary<string, WorkerEchoTableData> WorkerEchoTableDict = new Dictionary<string, WorkerEchoTableData>();

        private ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableData[]> ABJAProcessor;
        public void LoadTableData()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableData[]>("OCTableData", "WorkerEcho", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.WorkerEchoTableDict.Add(data.ID, data);
                }
            }, "隐兽共鸣表数据");
            ABJAProcessor.StartLoadJsonAssetData();
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