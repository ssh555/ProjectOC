using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem.BuildingPart;
using System.Linq;
using UnityEngine.U2D;
using ML.Engine.Manager;

namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct BuildingTableData
    {
        public string id;
        public int sort;
        public TextContent.TextContent name;
        public string icon;
        public string category1;
        public string category2;
        public string category3;
        public string category4;
        public string actorID;
        public List<InventorySystem.Formula> raw;
        public string upgradeID;
        public TextContent.TextContent ItemDescription;
        public TextContent.TextContent EffectDescription;
        public string GetClassificationString()
        {
            return $"{category1}_{category2}_{category3}_{category4}";
        }

        public BuildingPartClassification GetClassification()
        {
            return new BuildingPartClassification(GetClassificationString());
        }
    }

    [System.Serializable]
    public struct FurnitureThemeTableData
    {
        public string ID;
        public int Sort;
        public TextContent.TextContent Name;
        public string Icon;
        public List<string> BuildID;
    }

    [LabelText("建造模式")]
    public enum BuildingMode
    {
        [LabelText("禁用建造")]
        None,
        [LabelText("启用交互")]
        Interact,
        [LabelText("新建放置")]
        Place,
        [LabelText("编辑移动")]
        Edit,
        [LabelText("选中销毁")]
        Destroy,
    }

    /// <summary>
    /// 复制的材质
    /// </summary>
    public struct BuildingCopiedMaterial
    {
        /// <summary>
        /// RootGameObject.Renderer.Mat -> 可以为null
        /// </summary>
        public Material[] ParentMat;

        /// <summary>
        /// ChildGameObject.Renderer.Mat
        /// <ChildIndex, Material>
        /// </summary>
        public Dictionary<int, Material[]> ChildrenMat;

        public override bool Equals(object obj)
        {
            return this == (BuildingCopiedMaterial)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(BuildingCopiedMaterial A, BuildingCopiedMaterial B)
        {
            if(A.ParentMat != null && B.ParentMat != null)
            {
                foreach(var ap in A.ParentMat)
                {
                    foreach (var bp in B.ParentMat)
                    {
                        if(ap.name != bp.name)
                        {
                            return false;
                        }
                    }
                }
                if (A.ChildrenMat != null && B.ChildrenMat != null)
                {
                    foreach (var ap in A.ChildrenMat)
                    {
                        if(B.ChildrenMat.ContainsKey(ap.Key))
                        {
                            foreach(var t in ap.Value)
                            {
                                foreach (var bp in B.ChildrenMat[ap.Key])
                                {
                                    if (t.name != bp.name)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else if(A.ChildrenMat == null && B.ChildrenMat== null)
                {
                    return true;
                }
            }
            else if(A.ParentMat == null && B.ParentMat == null)
            {
                if (A.ChildrenMat != null && B.ChildrenMat != null)
                {
                    foreach (var ap in A.ChildrenMat)
                    {
                        if (B.ChildrenMat.ContainsKey(ap.Key))
                        {
                            foreach (var t in ap.Value)
                            {
                                foreach (var bp in B.ChildrenMat[ap.Key])
                                {
                                    if (t.name != bp.name)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else if (A.ChildrenMat == null && B.ChildrenMat == null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool operator !=(BuildingCopiedMaterial A, BuildingCopiedMaterial B)
        {
            return !(A == B);
        }
    }

    [System.Serializable]
    public sealed class BuildingManager : Manager.LocalManager.ILocalManager
    {
        #region
        private static BuildingManager instance = null;
        public static BuildingManager Instance
        {
            get
            {
                if(instance == null && Manager.GameManager.Instance != null)
                {
                    instance = Manager.GameManager.Instance.GetLocalManager<BuildingManager>();
                    if(instance != null)
                    {
                        instance.LoadTableData();
                    }

                }
                return instance;
            }
        }

        public void OnRegister()
        {
            instance = this;
            LoadTableData();
            LoadItemAtlas();
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<Material>(_ABMatName).Completed += (handle) =>
            {
                _buildingMaterial = handle.Result;
            };

            // 创建一个可视化的球体
            VisualSocket = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            VisualSocket.GetComponent<Renderer>().material.color = UnityEngine.Color.blue;
            VisualSocket.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }


        public void OnUnregister()
        {
            if (instance == this)
            {
                instance = null;
                Manager.GameManager.Instance.ABResourceManager.Release(_buildingMaterial);
            }
            Manager.GameManager.Instance.ABResourceManager.Release(buildIconAtlas);
            GameManager.DestroyObj(VisualSocket);
        }


        [HideInInspector, SerializeField]
        private BuildingMode mode = BuildingMode.None;
        /// <summary>
        /// 当前处于的建造模式
        /// </summary>
        [LabelText("建造模式"), ShowInInspector]
        public BuildingMode Mode
        {
            get => mode;
            set
            {
                this.OnModeChanged?.Invoke(mode, value);
                var last = mode;
                mode = value;
                bool isEnabled = mode != BuildingMode.None;
                foreach (var socket in this.BuildingSocketList)
                {
                    socket.enabled = isEnabled;
                }
                foreach (var area in this.BuildingAreaList)
                {
                    area.enabled = isEnabled;
                }

#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    if (mode == BuildingMode.None && last != BuildingMode.None)
                    {
                        BInput.Disable();
                    }
                    else if (mode != BuildingMode.None && last == BuildingMode.None)
                    {
                        BInput.Enable();
                        BInput.Build.Enable();
                        BInput.BuildKeyCom.Disable();
                        BInput.BuildSelection.Disable();
                        BInput.BuildPlaceMode.Disable();
                        BInput.BuildingAppearance.Disable();
                    }
                }
#else
                    if (mode == BuildingMode.None && last != BuildingMode.None)
                    {
                        BInput.Disable();
                    }
                    else if (mode != BuildingMode.None && last == BuildingMode.None)
                    {
                        BInput.Enable();
                        BInput.Build.Enable();
                        BInput.BuildKeyCom.Disable();
                        BInput.BuildSelection.Disable();
                        BInput.BuildPlaceMode.Disable();
                        BInput.BuildingAppearance.Disable();
                    }
#endif
            }
        }

        /// <summary>
        /// 建造模式改变时调用
        /// PreMode, CurMode
        /// </summary>
        public event System.Action<BuildingMode, BuildingMode> OnModeChanged;
        [ShowInInspector]
        private BuildingPlacer.BuildingPlacer placer;
        /// <summary>
        /// 控制的Placer
        /// </summary>
        public BuildingPlacer.BuildingPlacer Placer
        {
            get
            {
                if(this.placer == null)
                {
                    // to-do
                    placer = UnityEngine.Object.FindFirstObjectByType<BuildingPlacer.BuildingPlacer>();
                    this.OnModeChanged += placer.OnModeChanged;
                }
                return placer;
            }
        }

        // to-do : EditMode使用PlaceMode的按键输入 -> PlaceMode全部启用,EditMode禁用: ChangeOutLook|ChangeStyle|SwitchFrame_PreHold|KeyCom
        private Input.BuildingInput binput = null;
        /// <summary>
        /// 建造系统的统一输入
        /// </summary>
        public Input.BuildingInput BInput
        {
            get
            {
                if(this.binput == null)
                {
                    this.binput = new Input.BuildingInput();
                }
                return this.binput;
            }
        }

        public List<BuildingSocket.BuildingSocket> BuildingSocketList = new List<BuildingSocket.BuildingSocket>();
        public List<BuildingArea.BuildingArea> BuildingAreaList = new List<BuildingArea.BuildingArea>();
        #endregion

        #region BPartPrefab
        private Dictionary<BuildingPartClassification, IBuildingPart> registeredBPart = new Dictionary<BuildingPartClassification, IBuildingPart>();

        /// <summary>
        /// 当前允许放置生成的建筑物数量
        /// </summary>
        /// <returns></returns>
        public int GetRegisterBPartCount()
        {
            return registeredBPart.Count;
        }
        /// <summary>
        /// 是否是合法的建筑物ID -> 必须是已经注册的建筑物，未注册但是依旧合法的建筑物ID仍然返回fasle
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidBPartID(string id)
        {
            BuildingPartClassification classification = new BuildingPartClassification(id);
            return this.registeredBPart.ContainsKey(classification);
        }


        /// <summary>
        /// 将BPart加入管理队列
        /// </summary>
        /// <param name="BPart"></param>
        public void RegisterBPartPrefab(IBuildingPart BPart)
        {
            if (BPart == null || registeredBPart.ContainsKey(BPart.Classification))
            {
                return;
            }


            this.AddBPartPrefabOnStyle(BPart);
            this.AddBPartPrefabOnHeight(BPart);

            this.registeredBPart.Add(BPart.Classification, BPart);

            //家具类别
            if (BPart.Classification.Category1 == BuildingCategory1.Furniture)
            {
                if (!FurnitureCategoryDic.ContainsKey(BPart.Classification.Category2))
                {
                    FurnitureCategoryDic.Add(BPart.Classification.Category2, new List<IBuildingPart>());
                }
                FurnitureCategoryDic[BPart.Classification.Category2].Add(BPart);
            }
        }

        /// <summary>
        /// 将BPart从管理队列移除
        /// </summary>
        /// <param name="BPart"></param>
        public void UnregisterBPartPrefab(IBuildingPart BPart)
        {
            if (BPart == null || !registeredBPart.ContainsKey(BPart.Classification))
            {
                return;
            }

            this.RemoveBPartPrefabOnStyle(BPart);
            this.RemoveBPartPrefabOnHeight(BPart);

            this.registeredBPart.Remove(BPart.Classification);
        }

        /// <summary>
        /// 注销全部
        /// </summary>
        public void UnregisterAllBPartPrefab()
        {
            this.BPartClassificationOnStyle.Clear();
            this.BPartClassificationOnHeight.Clear();
        }

        /// <summary>
        /// 获取一个新的唯一的建筑物实例ID
        /// </summary>
        /// <returns></returns>
        public string GetOneNewBPartInstanceID()
        {
            return ML.Engine.Utility.OSTime.OSCurMilliSeconedTime.ToString();
        }
        
        /// <summary>
        /// 获得一个指定二级分类的 BPartInstance
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public IBuildingPart GetOneBPartInstance(BuildingCategory1 Category, BuildingCategory2 Type)
        {
            if(this.BPartClassificationOnStyle.ContainsKey(Category) && this.BPartClassificationOnStyle[Category].ContainsKey(Type) && this.BPartClassificationOnStyle[Category][Type].Count > 0)
            {
                return GameObject.Instantiate<GameObject>(BPartClassificationOnStyle[Category][Type].First().Value.PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public string GetOneBPartBuildingPartClassificationString(BuildingCategory1 Category, BuildingCategory2 Type)
        {
            if (this.BPartClassificationOnStyle.ContainsKey(Category) && this.BPartClassificationOnStyle[Category].ContainsKey(Type) && this.BPartClassificationOnStyle[Category][Type].Count > 0)
            {
                IBuildingPart buildingPart = GameObject.Instantiate<GameObject>(BPartClassificationOnStyle[Category][Type].First().Value.PeekFront().gameObject).GetComponent<IBuildingPart>();
                string ts = buildingPart.Classification.ToString();
                GameManager.DestroyObj(buildingPart.gameObject);
                return ts;
            }
            return "";
        }

        public IBuildingPart GetOneBPartInstance(BuildingPartClassification classification)
        {
            if(this.registeredBPart.ContainsKey(classification))
            {
                return GameObject.Instantiate<GameObject>(this.registeredBPart[classification].gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart GetOneBPartInstance(string classification)
        {
            return GetOneBPartInstance(new BuildingPartClassification(classification));
        }

        public GameObject GetOneBPartInstanceGO(BuildingPartClassification classification)
        {
            if (this.registeredBPart.ContainsKey(classification))
            {
                return GameObject.Instantiate<GameObject>(this.registeredBPart[classification].gameObject);
            }
            return null;
        }

        public GameObject GetOneBPartInstanceGO(string classification)
        {
            return GetOneBPartInstanceGO(new BuildingPartClassification(classification));
        }
        /// <summary>
        /// 若BPart可以复制，则获得一个复制的实例
        /// 若不可以复制，则直接返回BPart
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public IBuildingPart GetOneBPartCopyInstance(IBuildingPart BPart)
        {
            if(BPart.GetCanCopy())
            {
                BPart.Mode = BuildingMode.None;
                return GameObject.Instantiate<GameObject>(BPart.gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }


        public BuildingCategory1[] GetRegisteredCategory()
        {
            return this.BPartClassificationOnStyle.Keys.ToArray();
        }

        public BuildingCategory2[] GetRegisteredType()
        {
            return this.BPartClassificationOnStyle.Values.SelectMany(innerDict => innerDict.Keys).ToArray();
        }
        #endregion

        #region 基于BuildingStyle的三级分类
        /// <summary>
        /// 基于BuildingStyle的三级分类
        /// </summary>
        [ShowInInspector]
        private Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<BuildingCategory3, Deque<IBuildingPart>>>> BPartClassificationOnStyle = new Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<BuildingCategory3, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if(BPart == null)
            {
                return;
            }
            // 不存在对应队列
            if (!this.BPartClassificationOnStyle.ContainsKey(BPart.Classification.Category1) || !this.BPartClassificationOnStyle[BPart.Classification.Category1].ContainsKey(BPart.Classification.Category2) || !this.BPartClassificationOnStyle[BPart.Classification.Category1][BPart.Classification.Category2].ContainsKey(BPart.Classification.Category3))
            {
                var onType = new Dictionary<BuildingCategory2, Dictionary<BuildingCategory3, Deque<IBuildingPart>>>();
                var onStyle = new Dictionary<BuildingCategory3, Deque<IBuildingPart>>();
                var Deque = new Deque<IBuildingPart>();

                if (this.BPartClassificationOnStyle.TryAdd(BPart.Classification.Category1, onType))
                {
                    if (onType.TryAdd(BPart.Classification.Category2, onStyle))
                    {
                        onStyle.TryAdd(BPart.Classification.Category3, Deque);
                    }
                }
                else
                {
                    if (this.BPartClassificationOnStyle[BPart.Classification.Category1].TryAdd(BPart.Classification.Category2, onStyle))
                    {
                        onStyle.TryAdd(BPart.Classification.Category3, Deque);
                    }
                    else
                    {
                        this.BPartClassificationOnStyle[BPart.Classification.Category1][BPart.Classification.Category2].TryAdd(BPart.Classification.Category3, Deque);
                    }
                }
            }

            this.BPartClassificationOnStyle[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category3].EnqueueBack(BPart);
        }

        private void RemoveBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if (BPart == null || this.GetBPartPeekInstanceOnStyle(BPart) == null)
            {
                return;
            }
            var Deque = this.BPartClassificationOnStyle[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category3];
            for (int i = Deque.Count; i > 0; --i)
            {
                if(Deque.PeekFront().Classification.Category4 == BPart.Classification.Category4)
                {
                    Deque.DequeueFront();
                    break;
                }
                else
                {
                    Deque.EnqueueBack(Deque.DequeueFront());
                }
            }
        }

        /// <summary>
        /// 获取指定Style类型的BPart队列的队首值的一个实例IBuilding.GameObject
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IBuildingPart GetBPartPeekInstanceOnStyle(BuildingCategory1 category, BuildingCategory2 type, BuildingCategory3 style)
        {
            if(this.BPartClassificationOnStyle.ContainsKey(category) && this.BPartClassificationOnStyle[category].ContainsKey(type) && this.BPartClassificationOnStyle[category][type].ContainsKey(style))
            {
                return GameObject.Instantiate<GameObject>(this.BPartClassificationOnStyle[category][type][style].PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart GetBPartPeekInstanceOnStyle(IBuildingPart BPart)
        {
            return GetBPartPeekInstanceOnStyle(BPart.Classification.Category1, BPart.Classification.Category2, BPart.Classification.Category3);
        }

        /// <summary>
        /// 轮询指定Style类型的BPart队列的队首值
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IBuildingPart PollingBPartPeekInstanceOnStyle(BuildingCategory1 category, BuildingCategory2 type, BuildingCategory3 style, bool isForward)
        {
            if (this.BPartClassificationOnStyle.ContainsKey(category) && this.BPartClassificationOnStyle[category].ContainsKey(type) && this.BPartClassificationOnStyle[category][type].ContainsKey(style))
            {
                if(isForward)
                {
                    var BPartQueue = this.BPartClassificationOnStyle[category][type][style];
                    var ret = BPartQueue.PeekFront();
                    if (BPartQueue.Count == 1)
                    {
                        return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                    }
                    BPartQueue.DequeueFront();
                    BPartQueue.EnqueueBack(ret);
                    ret = BPartQueue.PeekFront();
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
                else
                {
                    var BPartQueue = this.BPartClassificationOnStyle[category][type][style];
                    var ret = BPartQueue.PeekBack();
                    if (BPartQueue.Count == 1)
                    {
                        return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                    }
                    BPartQueue.DequeueBack();
                    BPartQueue.EnqueueFront(ret);
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
            }
            return null;
        }

        public IBuildingPart PollingBPartPeekInstanceOnStyle(IBuildingPart BPart, bool isForward)
        {
            return PollingBPartPeekInstanceOnStyle(BPart.Classification.Category1, BPart.Classification.Category2, BPart.Classification.Category3, isForward);
        }
        
        public int GetBPartPrefabCountOnStyle(BuildingCategory1 category, BuildingCategory2 type, BuildingCategory3 style)
        {
            if (this.BPartClassificationOnStyle.ContainsKey(category) && this.BPartClassificationOnStyle[category].ContainsKey(type) && this.BPartClassificationOnStyle[category][type].ContainsKey(style))
            {
                var BPartQueue = this.BPartClassificationOnStyle[category][type][style];

                return BPartQueue.Count;
            }
            return 0;
        }
        public int GetBPartPrefabCountOnStyle(IBuildingPart BPart)
        {
            return GetBPartPrefabCountOnStyle(BPart.Classification.Category1, BPart.Classification.Category2, BPart.Classification.Category3);
        }
        public short[] GetAllHeightByBPartStyle(IBuildingPart BPart)
        {
            var bparts = BPartClassificationOnStyle[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category3].ToArray();
            short[] heights = new short[bparts.Length];
            for(int i = 0; i < bparts.Length; ++i)
            {
                heights[i] = bparts[i].Classification.Category4;
            }
            return heights;
        }

        #endregion

        #region 基于BuildingHeight的三级分类
        /// <summary>
        /// 基于BuildingHeight的三级分类
        /// </summary>
        [ShowInInspector]
        private Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<short, Deque<IBuildingPart>>>> BPartClassificationOnHeight = new Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<short, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }
            // 不存在对应队列
            if (!this.BPartClassificationOnHeight.ContainsKey(BPart.Classification.Category1) || !this.BPartClassificationOnHeight[BPart.Classification.Category1].ContainsKey(BPart.Classification.Category2) || !this.BPartClassificationOnHeight[BPart.Classification.Category1][BPart.Classification.Category2].ContainsKey(BPart.Classification.Category4))
            {
                var onType = new Dictionary<BuildingCategory2, Dictionary<short, Deque<IBuildingPart>>>();
                var onHeight = new Dictionary<short, Deque<IBuildingPart>>();
                var Deque = new Deque<IBuildingPart>();

                if(this.BPartClassificationOnHeight.TryAdd(BPart.Classification.Category1, onType))
                {
                    if(onType.TryAdd(BPart.Classification.Category2, onHeight))
                    {
                        onHeight.TryAdd(BPart.Classification.Category4, Deque);
                    }
                }
                else
                {
                    if(this.BPartClassificationOnHeight[BPart.Classification.Category1].TryAdd(BPart.Classification.Category2, onHeight))
                    {
                        onHeight.TryAdd(BPart.Classification.Category4, Deque);
                    }
                    else
                    {
                        this.BPartClassificationOnHeight[BPart.Classification.Category1][BPart.Classification.Category2].TryAdd(BPart.Classification.Category4, Deque);
                    }
                }
            }

            this.BPartClassificationOnHeight[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category4].EnqueueBack(BPart);
        }

        private void RemoveBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null || this.GetBPartPeekInstanceOnHeight(BPart) == null)
            {
                return;
            }
            var Deque = this.BPartClassificationOnHeight[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category4];
            for (int i = Deque.Count; i > 0; --i)
            {
                if (Deque.PeekFront().Classification.Category3 == BPart.Classification.Category3)
                {
                    Deque.DequeueFront();
                    break;
                }
                else
                {
                    Deque.EnqueueBack(Deque.DequeueFront());
                }
            }
        }


        /// <summary>
        /// 获取指定Height的BPart队列的队首值的一个实例值
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IBuildingPart GetBPartPeekInstanceOnHeight(BuildingCategory1 category, BuildingCategory2 type, short height)
        {
            if (this.BPartClassificationOnHeight.ContainsKey(category) && this.BPartClassificationOnHeight[category].ContainsKey(type) && this.BPartClassificationOnHeight[category][type].ContainsKey(height))
            {
                return GameObject.Instantiate<GameObject>(this.BPartClassificationOnHeight[category][type][height].PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart GetBPartPeekInstanceOnHeight(IBuildingPart BPart)
        {
            return GetBPartPeekInstanceOnHeight(BPart.Classification.Category1, BPart.Classification.Category2, BPart.Classification.Category4);
        }

        /// <summary>
        /// 轮询指定Height的BPart队列的队首值
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IBuildingPart PollingBPartPeekInstanceOnHeight(BuildingCategory1 category, BuildingCategory2 type, short height, bool isForward)
        {
            if (this.BPartClassificationOnHeight.ContainsKey(category) && this.BPartClassificationOnHeight[category].ContainsKey(type) && this.BPartClassificationOnHeight[category][type].ContainsKey(height))
            {
                if(isForward)
                {
                    var BPartQueue = this.BPartClassificationOnHeight[category][type][height];

                    var ret = BPartQueue.PeekFront();
                    if (BPartQueue.Count == 1)
                    {
                        return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                    }
                    BPartQueue.DequeueFront();
                    BPartQueue.EnqueueBack(ret);
                    ret = BPartQueue.PeekFront();
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
                else
                {
                    var BPartQueue = this.BPartClassificationOnHeight[category][type][height];
                    var ret = BPartQueue.PeekBack();
                    if (BPartQueue.Count == 1)
                    {
                        return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                    }
                    BPartQueue.DequeueBack();
                    BPartQueue.EnqueueFront(ret);
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
            }
            return null;
        }

        public IBuildingPart PollingBPartPeekInstanceOnHeight(IBuildingPart BPart, bool isForward)
        {
            return PollingBPartPeekInstanceOnHeight(BPart.Classification.Category1, BPart.Classification.Category2, BPart.Classification.Category4, isForward);
        }

        public int GetBPartPrefabCountOnHeight(BuildingCategory1 category, BuildingCategory2 type, short height)
        {
            if (this.BPartClassificationOnHeight.ContainsKey(category) && this.BPartClassificationOnHeight[category].ContainsKey(type) && this.BPartClassificationOnHeight[category][type].ContainsKey(height))
            {
                var BPartQueue = this.BPartClassificationOnHeight[category][type][height];

                return BPartQueue.Count;
            }
            return 0;
        }
        public int GetBPartPrefabCountOnHeight(IBuildingPart BPart)
        {
            return GetBPartPrefabCountOnHeight(BPart.Classification.Category1, BPart.Classification.Category2, BPart.Classification.Category4);
        }

        public BuildingCategory3[] GetAllStyleByBPartHeight(IBuildingPart BPart)
        {
            var bparts = BPartClassificationOnHeight[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category4].ToArray();
            BuildingCategory3[] styles = new BuildingCategory3[bparts.Length];
            for (int i = 0; i < bparts.Length; ++i)
            {
                styles[i] = bparts[i].Classification.Category3;
            }
            return styles;
        }

        public List<IBuildingPart> GetAllStyleIBuildingPartOnHeight1(IBuildingPart BPart)
        {
            return BPartClassificationOnHeight[BPart.Classification.Category1][BPart.Classification.Category2][1].ToArray().ToList();
        }
        public List<IBuildingPart> GetAllHeightIBuildingPartOnStyle(IBuildingPart BPart)
        {
            return BPartClassificationOnStyle[BPart.Classification.Category1][BPart.Classification.Category2][BPart.Classification.Category3].ToArray().ToList();
        }

        #endregion

        #region Material
        // to-do :to-change
        [SerializeField]
        private const string _ABMatName = "Materials/Other/Mat_Common_Transparent.mat";

        /// <summary>
        /// 建造模式下所用的Material (Place, Edit, Destroy, Interact)
        /// </summary>
        [SerializeField, HideInInspector]
        private Material _buildingMaterial = null;
        private Material buildingMaterial
        {
            get
            {
                return this._buildingMaterial;
            }
        }

        /// <summary>
        /// None 返回为 null
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        public Material GetBuldingMat(BuildingMode mode)
        {
            if(mode == BuildingMode.Interact)
            {
                this.buildingMaterial.color = interactColor;
            }
            else if (mode == BuildingMode.Edit)
            {
                this.buildingMaterial.color = editColor;
            }
            else if(mode == BuildingMode.Place)
            {
                this.buildingMaterial.color = placeColor;
            }
            else if(mode == BuildingMode.Destroy)
            {
                this.buildingMaterial.color = destroyColor;
            }
            else
            {
                return null;
            }
            return this.buildingMaterial;
        }

        /// <summary>
        /// PlaceMode.Color
        /// </summary>
        [SerializeField]
        private Color placeColor;
        /// <summary>
        /// EditMode.Color
        /// </summary>
        [SerializeField]
        private Color editColor;
        /// <summary>
        /// DestroyMode.Color
        /// </summary>
        [SerializeField]
        private Color destroyColor;
        /// <summary>
        /// InteractMode.Color
        /// </summary>
        [SerializeField]
        private Color interactColor;

        #region 材质拷贝粘贴
        /// <summary>
        /// 存储全局复制的材质
        /// </summary>
        private Dictionary<BuildingPartClassification, BuildingCopiedMaterial> CopyMatDict = new
             Dictionary<BuildingPartClassification, BuildingCopiedMaterial>();

        /// <summary>
        /// 获取与BPart同类型的当前拷贝的材质，若不存在，则返回 default
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public BuildingCopiedMaterial GetBPartMaterial(IBuildingPart BPart)
        {
            if (this.CopyMatDict.ContainsKey(BPart.Classification))
            {
                return this.CopyMatDict[BPart.Classification];
            }
            return default;
        }

        /// <summary>
        /// 复制并记录
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public BuildingCopiedMaterial CopyBPartMaterial(IBuildingPart BPart)
        {
            var mat = BPart.GetCopiedMaterial();
            // 存在对应项
            if (this.CopyMatDict.ContainsKey(BPart.Classification))
            {
                this.CopyMatDict[BPart.Classification] = mat;
            }
            else
            {
                this.CopyMatDict.Add(BPart.Classification, mat);
            }
            return mat;
        }

        /// <summary>
        /// 将BPart同类型拷贝的材质赋值给BPart
        /// 返回值为是否成功,即是否有拷贝值
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public bool PasteBPartMaterial(IBuildingPart BPart)
        {
            if (this.CopyMatDict.ContainsKey(BPart.Classification))
            {
                BPart.SetCopiedMaterial(this.CopyMatDict[BPart.Classification]);
            }
            return false;
        }
        #endregion

        #endregion

        #region 建筑升级
        public IBuildingUpgrade Upgrade(IBuildingUpgrade build)
        {
            if (build != null && build.CanUpgrade())
            {
                IBuildingPart upgradeBP = GetOneBPartInstance(GetUpgradeCID(build.Classification.ToString()));
                if (upgradeBP is IBuildingUpgrade upgrade)
                {
                    upgrade.OnUpgrade(build);
                    return upgrade;
                }
            }
            return null;
        }
        #endregion

        #region BuildIcon
        private SpriteAtlas buildIconAtlas;
        private void LoadItemAtlas()
        {
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<SpriteAtlas>("SA_UI_BuildIcon").Completed += (handle) =>
            {
                buildIconAtlas = handle.Result;
            };
        }


        public Sprite GetBuildIcon(string id)
        {
            if (this.BPartTableDictOnID.ContainsKey(id))
            {
                return this.buildIconAtlas.GetSprite(BPartTableDictOnID[id].icon);
            }
            if (this.BPartTableDictOnClass.ContainsKey(id))
            {
                return this.buildIconAtlas.GetSprite(BPartTableDictOnClass[id].icon);
            }
            return this.buildIconAtlas.GetSprite(id);
        }

        #endregion

        #region 读表
        public const string Tex2DABPath = "UI/BuildingSystem/Texture2D/type";
        // Materials/Character/Player

        public Dictionary<string, BuildingTableData> BPartTableDictOnID = new Dictionary<string, BuildingTableData>();
        [ShowInInspector]
        public Dictionary<string, BuildingTableData> BPartTableDictOnClass = new Dictionary<string, BuildingTableData>();

        [ShowInInspector]
        public Dictionary<string, FurnitureThemeTableData> FurnitureThemeTableDataDic = new Dictionary<string, FurnitureThemeTableData>();

        private Dictionary<BuildingCategory2, List<IBuildingPart>> FurnitureCategoryDic = new Dictionary<BuildingCategory2, List<IBuildingPart>>();
        public bool IsLoadOvered => ABJAProcessorBuildingTableData != null && ABJAProcessorBuildingTableData.IsLoaded;

        public ML.Engine.ABResources.ABJsonAssetProcessor<BuildingTableData[]> ABJAProcessorBuildingTableData;
        public ML.Engine.ABResources.ABJsonAssetProcessor<FurnitureThemeTableData[]> ABJAProcessorFurnitureThemeTableData;

        public void LoadTableData()
        {
            ABJAProcessorBuildingTableData = new ML.Engine.ABResources.ABJsonAssetProcessor<BuildingTableData[]>("OCTableData", "Building", (datas) =>
            {
                foreach (var data in datas)
                {
                    BPartTableDictOnID.Add(data.id, data);
                    BPartTableDictOnClass.Add(data.GetClassificationString(), data);
                }
            }, "建筑表数据");
            ABJAProcessorBuildingTableData.StartLoadJsonAssetData();

            ABJAProcessorFurnitureThemeTableData = new ML.Engine.ABResources.ABJsonAssetProcessor<FurnitureThemeTableData[]>("OCTableData", "FurnitureTheme", (datas) =>
            {
                foreach (var data in datas)
                {
                    FurnitureThemeTableDataDic.Add(data.ID, data);
                }
            }, "家具主题表数据");
            ABJAProcessorFurnitureThemeTableData.StartLoadJsonAssetData();

            // BuildIcon

        }

        public string GetID(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].id;
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CID">4级分类ID</param>
        /// <returns></returns>
        public string GetActorID(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].actorID;
            }
            return null;
        }

        public string GetName(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].name;
            }
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnID.ContainsKey(CID))
            {
                return BPartTableDictOnID[CID].name;
            }
            return null;
        }

        

        public List<InventorySystem.Formula> GetRaw(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].raw;
            }
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnID.ContainsKey(CID))
            {
                return BPartTableDictOnID[CID].raw;
            }
            return null;
        }

        public List<InventorySystem.Formula> GetRawAll(string CID)
        {
            List<InventorySystem.Formula> result = new List<InventorySystem.Formula>();
            /*string[] cids = CID.Split('_');
            if (cids.Length == 4)
            {
                int level = int.Parse(cids[3]);
                for (int i = 1; i <= level; i++)
                {
                    string cid = cids[0] + "_" + cids[1] + "_" + cids[2] + "_" + i.ToString();
                    var raws = GetRaw(cid);
                    if (raws != null)
                    {
                        result.AddRange(raws);
                    }
                }
                
            }*/
            var raws = GetRaw(CID);
            if (raws != null)
            {
                result.AddRange(raws);
            }
            return result;
        }

        public string GetUpgradeID(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].upgradeID;
            }
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnID.ContainsKey(CID))
            {
                return BPartTableDictOnID[CID].upgradeID;
            }
            return null;
        }

        public int GetSort(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].sort;
            }
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnID.ContainsKey(CID))
            {
                return BPartTableDictOnID[CID].sort;
            }
            return -1;
        }

        public string GetUpgradeCID(string CID)
        {
            string upgradeID = GetUpgradeID(CID);
            if (upgradeID != null)
            {
                return GetClassification(upgradeID);
            }
            return null;
        }

        public List<InventorySystem.Formula> GetUpgradeRaw(string CID)
        {
            List<InventorySystem.Formula> result = new List<InventorySystem.Formula>();
            List<InventorySystem.Formula> formulas = GetRaw(GetUpgradeCID(CID));
            if (formulas != null)
            {
                result.AddRange(formulas);
            }
            return result;
        }

        public string GetClassification(string ID)
        {
            if (!string.IsNullOrEmpty(ID) && BPartTableDictOnID.ContainsKey(ID))
            {
                return BPartTableDictOnID[ID].GetClassificationString();
            }
            return null;
        }

        public List<IBuildingPart> GetFurnitureIBuildingParts(BuildingCategory2 buildingCategory2)
        {
            if (FurnitureCategoryDic.ContainsKey(buildingCategory2))
            {
                return FurnitureCategoryDic[buildingCategory2];
            }
            return new List<IBuildingPart>();
        }
        public List<string> GetThemeContainBuildings(string ID)
        {
            if (!string.IsNullOrEmpty(ID) && FurnitureThemeTableDataDic.ContainsKey(ID))
            {
                return FurnitureThemeTableDataDic[ID].BuildID;
            }
            return null;
        }

        //仅获取家具建筑的icon
        public string GetFurtureBuildingIcon(string classification)
        {
            if (this.BPartTableDictOnClass.ContainsKey(classification))
            {
                return this.BPartTableDictOnClass[classification].icon;
            }
            return null;
        }

        public string GetItemDescription(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].ItemDescription;
            }
            return "";
        }

        public string GetEffectDescription(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].EffectDescription;
            }
            return "";
        }



        #endregion

        #region 已建造建筑存储

        [ShowInInspector]
        private Dictionary<BuildingCategory2, List<IBuildingPart>> buildingInstanceDic = new Dictionary<BuildingCategory2, List<IBuildingPart>>();

        public void AddBuildingInstance(ML.Engine.BuildingSystem.BuildingPart.BuildingPart buildingInstance)
        {
            BuildingCategory2 buildingType = buildingInstance.Classification.Category2;
            if (!buildingInstanceDic.ContainsKey(buildingType))
            {
                buildingInstanceDic.Add(buildingType,new List<IBuildingPart>());
            }
            buildingInstanceDic[buildingType].Add(buildingInstance);
        }

        public void RemoveBuildingInstance(ML.Engine.BuildingSystem.BuildingPart.BuildingPart buildingInstance)
        {
            BuildingCategory2 buildingType = buildingInstance.Classification.Category2;
            buildingInstanceDic[buildingType].Remove(buildingInstance);
            if (buildingInstanceDic[buildingType].Count == 0)
            {
                buildingInstanceDic.Remove(buildingType);
            }
        }

        public int GetBuildingCount(BuildingCategory2 _buildingType,int level)
        {
            if (!buildingInstanceDic.ContainsKey(_buildingType))
            {
                return 0;
            }
            else
            {
                if (level == 0)
                {
                    return buildingInstanceDic[_buildingType].Count;
                }
                else
                {
                    int _res = 0;
                    foreach (var _buildingPart in buildingInstanceDic[_buildingType])
                    {
                        //向下兼容,实际等级大于需要等级
                        if ((_buildingPart.Classification.Category4 - 1) >= level)
                            _res++;
                    }
                    return _res;
                }
            }
        }
        #endregion

        #region Gizmos
        [System.Serializable]
        public struct DrawGizmos
        {
            public bool IsDraw;
            public Color color;
        }
        [LabelText("绘制Socket")]
        public DrawGizmos DrawSocket;
        [LabelText("绘制ActiveSocket")]
        public DrawGizmos DrawActiveSocket;
        [LabelText("绘制Area小格子")]
        public DrawGizmos DrawAreaBaseGrid;
        [LabelText("绘制Area大格子")]
        public DrawGizmos DrawAreaBoundGrid;

        [ShowInInspector]
        [SerializeField]
        public GameObject VisualSocket;

        public void ResetVisualSocket()
        {
            //Debug.Log("ResetVisualSocket ");
            VisualSocket.transform.parent = null;
            VisualSocket.gameObject.SetActive(false);
        }
        #endregion
    }

}

