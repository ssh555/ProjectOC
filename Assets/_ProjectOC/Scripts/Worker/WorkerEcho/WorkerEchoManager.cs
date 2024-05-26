using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using static Cinemachine.DocumentationSortingAttribute;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct WorkerEchoTableData
    {
        public string ID;
        public WorkerCategory Category;
        public List<ML.Engine.InventorySystem.Formula> Raw;
        public int TimeCost;
    }

    [System.Serializable]
    public sealed class WorkerEchoManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        [ShowInInspector]
        private Dictionary<string, WorkerEchoTableData> WorkerEchoTableDict = new Dictionary<string, WorkerEchoTableData>();
        private ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableData[]> ABJAProcessor;
        private int level = 1;
        public int Level { get { return level; } }
        public void OnRegister()
        {
            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableData[]>("OCTableData", "WorkerEcho", (datas) =>
            {
                foreach (var data in datas)
                {
                    WorkerEchoTableDict.Add(data.ID, data);
                }
            }, "隐兽共鸣表数据");
            ABJAProcessor.StartLoadJsonAssetData();
        }
        public bool IsValidID(string id)
        {
            return !string.IsNullOrEmpty(id) ? WorkerEchoTableDict.ContainsKey(id) : false;
        }
        public WorkerCategory GetRandomCategory()
        {
            Random random = new Random();
            int randomValue = random.Next(2, 8);
            WorkerCategory randomWorkerCategory = (WorkerCategory)randomValue;
            return randomWorkerCategory;
        }
        public WorkerCategory GetCategory(string id)
        {
            return IsValidID(id) ? WorkerEchoTableDict[id].Category : WorkerCategory.None;
        }
        public List<ML.Engine.InventorySystem.Formula> GetRaw(string id)
        {
            return IsValidID(id) ? WorkerEchoTableDict[id].Raw : new List<ML.Engine.InventorySystem.Formula>();
        }
        public int GetTimeCost(string id)
        {
            return IsValidID(id) ? WorkerEchoTableDict[id].TimeCost : 0;
        }

        public void LevelUp()
        {
            if (level == 2) return;
            level = 2;
        }
    }
}