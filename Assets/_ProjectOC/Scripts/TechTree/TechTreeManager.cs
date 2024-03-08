using System;
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
using static ProjectOC.WorkerNS.EffectManager;
using System.Runtime.Serialization;
using Sirenix.Serialization;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

namespace ProjectOC.TechTree
{
    public sealed class TechTreeManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Base
        public static TechTreeManager Instance;

        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;

        /// <summary>
        /// ��������
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }
            Instance = this;
        }

        /// <summary>
        /// ���������ʼ��
        /// </summary>
        private void Start()
        {
            // ע�� Manager
            GM.RegisterLocalManager(this);

            // ���� Item �� �ϳɱ��
            LoadResource();

            // ����Ƽ���������� �Լ� �ָ��浵
            StartCoroutine(LoadTableData());

            // ����UI����
            InitUITextContents();
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

        /// <summary>
        /// ����ĿƼ���������
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
            var tex = GM.ABResourceManager.LoadAsset<Texture2D>(TPIconTexture2DABPath, category.ToString());
            if(tex == null)
            {
                tex = GM.ABResourceManager.LoadAsset<Texture2D>(TPIconTexture2DABPath, "None");
            }
            return tex;
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
        [ReadOnly]
        public bool IsLoadOvered = false;

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
        /// ���ڽ����ĿƼ���
        /// </summary>
        public Dictionary<string, UnlockingTechPoint> UnlockingTechPointDict = new Dictionary<string, UnlockingTechPoint>();

        #endregion

        #region Load
        private IEnumerator LoadTableData()
        {
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif

            #region Load TPJsonData
            ML.Engine.ABResources.ABJsonAssetProcessor<TechPoint[]> ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<TechPoint[]>("Json/TableData", "TechPoint", (datas) =>
            {
                foreach (var data in datas)
                {
                    this.registerTechPoints.Add(data.ID, data);
                }
                // ��ȡAB��������ݣ����д浵������ȡ�����ݵ��Ƿ��������Ϊ�浵���ݣ����´浵
                // ���������
                if (System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath)))
                {
                    var str = File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath));
                    datas = JsonConvert.DeserializeObject<TechPoint[]>(str);
                    foreach (var data in datas)
                    {
                        // ֻ��Ҫ���� �Ƿ����
                        if (this.registerTechPoints.ContainsKey(data.ID))
                        {
                            // ��һ����������Ϊ����
                            this.registerTechPoints[data.ID].IsUnlocked = data.IsUnlocked || this.registerTechPoints[data.ID].IsUnlocked;
                        }
                    }
                }
            }, () => {
                return (ML.Engine.InventorySystem.ItemManager.Instance.IsLoadOvered == false || ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.IsLoadOvered == false || ML.Engine.BuildingSystem.MonoBuildingManager.Instance.IsLoadOvered == false);
            }, "�Ƽ�������");
            ABJAProcessor.StartLoadJsonAssetData();
            while(!ABJAProcessor.IsLoaded)
            {
                yield return null;
            }
            #endregion

            #region Load UnlockingTPData
            if (System.IO.File.Exists(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath)))
            {
                var str = File.ReadAllText(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath));
                UnlockingTechPoint[] datas = JsonConvert.DeserializeObject<UnlockingTechPoint[]>(str);

                foreach (var data in datas)
                {
                    // �޳��������ݺ󲻴��ڵĽڵ�
                    if (this.registerTechPoints.ContainsKey(data.id))
                    {
                        this.UnlockingTechPointDict.Add(data.id, data);
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
            Debug.Log("LoadTableData cost time: " + (Time.realtimeSinceStartup - startT));
            Debug.Log($"�洢·��: {Application.persistentDataPath}");
#endif
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

        [Button("ɾ���浵")]
        public void DeleteData()
        {
            System.IO.File.Delete(System.IO.Path.Combine(Application.persistentDataPath, SaveTPJsonDataPath));
            System.IO.File.Delete(System.IO.Path.Combine(Application.persistentDataPath, SaveUnlockingTPJsonDataPath));
        }

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

                MonoBuildingManager.Instance.BM.RegisterBPartPrefab(MonoBuildingManager.Instance.LoadedBPart[new ML.Engine.BuildingSystem.BuildingPart.BuildingPartClassification(str)]);
            }    

        }
        #endregion

        #region TextContent
        public Dictionary<string, TextTip> CategoryDict = new Dictionary<string, TextTip>();
        public TPPanel TPPanelTextContent_Main => ABJAProcessor_TPPanel.Datas;

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
        public static ML.Engine.ABResources.ABJsonAssetProcessor<TPPanel> ABJAProcessor_TPPanel;

        public void InitUITextContents()
        {
            if (ABJAProcessor_TPPanel == null)
            {
                ABJAProcessor_TPPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<TPPanel>("Json/TextContent/TechTree", "TechPointPanel", (datas) =>
                {
                    foreach (var tip in datas.category)
                    {
                        this.CategoryDict.Add(tip.name, tip);
                    }
                }, null, "�Ƽ���UIPanel");
                ABJAProcessor_TPPanel.StartLoadJsonAssetData();
            }
        }
        #endregion

        #region to-delete
        /// <summary>
        /// to-do : ���Ƴ����ģ����ܷ����ڴˣ�����ΪItemTable��CompositeTable�����ݼ�����õ�������ģ�����Ҫ���� => ������Level�и�����
        /// </summary>
        /// <returns></returns>
        void LoadResource()
        {
            ML.Engine.InventorySystem.ItemManager.Instance.LoadTableData();
            ML.Engine.InventorySystem.CompositeSystem.CompositeManager.Instance.LoadTableData();
            this.LoadTechAtlas();
        }

        [Button("���ɲ����ļ�")]
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
                        tp.Name.Chinese = "����" + tp.ID;
                        tp.Name.English = "Test" + tp.ID;
                        tp.Grid = new int[2] { row, col };
                        tp.Icon = row.ToString() + "_" + col.ToString();
                        tp.IsUnlocked = false;
                        tp.Description.Chinese = "����";
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
            Debug.Log("���·��: " + Application.streamingAssetsPath + "/../../../t.json");
        }
        #endregion

        private void LoadTechAtlas()
        {
            AssetBundle ab = null;
            AssetBundleCreateRequest request = null;
            request = GM.ABResourceManager
            .LoadLocalABAsync(TPIconTexture2DABPath, (asop) =>
            {
                if (request != null)
                {
                    ab = request.assetBundle;
                }
                AssetBundleRequest request2 = ab.LoadAssetAsync<SpriteAtlas>("SA_TechPoint_UI");
                if (request2 != null)
                {
                    techAtlas = request2.asset as SpriteAtlas;
                }
            },out ab);
        }
        
    }

}
