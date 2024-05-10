using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.WorkerNS
{
    [System.Serializable]
    public struct WorkerNameTableData
    {
        public string ID;
        public ML.Engine.TextContent.TextContent Name;
    }

    [LabelText("隐兽管理器"), System.Serializable, ShowInInspector]
    public sealed class WorkerManager: ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region ILocalManager
        private const string SpriteAtlasPath = "SA_Worker_UI";
        private const string WorldObjPath = "Prefab_AICharacter";
        private UnityEngine.U2D.SpriteAtlas workerAtlas = null;
        private UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle spriteAtalsHandle;
        private ML.Engine.ABResources.ABJsonAssetProcessor<WorkerNameTableData[]> ABJAProcessor;
        public void OnRegister()
        {
            spriteAtalsHandle = ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<UnityEngine.U2D.SpriteAtlas>(SpriteAtlasPath);
            spriteAtalsHandle.Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    workerAtlas = handle.Result as UnityEngine.U2D.SpriteAtlas;
                }
                else
                {
                    throw new System.Exception($"WorkerSpriteAtlas 不存在，Addressable Path为: {SpriteAtlasPath}");
                }
            };

            ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<WorkerNameTableData[]>("OCTableData", "WorkerName", (datas) =>
            {
                foreach (var data in datas.ToList().OrderBy(x => x.ID))
                {
                    WorkerNames.Add(data.Name);
                }
            }, "隐兽名字表数据");
            ABJAProcessor.StartLoadJsonAssetData();

            ML.Engine.Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<WorkerConfigAsset>("Config_Worker").Completed += (handle) =>
            {
                WorkerConfigAsset data = handle.Result;
                Config = new WorkerConfig(data.Config);
            };
        }

        public void OnUnregister()
        {
            DeleteAllWorker();
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.Release(spriteAtalsHandle);
        }
        #endregion

        #region Data
        public WorkerConfig Config;
        [LabelText("隐兽名字"), ShowInInspector, ReadOnly]
        private List<string> WorkerNames = new List<string>();
        [LabelText("刁民"), ShowInInspector, ReadOnly]
        private HashSet<Worker> Workers = new HashSet<Worker>();
        public System.Action<Worker> OnAddWokerEvent;
        public System.Action<Worker> OnDeleteWokerEvent;
        private System.Random Random = new System.Random();
        #endregion

        #region Get
        public List<Worker> GetWorkers(bool needSort = true)
        {
            Workers.RemoveWhere(item => item == null);
            return needSort ? Workers.OrderBy(worker => worker.ID).ToList() : Workers.ToList();
        }
        /// <summary>
        /// 获取能执行搬运任务的刁民
        /// </summary>
        public Worker GetCanTransportWorker()
        {
            Worker result = null;
            foreach (Worker worker in Workers.ToArray())
            {
                if (worker != null && worker.Status == Status.Fishing && !worker.HaveProNode && !worker.HaveTransport)
                {
                    result = worker;
                    break;
                }
            }
            return result;
        }
        public string GetOneNewWorkerID()
        {
            return ML.Engine.Utility.OSTime.OSCurMilliSeconedTime.ToString();
        }
        public string GetOneNewWorkerName()
        {
            return WorkerNames[Random.Next(0, WorkerNames.Count)];
        }
        public Gender GetOneNewWorkerGender()
        {
            return Random.Next(0, 2) == 0 ? Gender.Male : Gender.Female;
        }
        public Sprite GetWorkerIcon(Worker worker)
        {
            if (worker != null)
            {
                return GetSprite($"{worker.Category}");
            }
            return null;
        }
        public List<int> GetSkillLevel()
        {
            List<int> result = new List<int>(6);
            double mean = Config.SkillStdMean;
            double standardDeviation = System.Math.Sqrt(Config.SkillStdDev);
            double beta = (mean - Config.SkillStdLowBound) / standardDeviation;
            double alpha = (Config.SkillStdHighBound - mean) / standardDeviation;
            double u = Random.NextDouble();
            int k;
            if (u < alpha / (alpha + beta))
            {
                k = (int)(Config.SkillStdHighBound - standardDeviation * System.Math.Log((1 - u) * alpha / alpha + u * (alpha / beta)));
            }
            else
            {
                k = (int)(Config.SkillStdLowBound + standardDeviation * System.Math.Log(u * beta / (1 - u) * beta + (1 - u) * (beta / alpha)));
            }
            for (int i = 0; i < 6; i++)
            {
                int min = System.Math.Max(k - (5 - i) * 10, 0);
                int randomInt = Random.Next(min, System.Math.Min(k, 10) + 1);
                k -= randomInt;
                result.Add(randomInt);
            }
            return result;
        }

        public Sprite GetSprite(string name)
        {
            return workerAtlas.GetSprite(name);
        }
        /// <summary>
        /// 所有调用的地方，都必须维护好GameObject或者Handle，在不使用GameObejct的时候，除了destroy之外还需要Release(handle)
        /// </summary>
        public UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> GetObject(string name, Vector3 pos, Quaternion rot)
        {
            return ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(WorldObjPath + "/" + name + ".prefab", pos, rot);
        }
        #endregion

        #region Spawn Delete
        public UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> SpawnWorker(Vector3 pos, Quaternion rot, string name = "Prefab_Worker_Worker", bool isAdd = true)
        {
            var handle = GetObject(name, pos, rot);
            handle.Completed += (asHandle) =>
            {
                if (asHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    GameObject obj = asHandle.Result;
                    Worker worker = obj.GetComponent<Worker>();
                    if (worker == null)
                    {
                        worker = obj.AddComponent<Worker>();
                    }
                    worker.Init();
                    if (isAdd)
                    {
                        Workers.Add(worker);
                        OnAddWokerEvent?.Invoke(worker);
                    }
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
        public bool CanComposite(ML.Engine.InventorySystem.IInventory inventory, string workerID)
        {
            if (!string.IsNullOrEmpty(workerID))
            {
                var raw = ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetRaw(workerID);
                if (raw != null)
                {
                    foreach (var formula in raw)
                    {
                        if (inventory.GetItemAllNum(formula.id) < formula.num)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            return false;
        }
        public bool OnlyCostResource(ML.Engine.InventorySystem.IInventory inventory, string workerID)
        {
            if (!CanComposite(inventory, workerID))
            {
                return false;
            }
            foreach (var formula in ManagerNS.LocalGameManager.Instance.WorkerEchoManager.GetRaw(workerID))
            {
                inventory.RemoveItem(formula.id, formula.num);
            }
            return true;
        }

        public void DeleteAllWorker()
        {
            foreach (Worker worker in Workers.ToArray())
            {
                if (worker != null)
                {
                    OnDeleteWokerEvent?.Invoke(worker);
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
            // 通过ManagerSpawn的Worker都是这个流程产生的，所以必须Release
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.ReleaseInstance(worker.gameObject);
            return Workers.Remove(worker);
        }
        #endregion
    }
}