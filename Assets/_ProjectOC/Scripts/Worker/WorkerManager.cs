using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using ML.Engine.Manager;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.InventorySystem;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;
using ML.Engine.UI;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class WorkerManager: ML.Engine.Manager.LocalManager.ILocalManager
    {
        public void OnRegister()
        {
            spriteAtalsHandle = GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(SpriteAtlasPath);
            spriteAtalsHandle.Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    this.workerAtlas = handle.Result as SpriteAtlas;
                }
                else
                {
                    throw new Exception($"WorkerSpriteAtlas 不存在，Addressable Path为: {SpriteAtlasPath}");
                }
            };
        }

        ~WorkerManager()
        {
            GameManager.Instance.ABResourceManager.Release(spriteAtalsHandle);
        }

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

        public void DeleteAllWorker()
        {
            foreach (Worker worker in Workers)
            {
                if (worker != null)
                {
                    ML.Engine.Manager.GameManager.DestroyObj(worker.gameObject);
                    ML.Engine.Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(worker.gameObject);
                }
            }
            this.Workers.Clear();
        }

        public bool DeleteWorker(Worker worker)
        {
            ML.Engine.Manager.GameManager.DestroyObj(worker.gameObject);
            // 通过ManagerSpawn的Worker都是这个流程产生的，所以必须Release
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(worker.gameObject);
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
                if (worker != null && worker.Status == Status.Fishing && !worker.HasProNode && !worker.HasTransport)
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
        public AsyncOperationHandle<GameObject> SpawnWorker(Vector3 pos, Quaternion rot, string name = "Worker")
        {
            //TODO 
            name = "Worker";
            var handle = GetObject(name, pos, rot);
            handle.Completed += (asHandle) =>
            {
                if(asHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    GameObject obj = asHandle.Result;
                    Worker worker = obj.GetComponent<Worker>();
                    if (worker == null)
                    {
                        worker = obj.AddComponent<Worker>();
                    }
                    Workers.Add(worker);
                }
                else
                {
                    Debug.Log($"实例化隐兽 {name} 失败");
                }
            };
            return handle;
        }

        public AsyncOperationHandle<GameObject> SpawnWorker(Vector3 pos, Quaternion rot, IInventory inventory, string workerID, string name = "Worker")
        {
            if (CompositeManager.Instance.OnlyCostResource(inventory, workerID))
            {
                return SpawnWorker(pos, rot, name);
            }
            else
            {
                return default(AsyncOperationHandle<GameObject>);
            }
        }

        public const string SpriteAtlasPath = "OC/UI/Worker/Texture/SA_Worker_UI.spriteatlasv2";
        public const string WorldObjPath = "OC/Character/Worker/Prefabs/";
        private SpriteAtlas workerAtlas = null;
        private AsyncOperationHandle spriteAtalsHandle;

        public Texture2D GetTexture2D(string name)
        {
            return this.workerAtlas.GetSprite(name).texture;
        }
        public Sprite GetSprite(string name)
        {
            return workerAtlas.GetSprite(name);
        }

        /// <summary>
        /// 所有调用的地方，都必须维护好GameObject或者Handle，在不使用GameObejct的时候，除了destroy之外还需要Release(handle)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public AsyncOperationHandle<GameObject> GetObject(string name, Vector3 pos, Quaternion rot)
        {
            return GameManager.Instance.ABResourceManager.InstantiateAsync(WorldObjPath +"/"+ name +".prefab", pos, rot);
        }
    }
}

