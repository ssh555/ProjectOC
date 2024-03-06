using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using ML.Engine.Manager;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.InventorySystem;
using UnityEngine.U2D;

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

        public bool DeleteWorker(Worker worker)
        {
            return this.Workers.Remove(worker);
        }


        /// <summary>
        /// 获取能执行搬运任务的刁民
        /// </summary>
        public Worker GetCanTransportWorker()
        {
            Worker result = null;
            foreach (Worker worker in this.Workers)
            {
                if (worker != null && worker.Status == Status.Fishing && worker.ProNode == null && worker.Transport == null)
                {
                    result = worker;
                    break;
                }
            }
            return result;
        }

        public bool OnlyCostResource(IInventory inventory, string workerID)
        {
            return CompositeManager.Instance.OnlyCostResource(inventory, workerID);
        }
        public Worker SpawnWorker(Vector3 pos, Quaternion rot)
        {
            GameObject obj = GameObject.Instantiate(GetObject(), pos, rot);
            Worker worker = obj.GetComponent<Worker>();
            if (worker == null)
            {
                worker = obj.AddComponent<Worker>();
            }
            Workers.Add(worker);
            return worker;
        }
        public Worker SpawnWorker(Vector3 pos, Quaternion rot, string workerID)
        {
            GameObject obj = GameObject.Instantiate(GetObject(), pos, rot);
            Worker worker = obj.GetComponent<Worker>();
            if (worker == null)
            {
                worker = obj.AddComponent<Worker>();
            }
            Workers.Add(worker);
            return worker;
        }
        public Worker SpawnWorker(Vector3 pos, Quaternion rot, IInventory inventory, string workerID)
        {
            if (CompositeManager.Instance.OnlyCostResource(inventory, workerID))
            {
                GameObject obj = GameObject.Instantiate(GetObject(), pos, rot);
                Worker worker = obj.GetComponent<Worker>();
                if (worker == null)
                {
                    worker = obj.AddComponent<Worker>();
                }
                Workers.Add(worker);
                return worker;
            }
            else
            {
                return null;
            }
        }

        public const string Texture2DPath = "ui/Worker/texture2d";
        public const string WorldObjPath = "prefabs/Character/Worker";
        private SpriteAtlas workerAtlas = null;
        public Texture2D GetTexture2D()
        {
            return GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<Texture2D>("Worker");
        }
        public Sprite GetSprite()
        {
            if (workerAtlas == null)
            {
                workerAtlas = GameManager.Instance.ABResourceManager.LoadLocalAB(Texture2DPath).LoadAsset<SpriteAtlas>("SA_Worker_UI");
            }
            return workerAtlas.GetSprite("Worker");
        }
        public GameObject GetObject()
        {
            return GameManager.Instance.ABResourceManager.LoadLocalAB(WorldObjPath).LoadAsset<GameObject>("Worker");
        }
    }
}

