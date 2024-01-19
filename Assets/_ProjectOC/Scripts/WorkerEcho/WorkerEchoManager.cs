using ML.Engine.Manager;
using ML.Engine.TextContent;
using ProjectOC.WorkerNS;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using static ProjectOC.WorkerNS.EffectManager;

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
    public sealed class WorkerEchoManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// 是否已加载完数据
        /// </summary>
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;
        /// <summary>
        /// 基础数据表
        /// </summary>
        private Dictionary<string, EffectTableJsonData> EffectTableDict = new Dictionary<string, EffectTableJsonData>();
        [System.Serializable]
        public struct WorkerEchoTableJsonData
        {
            public string ID;
            public Category Category;
            public Dictionary<string,int> Raw;
            public int TimeCost;
        }
        private Dictionary<string, WorkerEchoTableJsonData> WorkerEchoTableDict = new Dictionary<string, WorkerEchoTableJsonData>();
        public static ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableJsonData[]> ABJAProcessor;
        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerEchoTableJsonData[]>("Json/TableData", "WorkerEchoTableData", (datas) =>
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
        public Dictionary<string,int> GetRaw(string id)
        {
            if (!this.WorkerEchoTableDict.ContainsKey(id))
            {
                return null;
            }
            return this.WorkerEchoTableDict[id].Raw;
        }

    }
}