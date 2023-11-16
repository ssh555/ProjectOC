using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem.BuildingPart;

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
        Material ParentMat;

        /// <summary>
        /// ChildGameObject.Renderer.Mat
        /// <ChildIndex, Material>
        /// </summary>
        Dictionary<int, Material> ChildrenMat;
    }

    public sealed class BuildingManager : Utility.NoMonoSingletonClass<BuildingManager>, Manager.LocalManager.ILocalManager
    {
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

                mode = value;
#if UNITY_EDITOR
                if(UnityEditor.EditorApplication.isPlaying)
                {
                    if (mode == BuildingMode.None)
                    {
                        BInput.Disable();
                    }
                    else
                    {
                        BInput.Enable();
                    }
                    if(mode == BuildingMode.Interact)
                    {
                        BInput.Build.Enable();
                    }
                    else
                    {
                        BInput.Build.Disable();
                    }
                }
#else
                    if (mode == BuildingMode.None)
                    {
                        BInput.Disable();
                    }
                    else
                    {
                        BInput.Enable();
                    }
                    if(mode == BuildingMode.Interact)
                    {
                        BInput.Build.Enable();
                    }
                    else
                    {
                        BInput.Build.Disable();
                    }
#endif
            }
        }

        /// <summary>
        /// 建造模式改变时调用
        /// PreMode, CurMode
        /// </summary>
        public event System.Action<BuildingMode, BuildingMode> OnModeChanged;

        /// <summary>
        /// 控制的Placer
        /// </summary>
        public BuildingPlacer.BuildingPlacer Placer { get; set; }

        // to-do : EditMode使用PlaceMode的按键输入 -> PlaceMode全部启用,EditMode禁用: ChangeOutLook|ChangeStyle|SwitchFrame_PreHold|KeyCom
        /// <summary>
        /// 建造系统的统一输入
        /// </summary>
        public Input.BuildingInput BInput = new Input.BuildingInput();

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
                return GameObject.Instantiate<GameObject>(BPartClassificationOnStyle[Category][Type][0].Peek().gameObject).GetComponent<IBuildingPart>();
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
            return GameObject.Instantiate<GameObject>(BPart.gameObject).GetComponent<IBuildingPart>();
        }
        #endregion

        #region 基于BuildingStyle的三级分类
        /// <summary>
        /// 基于BuildingStyle的三级分类
        /// </summary>
        private Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<BuildingStyle, Queue<IBuildingPart>>>> BPartClassificationOnStyle = new Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<BuildingStyle, Queue<IBuildingPart>>>>();

        private void AddBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if(BPart == null)
            {
                return;
            }
            // 不存在对应队列
            if (this.GetBPartPeekInstanceOnStyle(BPart) == null)
            {
                var onType = new Dictionary<BuildingType, Dictionary<BuildingStyle, Queue<IBuildingPart>>>();
                var onStyle = new Dictionary<BuildingStyle, Queue<IBuildingPart>>();
                var queue = new Queue<IBuildingPart>();

                this.BPartClassificationOnStyle.Add(BPart.Classification.Category, onType);
                onType.Add(BPart.Classification.Type, onStyle);
                onStyle.Add(BPart.Classification.Style, queue);
            }

            this.BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Style].Enqueue(BPart);
        }

        private void RemoveBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if (BPart == null || this.GetBPartPeekInstanceOnStyle(BPart) == null)
            {
                return;
            }
            var queue = this.BPartClassificationOnStyle[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Style];
            for (int i = queue.Count; i > 0; --i)
            {
                if(queue.Peek().Classification.Height == BPart.Classification.Height)
                {
                    queue.Dequeue();
                    break;
                }
                else
                {
                    queue.Enqueue(queue.Dequeue());
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
                return GameObject.Instantiate<GameObject>(this.BPartClassificationOnStyle[category][type][style].Peek().gameObject).GetComponent<IBuildingPart>();
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
        public IBuildingPart PollingBPartPeekInstanceOnStyle(BuildingCategory category, BuildingType type, BuildingStyle style)
        {
            if (this.BPartClassificationOnStyle.ContainsKey(category) && this.BPartClassificationOnStyle[category].ContainsKey(type) && this.BPartClassificationOnStyle[category][type].ContainsKey(style))
            {
                var BPartQueue = this.BPartClassificationOnStyle[category][type][style];
                var ret = BPartQueue.Peek();
                BPartQueue.Dequeue();
                BPartQueue.Enqueue(ret);
                return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart PollingBPartPeekInstanceOnStyle(IBuildingPart BPart)
        {
            return PollingBPartPeekInstanceOnStyle(BPart.Classification.Category, BPart.Classification.Type, BPart.Classification.Style);
        }

        #endregion

        #region 基于BuildingHeight的三级分类
        /// <summary>
        /// 基于BuildingHeight的三级分类
        /// </summary>
        private Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<short, Queue<IBuildingPart>>>> BPartClassificationOnHeight = new Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<short, Queue<IBuildingPart>>>>();

        private void AddBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }
            // 不存在对应队列
            if (this.GetBPartPeekInstanceOnHeight(BPart) == null)
            {
                var onType = new Dictionary<BuildingType, Dictionary<short, Queue<IBuildingPart>>>();
                var onHeight = new Dictionary<short, Queue<IBuildingPart>>();
                var queue = new Queue<IBuildingPart>();

                this.BPartClassificationOnHeight.Add(BPart.Classification.Category, onType);
                onType.Add(BPart.Classification.Type, onHeight);
                onHeight.Add(BPart.Classification.Height, queue);
            }

            this.BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Height].Enqueue(BPart);
        }

        private void RemoveBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null || this.GetBPartPeekInstanceOnHeight(BPart) == null)
            {
                return;
            }
            var queue = this.BPartClassificationOnHeight[BPart.Classification.Category][BPart.Classification.Type][BPart.Classification.Height];
            for (int i = queue.Count; i > 0; --i)
            {
                if (queue.Peek().Classification.Style == BPart.Classification.Style)
                {
                    queue.Dequeue();
                    break;
                }
                else
                {
                    queue.Enqueue(queue.Dequeue());
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
                return GameObject.Instantiate<GameObject>(this.BPartClassificationOnHeight[category][type][height].Peek().gameObject).GetComponent<IBuildingPart>();
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
        public IBuildingPart PollingBPartPeekInstanceOnHeight(BuildingCategory category, BuildingType type, short height)
        {
            if (this.BPartClassificationOnHeight.ContainsKey(category) && this.BPartClassificationOnHeight[category].ContainsKey(type) && this.BPartClassificationOnHeight[category][type].ContainsKey(height))
            {
                var BPartQueue = this.BPartClassificationOnHeight[category][type][height];
                var ret = BPartQueue.Peek();
                BPartQueue.Dequeue();
                BPartQueue.Enqueue(ret);
                return GameObject.Instantiate<GameObject>(ret.gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }

        public IBuildingPart PollingBPartPeekInstanceOnHeight(IBuildingPart BPart)
        {
            return PollingBPartPeekInstanceOnHeight(BPart.Classification.Category, BPart.Classification.Type, BPart.Classification.Height);
        }

        #endregion

        #region Material
        /// <summary>
        /// 建造模式下所用的Material (Place, Edit, Destroy, Interact)
        /// </summary>
        [SerializeField]
        private Material buildingMaterial;

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

