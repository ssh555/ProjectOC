using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;
using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public sealed class WorkerManager: ML.Engine.Manager.LocalManager.ILocalManager
    {
        [LabelText("����"), ShowInInspector, ReadOnly]
        private HashSet<Worker> Workers = new HashSet<Worker>();
        [LabelText("��������"), ShowInInspector, ReadOnly]
        public int WorkerNum { get { return Workers.Count; } }
        public Action<Worker> OnAddWokerEvent;
        public Action<Worker> OnDeleteWokerEvent;

        public void OnRegister()
        {
            spriteAtalsHandle = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>(SpriteAtlasPath);
            spriteAtalsHandle.Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    workerAtlas = handle.Result as SpriteAtlas;
                }
                else
                {
                    throw new Exception($"WorkerSpriteAtlas �����ڣ�Addressable PathΪ: {SpriteAtlasPath}");
                }
            };
        }

        public void OnUnregister()
        {
            DeleteAllWorker();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.Release(spriteAtalsHandle);
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
            Workers.Clear();
        }
        public bool DeleteWorker(Worker worker)
        {
            OnDeleteWokerEvent?.Invoke(worker);
            ML.Engine.Manager.GameManager.DestroyObj(worker.gameObject);
            // ͨ��ManagerSpawn��Worker����������̲����ģ����Ա���Release
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(worker.gameObject);
            return Workers.Remove(worker);
        }

        public string GetOneNewWorkerInstanceID()
        {
            return ML.Engine.Utility.OSTime.OSCurMilliSeconedTime.ToString();
        }

        public List<Worker> GetWorkers(bool needSort=true)
        {
            Workers.RemoveWhere(item => item == null);
            if (needSort)
            {
                return Workers.OrderBy(worker => worker.InstanceID).ToList();
            }
            else
            {
                return Workers.ToList();
            }
        }

        /// <summary>
        /// ��ȡ��ִ�а�������ĵ���
        /// </summary>
        public Worker GetCanTransportWorker()
        {
            Worker result = null;
            foreach (Worker worker in Workers)
            {
                if (worker != null && worker.Status == Status.Fishing && !worker.HaveProNode && !worker.HaveTransport)
                {
                    result = worker;
                    break;
                }
            }
            return result;
        }

        public bool OnlyCostResource(ML.Engine.InventorySystem.IInventory inventory, string workerID)
        {
            return ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.OnlyCostResource(inventory, workerID);
        }
        public AsyncOperationHandle<GameObject> SpawnWorker(Vector3 pos, Quaternion rot, string name = "Worker", bool isAdd=true)
        {
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
                    worker.InstanceID = GetOneNewWorkerInstanceID();
                    if (isAdd)
                    {
                        Workers.Add(worker);
                        OnAddWokerEvent?.Invoke(worker);
                    }
                }
                else
                {
                    Debug.Log($"ʵ�������� {name} ʧ��");
                }
            };
            return handle;
        }
        public bool AddToWorkers(Worker worker)
        {
            if (worker != null && !Workers.Contains(worker))
            {
                Workers.Add(worker);
                OnAddWokerEvent?.Invoke(worker);
                return true;
            }
            return false;
        }

        public AsyncOperationHandle<GameObject> SpawnWorker(Vector3 pos, Quaternion rot, ML.Engine.InventorySystem.IInventory inventory, string workerID, string name = "Worker")
        {
            if (ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.OnlyCostResource(inventory, workerID))
            {
                return SpawnWorker(pos, rot, name);
            }
            else
            {
                return default(AsyncOperationHandle<GameObject>);
            }
        }

        public const string SpriteAtlasPath = "SA_Worker_UI";
        public const string WorldObjPath = "Prefab_AICharacter";
        private SpriteAtlas workerAtlas = null;
        private AsyncOperationHandle spriteAtalsHandle;

        public Sprite GetSprite(string name)
        {
            return workerAtlas.GetSprite(name);
        }
        /// <summary>
        /// ���е��õĵط���������ά����GameObject����Handle���ڲ�ʹ��GameObejct��ʱ�򣬳���destroy֮�⻹��ҪRelease(handle)
        /// </summary>
        public AsyncOperationHandle<GameObject> GetObject(string name, Vector3 pos, Quaternion rot)
        {
            return ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(WorldObjPath +"/"+ name +".prefab", pos, rot);
        }
    }
}