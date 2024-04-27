using ML.Engine.BuildingSystem.BuildingPart;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ML.Engine.BuildingSystem.Config;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ML.Engine.BuildingSystem.BuildingPlacer
{
    // to-do : 使用时需在 BuildingManager 注册
    public class BuildingPlacer : MonoBehaviour
    {
        #region 建造模式
        //[LabelText("建造模式"), ShowInInspector, PropertyOrder(-1)]
        private MonoBuildingManager monoBM ;

        //好像是BuildingPlacer是在MonoBuildingManager初始化里面被创建的，此时MonoBuildingManager还没注册拿不到
        public MonoBuildingManager MONOBM
        {
            get
            {
                if (this.monoBM == null)
                {
                    monoBM = ProjectOC.ManagerNS.LocalGameManager.Instance.MonoBuildingManager;
                }
                return monoBM;
            }
        }
        
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
            BInput.Build.Enable();

            MONOBM.PushPanel<UI.BSInteractModePanel>();

            this.OnBuildingModeEnter?.Invoke();
        }

        protected void ExitBuildingMode()
        {
            this.InteractBPartList.Clear();

            this.SelectedPartInstance = null;

            MONOBM.PopPanel();

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
        /// 在销毁BPart时调用
        /// </summary>
        public event System.Action<IBuildingPart> OnDestroySelectedBPart;

        /// <summary>
        /// 一次成功移动
        /// 旧位置, 新位置
        /// </summary>
        public event System.Action<IBuildingPart, Vector3, Vector3> OnEditModeSuccess;



        public event System.Action<IBuildingPart> OnPlaceModeChangeBPart;
        /// <summary>
        /// 一次成功放置
        /// </summary>
        public event System.Action<IBuildingPart> OnPlaceModeSuccess;
        #endregion

        #region MatchConfig
        public Config.BuildingAreaSocketMatchAsset AreaSocketMatch { get; private set; }
        public Config.BuildingSocket2SocketMatchAsset Socket2SocketMatch { get; private set; }
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
/*                if (value == null)
                {
                    Debug.Log("QWQ" + Time.frameCount);
                }*/
                if (this.selectedPartInstance != null)
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
                Manager.GameManager.DestroyObj(this.SelectedPartInstance.gameObject);
                this.SelectedPartInstance = null;
            }
        }

        /// <summary>
        /// 基于样式Style轮换当前BPart -> 更改Height
        /// </summary>
        public void AlternateBPartOnStyle(bool isForward)
        {
            if (BuildingManager.Instance.GetBPartPrefabCountOnStyle(this.SelectedPartInstance) < 1)
            {
                return;
            }
            var tmp = BuildingManager.Instance.PollingBPartPeekInstanceOnStyle(this.SelectedPartInstance, isForward);
            //tmp.BaseRotation = SelectedPartInstance.BaseRotation;
            tmp.RotOffset = SelectedPartInstance.RotOffset;
            tmp.Mode = this.SelectedPartInstance.Mode;
            this.ResetBPart();
            this.SelectedPartInstance = tmp;
            // to-do : 只有PlaceMode会调用
            this.OnPlaceModeChangeBPart?.Invoke(this.SelectedPartInstance);
        }

        /// <summary>
        /// 基于高度Height轮换当前BPart -> 更改Style
        /// </summary>
        public void AlternateBPartOnHeight(bool isForward)
        {
            if(BuildingManager.Instance.GetBPartPrefabCountOnHeight(this.SelectedPartInstance) < 1)
            {
                return;
            }
            var tmp = BuildingManager.Instance.PollingBPartPeekInstanceOnHeight(this.SelectedPartInstance, isForward);
            //tmp.BaseRotation = SelectedPartInstance.BaseRotation;
            tmp.RotOffset = SelectedPartInstance.RotOffset;
            tmp.Mode = this.SelectedPartInstance.Mode;
            this.ResetBPart();
            this.SelectedPartInstance = tmp;
            // to-do : 只有PlaceMode会调用
            this.OnPlaceModeChangeBPart?.Invoke(this.SelectedPartInstance);
        }

        public bool ComfirmPlaceBPart()
        {
            if (this.SelectedPartInstance.CanPlaceInPlaceMode)
            {
                this.SelectedPartInstance.Mode = BuildingMode.None;
                // 拷贝一份
                var tmp = GameObject.Instantiate(this.SelectedPartInstance.gameObject).GetComponent<BuildingPart.IBuildingPart>();

                // 新建建筑物 -> 赋予实例ID
                this.SelectedPartInstance.InstanceID = BuildingManager.Instance.GetOneNewBPartInstanceID();
                this.SelectedPartInstance.OnChangePlaceEvent(this.SelectedPartInstance.gameObject.transform.position, this.SelectedPartInstance.gameObject.transform.position);
                this.OnPlaceModeSuccess?.Invoke(this.SelectedPartInstance);


                this.SelectedPartInstance = tmp;
                return true;
            }
            return false;

        }

        public bool ComfirmEditBPart(Vector3 _editOldPos)
        {
            if(this.SelectedPartInstance.CanPlaceInPlaceMode)
            {
                this.SelectedPartInstance.OnChangePlaceEvent(_editOldPos, this.SelectedPartInstance.gameObject.transform.position);

                this.OnEditModeSuccess?.Invoke(this.SelectedPartInstance, _editOldPos, this.SelectedPartInstance.gameObject.transform.position);

                return true;
            }
            return false;
        }

        /// <summary>
        /// 直接销毁BPart
        /// </summary>
        public void DestroySelectedBPart()
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
            Manager.GameManager.DestroyObj(tmp.gameObject);
        }

        public void EnablePlayerInput()
        {
            // to-do
            ProjectOC.Input.InputManager.PlayerInput.Player.Enable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.MouseScroll.Disable();
            ProjectOC.Input.InputManager.PlayerInput.Player.OpenBotUI.Disable();
        }

        public void DisablePlayerInput()
        {
            // to-do
            ProjectOC.Input.InputManager.PlayerInput.Player.Disable();

        }
        #endregion

        #region Transform
        #region 落点检测
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
                var ray = this.GetCameraRay();

                // ScreenCenter->ScreenNormal 射线检测
                var hits = Physics.RaycastAll(ray, this.checkRadius, this.checkLayer);
                if (hits == null || hits.Length == 0)
                {
                    ray = this.GetCameraRayEndPointDownRay();
                    hits = Physics.RaycastAll(ray, this.checkRadius, this.checkLayer);
                }
                // 命中 || 未命中 => 在终点处向下检测
                if (hits != null && hits.Length > 0)
                {
                    bool bisHit = false;
                    bool bSocket = false;
                    RaycastHit hitInfo = hits[0];
                    int tmp = 0;
                    // 找到第一个可以匹配的，Socket优先级更高
                    for(int i = 0; i < hits.Length; ++i)
                    {
                        hitInfo = hits[i];
                        var socket = hitInfo.collider.GetComponentInParent<BuildingSocket.BuildingSocket>();
                        if (socket != null && this.SelectedPartInstance.ActiveSocket.CheckMatch(socket))
                        {
                            bisHit = true;
                            bSocket = true;
                            tmp = i;
                            break;
                        }
                        if(!bisHit)
                        {
                            var area = hitInfo.collider.GetComponentInParent<BuildingArea.BuildingArea>();
                            if (area != null && area.CheckAreaTypeMatch(this.SelectedPartInstance))
                            {
                                bisHit = true;
                                tmp = i;
                            }
                        }
                    }
                    if (bisHit)
                    {
                        hitInfo = hits[tmp];
                        for (int i = tmp + 1; i < hits.Length; ++i)
                        {
                            var h = hits[i];
                            var socket = h.collider.GetComponentInParent<BuildingSocket.BuildingSocket>();
                            if (socket != null && this.SelectedPartInstance.ActiveSocket.CheckMatch(socket))
                            {
                                if (h.distance < hitInfo.distance || (h.distance == hitInfo.distance && Vector3.Distance(h.collider.transform.position, this.transform.position) <= Vector3.Distance(hitInfo.collider.transform.position, this.transform.position)))
                                {
                                    bSocket = true;
                                    hitInfo = h;
                                }
                            }
                            else if (bSocket == false)
                            {
                                var area = h.collider.GetComponentInParent<BuildingArea.BuildingArea>();
                                // 类型匹配 & 位于正面
                                if (area != null && area.CheckAreaTypeMatch(this.SelectedPartInstance) && (Vector3.Dot(ray.direction, area.transform.up) < 0))
                                {
                                    if (h.distance < hitInfo.distance || (h.distance == hitInfo.distance && Vector3.Distance(h.collider.transform.position, this.transform.position) <= Vector3.Distance(hitInfo.collider.transform.position, this.transform.position)))
                                    {
                                        hitInfo = h;
                                    }
                                }
                            }
                        }

                        pos = hitInfo.point;
                        this.SelectedPartInstance.AttachedSocket = hitInfo.collider.GetComponentInParent<BuildingSocket.BuildingSocket>();
                        this.SelectedPartInstance.AttachedArea = hitInfo.collider.GetComponentInParent<BuildingArea.BuildingArea>();

                        if (this.SelectedPartInstance.AttachedSocket && !this.SelectedPartInstance.ActiveSocket.CheckMatch(this.SelectedPartInstance.AttachedSocket))
                        {
                            this.SelectedPartInstance.AttachedSocket = null;
                        }
                        if (this.SelectedPartInstance.AttachedArea && !this.SelectedPartInstance.AttachedArea.CheckAreaTypeMatch(this.SelectedPartInstance))
                        {
                            this.SelectedPartInstance.AttachedArea = null;
                        }
                    }

                    else
                    {
                        this.SelectedPartInstance.AttachedSocket = null;
                        this.SelectedPartInstance.AttachedArea = null;
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
                        pos = tmpP - (this.SelectedPartInstance.ActiveSocket.transform.position - this.SelectedPartInstance.transform.position);
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

        // private Vector3 _pos;
        [SerializeField]Vector3 posOffset;
        /// <summary>p
        /// 应用位置和旋转于SelectedPartInstance
        /// </summary>
        public void TransformSelectedPartInstance()
        {
            if (this.GetPlacePosAndRot(out Vector3 pos, out Quaternion rot))
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
                this.SelectedPartInstance.transform.RotateAround(rotAroundPoint, this.SelectedPartInstance.transform.right, euler.x);
                // Forward-Z
                this.SelectedPartInstance.transform.RotateAround(rotAroundPoint, this.SelectedPartInstance.transform.forward, euler.z);
                
                if (this.SelectedPartInstance.AttachedArea == null && this.SelectedPartInstance.AttachedSocket == null)
                {
                    this.SelectedPartInstance.transform.position = pos + this.Camera.transform.rotation * posOffset;
                }
            }
            // else if((_pos - pos).sqrMagnitude >0.001f)
            else
            {
                // _pos = pos;
                
                this.SelectedPartInstance.transform.position = pos + posOffset;
            }
        }

        #endregion

        #region 交互检测

        [LabelText("检测半径"), SerializeField, FoldoutGroup("交互检测"), PropertyTooltip("单位 m")]
        protected float interactRadius = 5;

        [LabelText("检测层"), SerializeField, FoldoutGroup("交互检测")]
        protected LayerMask interactLayer;

        [LabelText("球形半径"), SerializeField, FoldoutGroup("交互检测"), PropertyTooltip("单位 m")]
        protected float sphereRadius = 0.5f;

        [LabelText("检测时间间隔"), SerializeField, FoldoutGroup("交互检测"), PropertyTooltip("单位 s")]
        protected float interactCheckInterval = 0.1f;
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

        public event System.Action<IBuildingPart> OnSelectBPart;

        public void CheckInteractBPart(float deltatime)
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
                        OnSelectBPart?.Invoke(this.SelectedPartInstance);
                    }
                }
                else
                {
                    this.SelectedPartInstance = null;
                    OnSelectBPart?.Invoke(this.SelectedPartInstance);
                }

            }
        }

        public void SwitchInteractBPart(int offset)
        {
            if(offset != 0)
            {
                int index = offset > 0 ? 1 : -1;
                index = (this.InteractBPartList.IndexOf(this.SelectedPartInstance) + index) % this.InteractBPartList.Count;
                this.SelectedPartInstance = this.InteractBPartList[index];
            }
        }

        #endregion

        #endregion

        #region Unity
        private void Awake()
        {
            // 载入匹配数据
            string path = "Config";
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<BuildingAreaSocketMatchAsset>(path + "/Config_BS_Match_AreaSocket.asset").Completed += (handle) =>
            {
                AreaSocketMatch = handle.Result;
            };
            Manager.GameManager.Instance.ABResourceManager.LoadAssetAsync<BuildingSocket2SocketMatchAsset>(path + "/Config_BS_Match_Socket2Socket.asset").Completed += (handle) =>
            {
                Socket2SocketMatch = handle.Result;
            };
            this.enabled = false;
        }

        public void OnDrawGizmos()
        {
            if (BuildingManager.Instance == null || this.SelectedPartInstance == null)
            {
                return;
            }

            //// 绘制射线
            //var ray = GetCameraRay();
            //Gizmos.color = Color.red;
            //Vector3 start = ray.origin;
            //Vector3 end = ray.origin + ray.direction * this.checkRadius;
            //// 设置线的宽度
            //float lineWidth = 0.1f;
            //// 计算线的中心点
            //Vector3 center = (start + end) * 0.5f;
            //// 计算线的长度和方向
            //float lineLength = Vector3.Distance(start, end);
            //Vector3 lineDirection = ray.direction;
            //// 计算长方体的尺寸
            //Vector3 cubeSize = new Vector3(lineWidth, lineWidth, lineLength);
            //// 将长方体绘制在线的中心点，并根据线的方向和长度进行旋转
            //Matrix4x4 cubeMatrix = Matrix4x4.TRS(center, Quaternion.LookRotation(lineDirection), cubeSize);
            //var m = Gizmos.matrix;
            //Gizmos.matrix = cubeMatrix;
            //Gizmos.DrawCube(Vector3.zero, Vector3.one);
            //Gizmos.matrix = m;

            if (this.SelectedPartInstance.ActiveSocket != null)
            {
                var cols = this.SelectedPartInstance.gameObject.GetComponentsInChildren<Collider>();
                foreach (Collider col in cols)
                {
                    var gcolor = Gizmos.color;
                    Gizmos.color = BuildingManager.Instance.DrawActiveSocket.color;
                    Gizmos.DrawSphere(this.SelectedPartInstance.ActiveSocket.transform.position, 0.1f);
                    //Extension.GizmosExtension.DrawMeshCollider(col, BuildingManager.Instance.DrawActiveSocket.color);
                    Gizmos.color = gcolor;
                }
            }
            if(this.SelectedPartInstance.AttachedSocket != null)
            {
                var cols = this.SelectedPartInstance.AttachedSocket.GetComponentsInChildren<Collider>();
                foreach (Collider col in cols)
                {
                    Extension.GizmosExtension.DrawMeshCollider(col, BuildingManager.Instance.DrawSocket.color);
                }
            }
            if (this.SelectedPartInstance.AttachedArea != null)
            {
                var cols = this.SelectedPartInstance.AttachedArea.GetComponentsInChildren<Collider>();
                foreach (Collider col in cols)
                {
                    Extension.GizmosExtension.DrawMeshCollider(col, BuildingManager.Instance.DrawAreaBaseGrid.color);
                }
            }
        }
        #endregion

        #region InputAction
        /// <summary>
        /// 统一输入
        /// </summary>
        public Input.BuildingInput BInput => BuildingManager.Instance.BInput;

        public UnityEngine.InputSystem.InputAction comfirmInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Confirm;

        public UnityEngine.InputSystem.InputAction backInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Back;
        #endregion

    }
}

