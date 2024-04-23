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
using ML.Engine.Manager;
using ML.Engine.SaveSystem;
using Unity.VisualScripting;

namespace ProjectOC.TechTree
{
    [System.Serializable]
    public sealed class TechTreeManager : ML.Engine.Manager.LocalManager.ILocalManager
    {

        #region Base
        public static TechTreeManager Instance;

        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// ��������
        /// </summary>
        public void Init()
        {
            Instance = this;
            
            // ע�� Manager
            GM.RegisterLocalManager(this);

            
            LoadResource();

            // ����Ƽ���������� �Լ� �ָ��浵
            LoadTableData();

        }

        public void OnRegister()
        {
            Init();
        }


        public void OnUnregister()
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
        /// ����ĿƼ���������
        /// </summary>
        [ShowInInspector]
        private Dictionary<string, TechPoint> registerTechPoints = new Dictionary<string, TechPoint>();
        [ShowInInspector]
        private SpriteAtlas techAtlas;
        public string[] GetAllTPID()
        {
            return this.registerTechPoints.Keys.ToArray();
        }

        public TechPoint GetTechPoint(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID] : null;
        }

        public Texture2D GetTPTexture2D(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            return this.registerTechPoints.ContainsKey(ID) ? this.techAtlas.GetSprite(this.registerTechPoints[ID].Icon).texture : null;
        }

        public string GetTPName(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return "";
            return this.registerTechPoints.ContainsKey(ID) ?  this.registerTechPoints[ID].Name.GetText() : "";
        }

        public int[] GetTPGrid(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].Grid : null;
        }

        public TechPointCategory GetTPCategory(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return TechPointCategory.None;
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].Category : TechPointCategory.None;
        }

        public Sprite GetTPSprite(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            return this.registerTechPoints.ContainsKey(ID)? techAtlas.GetSprite(this.registerTechPoints[ID].Icon) : null;
        }

        public string GetTPDescription(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return "";
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].Description.GetText() : "";
        }

        public bool IsUnlockedTP(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return false;
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].IsUnlocked : false;
        }

        public string[] GetTPCanUnlockedID(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            var retVal = this.registerTechPoints[ID].UnLockRecipe.ToList();
            retVal.AddRange(this.registerTechPoints[ID].UnLockBuild);
            return this.registerTechPoints.ContainsKey(ID) ? retVal.ToArray() : null;
        }

        public string[] GetPreTechPoints(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return null;
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].PrePoint : null;
        }

        public bool IsAllUnlockedPreTP(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return false;
            if (this.registerTechPoints.ContainsKey(ID))
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
            if (string.IsNullOrEmpty(ID)) return null;
            return this.registerTechPoints.ContainsKey(ID) ? this.registerTechPoints[ID].ItemCost : null;
        }

        public int GetTPTimeCost(string ID)
        {
            if (string.IsNullOrEmpty(ID)) return -1;
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
        public int GetWaitingOrder(string ID)
        {
            if(this.WaitingLockSet.Contains(ID))
            {

                return this.WaitingLockQueue.IndexOf(ID) + 1;
            }
            return -1;
        }

        public void CancelWaitingOrder(string ID)
        {
            if (this.WaitingLockSet.Contains(ID))
            {
                var t = this.WaitingLockQueue.IndexOf(ID);
                if (t != -1)
                {
                    this.WaitingLockSet.Remove(ID);
                    this.WaitingLockQueue.RemoveAt(t);
                }
            }
        }

        #endregion

        #region SaveAndLoadData
        #region Struct
        /// <summary>
        /// ���ڽ����еĿƼ���
        /// </summary>
        [System.Serializable]
        public struct UnlockingTechPoint
        {
            /// <summary>
            /// �Ƽ���ID
            /// </summary>
            public string id;
            /// <summary>
            /// ��ǰ����ʱ��
            /// </summary>
            public float time;
        }
        #endregion

        #region Data
        /// <summary>
        /// �Ƽ����浵
        /// </summary>
        public const string SaveTPJsonDataPath = "TechTree";
        /// <summary>
        /// ���ڽ����еĿƼ���Ĵ浵
        /// </summary>
        public const string SaveUnlockingTPJsonDataPath = "UnlockingTechPoint";

        /// <summary>
        /// ���ڽ����еĿƼ���ļ�ʱ��
        /// ���ڼ�ʱ�����Լ��ж��Ƿ����ڽ���
        /// </summary>
        public Dictionary<string, CounterDownTimer> UnlockingTPTimers = new Dictionary<string, CounterDownTimer>();

        [ShowInInspector]
        [LabelText("�Ǵ��������д�С")]
        public int QueueSize = 5;
        public List<string> WaitingLockQueue = new List<string>();
        public HashSet<string> WaitingLockSet = new HashSet<string>();

        private void OnTimerStart(CounterDownTimer timer, string ID, bool isSave = true)
        {
            this.UnlockingTPTimers.Add(ID, timer);
            if(!this.UnlockingTechPointDict.ContainsKey(ID))
            {
                this.UnlockingTechPointDict.Add(ID, new UnlockingTechPoint() { id = ID, time = (float)timer.CurrentTime });
            }
            if (isSave)
            {
                this.SaveData(false, true);
            }
        }
        private void OnTimerEnd(CounterDownTimer timer, string id, bool isSave = true)
        {
            this.__Intenal__UnlockTechPoint(id);
            this.UnlockingTPTimers.Remove(id);
            this.UnlockingTechPointDict.Remove(id);
            this.WaitingLockQueue.RemoveAt(0);
            this.WaitingLockSet.Remove(id);
            if(this.WaitingLockQueue.Count>0)
            {
                string TopID = this.WaitingLockQueue[0] ;
                this.UnlockTechPoint(this.inventory, TopID, this.IsCheck);
            }
            
            
            if (isSave)
            {
                this.SaveData();
            }
        }

        /// <summary>
        /// ���ڽ����ĿƼ���
        /// </summary>
        [ShowInInspector]
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
                // ��ȡAB��������ݣ����д浵������ȡ�����ݵ��Ƿ��������Ϊ�浵���ݣ����´浵
                // ���������
                TechTreeSaveData saveData = GameManager.Instance.SaveManager.SaveController.GetSaveData<TechTreeSaveData>();
                if (saveData != null)
                {
                    foreach (TechPoint data in saveData.Datas)
                    {
                        // ֻ��Ҫ���� �Ƿ����
                        if (this.registerTechPoints.ContainsKey(data.ID))
                        {
                            // ��һ����������Ϊ����
                            this.registerTechPoints[data.ID].IsUnlocked = data.IsUnlocked || this.registerTechPoints[data.ID].IsUnlocked;
                        }
                    }
                }
                #region Load UnlockingTPData
                if (saveData != null)
                {
                    foreach (UnlockingTechPoint unlock in saveData.Unlocks)
                    {
                        // �޳��������ݺ󲻴��ڵĽڵ�
                        if (this.registerTechPoints.ContainsKey(unlock.id))
                        {
                            this.UnlockingTechPointDict.Add(unlock.id, unlock);
                        }
                    }
                }
                // ���´浵
                SaveData();
                #endregion

                // �ָ��浵����
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
            }, "�Ƽ�������");
            ABJAProcessor.StartLoadJsonAssetData();

            #endregion
        }
        #endregion

        #region Save
        public void SaveData(bool saveTechPoint=true, bool saveUnlock=true)
        {
            TechTreeSaveData saveData = GameManager.Instance.SaveManager.SaveController.GetSaveData<TechTreeSaveData>();
            if (saveData == null)
            {
                saveData = new TechTreeSaveData();
                GameManager.Instance.SaveManager.SaveController.AddSaveData<TechTreeSaveData>(saveData);
            }
            if (saveTechPoint)
            {
                saveData.Reset(registerTechPoints.Values.ToList());
            }
            if (saveUnlock)
            {
                saveData.Reset(UnlockingTechPointDict.Values.ToList());
            }
            GameManager.Instance.SaveManager.SaveController.SaveSaveDataFolder();
        }
        #endregion
        #endregion

        #region UnLock
        /// <summary>
        /// �ѽ�������ʹ�õ��䷽
        /// </summary>
        [ReadOnly]
        public List<string> UnlockedRecipe = new List<string>();
        /// <summary>
        /// �ѽ�������ʹ�õĽ�����
        /// </summary>
        [ReadOnly]
        public List<string> UnlockedBuild = new List<string>();

        /// <summary>
        /// �жϱ����Ĳ����Ƿ��㹻����id��Ӧ�ĿƼ���
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
                // ��������
                if (inventory.GetItemAllNum(f.id) < f.num)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// ����Item
        /// </summary>
        /// <param name="inventory"></param>
        /// <param name="ID"></param>
        private void CostItem(IInventory inventory, string ID)
        {
            // �Ƴ����ĵ���Դ
            lock (inventory)
            {
                foreach (var formula in this.GetTPItemCost(ID))
                {
                    inventory.RemoveItem(formula.id, formula.num);
                }
            }
        }

        /// <summary>
        /// �Ƿ��ܵ�����Ӧ�ĿƼ���
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CanUnlockTechPoint(IInventory inventory, string ID)
        {
            return !this.UnlockingTechPointDict.ContainsKey(ID) && this.IsAllUnlockedPreTP(ID) && this.ItemIsEnough(inventory, ID);
        }

        private IInventory inventory;
        private bool IsCheck;
        /// <summary>
        /// ����
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool UnlockTechPoint(IInventory inventory, string ID, bool IsCheck = true)
        {
            if(IsCheck && !CanUnlockTechPoint(inventory, ID))
            {
                return false;
            }
            this.inventory = inventory;
            this.IsCheck = IsCheck;

            if (!this.WaitingLockSet.Contains(ID) && this.WaitingLockQueue.Count < this.QueueSize) 
            {
                this.WaitingLockQueue.Add(ID);
                this.WaitingLockSet.Add(ID);
            }

            if (this.UnlockingTechPointDict.Count > 0)
            {
                return false;
            }
            // to-do : �ݶ�Ϊ��ǰ����
            this.CostItem(inventory, ID);

            // ��ʱ
            var tp = this.registerTechPoints[ID];
            var timer = new CounterDownTimer(tp.TimeCost);
            OnTimerStart(timer, tp.ID);
            // ���������
            timer.OnEndEvent += () =>
            {
                OnTimerEnd(timer, tp.ID);
            };
            return true;
        }

        /// <summary>
        /// ֱ�ӽ���������Ҫ������ʱ��
        /// </summary>
        /// <param name="ID"></param>
        private void __Intenal__UnlockTechPoint(string ID)
        {
            this.registerTechPoints[ID].IsUnlocked = true;

            // �����䷽
            this.UnlockedRecipe.AddRange(this.registerTechPoints[ID].UnLockRecipe);
            // ����������
            // to-do : ���Ż�
            // to-do : �ļ����಻����ID������������Build�����ݣ����Ϊ�ϳ��ӱ�<����ID, ��������>ӳ���
            // ����ʹ��ӳ������
            // �ϳ��ӱ���ExcelתJSONʱ�Զ�����ϳɱ���
            // <����ID, ��������>ӳ���Ϊ������JSON���ݱ������Ƽ���
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
        /// to-do : ���Ƴ����ģ����ܷ����ڴˣ�����ΪItemTable��CompositeTable�����ݼ�����õ�������ģ�����Ҫ���� => ������Level�и�����
        /// </summary>
        /// <returns></returns>
        void LoadResource()
        {
            LoadTechAtlas();
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
