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
using ML.Engine.TextContent;
using Newtonsoft.Json;

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

            StartCoroutine(InitUITextContents());
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

        public string[] GetAllTPID()
        {
            return this.registerTechPoints.Keys.ToArray();
        }

        public TechPoint GetTechPoint(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID] : null;
        }

        public Texture2D GetTPTexture2D(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID)? GM.ABResourceManager.LoadAsset<Texture2D>(TPIconTexture2DABPath, this.registerTechPoints[ID].Icon) : null;
        }

        public string GetTPName(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ?  this.registerTechPoints[ID].Name.GetText() : "";
        }

        public int[] GetTPGrid(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].Grid : null;
        }

        public TechPointCategory GetTPCategory(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].Category : TechPointCategory.None;
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
                    if(!this.IsUnlockedTP(point))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public ML.Engine.InventorySystem.CompositeSystem.Formula[] GetTPItemCost(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].ItemCost : null;
        }

        public int GetTPTimeCost(string ID)
        {
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].TimeCost : -1;
        }

        public Texture2D GetTPCategoryTexture2D(TechPointCategory category)
        {
            var tex = GM.ABResourceManager.LoadAsset<Texture2D>(TPIconTexture2DABPath, category.ToString());
            if(tex == null)
            {
                tex = GM.ABResourceManager.LoadAsset<Texture2D>(TPIconTexture2DABPath, "None");
            }
            return tex;
        }
        
        public Sprite GetTPCategorySprite(TechPointCategory category)
        {
            var tex = this.GetTPCategoryTexture2D(category);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
        }
        #endregion

        #region SaveAndLoadData
        #region Struct
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
            public float time;
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
        private void OnTimerStart(CounterDownTimer timer, string ID, bool isSave = true)
        {
            this.UnlockingTPTimers.Add(ID, timer);
            if(!this.UnlockingTechPointDict.ContainsKey(ID))
            {
                this.UnlockingTechPointDict.Add(ID, new UnlockingTechPoint() { id = ID, time = (float)timer.CurrentTime });
            }
            if (isSave)
            {
                this.SaveUnlockingTPData();
            }

        }
        private void OnTimerEnd(CounterDownTimer timer, string id, bool isSave = true)
        {
            this.__Intenal__UnlockTechPoint(id);
            this.UnlockingTPTimers.Remove(id);
            this.UnlockingTechPointDict.Remove(id);
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
            while (GM.ABResourceManager == null || ML.Engine.InventorySystem.ItemSpawner.Instance.IsLoadOvered == false || ML.Engine.InventorySystem.CompositeSystem.CompositeSystem.Instance.IsLoadOvered == false || ML.Engine.BuildingSystem.Test_BuildingManager.Instance.IsLoadOvered == false)
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
                TechPoint[] datas = JsonConvert.DeserializeObject<TechPoint[]>((request.asset as TextAsset).text);
                
                foreach (var data in datas)
                {
                    //// 读取合成表数据
                    //data.ItemCost = CompositeSystem.Instance.GetCompositonFomula(data.ID);

                    this.registerTechPoints.Add(data.ID, data);
                }

                // 存档
                SaveTPJsonData();
            }
            // 直接读取文件载入
            else
            {
                string json = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath));
                TechPoint[] datas = JsonConvert.DeserializeObject<TechPoint[]>(json);
                foreach (var data in datas)
                {
                    //// 读取合成表数据
                    //data.ItemCost = CompositeSystem.Instance.GetCompositonFomula(data.ID);

                    this.registerTechPoints.Add(data.ID, data);
                }
            }
            #endregion

            #region Load UnlockingTPData
            if (System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath)))
            {
                string json = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath));
                UnlockingTechPoint[] datas = JsonConvert.DeserializeObject<UnlockingTechPoint[]>(json);

                foreach (var data in datas)
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

            TechPoint[] array = registerTechPoints.Values.ToArray();
            string json = JsonConvert.SerializeObject(array);

            WriteToFileAsync(path, json, SaveDataCTS);
        }

        private CancellationTokenSource SaveUnlockingDataCTS;
        private void SaveUnlockingTPData()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath);

            UnlockingTechPoint[] array = UnlockingTechPointDict.Values.ToArray();
            string json = JsonConvert.SerializeObject(array);
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

        private bool ItemIsEnough(Inventory inventory, string ID)
        {
            var formula = this.GetTPItemCost(ID);
            if(formula == null || formula.Length == 0)
            {
                return true;
            }
            foreach(var f in formula)
            {
                // 数量不够
                if (inventory.GetItemAllNum(f.id) < f.num)
                {
                    return false;
                }
            }
            return true;
        }

        private void CostItem(Inventory inventory, string ID)
        {
            // 移除消耗的资源
            lock (inventory)
            {
                foreach (var formula in this.GetTPItemCost(ID))
                {
                    inventory.RemoveItem(formula.id, formula.num);
                }
            }
        }

        /// <summary>
        /// 是否能点亮对应的科技点
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CanUnlockTechPoint(Inventory inventory, string ID)
        {
            return !this.UnlockingTechPointDict.ContainsKey(ID) && this.IsAllUnlockedPreTP(ID) && this.ItemIsEnough(inventory, ID);
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool UnlockTechPoint(Inventory inventory, string ID, bool IsCheck = true)
        {
            if(IsCheck && !CanUnlockTechPoint(inventory, ID))
            {
                return false;
            }

            // to-do : 暂定为提前消耗
            this.CostItem(inventory, ID);

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

        #region TextContent
        public const string TextContentABPath = "JSON/TextContent/TechTree";
        public const string TPPanelName = "TechPointPanel";

        public Dictionary<string, TextTip> CategoryDict = new Dictionary<string, TextTip>();
        public TPPanel TPPanelTextContent;

        [System.Serializable]
        public struct TPPanel
        {
            public TextContent toptitle;
            public TextTip[] category;
            public KeyTip categorylast;
            public KeyTip categorynext;
            public TextContent lockedtitletip;
            public TextContent unlockedtitletip;
            public KeyTip inspector;
            public TextContent timecosttip;
            public KeyTip decipher;
            public KeyTip back;
        }

        private IEnumerator InitUITextContents()
        {
            while (ML.Engine.Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            var abmgr = ML.Engine.Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TextContentABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAssetAsync<TextAsset>(TPPanelName);
            yield return request;
            TPPanel tips = JsonConvert.DeserializeObject<TPPanel>((request.asset as TextAsset).text);
            foreach (var tip in tips.category)
            {
                this.CategoryDict.Add(tip.name, tip);
            }
            TPPanelTextContent = tips;

#if UNITY_EDITOR
            Debug.Log("InitUITextContents cost time: " + (Time.realtimeSinceStartup - startT));
#endif
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

        [Button("生成测试文件")]
        void GenTESTFILE()
        {
            List<TechPoint> datas = new List<TechPoint>();

            for (int c = 1; c < 6; ++c)
            {
                for (int row = 0; row < 10; ++row)
                {
                    for (int col = 0; col < 16; ++col)
                    {
                        var tp = new TechPoint();

                        tp.Category = (TechPointCategory)c;

                        tp.ID = tp.Category.ToString() + "_" + row.ToString() + "_" + col.ToString();
                        tp.Name.Chinese = "测试" + tp.ID;
                        tp.Name.English = "Test" + tp.ID;
                        tp.Grid = new int[2] { row, col };
                        tp.Icon = row.ToString() + "_" + col.ToString();
                        tp.IsUnlocked = false;
                        tp.Description.Chinese = "测试";
                        tp.Description.English = "Test";

                        tp.UnLockBuild = new string[0];
                        tp.UnLockRecipe = new string[0];
                        // to-do
                        int num = Random.Range(1, 3);
                        tp.ItemCost = new ML.Engine.InventorySystem.CompositeSystem.Formula[num];
                        for(int i = 0; i < tp.ItemCost.Length; ++i)
                        {
                            Formula f = new Formula();
                            f.id = i == 0 ? "100001" : (i == 1 ? "100002" : "100003");
                            f.num = Random.Range(1, 5);
                            tp.ItemCost[i] = f;
                        }
                        tp.TimeCost = Random.Range(5, 60);

                        if (row == 0 && col == 0)
                        {
                            tp.PrePoint = new string[0];
                        }
                        else if (row != 0 && col == 0)
                        {
                            tp.PrePoint = new string[1] { tp.Category.ToString() + "_" + (row - 1).ToString() + "_" + col.ToString() };
                        }
                        else if (row == 0 && col != 0)
                        {
                            tp.PrePoint = new string[1] { tp.Category.ToString() + "_" + row.ToString() + "_" + (col - 1).ToString() };
                        }
                        else if (row != 0 && col != 0)
                        {
                            tp.PrePoint = new string[2] { tp.Category.ToString() + "_" + row.ToString() + "_" + (col - 1).ToString(), tp.Category.ToString() + "_" + (row - 1).ToString() + "_" + col.ToString() };
                        }


                        datas.Add(tp);
                    }
                }
            }


            string json = JsonConvert.SerializeObject(datas.ToArray());
            System.IO.File.WriteAllText(Application.streamingAssetsPath + "/../../../t.json", json);
            Debug.Log("输出路径: " + Application.streamingAssetsPath + "/../../../t.json");
        }
        #endregion

    }

}
