using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ML.Engine.Timer;
using ML.Engine.InventorySystem;
using ML.Engine.BuildingSystem;
using Sirenix.OdinInspector;
using System.IO;
using ML.Engine.TextContent;
using Newtonsoft.Json;
using UnityEngine.U2D;


namespace ProjectOC.TechTree
{
    public sealed class TechTreeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Base
        public static TechTreeManager Instance;

        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// 单例管理
        /// </summary>
        public TechTreeManager()
        {
            Instance = this;
            
            // 注册 Manager
            GM.RegisterLocalManager(this);

            // 载入 Item 和 合成表格
            LoadResource();

            // 载入科技树表格数据 以及 恢复存档
            LoadTableData();

        }


        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                if(ML.Engine.Manager.GameManager.Instance != null)
                    ML.Engine.Manager.GameManager.Instance.ABResourceManager.Release(this.techAtlas);
            }
        }
        #endregion

        #region TechPoint
        public const string TPIconSpriteAtlasPath = "OC/UI/TechPoint/Texture/SA_TechPoint_UI.spriteatlasv2";

        /// <summary>
        /// 载入的科技点表格数据
        /// </summary>
        private Dictionary<string, TechPoint> registerTechPoints = new Dictionary<string, TechPoint>();

        private SpriteAtlas techAtlas;
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

            return this.registerTechPoints.ContainsKey(ID) ? this.techAtlas.GetSprite(this.registerTechPoints[ID].Icon).texture : null;
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
            return this.registerTechPoints.ContainsKey(ID)? techAtlas.GetSprite(this.registerTechPoints[ID].Icon) : null;
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
            return this.techAtlas.GetSprite(category.ToString()).texture;
        }
        
        public Sprite GetTPCategorySprite(TechPointCategory category)
        {
            Sprite _res = null;
            _res = techAtlas.GetSprite(category.ToString());

            if (_res == null)
            {
                _res = techAtlas.GetSprite("None");
            }

            return _res;
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
        /// <summary>
        /// 科技树存档
        /// </summary>
        public const string SaveTPJsonDataPath = "TechTree";
        /// <summary>
        /// 正在解锁中的科技点的存档
        /// </summary>
        public const string SaveUnlockingTPJsonDataPath = "UnlockingTechPoint";

        /// <summary>
        /// 正在解锁中的科技点的计时器
        /// 用于计时解锁以及判断是否正在解锁
        /// </summary>
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

        /// <summary>
        /// 正在解锁的科技点
        /// </summary>
        public Dictionary<string, UnlockingTechPoint> UnlockingTechPointDict = new Dictionary<string, UnlockingTechPoint>();

        #endregion

        #region Load
        private void LoadTableData()
        {
            #region Load TPJsonData
            ML.Engine.ABResources.ABJsonAssetProcessor<TechPoint[]> ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TechPoint[]>("OC/Json/TableData", "TechPoint", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.registerTechPoints.Add(data.ID, data);
                }
                // 读取AB包表格数据，若有存档，将读取的数据的是否解锁更新为存档数据，更新存档
                // 存在则更新
                if (System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath)))
                {
                    var str = File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath));
                    datas = JsonConvert.DeserializeObject<TechPoint[]>(str);
                    foreach (var data in datas)
                    {
                        // 只需要更新 是否解锁
                        if (this.registerTechPoints.ContainsKey(data.ID))
                        {
                            // 有一个解锁则标记为解锁
                            this.registerTechPoints[data.ID].IsUnlocked = data.IsUnlocked || this.registerTechPoints[data.ID].IsUnlocked;
                        }
                    }
                }

                #region Load UnlockingTPData
                if (System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath)))
                {
                    var str = File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath));
                    UnlockingTechPoint[] datas1 = JsonConvert.DeserializeObject<UnlockingTechPoint[]>(str);

                    foreach (var data in datas1)
                    {
                        // 剔除更新数据后不存在的节点
                        if (this.registerTechPoints.ContainsKey(data.id))
                        {
                            this.UnlockingTechPointDict.Add(data.id, data);
                        }
                    }
                }
                // 更新存档
                SaveData();
                #endregion

                // 恢复存档数据
                #region TPJsonData
                foreach (var tp in this.registerTechPoints.Values)
                {
                    if (tp.IsUnlocked)
                    {
                        __Intenal__UnlockTechPoint(tp.ID);
                    }
                }
                #endregion

                #region UnlockingTPData
                foreach (var utp in UnlockingTechPointDict.Values)
                {
                    var timer = new CounterDownTimer(utp.time);
                    OnTimerStart(timer, utp.id, false);
                    timer.OnEndEvent += () =>
                    {
                        OnTimerEnd(timer, utp.id);
                    };
                }
                #endregion
            }, "科技树数据");
            ABJAProcessor.StartLoadJsonAssetData();

            #endregion
        }
        #endregion

        #region Save
        public void SaveData()
        {
            this.SaveTPJsonData();
            this.SaveUnlockingTPData();
        }

        private void SaveTPJsonData()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath);

            TechPoint[] array = registerTechPoints.Values.ToArray();
            File.WriteAllText(path, JsonConvert.SerializeObject(array));
        }

        private void SaveUnlockingTPData()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath);

            UnlockingTechPoint[] array = UnlockingTechPointDict.Values.ToArray();
            File.WriteAllText(path, JsonConvert.SerializeObject(array));
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
        /// 判断背包的材料是否足够解锁id对应的科技点
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        private bool ItemIsEnough(IInventory inventory, string ID)
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

        /// <summary>
        /// 消耗Item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="ID"></param>
        private void CostItem(IInventory inventory, string ID)
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
        public bool CanUnlockTechPoint(IInventory inventory, string ID)
        {
            return !this.UnlockingTechPointDict.ContainsKey(ID) && this.IsAllUnlockedPreTP(ID) && this.ItemIsEnough(inventory, ID);
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool UnlockTechPoint(IInventory inventory, string ID, bool IsCheck = true)
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
            // to-do : 待优化
            // to-do : 四级分类不再是ID，后续会载入Build表数据，拆分为合成子表、<建筑ID, 建筑分类>映射表
            // 后续使用映射表加入
            // 合成子表在Excel转JSON时自动加入合成表中
            // <建筑ID, 建筑分类>映射表为单独的JSON数据表，待完善加入
            foreach (var c in this.registerTechPoints[ID].UnLockBuild)
            {
                var str = BuildingManager.Instance.BPartTableDictOnID[c].GetClassificationString();
                this.UnlockedBuild.Add(str);
                
                MonoBuildingManager monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
                monoBM.BM.RegisterBPartPrefab(monoBM.LoadedBPart[new ML.Engine.BuildingSystem.BuildingPart.BuildingPartClassification(str)]);
            }    

        }
        #endregion

        #region TextContent
        public Dictionary<string, TextTip> CategoryDict = new Dictionary<string, TextTip>();


        [System.Serializable]
        public struct TPPanel
        {
            public TextContent toptitle;
            public TextTip[] category;
            public KeyTip LastTerm;
            public KeyTip NextTerm;
            public TextContent lockedtitletip;
            public TextContent unlockedtitletip;
            public KeyTip Inspect;
            public TextContent timecosttip;
            public KeyTip Decipher;
            public KeyTip Back;
        }

        #endregion

        #region to-delete
        /// <summary>
        /// to-do : 待移除更改，不能放置于此，此项为ItemTable和CompositeTable表数据加载项，用到这两个模块才需要加载 => 放置于Level中更合适
        /// </summary>
        /// <returns></returns>
        void LoadResource()
        {
            ML.Engine.InventorySystem.ItemManager.Instance.LoadTableData();
            ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.LoadTableData();
            LoadTechAtlas();
            ItemManager.Instance.LoadItemAtlas();
        }

        
        #endregion

        private void LoadTechAtlas()
        {
            GM.ABResourceManager.LoadAssetAsync<SpriteAtlas>(TPIconSpriteAtlasPath).Completed += (handle) =>
                {
                    techAtlas = handle.Result as SpriteAtlas;
                };
        }
        
    }

}
