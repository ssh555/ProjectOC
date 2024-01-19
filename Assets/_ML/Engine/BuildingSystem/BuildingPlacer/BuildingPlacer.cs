using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static ML.Engine.BuildingSystem.MonoBuildingManager;

namespace ML.Engine.BuildingSystem.BuildingPlacer
{
    // to-do : 使用时需在 BuildingManager 注册
    public class BuildingPlacer : MonoBehaviour, Timer.ITickComponent
    {
        #region 建造模式
        //[LabelText("建造模式"), ShowInInspector, PropertyOrder(-1)]

        /// <summary>
        /// 当前交互模式
        /// </summary>
        public BuildingMode Mode
        {
            get => BuildingManager.Instance.Mode;
            set
            {
                BuildingManager.Instance.Mode = value;
            }
        }

        protected void EnterBuildingMode()
        {
            Manager.GameManager.Instance.TickManager.RegisterTick(0, this);

            BInput.Build.Enable();

            this.OnBuildingModeEnter?.Invoke();
        }

        protected void ExitBuildingMode()
        {
            Manager.GameManager.Instance.TickManager.UnregisterTick(this);

            this.InteractBPartList.Clear();

            this.SelectedPartInstance = null;

            BInput.Build.Disable();
            BInput.BuildKeyCom.Disable();
            BInput.BuildSelection.Disable();
            BInput.BuildingAppearance.Disable();
            BInput.BuildPlaceMode.Disable();

            this.OnBuildingModeExit?.Invoke();
        }
        
        public void OnModeChanged(BuildingMode pre, BuildingMode cur)
        {
            if (this.SelectedPartInstance != null)
            {
                this.SelectedPartInstance.Mode = cur;
            }
            // 不启用
            if (cur == BuildingMode.None && pre != BuildingMode.None)
            {
                this.ExitBuildingMode();
            }
            // 启用
            else if (pre == BuildingMode.None && cur != BuildingMode.None)
            {
                this.EnterBuildingMode();
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// 进入建造模式时调用
        /// </summary>
        public event System.Action OnBuildingModeEnter;
        /// <summary>
        /// 退出建造模式时调用
        /// </summary>
        public event System.Action OnBuildingModeExit;

        /// <summary>
        /// 长按开始的时间
        /// </summary>
        private float keyComStartTime;
        /// <summary>
        /// 长按总的目标时间
        /// </summary>
        private float keyComTotalTime;
        /// <summary>
        /// 次交互轮 长按开始时调用
        /// </summary>
        public event System.Action OnKeyComStart;
        /// <summary>
        /// 次交互轮 长按按住时响应
        /// CurTime, TotalTime
        /// </summary>
        public event System.Action<float, float> OnKeyComInProgress;
        /// <summary>
        /// 次交互轮 长按成功完成时调用, 即进入次交互轮
        /// </summary>
        public event System.Action OnKeyComComplete;
        /// <summary>
        /// 次交互轮 长按提前结束，未完成时调用
        /// CurTime, TotalTime
        /// </summary>
        public event System.Action<float, float> OnKeyComCancel;
        /// <summary>
        /// 次交互轮 长按成功完成且退出时调用
        /// </summary>
        public event System.Action OnKeyComExit;

        /// <summary>
        /// 是否处于外观更改状态
        /// </summary>
        public bool IsInAppearance { get; protected set; } = false;
        /// <summary>
        /// 进入外观选择面板时调用，即打开外观选择UI交互面板
        /// 参数 : 选择使用的BPart
        /// </summary>
        public event System.Action<IBuildingPart, Texture2D[], BuildingCopiedMaterial[], int> OnEnterAppearance;
        public event System.Action<IBuildingPart> OnExitAppearance;
        public event System.Action<Texture2D[], BuildingCopiedMaterial[], int> OnChangeAppearance;

        /// <summary>
        /// 在销毁BPart时调用
        /// </summary>
        public event System.Action<IBuildingPart> OnDestroySelectedBPart;

        /// <summary>
        /// 进入EditMode
        /// </summary>
        public event System.Action<IBuildingPart> OnEditModeEnter;
        /// <summary>
        /// 退出EditMode
        /// </summary>
        public event System.Action<IBuildingPart> OnEditModeExit;

        /// <summary>
        /// 流程1 : 一级二级分类选择UI交互
        /// </summary>
        public event System.Action<BuildingCategory1[], int, BuildingCategory2[], int> OnBuildSelectionEnter;
        /// <summary>
        /// 流程1 : 一级二级分类选择UI交互
        /// </summary>
        public event System.Action OnBuildSelectionExit;
        /// <summary>
        /// 确认
        /// </summary>
        public event System.Action<IBuildingPart> OnBuildSelectionComfirm;
        /// <summary>
        /// 取消
        /// </summary>
        public event System.Action OnBuildSelectionCancel;
        /// <summary>
        /// 数组总枚举, 当前index
        /// </summary>
        public event System.Action<BuildingCategory1[], int> OnBuildSelectionCategoryChanged;
        /// <summary>
        /// 数组总枚举, 当前index
        /// </summary>
        public event System.Action<BuildingCategory1, BuildingCategory2[], int> OnBuildSelectionTypeChanged;
        /// <summary>
        /// 流程3 : 放置
        /// </summary>
        public event System.Action<IBuildingPart> OnPlaceModeEnter;
        /// <summary>
        /// 流程3 : 放置
        /// </summary>
        public event System.Action OnPlaceModeExit;
        /// <summary>
        /// Style 更改时调用
        /// 当前选择的BPart, 为向前还是向后选择
        /// </summary>
        public event System.Action<IBuildingPart, bool> OnPlaceModeChangeStyle;
        /// <summary>
        /// Height 更改时调用
        /// 当前选择的BPart, 为向前还是向后选择
        /// </summary>
        public event System.Action<IBuildingPart, bool> OnPlaceModeChangeHeight;
        /// <summary>
        /// 一次成功放置
        /// </summary>
        public event System.Action<IBuildingPart> OnPlaceModeSuccess;
        #endregion

        #region 网格辅助
        /// <summary>
        /// 网格辅助
        /// </summary>
        [LabelText("启用网格辅助"), FoldoutGroup("网格辅助")]
        public bool IsEnableGridSupport = true;

        /// <summary>
        /// 禁用网格辅助时的旋转率
        /// </summary>
        [LabelText("禁用时旋转率"), FoldoutGroup("网格辅助"), Range(0, 180)]
        public float DisableGridRotRate = 0.5f;
        /// <summary>
        /// 启用网格辅助时的旋转率
        /// </summary>
        [LabelText("启用时旋转率"), FoldoutGroup("网格辅助"), Range(0, 180)]
        public float EnableGridRotRate = 15f;
        #endregion

        #region BPart
        private IBuildingPart selectedPartInstance;
        /// <summary>
        /// 当前选中交互的建筑物
        /// </summary>
        public IBuildingPart SelectedPartInstance
        {
            get => this.selectedPartInstance;
            set
            {
                if(this.selectedPartInstance != null)
                {
                    this.selectedPartInstance.Mode = BuildingMode.None;
                }
                if (value != null)
                {
                    this.selectedPartInstance = value;
                    this.selectedPartInstance.Mode = Mode;
                }
                // 从可交互列表中寻找下一个
                else
                {
                    // 没有下一个
                    if(this.InteractBPartList.Count < 2)
                    {
                        this.selectedPartInstance = value;
                    }
                    // 有下一个
                    else
                    {
                        int index = (this.InteractBPartList.IndexOf(this.selectedPartInstance) + 1) % this.InteractBPartList.Count;
                        this.selectedPartInstance = this.InteractBPartList[index];
                        this.selectedPartInstance.Mode = Mode;
                    }
                }
            }
        }

        /// <summary>
        /// 根据类别和类型更换BPart
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Type"></param>
        public void ChangeBPart(BuildingCategory1 Category, BuildingCategory2 Type)
        {
            this.ResetBPart();
            this.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(Category, Type);
        }

        /// <summary>
        /// 重置BPart, 即置为null
        /// </summary>
        public void ResetBPart()
        {
            if (this.SelectedPartInstance != null)
            {
                Destroy(this.SelectedPartInstance.gameObject);
                this.SelectedPartInstance = null;
            }
        }

        /// <summary>
        /// 基于样式Style轮换当前BPart
        /// </summary>
        public void AlternateBPartOnStyle(bool isForward)
        {
            var tmp = BuildingManager.Instance.PollingBPartPeekInstanceOnStyle(this.SelectedPartInstance, isForward);
            this.ResetBPart();
            this.SelectedPartInstance = tmp;
        }

        /// <summary>
        /// 基于高度Height轮换当前BPart
        /// </summary>
        public void AlternateBPartOnHeight(bool isForward)
        {
            var tmp = BuildingManager.Instance.PollingBPartPeekInstanceOnHeight(this.SelectedPartInstance, isForward);
            this.ResetBPart();
            this.SelectedPartInstance = tmp;
        }
        #endregion

        #region Transform
        [LabelText("使用的Camera"), SerializeField, FoldoutGroup("落点检测")]
        protected new Camera camera = null;
        /// <summary>
        /// 使用的摄像机 => 为null时默认使用 Camera.main
        /// </summary>
        public Camera Camera
        {
            get
            {
                return camera == null ? Camera.main : camera;
            }
            set => camera = value;
        }

        [LabelText("检测半径"), SerializeField, FoldoutGroup("落点检测"), PropertyTooltip("单位 m")]
        protected float checkRadius = 5;

        [LabelText("检测层"), SerializeField, FoldoutGroup("落点检测")]
        protected LayerMask checkLayer;

        /// <summary>
        /// 获取摄像机检测的Ray
        /// </summary>
        /// <returns></returns>
        protected Ray GetCameraRay()
        {
            return new Ray(Camera.transform.position, Camera.transform.forward);
        }

        /// <summary>
        /// 获得摄像机射线终点处世界轴向下的射线
        /// </summary>
        /// <returns></returns>
        protected Ray GetCameraRayEndPointDownRay()
        {
            return new Ray(GetCameraRay().GetPoint(this.checkRadius), Vector3.down);
        }

        /// <summary>
        /// 获取BPart当前落点的位置和旋转
        /// 位置 => SelectedPartInstance.Pos = pos
        /// 旋转 => SelectedPartInstance.Rot = RotateByAxis(Axis = ActiveSocket.UpAxis, rot)
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        protected bool GetPlacePosAndRot(out Vector3 pos, out Quaternion rot)
        {
            if (this.SelectedPartInstance != null)
            {
                Vector3 oldP = this.SelectedPartInstance.transform.position;
                var oldR = this.SelectedPartInstance.transform.rotation;

                // 归0
                this.SelectedPartInstance.transform.position = this.SelectedPartInstance.ActiveSocket.transform.position - this.SelectedPartInstance.ActiveSocket.transform.localPosition;
                this.SelectedPartInstance.transform.rotation = Quaternion.identity;

                // 位置 -> 自身
                pos = this.transform.position - this.SelectedPartInstance.ActiveSocket.transform.position + this.SelectedPartInstance.transform.position;

                // ScreenCenter->ScreenNormal 射线检测
                var hits = Physics.RaycastAll(this.GetCameraRay(), this.checkRadius, this.checkLayer);
                if (hits == null || hits.Length == 0)
                {
                    hits = Physics.RaycastAll(this.GetCameraRayEndPointDownRay(), this.checkRadius, this.checkLayer);
                }

                // 命中 || 未命中 => 在终点处向下检测
                if (hits != null && hits.Length > 0)
                {
                    bool bisHit = false;
                    bool bArea = false;
                    RaycastHit hitInfo = hits[0];
                    foreach (var h in hits)
                    {
                        var socket = h.collider.GetComponentInParent<BuildingSocket.BuildingSocket>();
                        if (socket != null)
                        {
                            if((socket.InTakeType & this.SelectedPartInstance.ActiveSocket.Type) != 0)
                            {
                                bisHit = true;
                                hitInfo = h;
                                break;
                            }
                        }
                        else if(bArea == false)
                        {
                            var area = h.collider.GetComponentInParent<BuildingArea.BuildingArea>();
                            if (area != null && (area.Type & this.SelectedPartInstance.ActiveAreaType) != 0)
                            {
                                hitInfo = h;
                                bisHit = true;
                                bArea = true;
                            }
                        }
                    }


                    if(!bisHit)
                    {
                        this.SelectedPartInstance.AttachedSocket = null;
                        this.SelectedPartInstance.AttachedArea = null;
                    }
                    else
                    {
                        pos = hitInfo.point - this.SelectedPartInstance.ActiveSocket.transform.position + this.SelectedPartInstance.transform.position;
                        this.SelectedPartInstance.AttachedSocket = hitInfo.collider.GetComponentInParent<BuildingSocket.BuildingSocket>();
                        this.SelectedPartInstance.AttachedArea = hitInfo.collider.GetComponentInParent<BuildingArea.BuildingArea>();

                        if (this.SelectedPartInstance.AttachedSocket && (this.SelectedPartInstance.ActiveSocket.Type & this.SelectedPartInstance.AttachedSocket.InTakeType) == 0)
                        {
                            this.SelectedPartInstance.AttachedSocket = null;
                        }
                        if (this.SelectedPartInstance.AttachedArea && (this.SelectedPartInstance.ActiveAreaType & this.SelectedPartInstance.AttachedArea.Type) == 0)
                        {
                            this.SelectedPartInstance.AttachedArea = null;
                        }
                    }
                }
                else
                {
                    this.SelectedPartInstance.AttachedSocket = null;
                    this.SelectedPartInstance.AttachedArea = null;
                }

                // 旋转 -> 自身
                rot = this.SelectedPartInstance.BaseRotation * this.SelectedPartInstance.RotOffset;

                Vector3 tmpP;
                Quaternion tmpR;
                // 位置&旋转 -> AttachedArea
                if(this.SelectedPartInstance.AttachedArea)
                {
                    if (this.SelectedPartInstance.ActiveSocket.GetMatchTransformOnArea(pos, out tmpP, out tmpR))
                    {
                        pos = tmpP;
                        rot = tmpR;
                        return true;
                    }
                    this.SelectedPartInstance.transform.position = oldP;
                    this.SelectedPartInstance.transform.rotation = oldR;
                    return false;
                }
                // 位置&旋转 -> AttachedSocket
                if (this.SelectedPartInstance.AttachedSocket)
                {
                    if (this.SelectedPartInstance.ActiveSocket.GetMatchTransformOnSocket(out tmpP, out tmpR))
                    {
                        pos = tmpP;
                        rot = tmpR;
                        return true;
                    }
                    this.SelectedPartInstance.transform.position = oldP;
                    this.SelectedPartInstance.transform.rotation = oldR;
                    return false;
                }

                return true;
            }

            pos = Vector3.negativeInfinity;
            rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
            return false;
        }

        /// <summary>
        /// 应用位置和旋转于SelectedPartInstance
        /// </summary>
        protected void TransformSelectedPartInstance()
        {
            if(this.GetPlacePosAndRot(out Vector3 pos, out Quaternion rot))
            {
                //if(pos == Vector3.negativeInfinity || float.IsNaN(rot.x))
                //{
                //    return;
                //}
                Vector3 euler = rot.eulerAngles;

                this.SelectedPartInstance.transform.rotation = Quaternion.identity;
                this.SelectedPartInstance.transform.position = pos;

                Vector3 rotAroundPoint = this.SelectedPartInstance.ActiveSocket.transform.position;

                // Up-Y
                this.SelectedPartInstance.transform.RotateAround(rotAroundPoint, Vector3.up, euler.y);
                // Right-X
                this.SelectedPartInstance.transform.RotateAround(rotAroundPoint, Vector3.right, euler.x);
                // Forward-Z
                this.SelectedPartInstance.transform.RotateAround(rotAroundPoint, Vector3.forward, euler.z);

            }
        }

        [LabelText("检测半径"), SerializeField, FoldoutGroup("交互检测"), PropertyTooltip("单位 m")]
        protected float interactRadius = 5;

        [LabelText("检测层"), SerializeField, FoldoutGroup("交互检测")]
        protected LayerMask interactLayer;

        [LabelText("球形半径"), SerializeField, FoldoutGroup("交互检测"), PropertyTooltip("单位 m")]
        protected float sphereRadius = 0.5f;

        [LabelText("检测时间间隔"), SerializeField, FoldoutGroup("交互检测"), PropertyTooltip("单位 s")]
        protected float interactCheckInterval = 1f;
        private float _curTimer = 0f;

        [HideInInspector]
        public List<IBuildingPart> InteractBPartList = new List<IBuildingPart>();

        /// <summary>
        /// 获取 Edit|Destroy 下检测到的所有可交互的建筑物
        /// </summary>
        /// <returns></returns>
        public List<IBuildingPart> GetSphereCastAllBPart()
        {
            var hitResult = Physics.SphereCastAll(this.GetCameraRay(), sphereRadius, interactRadius, interactLayer);
            List<IBuildingPart> ret = new List<IBuildingPart>();
            foreach (var hit in hitResult)
            {
                var bpart = hit.collider.GetComponentInParent<IBuildingPart>();
                if (bpart != null && !ret.Contains(bpart))
                {
                    ret.Add(bpart);
                }
            }

            return ret;
        }

        /// <summary>
        /// 当前位置是否可以放置BPart
        /// </summary>
        protected bool GetCanPlaceBPart => this.SelectedPartInstance == null ? false : this.SelectedPartInstance.CanPlaceInPlaceMode;
        #endregion

        #region ITickComponent
        private void Start()
        {
            StartCoroutine(LoadMatPackages());
            BuildingManager.Instance.Placer = this;

            // 解析interactions字符串来获取hold的总时间值
            var interactions = BInput.Build.KeyCom.interactions.Split(';');
            foreach (var interaction in interactions)
            {
                if (interaction.StartsWith("Hold"))
                {
                    var parameters = interaction[4..interaction.Length].TrimStart('(').TrimEnd(')').Split(',');
                    foreach (var parameter in parameters)
                    {
                        if (parameter.StartsWith("duration"))
                        {
                            var keyValue = parameter.Split('=');
                            keyComTotalTime = float.Parse(keyValue[1]);
                            break;
                        }
                    }
                    break;
                }
            }
        }


        #region 必须有，但不能动
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion

        /// <summary>
        /// Mode 切换 & Mode 逻辑执行
        /// </summary>
        /// <param name="deltatime"></param>
        public void Tick(float deltatime)
        {
            // None Mode => 设置时自动退出
            if(Mode == BuildingMode.Interact)
            {
                this.HandleInteractMode(deltatime);
            }
            else if(Mode == BuildingMode.Place)
            {
                this.HandlePlaceMode(deltatime);
            }
            else if(Mode == BuildingMode.Edit)
            {
                this.HandleEditMode(deltatime);
            }
            // Destroy Mode => 直接销毁，不需要切入
        }
        #endregion

        #region Input Interaction
        #region InputAction
        /// <summary>
        /// 统一输入
        /// </summary>
        protected Input.BuildingInput BInput => BuildingManager.Instance.BInput;

        protected UnityEngine.InputSystem.InputAction comfirmInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm;

        protected UnityEngine.InputSystem.InputAction backInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Back;
        #endregion

        #region Interact
        protected void HandleInteractMode(float deltatime)
        {
            // 使用BInput.Build
            // 可交互建筑物检测, 获取可交互列表
            this._curTimer += deltatime;
            if (this._curTimer >= this.interactCheckInterval)
            {
                this._curTimer = 0;
                this.InteractBPartList = this.GetSphereCastAllBPart();
                if (this.InteractBPartList.Count > 0)
                {
                    if (this.SelectedPartInstance == null || !this.InteractBPartList.Contains(this.SelectedPartInstance))
                    {
                        this.SelectedPartInstance = this.InteractBPartList[0];
                    }
                }
                else
                {
                    this.SelectedPartInstance = null;
                }

            }

            if (this.SelectedPartInstance != null)
            {
                #region Input.Build
                // 长按开始 => 用于事件调用，与建造系统逻辑无关
                if (BInput.Build.KeyCom.WasPressedThisFrame())
                {
                    this.keyComStartTime = Time.time;
                    this.OnKeyComStart?.Invoke();
                }
                // 长按进行中
                else if (BInput.Build.KeyCom.IsPressed())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // 用于事件调用
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComInProgress?.Invoke(curT, this.keyComTotalTime);
                    }
                    // 长按完成 => 切入次交互轮, 切入后不再是Input.Build，不会重复执行
                    else
                    {
                        // 切入次交互轮 => 依旧是InteractMode
                        this.EnterKeyCom();
                    }
                }
                // 长按结束
                else if (BInput.Build.KeyCom.WasReleasedThisFrame())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // 长按取消
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComCancel?.Invoke(curT, this.keyComTotalTime);
                    }
                    //// 长按退出 => 由次交互轮的输入接管
                    //else
                    //{
                    //    this.OnKeyComExit?.Invoke();
                    //    // 退出KeyCom
                    //    this.ExitKeyCom();
                    //}
                }
                // 切入更改选中的可交互建筑物的外观更改界面 => 依旧处于InteractMode，且选择完或退出时表现仅为关闭UI面板
                else if (BInput.Build.ChangeOutLook.WasPressedThisFrame())
                {
                    this.EnterAppearancePanel();
                }
                // 切入 Edit Mode -> 选中对应BPart, 进入EditMode对其进行移动搬运
                else if (BInput.Build.MoveBuild.WasPressedThisFrame())
                {
                    this.EnterEditMode();
                }
                // 切入 Destroy Mode -> 直接销毁选中BPart
                else if (BInput.Build.DestroyBuild.WasPressedThisFrame())
                {
                    this.EnterDestroyMode();
                }
                #endregion
                #region Input.KeyCom
                // 切入次交互轮后 : 禁用Input.Build 启用Input.Keycom
                else if (BInput.BuildKeyCom.KeyCom.IsPressed())
                {
                    this.HandelKeyCom();
                }
                // 退出次交互轮
                if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
                {
                    this.ExitKeyCom();
                }
                #endregion
                #region Input.Appearance
                else if(this.IsInAppearance)
                {
                    this.HandleAppearance();
                }
                #endregion
                // 没有按键响应时，响应可交互项切换
                else
                {
                    // 轮换可交互列表
                    if (BInput.Build.ChangeInteractiveActor.WasPressedThisFrame())
                    {
                        float t = BInput.Build.ChangeInteractiveActor.ReadValue<float>();
                        int index = Mathf.Abs(t) < 0.3f ? 0 : (t > 0.3f ? 1 : -1);
                        if (index != 0)
                        {
                            index = (this.InteractBPartList.IndexOf(this.SelectedPartInstance) + index) % this.InteractBPartList.Count;
                            this.SelectedPartInstance = this.InteractBPartList[index];
                        }
                    }
                }

                // 按键阻断 => 同一帧只能响应 InteractMode 的一种按键
                //return;
            }
            else
            {
                // 退出次交互轮
                if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
                {
                    this.ExitKeyCom();
                }
            }
           
            // 切入 Place Mode -> 进入建筑物选择界面
            if (!this.IsInAppearance && BInput.Build.SelectBuild.WasPressedThisFrame())
            {
                this.EnterBuildSelection();
            }
            // to-do : to-delete : 仅测试用, 退出建造模式
            else if (!this.IsInAppearance && BInput.Build.enabled &&  ML.Engine.Input.InputManager.Instance.Common.Common.Back.WasPressedThisFrame())
            {
                this.Mode = BuildingMode.None;
            }
        }
        #endregion

        #region Place
        [ShowInInspector, LabelText("CategoryIndex"), FoldoutGroup("PlaceMode")]
        protected int _placeSelectedCategoryIndex = 0;
        [ShowInInspector, LabelText("TypeIndex"), FoldoutGroup("PlaceMode")]
        protected int _placeSelectedTypeIndex = 0;
        [ShowInInspector, LabelText("CategoryArray"), FoldoutGroup("PlaceMode")]
        protected BuildingCategory1[] _placeCanSelectCategory;
        [ShowInInspector, LabelText("TypeArray"), FoldoutGroup("PlaceMode")]
        protected BuildingCategory2[] _placeCanSelectType;

        public BuildingCategory1 _placeSelectedCategory => this._placeCanSelectCategory[this._placeSelectedCategoryIndex];
        public BuildingCategory2 _placeSelectedType => this._placeCanSelectType[this._placeSelectedTypeIndex];
        private byte _placeControlFlow = 0;
        /// <summary>
        /// 0 -> 不处于PlaceMode, 回到InteractMode
        /// 1 -> 一级二级分类选择UI交互
        /// 2 -> 外观选择UI交互
        /// 3 -> 放置
        /// </summary>
        public byte PlaceControlFlow
        {
            get => this._placeControlFlow;
            protected set
            {
                if (this._placeControlFlow == value)
                    return;
                // 离开流程1
                if (this._placeControlFlow == 1)
                {
                    this.OnBuildSelectionExit?.Invoke();
                }
                // 离开流程3 && 不是回到2进行外观选择
                if (this._placeControlFlow == 3 && value != 2)
                {
                    this.OnPlaceModeExit?.Invoke();
                }
                this._placeControlFlow = value;
                if(this._placeControlFlow == 0)
                {
                    this.Mode = BuildingMode.Interact;
                }
                else
                {
                    // 进入流程1
                    if (this._placeControlFlow == 1)
                    {
                        this.OnBuildSelectionEnter?.Invoke(_placeCanSelectCategory, _placeSelectedCategoryIndex, _placeCanSelectType, _placeSelectedTypeIndex);
                    }
                    // 进入流程3
                    if (this._placeControlFlow == 3)
                    {
                        this.EnablePlaceModeInput();
                        this.OnPlaceModeEnter?.Invoke(this.SelectedPartInstance);
                    }
                    this.Mode = BuildingMode.Place;
                }
            }
        }
        protected void HandlePlaceMode(float deltatime)
        {
            // 流程1 : 一级二级分类选择UI交互
            if(this.PlaceControlFlow == 1)
            {
                // 确认
                if(this.comfirmInputAction.WasPressedThisFrame())
                {
                    this.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(this._placeCanSelectCategory[this._placeSelectedCategoryIndex], this._placeCanSelectType[this._placeSelectedTypeIndex]);
                    this.OnBuildSelectionComfirm?.Invoke(this.SelectedPartInstance);
                    this.ExitBuildSelection(3);
                }
                // 取消
                else if(this.backInputAction.WasPressedThisFrame())
                {
                    this.OnBuildSelectionCancel?.Invoke();
                    this.ExitBuildSelection(0);
                }
                // Category
                else if(BInput.BuildSelection.LastCategory.WasPressedThisFrame())
                {
                    this._placeSelectedCategoryIndex = (this._placeSelectedCategoryIndex + this._placeCanSelectCategory.Length - 1) % this._placeCanSelectCategory.Length;
                    this.OnBuildSelectionCategoryChanged?.Invoke(this._placeCanSelectCategory, this._placeSelectedCategoryIndex);
                    this.UpdatePlaceBuildingType(this._placeCanSelectCategory[this._placeSelectedCategoryIndex]);
                }
                else if (BInput.BuildSelection.NextCategory.WasPressedThisFrame())
                {
                    this._placeSelectedCategoryIndex = (this._placeSelectedCategoryIndex + 1) % this._placeCanSelectCategory.Length;
                    this.OnBuildSelectionCategoryChanged?.Invoke(this._placeCanSelectCategory, this._placeSelectedCategoryIndex);
                    this.UpdatePlaceBuildingType(this._placeCanSelectCategory[this._placeSelectedCategoryIndex]);
                }
                // Type
                else if(BInput.BuildSelection.AlternativeType.WasPressedThisFrame())
                {
                    this._placeSelectedTypeIndex = (this._placeSelectedTypeIndex + 1) % this._placeCanSelectType.Length;
                    this.OnBuildSelectionTypeChanged?.Invoke(this._placeCanSelectCategory[this._placeSelectedCategoryIndex], this._placeCanSelectType, this._placeSelectedTypeIndex);
                }
            }
            // 流程2 : 外观选择UI交互(流程1进入时自动选择并跳过) => 不用管，自动跳过
            else if(this.PlaceControlFlow == 2)
            {
                this.HandleAppearance();
            }
            // 流程3 : 放置(可回到流程2手动选择外观)
            else if(this.PlaceControlFlow == 3)
            {
                // 实时更新落点的位置和旋转以及是否可放置
                this.TransformSelectedPartInstance();
                // 长按开始 => 用于事件调用，与建造系统逻辑无关
                if (BInput.BuildPlaceMode.KeyCom.WasPressedThisFrame())
                {
                    this.keyComStartTime = Time.time;
                    this.OnKeyComStart?.Invoke();
                }
                // 长按进行中
                else if (BInput.BuildPlaceMode.KeyCom.IsPressed())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // 用于事件调用
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComInProgress?.Invoke(curT, this.keyComTotalTime);
                    }
                    // 长按完成 => 切入次交互轮, 切入后不再是Input.Build，不会重复执行
                    else
                    {
                        // 切入次交互轮 => 依旧是InteractMode
                        this.EnterKeyCom();
                    }
                }
                // 长按结束
                else if (BInput.BuildPlaceMode.KeyCom.WasReleasedThisFrame())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // 长按取消
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComCancel?.Invoke(curT, this.keyComTotalTime);
                    }
                    //// 长按退出 => 由次交互轮的输入接管
                    //else
                    //{
                    //    this.OnKeyComExit?.Invoke();
                    //    // 退出KeyCom
                    //    this.ExitKeyCom();
                    //}
                }
                #region Input.KeyCom
                // 切入次交互轮后 : 禁用Input.Build 启用Input.Keycom
                else if (BInput.BuildKeyCom.KeyCom.IsPressed())
                {
                    this.HandelKeyCom();
                }
                // 退出次交互轮
                else if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
                {
                    this.ExitKeyCom();
                }
                #endregion
                // 取消 -> 回到InteractMode
                else if (this.backInputAction.WasPressedThisFrame())
                {
                    this.ResetBPart();

                    this.ExitPlaceMode();
                }
                // RotOffset
                else if (!this.IsEnableGridSupport && (BInput.BuildPlaceMode.RotateLeft.IsPressed() || BInput.BuildPlaceMode.RotateRight.IsPressed()))
                {
                    this.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.DisableGridRotRate * deltatime * (BInput.BuildPlaceMode.RotateRight.ReadValue<float>() - BInput.BuildPlaceMode.RotateLeft.ReadValue<float>()), this.SelectedPartInstance.transform.up);
                }
                else if (this.IsEnableGridSupport && BInput.BuildPlaceMode.RotateLeft.WasPressedThisFrame())
                {
                    this.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(-this.EnableGridRotRate, this.SelectedPartInstance.transform.up);
                }
                else if (this.IsEnableGridSupport && BInput.BuildPlaceMode.RotateRight.WasPressedThisFrame())
                {
                    this.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.EnableGridRotRate, this.SelectedPartInstance.transform.up);
                }
                // 轮换ActiveSocket
                else if (BInput.BuildPlaceMode.ChangeActiveSocket.WasPressedThisFrame())
                {
                    this.SelectedPartInstance.AlternativeActiveSocket();
                }
                // Style、Height切换BPart
                else if(BInput.BuildPlaceMode.ChangeStyleLast.WasPressedThisFrame())
                {
                    this.AlternateBPartOnHeight (false);
                    this.OnPlaceModeChangeStyle?.Invoke(this.SelectedPartInstance, false);
                }
                else if (BInput.BuildPlaceMode.ChangeStyleNext.WasPressedThisFrame())
                {
                    this.AlternateBPartOnHeight(true);
                    this.OnPlaceModeChangeStyle?.Invoke(this.SelectedPartInstance, true);
                }
                else if (BInput.BuildPlaceMode.ChangeHeightLast.WasPressedThisFrame())
                {
                    this.AlternateBPartOnStyle(false);
                    this.OnPlaceModeChangeHeight?.Invoke(this.SelectedPartInstance, false);
                }
                else if (BInput.BuildPlaceMode.ChangeHeightNext.WasPressedThisFrame())
                {
                    this.AlternateBPartOnStyle(true);
                    this.OnPlaceModeChangeHeight?.Invoke(this.SelectedPartInstance, true);
                }
                // 回退到外观选择
                else if(BInput.BuildPlaceMode.ChangeOutLook.WasPressedThisFrame())
                {
                    this.EnterAppearancePanel();
                }
                // 确认 -> 放置于新位置, CopyBPartInstacne -> LoopPlace
                else if (this.SelectedPartInstance.CanPlaceInPlaceMode && this.comfirmInputAction.WasPressedThisFrame())
                {
                    this.SelectedPartInstance.Mode = BuildingMode.None;
                    // 拷贝一份
                    var tmp = GameObject.Instantiate(this.SelectedPartInstance.gameObject).GetComponent<BuildingPart.BuildingPart>();

                    // 新建建筑物 -> 赋予实例ID
                    this.SelectedPartInstance.InstanceID = BuildingManager.Instance.GetOneNewBPartInstanceID();
                    this.SelectedPartInstance.OnChangePlaceEvent(this.SelectedPartInstance.transform.position, this.SelectedPartInstance.transform.position);
                    this.OnPlaceModeSuccess?.Invoke(this.SelectedPartInstance);


                    this.SelectedPartInstance = tmp;
                }
            }
        }


        /// <summary>
        /// 进入建筑物选择界面(PlaceMode)
        /// </summary>
        protected void EnterBuildSelection()
        {
            this._placeCanSelectCategory = BuildingManager.Instance.GetRegisteredCategory().Where(c => (int)c >= 0).ToArray();
            this._placeSelectedCategoryIndex = 0;
            this.UpdatePlaceBuildingType(this._placeCanSelectCategory[this._placeSelectedCategoryIndex]);

            this.InteractBPartList.Clear();
            //if(this.SelectedPartInstance != null && this.SelectedPartInstance.Mode == BuildingMode.Place)
            //{
            //    Destroy(this.SelectedPartInstance.gameObject);
            //}
            this.SelectedPartInstance = null;

            // 禁用 Input->Build
            this.PlaceControlFlow = 1;
            // 切换模式时自动禁用
            // BInput.Build.Disable();
            // 启用 Input->PlaceMode => 建筑物选择界面
            BInput.BuildSelection.Enable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flow">切入流程几</param>
        protected void ExitBuildSelection(byte flow)
        {
            this.PlaceControlFlow = flow;

            // to-do : 将流程2中临时记忆的外观材质赋予当前选中的BPartInstance
        }

        protected void EnablePlaceModeInput()
        {
            BInput.BuildPlaceMode.Enable();
            BInput.BuildPlaceMode.ChangeOutLook.Enable();
            BInput.BuildPlaceMode.ChangeStyleLast.Enable();
            BInput.BuildPlaceMode.ChangeStyleNext.Enable();
            BInput.BuildPlaceMode.ChangeHeightLast.Enable();
            BInput.BuildPlaceMode.ChangeHeightNext.Enable();
            BInput.BuildPlaceMode.KeyCom.Enable();
        }

        protected void ExitPlaceMode()
        {
            this.PlaceControlFlow = 0;

            // 启用 Input.Build
            this.Mode = BuildingMode.Interact;
            // 禁用 Input.Place
            BInput.BuildPlaceMode.Disable();
            // 更新BPart状态
            this.SelectedPartInstance = null;

        }

        protected void UpdatePlaceBuildingType(BuildingCategory1 category)
        {
            this._placeCanSelectType = BuildingManager.Instance.GetRegisteredType().Where(type => (int)type >= ((int)category * 100) && (int)type < ((int)category * 100 + 100)).ToArray();

            this._placeSelectedTypeIndex = 0;
            this.OnBuildSelectionTypeChanged?.Invoke(category, this._placeCanSelectType, this._placeSelectedTypeIndex);
        }

        #endregion

        #region Edit
        protected Vector3 _editOldPos;
        protected Quaternion _editOldRotation;

        protected void HandleEditMode(float deltatime)
        {
            // 实时更新落点的位置和旋转以及是否可放置
            this.TransformSelectedPartInstance();
            // 长按开始 => 用于事件调用，与建造系统逻辑无关
            if (BInput.BuildPlaceMode.KeyCom.WasPressedThisFrame())
            {
                this.keyComStartTime = Time.time;
                this.OnKeyComStart?.Invoke();
            }
            // 长按进行中
            else if (BInput.BuildPlaceMode.KeyCom.IsPressed())
            {
                float curT = Time.time - this.keyComStartTime;
                // 用于事件调用
                if (curT < this.keyComTotalTime)
                {
                    this.OnKeyComInProgress?.Invoke(curT, this.keyComTotalTime);
                }
                // 长按完成 => 切入次交互轮, 切入后不再是Input.Build，不会重复执行
                else
                {
                    // 切入次交互轮 => 依旧是InteractMode
                    this.EnterKeyCom();
                }
            }
            // 长按结束
            else if (BInput.BuildPlaceMode.KeyCom.WasReleasedThisFrame())
            {
                float curT = Time.time - this.keyComStartTime;
                // 长按取消
                if (curT < this.keyComTotalTime)
                {
                    this.OnKeyComCancel?.Invoke(curT, this.keyComTotalTime);
                }
                //// 长按退出 => 由次交互轮的输入接管
                //else
                //{
                //    this.OnKeyComExit?.Invoke();
                //    // 退出KeyCom
                //    this.ExitKeyCom();
                //}
            }
            #region Input.KeyCom
            // 切入次交互轮后 : 禁用Input.Build 启用Input.Keycom
            else if (BInput.BuildKeyCom.KeyCom.IsPressed())
            {
                this.HandelKeyCom();
            }
            // 退出次交互轮
            else if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
            {
                this.ExitKeyCom();
            }
            #endregion
            // 取消 -> 回到原状态
            else if (this.backInputAction.WasPressedThisFrame())
            {
                this.SelectedPartInstance.transform.position = this._editOldPos;
                this.SelectedPartInstance.transform.rotation = this._editOldRotation;
                this.ExitEditMode();
            }
            // RotOffset
            else if (!this.IsEnableGridSupport && (BInput.BuildPlaceMode.RotateLeft.IsPressed() || BInput.BuildPlaceMode.RotateRight.IsPressed()))
            {
                this.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.DisableGridRotRate * deltatime * (BInput.BuildPlaceMode.RotateRight.ReadValue<float>() - BInput.BuildPlaceMode.RotateLeft.ReadValue<float>()), this.SelectedPartInstance.transform.up);
            }
            else if (this.IsEnableGridSupport && BInput.BuildPlaceMode.RotateLeft.WasPressedThisFrame())
            {
                this.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(-this.EnableGridRotRate, this.SelectedPartInstance.transform.up);
            }
            else if (this.IsEnableGridSupport && BInput.BuildPlaceMode.RotateRight.WasPressedThisFrame())
            {
                this.SelectedPartInstance.RotOffset *= Quaternion.AngleAxis(this.EnableGridRotRate, this.SelectedPartInstance.transform.up);
            }
            // 轮换ActiveSocket
            else if(BInput.BuildPlaceMode.ChangeActiveSocket.WasPressedThisFrame())
            {
                this.SelectedPartInstance.AlternativeActiveSocket();
            }
            // 确认 -> 放置于新位置, 回到InteractMode
            else if (this.SelectedPartInstance.CanPlaceInPlaceMode && this.comfirmInputAction.WasPressedThisFrame())
            {
                this.SelectedPartInstance.OnChangePlaceEvent(this._editOldPos, this.SelectedPartInstance.transform.position);

                this.ExitEditMode();
            }

        }

        /// <summary>
        /// 进入Edit移动搬运模式
        /// </summary>
        protected void EnterEditMode()
        {
            if (!this.SelectedPartInstance.CanEnterEditMode())
            {
                return;
            }
            // 禁用 Input.Build
            this.Mode = BuildingMode.Edit;
            // 启用 Input.Edit
            this.EnableEditModeInput();

            // 记录原 Transform
            this._editOldPos = this.SelectedPartInstance.transform.position;
            this._editOldRotation = this.SelectedPartInstance.transform.rotation;

            this.OnEditModeEnter?.Invoke(this.SelectedPartInstance);
        }

        protected void ExitEditMode()
        {
            // 启用 Input.Build
            this.Mode = BuildingMode.Interact;
            // 禁用 Input.Edit
            BInput.BuildPlaceMode.Disable();

            this.SelectedPartInstance = null;

            this.OnEditModeExit?.Invoke(this.SelectedPartInstance);
        }


        protected void EnableEditModeInput()
        {
            BInput.BuildPlaceMode.Enable();
            BInput.BuildPlaceMode.ChangeOutLook.Disable();
            BInput.BuildPlaceMode.ChangeStyleLast.Disable();
            BInput.BuildPlaceMode.ChangeStyleNext.Disable();
            BInput.BuildPlaceMode.ChangeHeightLast.Disable();
            BInput.BuildPlaceMode.ChangeHeightNext.Disable();
            //BInput.BuildPlaceMode.KeyCom.Disable();
        }

        #endregion

        #region Destroy
        /// <summary>
        /// 直接销毁BPart
        /// </summary>
        protected void EnterDestroyMode()
        {
            if (!this.SelectedPartInstance.CanEnterDestoryMode())
            {
                return;
            }

            var tmp = this.SelectedPartInstance;

            tmp.OnBPartDestroy();

            this.SelectedPartInstance = null;

            this.OnDestroySelectedBPart?.Invoke(tmp);

            // to-do :后续可能会更改销毁调用
            Destroy(tmp.gameObject);
        }
        #endregion

        #region Appearance
        private Texture2D[] _aCurrentTexs;
        private BuildingCopiedMaterial[] _aCurrentMatPackages;
        private int _aCurrentIndex;
        private Dictionary<BuildingPartClassification, Engine.BuildingSystem.BSBPartMatPackage> _allMatPackages;

        private BuildingMode _aMode;
        private BuildingCopiedMaterial _aMat;

        protected void HandleAppearance()
        {
            // 确认选择的材质
            if (this.comfirmInputAction.WasPressedThisFrame())
            {
                //this.ReplaceSelectedAppearance();
                this.ExitAppearancePanel();
            }
            // 取消外观编辑
            else if(this.backInputAction.WasPressedThisFrame())
            {
                this.SelectedPartInstance.SetCopiedMaterial(this._aMat);
                this.ExitAppearancePanel();
            }
            // 切换材质
            else if(BInput.BuildingAppearance.AlterMaterial.WasPressedThisFrame())
            {
                Vector2 offset = BInput.BuildingAppearance.AlterMaterial.ReadValue<Vector2>();
                if(offset != Vector2.zero)
                {
                    this.SwitchSelectedAppearance(offset);
                }
            }
        }

        /// <summary>
        /// 切换选中的外观材质
        /// </summary>
        /// <param name="offset"></param>
        protected void SwitchSelectedAppearance(Vector2 offset)
        {
            if(Mathf.Abs(offset.x) < 0.01)
            {
                return;
            }
            int of = offset.x > 0 ? 1 : -1;

            this._aCurrentIndex = (this._aCurrentIndex + of + _aCurrentTexs.Length) % _aCurrentTexs.Length;
            this.SelectedPartInstance.SetCopiedMaterial(this._aCurrentMatPackages[this._aCurrentIndex]);
            OnChangeAppearance?.Invoke(this._aCurrentTexs, this._aCurrentMatPackages, this._aCurrentIndex);
        }

        ///// <summary>
        ///// 为SelectedBPart替换选中的外观材质
        ///// </summary>
        //protected void ReplaceSelectedAppearance()
        //{
        //    this.SelectedPartInstance.SetCopiedMaterial(this._aCurrentMatPackages[this._aCurrentIndex]);
        //}

        /// <summary>
        /// 进入外观选择界面
        /// </summary>
        protected void EnterAppearancePanel()
        {
            // PlaceMode
            if (this.Mode == BuildingMode.Place)
            {
                this.PlaceControlFlow = 2;

                // 禁用 Input.PlaceMode
                BInput.BuildPlaceMode.Disable();
                // 启用 Input.Appearance
                BInput.BuildingAppearance.Enable();
            }
            else if (this.Mode == BuildingMode.Interact)
            {
                // 禁用 Input.Build
                BInput.Build.Disable();
                // 启用 Input.Appearance
                BInput.BuildingAppearance.Enable();
            }

            // 进入外观选择状态
            this.IsInAppearance = true;

            this._aMode = this.SelectedPartInstance.Mode;
            this._aMat = this.SelectedPartInstance.GetCopiedMaterial();
            this.SelectedPartInstance.Mode = BuildingMode.None;

            var kv = _allMatPackages[this.SelectedPartInstance.Classification].ToMatPackage();

            this._aCurrentTexs = kv.Keys.ToArray();
            this._aCurrentMatPackages = kv.Values.ToArray();
            var mat = this.SelectedPartInstance.GetCopiedMaterial();
            this._aCurrentIndex = System.Array.IndexOf(this._aCurrentMatPackages, mat);

            this.OnEnterAppearance?.Invoke(this.SelectedPartInstance, this._aCurrentTexs, this._aCurrentMatPackages, this._aCurrentIndex);
        }

        protected void ExitAppearancePanel()
        {
            // PlaceMode
            if (this.Mode == BuildingMode.Place)
            {
                this.PlaceControlFlow = 3;

                // 启用 Input.PlaceMode
                this.EnablePlaceModeInput();
                // 禁用 Input.Appearance
                BInput.BuildingAppearance.Disable();
            }
            else if (this.Mode == BuildingMode.Interact)
            {
                // 启用 Input.Build
                BInput.Build.Enable();
                // 禁用 Input.Appearance
                BInput.BuildingAppearance.Disable();
            }

            this.SelectedPartInstance.Mode = this._aMode;
            // 退出外观选择状态
            this.IsInAppearance = false;
            this.OnExitAppearance?.Invoke(this.SelectedPartInstance);
        }

        private const string MatPABPath = "Assets/BuildingSystem";
        protected IEnumerator LoadMatPackages()
        {
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            while (Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(MatPABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var packages = ab.LoadAllAssetsAsync<BSBPartMatPackage>();
            yield return packages;
            _allMatPackages = new Dictionary<BuildingPartClassification, BSBPartMatPackage>();
            foreach (BSBPartMatPackage package in packages.allAssets)
            {
                this._allMatPackages.Add(package.Classification, package);
            }

            //abmgr.UnLoadLocalABAsync(TextContentABPath, false, null);

#if UNITY_EDITOR
            Debug.Log("LoadMatPackages cost time: " + (Time.realtimeSinceStartup - startT));
#endif

            this.enabled = false;
        }
        #endregion

        #region 次交互轮
        protected void HandelKeyCom()
        {
            if (this.SelectedPartInstance == null)
            {
                return;
            }
            // CopyBuild
            if (BInput.BuildKeyCom.CopyBuild.WasPressedThisFrame())
            {
                // 辅助网格
                if (this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Edit)
                {
                    this.IsEnableGridSupport = !IsEnableGridSupport;
                }
                // 复制建筑物
                else
                {
                    // 复制当前选中的可交互物进入PlaceMode

                    // 复制一份当前选中的BPart(即一级二级分类选择和外观选择的流程)

                    var bpart = BuildingManager.Instance.GetOneBPartCopyInstance(this.SelectedPartInstance);
                    if(bpart == this.SelectedPartInstance)
                    {
                        this.ExitKeyCom();
                        // 进入PlaceMode的放置流程
                        this.PlaceControlFlow = 3;
                    }
                    // to-do : 待商定
                    // 不能复制则直接进入Edit模式
                    //else
                    //{
                    //    this.ExitKeyCom();
                    //    this.EnterEditMode();
                    //}
                }
            }
            // CopyOutLook
            else if(BInput.BuildKeyCom.CopyOutLook.WasPressedThisFrame())
            {
                // 复制当前选中的可交互建筑物的材质
                BuildingManager.Instance.CopyBPartMaterial(this.SelectedPartInstance);
            }
            // PasteOutLook
            else if (BInput.BuildKeyCom.PasteOutLook.WasPressedThisFrame())
            {
                // 拷贝复制的材质到当前选中的可交互建筑物
                BuildingManager.Instance.PasteBPartMaterial(this.SelectedPartInstance);
            }
        }

        protected void EnterKeyCom()
        {
            if (this.Mode == BuildingMode.Interact)
            {
                // 禁用 Input.Build
                BInput.Build.Disable();
            }

            else
            {
                BInput.BuildPlaceMode.Disable();
            }
            // 启用 Input.KeyCom
            BInput.BuildKeyCom.Enable();
            this.OnKeyComComplete?.Invoke();

        }

        protected void ExitKeyCom()
        {
            // 避免没有松开KeyCom, 再次进入时直接打开
            this.keyComStartTime = Time.time;

            if(this.Mode == BuildingMode.Interact)
            {
                // 启用 Input.Build
                BInput.Build.Enable();
            }
            else
            {
                BInput.BuildPlaceMode.Enable();
            }

            // 禁用 Input.KeyCom
            BInput.BuildKeyCom.Disable();
            this.OnKeyComExit?.Invoke();
        }
        #endregion

        #endregion

    }
}

