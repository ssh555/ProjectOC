using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using System;
using ML.Engine.Manager;
using static ProjectOC.InventorySystem.UI.UIInfiniteInventory;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class WorkerManager: ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region Instance
        private WorkerManager() { }

        private static WorkerManager instance;

        public static WorkerManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new WorkerManager();
                    GameManager.Instance.RegisterGlobalManager(instance);
                    instance.LoadTableData();
                }
                return instance;
            }
        }
        #endregion

        /// <summary>
        /// ��������
        /// </summary>
        public int WorkerNum { get { return Workers.Count; } }
        /// <summary>
        /// �����б�
        /// </summary>
        private HashSet<Worker> Workers = new HashSet<Worker>();
        /// <summary>
        /// ���� Worker ���ݱ�
        /// </summary>
        private Dictionary<string, WorkerTableJsonData> WorkerTableDict = new Dictionary<string, WorkerTableJsonData>();
        /// <summary>
        /// �Ƿ�����Ч��Worker ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidID(string id)
        {
            return this.WorkerTableDict.ContainsKey(id);
        }
        public List<Worker> GetWorkers()
        {
            this.Workers.RemoveWhere(item => item == null);
            return this.Workers.ToList();
        }
        /// <summary>
        /// ��ȡ��ִ�а�������ĵ���
        /// </summary>
        /// <returns></returns>
        public Worker GetCanTransportWorker()
        {
            Worker result = null;
            foreach (Worker worker in this.Workers)
            {
                if (worker != null && worker.Status == Status.Fishing && worker.DutyProductionNode == null)
                {
                    result = worker;
                    break;
                }
            }
            return result;
        }
        public void RemoveWorker(Worker worker)
        {
            this.Workers.Remove(worker);
        }

        /// <summary>
        /// ����id�����µĵ���
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Worker SpawnWorker(string id, Vector3 pos, Quaternion rot)
        {
            if (this.WorkerTableDict.ContainsKey(id))
            {
                WorkerTableJsonData row = this.WorkerTableDict[id];
                // to-do : �ɲ��ö������ʽ
                GameObject obj = GameObject.Instantiate(GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(row.worldobject), pos, rot);
                Worker worker = obj.GetComponent<Worker>();
                if (worker == null)
                {
                    worker = obj.AddComponent<Worker>();
                }
                worker.Init(row);
                this.Workers.Add(worker);
                return worker;
            }
            Debug.LogError("û�ж�ӦIDΪ " + id + " �ĵ���");
            return null;
        }

        public Texture2D GetTexture2D(string id)
        {
            if (!this.WorkerTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>(this.WorkerTableDict[id].texture2d);
        }
        public Sprite GetSprite(string id)
        {
            var tex = this.GetTexture2D(id);
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        public GameObject GetObject(string id)
        {
            if (!this.WorkerTableDict.ContainsKey(id))
            {
                return null;
            }

            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>(this.WorkerTableDict[id].worldobject);
        }

        #region to-do : ���������������� Worker ����
        public const string Texture2DPath = "ui/Worker/texture2d";
        public const string WorldObjPath = "prefabs/Worker/WorldWorker";

        [System.Serializable]
        public struct WorkerTableJsonData
        {
            public string id;
            public string name;
            public WorkType workType;
            public int apMax;
            public int apWorkThreshold;
            public int apRelaxThreshold;
            public int apCost;
            public int apCostTransport;
            public int burMax;
            public Dictionary<WorkType, string> skillDict;
            public string texture2d;
            public string worldobject;
        }

        public static ML.Engine.ABResources.ABJsonAssetProcessor<WorkerTableJsonData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerTableJsonData[]>("Json/TableData", "WorkerTableData", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        this.WorkerTableDict.Add(data.id, data);
                    }
                }, null, "���ޱ�����");
                ABJAProcessor.StartLoadJsonAssetData();
            }
        }
        #endregion
    }
}

