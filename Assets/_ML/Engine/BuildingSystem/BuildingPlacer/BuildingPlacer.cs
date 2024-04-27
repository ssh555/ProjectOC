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
    // to-do : ʹ��ʱ���� BuildingManager ע��
    public class BuildingPlacer : MonoBehaviour
    {
        #region ����ģʽ
        //[LabelText("����ģʽ"), ShowInInspector, PropertyOrder(-1)]
        private MonoBuildingManager monoBM ;

        //������BuildingPlacer����MonoBuildingManager��ʼ�����汻�����ģ���ʱMonoBuildingManager��ûע���ò���
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
        /// ��ǰ����ģʽ
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
            // ������
            if (cur == BuildingMode.None && pre != BuildingMode.None)
            {
                this.ExitBuildingMode();
            }
            // ����
            else if (pre == BuildingMode.None && cur != BuildingMode.None)
            {
                this.EnterBuildingMode();
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// ���뽨��ģʽʱ����
        /// </summary>
        public event System.Action OnBuildingModeEnter;
        /// <summary>
        /// �˳�����ģʽʱ����
        /// </summary>
        public event System.Action OnBuildingModeExit;

        /// <summary>
        /// ������BPartʱ����
        /// </summary>
        public event System.Action<IBuildingPart> OnDestroySelectedBPart;

        /// <summary>
        /// һ�γɹ��ƶ�
        /// ��λ��, ��λ��
        /// </summary>
        public event System.Action<IBuildingPart, Vector3, Vector3> OnEditModeSuccess;



        public event System.Action<IBuildingPart> OnPlaceModeChangeBPart;
        /// <summary>
        /// һ�γɹ�����
        /// </summary>
        public event System.Action<IBuildingPart> OnPlaceModeSuccess;
        #endregion

        #region MatchConfig
        public Config.BuildingAreaSocketMatchAsset AreaSocketMatch { get; private set; }
        public Config.BuildingSocket2SocketMatchAsset Socket2SocketMatch { get; private set; }
        #endregion

        #region ������
        /// <summary>
        /// ������
        /// </summary>
        [LabelText("����������"), FoldoutGroup("������")]
        public bool IsEnableGridSupport = true;

        /// <summary>
        /// ����������ʱ����ת��
        /// </summary>
        [LabelText("����ʱ��ת��"), FoldoutGroup("������"), Range(0, 180)]
        public float DisableGridRotRate = 0.5f;
        /// <summary>
        /// ����������ʱ����ת��
        /// </summary>
        [LabelText("����ʱ��ת��"), FoldoutGroup("������"), Range(0, 180)]
        public float EnableGridRotRate = 15f;
        #endregion

        #region BPart
        private IBuildingPart selectedPartInstance;
        /// <summary>
        /// ��ǰѡ�н����Ľ�����
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
                // �ӿɽ����б���Ѱ����һ��
                else
                {
                    // û����һ��
                    if(this.InteractBPartList.Count < 2)
                    {
                        this.selectedPartInstance = value;
                    }
                    // ����һ��
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
        /// �����������͸���BPart
        /// </summary>
        /// <param name="Category"></param>
        /// <param name="Type"></param>
        public void ChangeBPart(BuildingCategory1 Category, BuildingCategory2 Type)
        {
            this.ResetBPart();
            this.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(Category, Type);
        }

        /// <summary>
        /// ����BPart, ����Ϊnull
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
        /// ������ʽStyle�ֻ���ǰBPart -> ����Height
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
            // to-do : ֻ��PlaceMode�����
            this.OnPlaceModeChangeBPart?.Invoke(this.SelectedPartInstance);
        }

        /// <summary>
        /// ���ڸ߶�Height�ֻ���ǰBPart -> ����Style
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
            // to-do : ֻ��PlaceMode�����
            this.OnPlaceModeChangeBPart?.Invoke(this.SelectedPartInstance);
        }

        public bool ComfirmPlaceBPart()
        {
            if (this.SelectedPartInstance.CanPlaceInPlaceMode)
            {
                this.SelectedPartInstance.Mode = BuildingMode.None;
                // ����һ��
                var tmp = GameObject.Instantiate(this.SelectedPartInstance.gameObject).GetComponent<BuildingPart.IBuildingPart>();

                // �½������� -> ����ʵ��ID
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
        /// ֱ������BPart
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

            // to-do :�������ܻ�������ٵ���
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
        #region �����
        [LabelText("ʹ�õ�Camera"), SerializeField, FoldoutGroup("�����")]
        protected new Camera camera = null;
        /// <summary>
        /// ʹ�õ������ => ΪnullʱĬ��ʹ�� Camera.main
        /// </summary>
        public Camera Camera
        {
            get
            {
                return camera == null ? Camera.main : camera;
            }
            set => camera = value;
        }

        [LabelText("���뾶"), SerializeField, FoldoutGroup("�����"), PropertyTooltip("��λ m")]
        protected float checkRadius = 5;

        [LabelText("����"), SerializeField, FoldoutGroup("�����")]
        protected LayerMask checkLayer;

        /// <summary>
        /// ��ȡ���������Ray
        /// </summary>
        /// <returns></returns>
        protected Ray GetCameraRay()
        {
            return new Ray(Camera.transform.position, Camera.transform.forward);
        }

        /// <summary>
        /// �������������յ㴦���������µ�����
        /// </summary>
        /// <returns></returns>
        protected Ray GetCameraRayEndPointDownRay()
        {
            return new Ray(GetCameraRay().GetPoint(this.checkRadius), Vector3.down);
        }


        /// <summary>
        /// ��ȡBPart��ǰ����λ�ú���ת
        /// λ�� => SelectedPartInstance.Pos = pos
        /// ��ת => SelectedPartInstance.Rot = RotateByAxis(Axis = ActiveSocket.UpAxis, rot)
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        protected bool GetPlacePosAndRot(out Vector3 pos, out Quaternion rot)
        {
            if (this.SelectedPartInstance != null)
            {
                Vector3 oldP = this.SelectedPartInstance.transform.position;
                var oldR = this.SelectedPartInstance.transform.rotation;

                // ��0
                this.SelectedPartInstance.transform.position = this.SelectedPartInstance.ActiveSocket.transform.position - this.SelectedPartInstance.ActiveSocket.transform.localPosition;
                this.SelectedPartInstance.transform.rotation = Quaternion.identity;

                // λ�� -> ����
                pos = this.transform.position - this.SelectedPartInstance.ActiveSocket.transform.position + this.SelectedPartInstance.transform.position;
                var ray = this.GetCameraRay();

                // ScreenCenter->ScreenNormal ���߼��
                var hits = Physics.RaycastAll(ray, this.checkRadius, this.checkLayer);
                if (hits == null || hits.Length == 0)
                {
                    ray = this.GetCameraRayEndPointDownRay();
                    hits = Physics.RaycastAll(ray, this.checkRadius, this.checkLayer);
                }
                // ���� || δ���� => ���յ㴦���¼��
                if (hits != null && hits.Length > 0)
                {
                    bool bisHit = false;
                    bool bSocket = false;
                    RaycastHit hitInfo = hits[0];
                    int tmp = 0;
                    // �ҵ���һ������ƥ��ģ�Socket���ȼ�����
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
                                // ����ƥ�� & λ������
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

                // ��ת -> ����
                rot = this.SelectedPartInstance.BaseRotation * this.SelectedPartInstance.RotOffset;

                Vector3 tmpP;
                Quaternion tmpR;
                // λ��&��ת -> AttachedArea
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
                // λ��&��ת -> AttachedSocket
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
        /// Ӧ��λ�ú���ת��SelectedPartInstance
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

        #region �������

        [LabelText("���뾶"), SerializeField, FoldoutGroup("�������"), PropertyTooltip("��λ m")]
        protected float interactRadius = 5;

        [LabelText("����"), SerializeField, FoldoutGroup("�������")]
        protected LayerMask interactLayer;

        [LabelText("���ΰ뾶"), SerializeField, FoldoutGroup("�������"), PropertyTooltip("��λ m")]
        protected float sphereRadius = 0.5f;

        [LabelText("���ʱ����"), SerializeField, FoldoutGroup("�������"), PropertyTooltip("��λ s")]
        protected float interactCheckInterval = 0.1f;
        private float _curTimer = 0f;

        [HideInInspector]
        public List<IBuildingPart> InteractBPartList = new List<IBuildingPart>();

        /// <summary>
        /// ��ȡ Edit|Destroy �¼�⵽�����пɽ����Ľ�����
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
        /// ��ǰλ���Ƿ���Է���BPart
        /// </summary>
        protected bool GetCanPlaceBPart => this.SelectedPartInstance == null ? false : this.SelectedPartInstance.CanPlaceInPlaceMode;

        public event System.Action<IBuildingPart> OnSelectBPart;

        public void CheckInteractBPart(float deltatime)
        {
            // ʹ��BInput.Build
            // �ɽ�����������, ��ȡ�ɽ����б�
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
            // ����ƥ������
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

            //// ��������
            //var ray = GetCameraRay();
            //Gizmos.color = Color.red;
            //Vector3 start = ray.origin;
            //Vector3 end = ray.origin + ray.direction * this.checkRadius;
            //// �����ߵĿ��
            //float lineWidth = 0.1f;
            //// �����ߵ����ĵ�
            //Vector3 center = (start + end) * 0.5f;
            //// �����ߵĳ��Ⱥͷ���
            //float lineLength = Vector3.Distance(start, end);
            //Vector3 lineDirection = ray.direction;
            //// ���㳤����ĳߴ�
            //Vector3 cubeSize = new Vector3(lineWidth, lineWidth, lineLength);
            //// ��������������ߵ����ĵ㣬�������ߵķ���ͳ��Ƚ�����ת
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
        /// ͳһ����
        /// </summary>
        public Input.BuildingInput BInput => BuildingManager.Instance.BInput;

        public UnityEngine.InputSystem.InputAction comfirmInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Confirm;

        public UnityEngine.InputSystem.InputAction backInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Back;
        #endregion

    }
}

