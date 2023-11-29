using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem.BuildingPart;
using System.Linq;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem
{
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
                if(instance == null)
                {
                    instance = Manager.GameManager.Instance.GetLocalManager<BuildingManager>();
                }
                return instance;
            }
        }

        ~BuildingManager()
        {
            if(instance == this)
            {
                instance = null;
            }
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
        private BuildingPlacer.BuildingPlacer placer;
        /// <summary>
        /// 控制的Placer
        /// </summary>
        public BuildingPlacer.BuildingPlacer Placer
        {
            get => this.placer;
            set
            {
                if(this.placer)
                {
                    this.OnModeChanged -= this.placer.OnModeChanged;
                }
                this.placer = value;
                if (this.placer)
                {
                    this.OnModeChanged += this.placer.OnModeChanged;
                }
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
        /// <summary>
        /// 将BPart加入管理队列
        /// </summary>
        /// <param name="BPart"></param>
        public void RegisterBPartPrefab(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }

            this.AddBPartPrefabOnStyle(BPart);
            this.AddBPartPrefabOnHeight(BPart);
        }

        /// <summary>
        /// 将BPart从管理队列移除
        /// </summary>
        /// <param name="BPart"></param>
        public void UnregisterBPartPrefab(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }

            this.RemoveBPartPrefabOnStyle(BPart);
            this.RemoveBPartPrefabOnHeight(BPart);
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
        public IBuildingPart GetOneBPartInstance(BuildingCategory Category, BuildingType Type)
        {
            if(this.BPartClassificationOnStyle.ContainsKey(Category) && this.BPartClassificationOnStyle[Category].ContainsKey(Type) && this.BPartClassificationOnStyle[Category][Type].Count > 0)
            {
                return GameObject.Instantiate<GameObject>(BPartClassificationOnStyle[Category][Type].First().Value.PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }
        
        /// <summary>
        /// 获得一个复制的实例
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public IBuildingPart GetOneBPartCopyInstance(IBuildingPart BPart)
        {
            BPart.Mode = BuildingMode.None;
            return GameObject.Instantiate<GameObject>(BPart.gameObject).GetComponent<IBuildingPart>();
        }
        #endregion

        #region 基于BuildingStyle的三级分类
        /// <summary>
        /// 基于BuildingStyle的三级分类
        /// </summary>
        private Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<BuildingStyle, Deque<IBuildingPart>>>> BPartClassificationOnStyle = new Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<BuildingStyle, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if(BPart == null)
            {
                return;
            }
            // 不存在对应队列
            if (!this.BPartClassificationOnStyle.ContainsKey(BPart.Classification.Category) || !this.BPartClassificationOnStyle[BPart.Classification.Category].ContainsKey(BPart.Classification.Type) || !this.BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type].ContainsKey(BPart.Classification.Style))
            {
                var onType = new Dictionary<BuildingType, Dictionary<BuildingStyle, Deque<IBuildingPart>>>();
                var onStyle = new Dictionary<BuildingStyle, Deque<IBuildingPart>>();
                var Deque = new Deque<IBuildingPart>();

                if (this.BPartClassificationOnStyle.TryAdd(BPart.Classification.Category, onType))
                {
                    if (onType.TryAdd(BPart.Classification.Type, onStyle))
                    {
                        onStyle.TryAdd(BPart.Classification.Style, Deque);
                    }
                }
                else
                {
                    if (this.BPartClassificationOnStyle[BPart.Classification.Category].TryAdd(BPart.Classification.Type, onStyle))
                    {
                        onStyle.TryAdd(BPart.Classification.Style, Deque);
                    }
                    else
                    {
                        this.BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type].TryAdd(BPart.Classification.Style, Deque);
                    }
                }
            }

            this.BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Style].EnqueueBack(BPart);
        }

        private void RemoveBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if (BPart == null || this.GetBPartPeekInstanceOnStyle(BPart) == null)
            {
                return;
            }
            var Deque = this.BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Style];
            for (int i = Deque.Count; i > 0; --i)
            {
                if(Deque.PeekFront().Classification.Height == BPart.Classification.Height)
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
        public IBuildingPart GetBPartPeekInstanceOnStyle(BuildingCategory category, BuildingType type, BuildingStyle style)
        {
            if(this.BPartClassificationOnStyle.ContainsKey(category) && this.BPartClassificationOnStyle[category].ContainsKey(type) && this.BPartClassificationOnStyle[category][type].ContainsKey(style))
            {
                return GameObject.Instantiate<GameObject>(this.BPartClassificationOnStyle[category][type][style].PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart GetBPartPeekInstanceOnStyle(IBuildingPart BPart)
        {
            return GetBPartPeekInstanceOnStyle(BPart.Classification.Category, BPart.Classification.Type, BPart.Classification.Style);
        }

        /// <summary>
        /// 轮询指定Style类型的BPart队列的队首值
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IBuildingPart PollingBPartPeekInstanceOnStyle(BuildingCategory category, BuildingType type, BuildingStyle style, bool isForward)
        {
            if (this.BPartClassificationOnStyle.ContainsKey(category) && this.BPartClassificationOnStyle[category].ContainsKey(type) && this.BPartClassificationOnStyle[category][type].ContainsKey(style))
            {
                if(isForward)
                {
                    var BPartQueue = this.BPartClassificationOnStyle[category][type][style];
                    var ret = BPartQueue.PeekFront();
                    BPartQueue.DequeueFront();
                    BPartQueue.EnqueueBack(ret);
                    ret = BPartQueue.PeekFront();
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
                else
                {
                    var BPartQueue = this.BPartClassificationOnStyle[category][type][style];
                    var ret = BPartQueue.PeekBack();
                    BPartQueue.DequeueBack();
                    BPartQueue.EnqueueFront(ret);
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
            }
            return null;
        }

        public IBuildingPart PollingBPartPeekInstanceOnStyle(IBuildingPart BPart, bool isForward)
        {
            return PollingBPartPeekInstanceOnStyle(BPart.Classification.Category, BPart.Classification.Type, BPart.Classification.Style, isForward);
        }

        public short[] GetAllHeightByBPartStyle(IBuildingPart BPart)
        {
            var bparts = BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Style].ToArray();
            short[] heights = new short[bparts.Length];
            for(int i = 0; i < bparts.Length; ++i)
            {
                heights[i] = bparts[i].Classification.Height;
            }
            return heights;
        }

        #endregion

        #region 基于BuildingHeight的三级分类
        /// <summary>
        /// 基于BuildingHeight的三级分类
        /// </summary>
        private Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<short, Deque<IBuildingPart>>>> BPartClassificationOnHeight = new Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<short, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }
            // 不存在对应队列
            if (!this.BPartClassificationOnHeight.ContainsKey(BPart.Classification.Category) || !this.BPartClassificationOnHeight[BPart.Classification.Category].ContainsKey(BPart.Classification.Type) || !this.BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type].ContainsKey(BPart.Classification.Height))
            {
                var onType = new Dictionary<BuildingType, Dictionary<short, Deque<IBuildingPart>>>();
                var onHeight = new Dictionary<short, Deque<IBuildingPart>>();
                var Deque = new Deque<IBuildingPart>();

                if(this.BPartClassificationOnHeight.TryAdd(BPart.Classification.Category, onType))
                {
                    if(onType.TryAdd(BPart.Classification.Type, onHeight))
                    {
                        onHeight.TryAdd(BPart.Classification.Height, Deque);
                    }
                }
                else
                {
                    if(this.BPartClassificationOnHeight[BPart.Classification.Category].TryAdd(BPart.Classification.Type, onHeight))
                    {
                        onHeight.TryAdd(BPart.Classification.Height, Deque);
                    }
                    else
                    {
                        this.BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type].TryAdd(BPart.Classification.Height, Deque);
                    }
                }
            }

            this.BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Height].EnqueueBack(BPart);
        }

        private void RemoveBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null || this.GetBPartPeekInstanceOnHeight(BPart) == null)
            {
                return;
            }
            var Deque = this.BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Height];
            for (int i = Deque.Count; i > 0; --i)
            {
                if (Deque.PeekFront().Classification.Style == BPart.Classification.Style)
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
        public IBuildingPart GetBPartPeekInstanceOnHeight(BuildingCategory category, BuildingType type, short height)
        {
            if (this.BPartClassificationOnHeight.ContainsKey(category) && this.BPartClassificationOnHeight[category].ContainsKey(type) && this.BPartClassificationOnHeight[category][type].ContainsKey(height))
            {
                return GameObject.Instantiate<GameObject>(this.BPartClassificationOnHeight[category][type][height].PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart GetBPartPeekInstanceOnHeight(IBuildingPart BPart)
        {
            return GetBPartPeekInstanceOnHeight(BPart.Classification.Category, BPart.Classification.Type, BPart.Classification.Height);
        }

        /// <summary>
        /// 轮询指定Height的BPart队列的队首值
        /// </summary>
        /// <param name="category"></param>
        /// <param name="type"></param>
        /// <param name="style"></param>
        /// <returns></returns>
        public IBuildingPart PollingBPartPeekInstanceOnHeight(BuildingCategory category, BuildingType type, short height, bool isForward)
        {
            if (this.BPartClassificationOnHeight.ContainsKey(category) && this.BPartClassificationOnHeight[category].ContainsKey(type) && this.BPartClassificationOnHeight[category][type].ContainsKey(height))
            {
                if(isForward)
                {
                    var BPartQueue = this.BPartClassificationOnHeight[category][type][height];
                    var ret = BPartQueue.PeekFront();
                    BPartQueue.DequeueFront();
                    BPartQueue.EnqueueBack(ret);
                    ret = BPartQueue.PeekFront();
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
                else
                {
                    var BPartQueue = this.BPartClassificationOnHeight[category][type][height];
                    var ret = BPartQueue.PeekBack();
                    BPartQueue.DequeueBack();
                    BPartQueue.EnqueueFront(ret);
                    return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
                }
            }
            return null;
        }

        public IBuildingPart PollingBPartPeekInstanceOnHeight(IBuildingPart BPart, bool isForward)
        {
            return PollingBPartPeekInstanceOnHeight(BPart.Classification.Category, BPart.Classification.Type, BPart.Classification.Height, isForward);
        }

        public BuildingStyle[] GetAllStyleByBPartHeight(IBuildingPart BPart)
        {
            var bparts = BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Height].ToArray();
            BuildingStyle[] styles = new BuildingStyle[bparts.Length];
            for (int i = 0; i < bparts.Length; ++i)
            {
                styles[i] = bparts[i].Classification.Style;
            }
            return styles;
        }
        #endregion

        #region Material
        // to-do :to-change
        [SerializeField]
        private const string _ABMatPath = "materials";
        [SerializeField]
        private const string _ABMatName = "Transparent";

        /// <summary>
        /// 建造模式下所用的Material (Place, Edit, Destroy, Interact)
        /// </summary>
        [SerializeField, HideInInspector]
        private Material _buildingMaterial = null;
        private Material buildingMaterial
        {
            get
            {
                if(this._buildingMaterial == null)
                {
#if UNITY_EDITOR
                    _buildingMaterial = Manager.GameManager.Instance.ABResourceManager.LoadAsset<Material>(_ABMatPath, _ABMatName, true);
#else
                    _buildingMaterial = Manager.GameManager.Instance.ABResourceManager.LoadAsset<Material>(_ABMatPath, _ABMatName);
#endif
                }
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


    }

}

