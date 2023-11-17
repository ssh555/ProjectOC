using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.BuildingSystem.BuildingPart;

namespace ML.Engine.BuildingSystem
{
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
        /// ��ǰ���ڵĽ���ģʽ
        /// </summary>
        [LabelText("����ģʽ"), ShowInInspector]
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
        /// ����ģʽ�ı�ʱ����
        /// PreMode, CurMode
        /// </summary>
        public event System.Action<BuildingMode, BuildingMode> OnModeChanged;

        /// <summary>
        /// ���Ƶ�Placer
        /// </summary>
        public BuildingPlacer.BuildingPlacer Placer { get; set; }

        // to-do : EditModeʹ��PlaceMode�İ������� -> PlaceModeȫ������,EditMode����: ChangeOutLook|ChangeStyle|SwitchFrame_PreHold|KeyCom
        /// <summary>
        /// ����ϵͳ��ͳһ����
        /// </summary>
        public Input.BuildingInput BInput = new Input.BuildingInput();

        #region BPartPrefab
        /// <summary>
        /// ��BPart����������
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
        /// ��BPart�ӹ�������Ƴ�
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
        public IBuildingPart GetOneBPartInstance(BuildingCategory Category, BuildingType Type)
        {
            if(this.BPartClassificationOnStyle.ContainsKey(Category) && this.BPartClassificationOnStyle[Category].ContainsKey(Type) && this.BPartClassificationOnStyle[Category][Type].Count > 0)
            {
                return GameObject.Instantiate<GameObject>(BPartClassificationOnStyle[Category][Type][0].PeekFront().gameObject).GetComponent<IBuildingPart>();
            }
            return null;
        }
        
        /// <summary>
        /// ���һ�����Ƶ�ʵ��
        /// </summary>
        /// <param name="BPart"></param>
        /// <returns></returns>
        public IBuildingPart GetOneBPartCopyInstance(IBuildingPart BPart)
        {
            return GameObject.Instantiate<GameObject>(BPart.gameObject).GetComponent<IBuildingPart>();
        }
        #endregion

        #region ����BuildingStyle����������
        /// <summary>
        /// ����BuildingStyle����������
        /// </summary>
        private Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<BuildingStyle, Deque<IBuildingPart>>>> BPartClassificationOnStyle = new Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<BuildingStyle, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnStyle(IBuildingPart BPart)
        {
            if(BPart == null)
            {
                return;
            }
            // �����ڶ�Ӧ����
            if (this.GetBPartPeekInstanceOnStyle(BPart) == null)
            {
                var onType = new Dictionary<BuildingType, Dictionary<BuildingStyle, Deque<IBuildingPart>>>();
                var onStyle = new Dictionary<BuildingStyle, Deque<IBuildingPart>>();
                var Deque = new Deque<IBuildingPart>();

                this.BPartClassificationOnStyle.Add(BPart.Classification.Category, onType);
                onType.Add(BPart.Classification.Type, onStyle);
                onStyle.Add(BPart.Classification.Style, Deque);
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
        /// ��ȡָ��Style���͵�BPart���еĶ���ֵ��һ��ʵ��IBuilding.GameObject
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
        /// ��ѯָ��Style���͵�BPart���еĶ���ֵ
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

        #endregion

        #region ����BuildingHeight����������
        /// <summary>
        /// ����BuildingHeight����������
        /// </summary>
        private Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<short, Deque<IBuildingPart>>>> BPartClassificationOnHeight = new Dictionary<BuildingCategory, Dictionary<BuildingType, Dictionary<short, Deque<IBuildingPart>>>>();

        private void AddBPartPrefabOnHeight(IBuildingPart BPart)
        {
            if (BPart == null)
            {
                return;
            }
            // �����ڶ�Ӧ����
            if (this.GetBPartPeekInstanceOnHeight(BPart) == null)
            {
                var onType = new Dictionary<BuildingType, Dictionary<short, Deque<IBuildingPart>>>();
                var onHeight = new Dictionary<short, Deque<IBuildingPart>>();
                var Deque = new Deque<IBuildingPart>();

                this.BPartClassificationOnHeight.Add(BPart.Classification.Category, onType);
                onType.Add(BPart.Classification.Type, onHeight);
                onHeight.Add(BPart.Classification.Height, Deque);
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
        /// ��ȡָ��Height��BPart���еĶ���ֵ��һ��ʵ��ֵ
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
        /// ��ѯָ��Height��BPart���еĶ���ֵ
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

        #endregion

        #region Material
        /// <summary>
        /// ����ģʽ�����õ�Material (Place, Edit, Destroy, Interact)
        /// </summary>
        [SerializeField]
        private Material buildingMaterial;

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
    
    
    }

}

