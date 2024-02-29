using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem.BuildingPart;
using System.Linq;
using System;
using UnityEditor;

namespace ML.Engine.BuildingSystem
{
    [System.Serializable]
    public struct BuildingTableData
    {
        public string id;
        public TextContent.TextContent name;
        public string icon;
        public string category1;
        public string category2;
        public string category3;
        public string category4;
        public string actorID;
        public List<InventorySystem.CompositeSystem.Formula> raw;
        public string upgradeCID;
        public List<InventorySystem.CompositeSystem.Formula> upgradeRaw;

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
    public struct BuildingUpgradeTableData
    {
        public string id;
        public TextContent.TextContent name;
        public List<InventorySystem.CompositeSystem.Formula> upgradeRaw;
    }
    [LabelText("����ģʽ")]
    public enum BuildingMode
    {
        [LabelText("���ý���")]
        None,
        [LabelText("���ý���")]
        Interact,
        [LabelText("�½�����")]
        Place,
        [LabelText("�༭�ƶ�")]
        Edit,
        [LabelText("ѡ������")]
        Destroy,
    }

    /// <summary>
    /// ���ƵĲ���
    /// </summary>
    public struct BuildingCopiedMaterial
    {
        /// <summary>
        /// RootGameObject.Renderer.Mat -> ����Ϊnull
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
                    bool f = false;
                    foreach (var bp in B.ParentMat)
                    {
                        if(ap.name == bp.name)
                        {
                            f = true;
                            break;
                        }
                    }
                    if(f == false)
                    {
                        return false;
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
                                bool f = false;
                                foreach (var bp in B.ParentMat)
                                {
                                    if (t.name == bp.name)
                                    {
                                        f = true;
                                        break;
                                    }
                                }
                                if (f == false)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
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
                                bool f = false;
                                foreach (var bp in B.ParentMat)
                                {
                                    if (t.name == bp.name)
                                    {
                                        f = true;
                                        break;
                                    }
                                }
                                if (f == false)
                                {
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
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
        /// ��ǰ���ڵĽ���ģʽ
        /// </summary>
        [LabelText("����ģʽ"), ShowInInspector]
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
        /// ����ģʽ�ı�ʱ����
        /// PreMode, CurMode
        /// </summary>
        public event System.Action<BuildingMode, BuildingMode> OnModeChanged;
        private BuildingPlacer.BuildingPlacer placer;
        /// <summary>
        /// ���Ƶ�Placer
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

        // to-do : EditModeʹ��PlaceMode�İ������� -> PlaceModeȫ������,EditMode����: ChangeOutLook|ChangeStyle|SwitchFrame_PreHold|KeyCom
        private Input.BuildingInput binput = null;
        /// <summary>
        /// ����ϵͳ��ͳһ����
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
        /// ��ǰ����������ɵĽ���������
        /// </summary>
        /// <returns></returns>
        public int GetRegisterBPartCount()
        {
            return registeredBPart.Count;
        }
        /// <summary>
        /// �Ƿ��ǺϷ��Ľ�����ID -> �������Ѿ�ע��Ľ����δע�ᵫ�����ɺϷ��Ľ�����ID��Ȼ����fasle
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsValidBPartID(string id)
        {
            BuildingPartClassification classification = new BuildingPartClassification(id);
            return this.registeredBPart.ContainsKey(classification);
        }

        /// <summary>
        /// ��BPart����������
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
        }

        /// <summary>
        /// ��BPart�ӹ�������Ƴ�
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
        /// ע��ȫ��
        /// </summary>
        public void UnregisterAllBPartPrefab()
        {
            this.BPartClassificationOnStyle.Clear();
            this.BPartClassificationOnHeight.Clear();
        }

        /// <summary>
        /// ��ȡһ���µ�Ψһ�Ľ�����ʵ��ID
        /// </summary>
        /// <returns></returns>
        public string GetOneNewBPartInstanceID()
        {
            return ML.Engine.Utility.OSTime.OSCurMilliSeconedTime.ToString();
        }
        
        /// <summary>
        /// ���һ��ָ����������� BPartInstance
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


        /// <summary>
        /// ��BPart���Ը��ƣ�����һ�����Ƶ�ʵ��
        /// �������Ը��ƣ���ֱ�ӷ���BPart
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
            return BPart;
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

        #region ����BuildingStyle����������
        /// <summary>
        /// ����BuildingStyle����������
        /// </summary>
        private Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<BuildingCategory3, Deque<IBuildingPart>>>> BPartClassificationOnStyle = new Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<BuildingCategory3, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if(BPart == null)
            {
                return;
            }
            // �����ڶ�Ӧ����
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
        /// ��ȡָ��Style���͵�BPart���еĶ���ֵ��һ��ʵ��IBuilding.GameObject
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
        /// ��ѯָ��Style���͵�BPart���еĶ���ֵ
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

        #region ����BuildingHeight����������
        /// <summary>
        /// ����BuildingHeight����������
        /// </summary>
        private Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<short, Deque<IBuildingPart>>>> BPartClassificationOnHeight = new Dictionary<BuildingCategory1, Dictionary<BuildingCategory2, Dictionary<short, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }
            // �����ڶ�Ӧ����
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
        /// ��ȡָ��Height��BPart���еĶ���ֵ��һ��ʵ��ֵ
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
        /// ��ѯָ��Height��BPart���еĶ���ֵ
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
        #endregion

        #region Material
        // to-do :to-change
        [SerializeField]
        private const string _ABMatPath = "materials/other";
        [SerializeField]
        private const string _ABMatName = "Transparent";

        /// <summary>
        /// ����ģʽ�����õ�Material (Place, Edit, Destroy, Interact)
        /// </summary>
        [SerializeField, HideInInspector]
        private Material _buildingMaterial = null;
        private Material buildingMaterial
        {
            get
            {
                if(this._buildingMaterial == null)
                {
                    bool tempiffull = false;
#if UNITY_EDITOR
                    tempiffull = true;
#endif
                    _buildingMaterial = Manager.GameManager.Instance.ABResourceManager.LoadAsset<Material>(_ABMatPath, _ABMatName, tempiffull);
                }
                return this._buildingMaterial;
            }
        }

        /// <summary>
        /// None ����Ϊ null
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

        #region ���ʿ���ճ��
        /// <summary>
        /// �洢ȫ�ָ��ƵĲ���
        /// </summary>
        private Dictionary<BuildingPartClassification, BuildingCopiedMaterial> CopyMatDict = new
             Dictionary<BuildingPartClassification, BuildingCopiedMaterial>();

        /// <summary>
        /// ��ȡ��BPartͬ���͵ĵ�ǰ�����Ĳ��ʣ��������ڣ��򷵻� default
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
        /// ���Ʋ���¼
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public BuildingCopiedMaterial CopyBPartMaterial(IBuildingPart BPart)
        {
            var mat = BPart.GetCopiedMaterial();
            // ���ڶ�Ӧ��
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
        /// ��BPartͬ���Ϳ����Ĳ��ʸ�ֵ��BPart
        /// ����ֵΪ�Ƿ�ɹ�,���Ƿ��п���ֵ
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

        #region ����
        public const string Tex2DABPath = "UI/BuildingSystem/Texture2D/BuildIcon";
        // Materials/Character/Player

        public Dictionary<string, BuildingTableData> BPartTableDictOnID = new Dictionary<string, BuildingTableData>();
        public Dictionary<string, BuildingTableData> BPartTableDictOnClass = new Dictionary<string, BuildingTableData>();
        public bool IsLoadOvered => ABJAProcessor != null && ABJAProcessor.IsLoaded;

        public static ML.Engine.ABResources.ABJsonAssetProcessor<BuildingTableData[]> ABJAProcessor;

        public void LoadTableData()
        {
            if (ABJAProcessor == null)
            {
                ABJAProcessor = new ML.Engine.ABResources.ABJsonAssetProcessor<BuildingTableData[]>("Json/TableData", "Building", (datas) =>
                {
                    foreach (var data in datas)
                    {
                        BPartTableDictOnID.Add(data.id, data);
                        BPartTableDictOnClass.Add(data.GetClassificationString(), data);
                    }
                }, null, "����������");
                ABJAProcessor.StartLoadJsonAssetData();
            }
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
        /// <param name="CID">4������ID</param>
        /// <returns></returns>
        public string GetActorID(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].actorID;
            }
            return null;
        }

        public List<InventorySystem.CompositeSystem.Formula> GetRaw(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].raw;
            }
            return null;
        }

        public List<InventorySystem.CompositeSystem.Formula> GetUpgradeRaw(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                //return BPartTableDictOnClass[CID].upgradeRaw;
            }
            return null;
        }

        public string GetUpgradeCID(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                return BPartTableDictOnClass[CID].upgradeCID;
            }
            return null;
        }

        public string GetUpgradeID(string CID)
        {
            if (!string.IsNullOrEmpty(CID) && BPartTableDictOnClass.ContainsKey(CID))
            {
                string upgradeCID = BPartTableDictOnClass[CID].upgradeCID;
                if (!string.IsNullOrEmpty(upgradeCID) && BPartTableDictOnClass.ContainsKey(upgradeCID))
                {
                    return BPartTableDictOnClass[upgradeCID].id;
                }
            }
            return null;
        }

        public string GetClassification(string ID)
        {
            if (!string.IsNullOrEmpty(ID) && BPartTableDictOnID.ContainsKey(ID))
            {
                return BPartTableDictOnID[ID].GetClassificationString();
            }
            return null;
        }
        #endregion


        #region Gizmos
        [System.Serializable]
        public struct DrawGizmos
        {
            public bool IsDraw;
            public Color color;
        }
        [LabelText("����Socket")]
        public DrawGizmos DrawSocket;
        [LabelText("����ActiveSocket")]
        public DrawGizmos DrawActiveSocket;
        [LabelText("����AreaС����")]
        public DrawGizmos DrawAreaBaseGrid;
        [LabelText("����Area�����")]
        public DrawGizmos DrawAreaBoundGrid;
        #endregion
    }

}

