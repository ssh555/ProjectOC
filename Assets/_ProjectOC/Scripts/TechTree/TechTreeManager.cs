using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ML.Engine.Timer;
using UnityEngine.Networking;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.InventorySystem;
using ML.Engine.BuildingSystem;
using Sirenix.OdinInspector;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ProjectOC.TechTree
{
    public sealed class TechTreeManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Base
        public static TechTreeManager Instance;

        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;


        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }
            Instance = this;
        }

        private void Start()
        {
            GM.RegisterLocalManager(this);

            StartCoroutine(LoadResource());

            StartCoroutine(LoadTableData());
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion

        #region TechPoint
        public const string TPIconTexture2DABPath = "UI/TechPoint/Texture2D";

        private Dictionary<string, TechPoint> registerTechPoints = new Dictionary<string, TechPoint>();

        public TechPoint GetTechPoint(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID] : null;
        }

        public Texture2D GetTPTexture2D(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID)? GM.ABResourceManager.LoadAsset<Texture2D>(TPIconTexture2DABPath, this.registerTechPoints[ID].Icon) : null;
        }

        public Sprite GetTPSprite(string ID)
        {
            var tex = this.GetTPTexture2D(ID);
            return tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2)) : null;
        }

        public string GetTPDescription(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].Description.GetText() : "";
        }

        public bool IsUnlockedTP(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].IsUnlocked : false;
        }

        public string[] GetTPCanUnlockedID(string ID)
        {
            var retVal = this.registerTechPoints[ID].UnLockRecipe.ToList();
            retVal.AddRange(this.registerTechPoints[ID].UnLockBuild);
            return this.registerTechPoints.ContainsKey(ID) ? retVal.ToArray() : null;
        }

        public string[] GetPreTechPoints(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].PrePoint : null;
        }

        public bool IsAllUnlockedPreTP(string ID)
        {
            if(this.registerTechPoints.ContainsKey(ID))
            {
                foreach(var point in this.registerTechPoints[ID].PrePoint)
                {
                    if(!this.IsUnlockedTP(ID))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Formula[] GetTPItemCost(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].ItemCost : null;
        }

        public int GetTPTimeCost(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].TimeCost : -1;
        }
        #endregion

        #region SaveAndLoadData
        #region Struct
        [System.Serializable]
        private struct TechPointArray
        {
            public TechPoint[] techPoints;
        }

        /// <summary>
        /// 正在解锁中的科技点
        /// </summary>
        [System.Serializable]
        public struct UnlockingTechPoint
        {
            /// <summary>
            /// 科技点ID
            /// </summary>
            public string id;
            /// <summary>
            /// 当前解析时间
            /// </summary>
            public int time;
        }

        [System.Serializable]
        private struct UnlockingTechPointArray
        {
            public UnlockingTechPoint[] data;
        }
        #endregion

        #region Data
        [ReadOnly]
        public bool IsLoadOvered = false;

        public const string TableDataABPath = "Json/TableData";
        public const string TableName = "TechTree";
        /// <summary>
        /// 科技树存档
        /// </summary>
        public const string SaveTPJsonDataPath = "TechTree.json";
        /// <summary>
        /// 正在解锁中的科技点的存档
        /// </summary>
        public const string SaveUnlockingTPJsonDataPath = "UnlockingTechPoint.json";

        public Dictionary<string, CounterDownTimer> UnlockingTPTimers = new Dictionary<string, CounterDownTimer>();
        private void OnTimerStart(CounterDownTimer timer, string id, bool isSave = true)
        {
            this.UnlockingTPTimers.Add(id, timer);
            if(isSave)
            {
                this.SaveUnlockingTPData();
            }

        }
        private void OnTimerEnd(CounterDownTimer timer, string id, bool isSave = true)
        {
            this.__Intenal__UnlockTechPoint(id);
            this.UnlockingTPTimers.Remove(id);
            if (isSave)
            {
                this.SaveData();
            }
        }

        public Dictionary<string, UnlockingTechPoint> UnlockingTechPointDict = new Dictionary<string, UnlockingTechPoint>();
        #endregion

        #region Load
        private IEnumerator LoadTableData()
        {

            // 需在ItemSystem、CompositonSystem、BuildingSystem加载完数据之后才能加载
            while (GM.ABResourceManager == null || ML.Engine.InventorySystem.ItemSpawner.Instance.IsLoadOvered == false || ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.IsLoadOvered == false)
            {
                yield return null;
            }
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            var abmgr = GM.ABResourceManager;
            #region Load TPJsonData
            // 不存在 则复制一份
            if (!System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath)))
            {
                AssetBundle ab;
                var crequest = abmgr.LoadLocalABAsync(TableDataABPath, null, out ab);
                yield return crequest;
                if (crequest != null)
                {
                    ab = crequest.assetBundle;
                }


                var request = ab.LoadAssetAsync<TextAsset>(TableName);
                yield return request;
                TechPointArray datas = JsonUtility.FromJson<TechPointArray>((request.asset as TextAsset).text);

                foreach (var data in datas.techPoints)
                {
                    this.registerTechPoints.Add(data.ID, data);
                }

                // 存档
                SaveTPJsonData();
            }
            // 直接读取文件载入
            else
            {
                string json = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath));
                TechPointArray datas = JsonUtility.FromJson<TechPointArray>(json);

                foreach (var data in datas.techPoints)
                {
                    this.registerTechPoints.Add(data.ID, data);
                }
            }
            #endregion

            #region Load UnlockingTPData
            if (System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath)))
            {
                string json = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath));
                UnlockingTechPointArray datas = JsonUtility.FromJson<UnlockingTechPointArray>(json);

                foreach (var data in datas.data)
                {
                    this.UnlockingTechPointDict.Add(data.id, data);
                }
            }
            #endregion


            // 恢复存档数据
            #region TPJsonData
            foreach(var tp in this.registerTechPoints.Values)
            {
                if(tp.IsUnlocked)
                {
                    __Intenal__UnlockTechPoint(tp.ID);
                }
            }
            #endregion

            #region UnlockingTPData
            foreach(var utp in UnlockingTechPointDict.Values)
            {
                var timer = new CounterDownTimer(utp.time);
                OnTimerStart(timer, utp.id, false);
                timer.OnEndEvent += () =>
                {
                    OnTimerEnd(timer, utp.id);
                };
            }
            #endregion

            IsLoadOvered = true;
#if UNITY_EDITOR
            Debug.Log($"存储路径: {Application.persistentDataPath}");
            Debug.Log("LoadTableData cost time: " + (Time.realtimeSinceStartup - startT));
#endif
        }
        #endregion

        #region Save
        public void SaveData()
        {
            this.SaveTPJsonData();
            this.SaveUnlockingTPData();
        }

        private CancellationTokenSource SaveDataCTS;
        private void SaveTPJsonData()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath);

            TechPointArray array = new TechPointArray();
            array.techPoints = registerTechPoints.Values.ToArray();
            string json = JsonUtility.ToJson(array);

            WriteToFileAsync(path, json, SaveDataCTS);
        }

        private CancellationTokenSource SaveUnlockingDataCTS;
        private void SaveUnlockingTPData()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath);

            UnlockingTechPointArray array = new UnlockingTechPointArray();
            array.data = UnlockingTechPointDict.Values.ToArray();
            string json = JsonUtility.ToJson(array);

            WriteToFileAsync(path, json, SaveUnlockingDataCTS);
        }
        private async void WriteToFileAsync(string path, string content, CancellationTokenSource cts)
        {
            // 如果之前的写入操作还在进行中，取消它
            cts?.Cancel();
            cts = new CancellationTokenSource();

            try
            {
                // 异步写入文件
                await System.IO.File.WriteAllTextAsync(path, content, Encoding.UTF8, cts.Token);
            }
            catch (TaskCanceledException)
            {
                // 写入操作被取消
            }
        }
        #endregion

        [Button("删除存档")]
        public void DeleteData()
        {
            System.IO.File.Delete(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath));
            System.IO.File.Delete(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath));
        }

        #endregion

        #region UnLock
        /// <summary>
        /// 已解锁可以使用的配方
        /// </summary>
        [ReadOnly]
        public List<string> UnlockedRecipe = new List<string>();
        /// <summary>
        /// 已解锁可以使用的建筑物
        /// </summary>
        [ReadOnly]
        public List<string> UnlockedBuild = new List<string>();


        /// <summary>
        /// 是否能点亮对应的科技点
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CanUnlockTechPoint(Inventory inventory, string ID)
        {
            return this.IsAllUnlockedPreTP(ID) && CompositeSystem.Instance.CanComposite(inventory, ID);
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool UnlockTechPoint(CompositeAbility owner, string ID)
        {
            if(!CanUnlockTechPoint(owner.ResourceInventory, ID))
            {
                return false;
            }

            // to-do : 暂定为提前消耗
            CompositeSystem.Instance.OnlyCostResource(owner.ResourceInventory, ID);

            // 计时
            var tp = this.registerTechPoints[ID];
            var timer = new CounterDownTimer(tp.TimeCost);
            OnTimerStart(timer, tp.ID);
            // 结束后解锁
            timer.OnEndEvent += () =>
            {
                OnTimerEnd(timer, tp.ID);
            };
            return true;
        }

        /// <summary>
        /// 直接解锁，不需要消耗与时间
        /// </summary>
        /// <param name="ID"></param>
        private void __Intenal__UnlockTechPoint(string ID)
        {
            this.registerTechPoints[ID].IsUnlocked = true;

            // 解锁配方
            this.UnlockedRecipe.AddRange(this.registerTechPoints[ID].UnLockRecipe);
            // 解锁建筑物
            this.UnlockedBuild.AddRange(this.registerTechPoints[ID].UnLockBuild);
            // to-do : 待优化
            foreach(var c in this.registerTechPoints[ID].UnLockBuild)
            {
                Test_BuildingManager.Instance.BM.RegisterBPartPrefab(Test_BuildingManager.Instance.LoadedBPart[new ML.Engine.BuildingSystem.BuildingPart.BuildingPartClassification(c)]);
            }    

        }
        #endregion


        #region to-delete
        /// <summary>
        /// to-do : 待移除更改，不能放置于此，此项为ItemTable和CompositeTable表数据加载项，用到这两个模块才需要加载 => 放置于Level中更合适
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadResource()
        {
            float startTime = Time.realtimeSinceStartup;
            var c1 = StartCoroutine(ML.Engine.InventorySystem.ItemSpawner.Instance.LoadTableData(this));
            var c2 = StartCoroutine(ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.LoadTableData(this));
            yield return c1;
            yield return c2;

            Debug.Log("LoadTableData Cost: " + (Time.realtimeSinceStartup - startTime));
            // 结束此协程
            yield break;
        }
        #endregion
    }

}
