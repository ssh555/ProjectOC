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
        /// ��������
        /// </summary>
        public int WorkerNum { get { return Workers.Count; } }

        /// <summary>
        /// ����
        /// </summary>
        private HashSet<Worker> Workers = new HashSet<Worker>();

        public List<Worker> GetWorkers()
        {
            this.Workers.RemoveWhere(item => item == null);
            return this.Workers.ToList();
        }

        /// <summary>
        /// ��ȡ��ִ�а�������ĵ���
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

