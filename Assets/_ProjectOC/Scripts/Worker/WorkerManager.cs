using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using ML.Engine.Manager;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.InventorySystem;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class WorkerManager: ML.Engine.Manager.LocalManager.ILocalManager
    {
        /// <summary>
        /// 刁民数量
        /// </summary>
        public int WorkerNum { get { return Workers.Count; } }

        /// <summary>
        /// 刁民
        /// </summary>
        private HashSet<Worker> Workers = new HashSet<Worker>();

        public List<Worker> GetWorkers()
        {
            this.Workers.RemoveWhere(item => item == null);
            return this.Workers.ToList();
        }

        /// <summary>
        /// 获取能执行搬运任务的刁民
        /// </summary>
        public Worker GetCanTransportWorker()
        {
            Worker result = null;
            foreach (Worker worker in this.Workers)
            {
                if (worker != null && worker.Status == Status.Fishing && worker.ProNode == null)
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

        public Worker Composite(IInventory inventory)
        {
            CompositeManager.CompositionObjectType compObjType = CompositeManager.Instance.Composite(inventory, "Worker", out var composition);
            if (compObjType == CompositeManager.CompositionObjectType.Worker)
            {
                return composition as Worker;
            }
            else
            {
                Debug.LogError($"Worker Composite Error");
                return null;
            }
        }

        /// <summary>
        /// 创建新的刁民
        /// </summary>
        public Worker SpawnWorker()
        {
            GameObject obj = GameObject.Instantiate(GetObject());
            Worker worker = obj.AddComponent<Worker>();
            Workers.Add(worker);
            return worker;
        }

        public const string Texture2DPath = "ui/Worker/texture2d";
        public const string WorldObjPath = "prefabs/Worker/WorldWorker";
        public Texture2D GetTexture2D()
        {
            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>("Worker");
        }
        public Sprite GetSprite()
        {
            var tex = this.GetTexture2D();
            if (tex == null)
            {
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        public GameObject GetObject()
        {
            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>("Worker");
        }
    }
}

