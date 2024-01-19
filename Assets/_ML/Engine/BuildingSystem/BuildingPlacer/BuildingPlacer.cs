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
    // to-do : ʹ��ʱ���� BuildingManager ע��
    public class BuildingPlacer : MonoBehaviour, Timer.ITickComponent
    {
        #region ����ģʽ
        //[LabelText("����ģʽ"), ShowInInspector, PropertyOrder(-1)]

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
        /// ������ʼ��ʱ��
        /// </summary>
        private float keyComStartTime;
        /// <summary>
        /// �����ܵ�Ŀ��ʱ��
        /// </summary>
        private float keyComTotalTime;
        /// <summary>
        /// �ν����� ������ʼʱ����
        /// </summary>
        public event System.Action OnKeyComStart;
        /// <summary>
        /// �ν����� ������סʱ��Ӧ
        /// CurTime, TotalTime
        /// </summary>
        public event System.Action<float, float> OnKeyComInProgress;
        /// <summary>
        /// �ν����� �����ɹ����ʱ����, ������ν�����
        /// </summary>
        public event System.Action OnKeyComComplete;
        /// <summary>
        /// �ν����� ������ǰ������δ���ʱ����
        /// CurTime, TotalTime
        /// </summary>
        public event System.Action<float, float> OnKeyComCancel;
        /// <summary>
        /// �ν����� �����ɹ�������˳�ʱ����
        /// </summary>
        public event System.Action OnKeyComExit;

        /// <summary>
        /// �Ƿ�����۸���״̬
        /// </summary>
        public bool IsInAppearance { get; protected set; } = false;
        /// <summary>
        /// �������ѡ�����ʱ���ã��������ѡ��UI�������
        /// ���� : ѡ��ʹ�õ�BPart
        /// </summary>
        public event System.Action<IBuildingPart, Texture2D[], BuildingCopiedMaterial[], int> OnEnterAppearance;
        public event System.Action<IBuildingPart> OnExitAppearance;
        public event System.Action<Texture2D[], BuildingCopiedMaterial[], int> OnChangeAppearance;

        /// <summary>
        /// ������BPartʱ����
        /// </summary>
        public event System.Action<IBuildingPart> OnDestroySelectedBPart;

        /// <summary>
        /// ����EditMode
        /// </summary>
        public event System.Action<IBuildingPart> OnEditModeEnter;
        /// <summary>
        /// �˳�EditMode
        /// </summary>
        public event System.Action<IBuildingPart> OnEditModeExit;

        /// <summary>
        /// ����1 : һ����������ѡ��UI����
        /// </summary>
        public event System.Action<BuildingCategory1[], int, BuildingCategory2[], int> OnBuildSelectionEnter;
        /// <summary>
        /// ����1 : һ����������ѡ��UI����
        /// </summary>
        public event System.Action OnBuildSelectionExit;
        /// <summary>
        /// ȷ��
        /// </summary>
        public event System.Action<IBuildingPart> OnBuildSelectionComfirm;
        /// <summary>
        /// ȡ��
        /// </summary>
        public event System.Action OnBuildSelectionCancel;
        /// <summary>
        /// ������ö��, ��ǰindex
        /// </summary>
        public event System.Action<BuildingCategory1[], int> OnBuildSelectionCategoryChanged;
        /// <summary>
        /// ������ö��, ��ǰindex
        /// </summary>
        public event System.Action<BuildingCategory1, BuildingCategory2[], int> OnBuildSelectionTypeChanged;
        /// <summary>
        /// ����3 : ����
        /// </summary>
        public event System.Action<IBuildingPart> OnPlaceModeEnter;
        /// <summary>
        /// ����3 : ����
        /// </summary>
        public event System.Action OnPlaceModeExit;
        /// <summary>
        /// Style ����ʱ����
        /// ��ǰѡ���BPart, Ϊ��ǰ�������ѡ��
        /// </summary>
        public event System.Action<IBuildingPart, bool> OnPlaceModeChangeStyle;
        /// <summary>
        /// Height ����ʱ����
        /// ��ǰѡ���BPart, Ϊ��ǰ�������ѡ��
        /// </summary>
        public event System.Action<IBuildingPart, bool> OnPlaceModeChangeHeight;
        /// <summary>
        /// һ�γɹ�����
        /// </summary>
        public event System.Action<IBuildingPart> OnPlaceModeSuccess;
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
                if(this.selectedPartInstance != null)
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
                Destroy(this.SelectedPartInstance.gameObject);
                this.SelectedPartInstance = null;
            }
        }

        /// <summary>
        /// ������ʽStyle�ֻ���ǰBPart
        /// </summary>
        public void AlternateBPartOnStyle(bool isForward)
        {
            var tmp = BuildingManager.Instance.PollingBPartPeekInstanceOnStyle(this.SelectedPartInstance, isForward);
            this.ResetBPart();
            this.SelectedPartInstance = tmp;
        }

        /// <summary>
        /// ���ڸ߶�Height�ֻ���ǰBPart
        /// </summary>
        public void AlternateBPartOnHeight(bool isForward)
        {
            var tmp = BuildingManager.Instance.PollingBPartPeekInstanceOnHeight(this.SelectedPartInstance, isForward);
            this.ResetBPart();
            this.SelectedPartInstance = tmp;
        }
        #endregion

        #region Transform
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

                // ScreenCenter->ScreenNormal ���߼��
                var hits = Physics.RaycastAll(this.GetCameraRay(), this.checkRadius, this.checkLayer);
                if (hits == null || hits.Length == 0)
                {
                    hits = Physics.RaycastAll(this.GetCameraRayEndPointDownRay(), this.checkRadius, this.checkLayer);
                }

                // ���� || δ���� => ���յ㴦���¼��
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

                // ��ת -> ����
                rot = this.SelectedPartInstance.BaseRotation * this.SelectedPartInstance.RotOffset;

                Vector3 tmpP;
                Quaternion tmpR;
                // λ��&��ת -> AttachedArea
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

        /// <summary>
        /// Ӧ��λ�ú���ת��SelectedPartInstance
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

        [LabelText("���뾶"), SerializeField, FoldoutGroup("�������"), PropertyTooltip("��λ m")]
        protected float interactRadius = 5;

        [LabelText("����"), SerializeField, FoldoutGroup("�������")]
        protected LayerMask interactLayer;

        [LabelText("���ΰ뾶"), SerializeField, FoldoutGroup("�������"), PropertyTooltip("��λ m")]
        protected float sphereRadius = 0.5f;

        [LabelText("���ʱ����"), SerializeField, FoldoutGroup("�������"), PropertyTooltip("��λ s")]
        protected float interactCheckInterval = 1f;
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
        #endregion

        #region ITickComponent
        private void Start()
        {
            StartCoroutine(LoadMatPackages());
            BuildingManager.Instance.Placer = this;

            // ����interactions�ַ�������ȡhold����ʱ��ֵ
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


        #region �����У������ܶ�
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion

        /// <summary>
        /// Mode �л� & Mode �߼�ִ��
        /// </summary>
        /// <param name="deltatime"></param>
        public void Tick(float deltatime)
        {
            // None Mode => ����ʱ�Զ��˳�
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
            // Destroy Mode => ֱ�����٣�����Ҫ����
        }
        #endregion

        #region Input Interaction
        #region InputAction
        /// <summary>
        /// ͳһ����
        /// </summary>
        protected Input.BuildingInput BInput => BuildingManager.Instance.BInput;

        protected UnityEngine.InputSystem.InputAction comfirmInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm;

        protected UnityEngine.InputSystem.InputAction backInputAction => ML.Engine.Input.InputManager.Instance.Common.Common.Back;
        #endregion

        #region Interact
        protected void HandleInteractMode(float deltatime)
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
                // ������ʼ => �����¼����ã��뽨��ϵͳ�߼��޹�
                if (BInput.Build.KeyCom.WasPressedThisFrame())
                {
                    this.keyComStartTime = Time.time;
                    this.OnKeyComStart?.Invoke();
                }
                // ����������
                else if (BInput.Build.KeyCom.IsPressed())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // �����¼�����
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComInProgress?.Invoke(curT, this.keyComTotalTime);
                    }
                    // ������� => ����ν�����, ���������Input.Build�������ظ�ִ��
                    else
                    {
                        // ����ν����� => ������InteractMode
                        this.EnterKeyCom();
                    }
                }
                // ��������
                else if (BInput.Build.KeyCom.WasReleasedThisFrame())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // ����ȡ��
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComCancel?.Invoke(curT, this.keyComTotalTime);
                    }
                    //// �����˳� => �ɴν����ֵ�����ӹ�
                    //else
                    //{
                    //    this.OnKeyComExit?.Invoke();
                    //    // �˳�KeyCom
                    //    this.ExitKeyCom();
                    //}
                }
                // �������ѡ�еĿɽ������������۸��Ľ��� => ���ɴ���InteractMode����ѡ������˳�ʱ���ֽ�Ϊ�ر�UI���
                else if (BInput.Build.ChangeOutLook.WasPressedThisFrame())
                {
                    this.EnterAppearancePanel();
                }
                // ���� Edit Mode -> ѡ�ж�ӦBPart, ����EditMode��������ƶ�����
                else if (BInput.Build.MoveBuild.WasPressedThisFrame())
                {
                    this.EnterEditMode();
                }
                // ���� Destroy Mode -> ֱ������ѡ��BPart
                else if (BInput.Build.DestroyBuild.WasPressedThisFrame())
                {
                    this.EnterDestroyMode();
                }
                #endregion
                #region Input.KeyCom
                // ����ν����ֺ� : ����Input.Build ����Input.Keycom
                else if (BInput.BuildKeyCom.KeyCom.IsPressed())
                {
                    this.HandelKeyCom();
                }
                // �˳��ν�����
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
                // û�а�����Ӧʱ����Ӧ�ɽ������л�
                else
                {
                    // �ֻ��ɽ����б�
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

                // ������� => ͬһֻ֡����Ӧ InteractMode ��һ�ְ���
                //return;
            }
            else
            {
                // �˳��ν�����
                if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
                {
                    this.ExitKeyCom();
                }
            }
           
            // ���� Place Mode -> ���뽨����ѡ�����
            if (!this.IsInAppearance && BInput.Build.SelectBuild.WasPressedThisFrame())
            {
                this.EnterBuildSelection();
            }
            // to-do : to-delete : ��������, �˳�����ģʽ
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
        /// 0 -> ������PlaceMode, �ص�InteractMode
        /// 1 -> һ����������ѡ��UI����
        /// 2 -> ���ѡ��UI����
        /// 3 -> ����
        /// </summary>
        public byte PlaceControlFlow
        {
            get => this._placeControlFlow;
            protected set
            {
                if (this._placeControlFlow == value)
                    return;
                // �뿪����1
                if (this._placeControlFlow == 1)
                {
                    this.OnBuildSelectionExit?.Invoke();
                }
                // �뿪����3 && ���ǻص�2�������ѡ��
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
                    // ��������1
                    if (this._placeControlFlow == 1)
                    {
                        this.OnBuildSelectionEnter?.Invoke(_placeCanSelectCategory, _placeSelectedCategoryIndex, _placeCanSelectType, _placeSelectedTypeIndex);
                    }
                    // ��������3
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
            // ����1 : һ����������ѡ��UI����
            if(this.PlaceControlFlow == 1)
            {
                // ȷ��
                if(this.comfirmInputAction.WasPressedThisFrame())
                {
                    this.SelectedPartInstance = BuildingManager.Instance.GetOneBPartInstance(this._placeCanSelectCategory[this._placeSelectedCategoryIndex], this._placeCanSelectType[this._placeSelectedTypeIndex]);
                    this.OnBuildSelectionComfirm?.Invoke(this.SelectedPartInstance);
                    this.ExitBuildSelection(3);
                }
                // ȡ��
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
            // ����2 : ���ѡ��UI����(����1����ʱ�Զ�ѡ������) => ���ùܣ��Զ�����
            else if(this.PlaceControlFlow == 2)
            {
                this.HandleAppearance();
            }
            // ����3 : ����(�ɻص�����2�ֶ�ѡ�����)
            else if(this.PlaceControlFlow == 3)
            {
                // ʵʱ��������λ�ú���ת�Լ��Ƿ�ɷ���
                this.TransformSelectedPartInstance();
                // ������ʼ => �����¼����ã��뽨��ϵͳ�߼��޹�
                if (BInput.BuildPlaceMode.KeyCom.WasPressedThisFrame())
                {
                    this.keyComStartTime = Time.time;
                    this.OnKeyComStart?.Invoke();
                }
                // ����������
                else if (BInput.BuildPlaceMode.KeyCom.IsPressed())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // �����¼�����
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComInProgress?.Invoke(curT, this.keyComTotalTime);
                    }
                    // ������� => ����ν�����, ���������Input.Build�������ظ�ִ��
                    else
                    {
                        // ����ν����� => ������InteractMode
                        this.EnterKeyCom();
                    }
                }
                // ��������
                else if (BInput.BuildPlaceMode.KeyCom.WasReleasedThisFrame())
                {
                    float curT = Time.time - this.keyComStartTime;
                    // ����ȡ��
                    if (curT < this.keyComTotalTime)
                    {
                        this.OnKeyComCancel?.Invoke(curT, this.keyComTotalTime);
                    }
                    //// �����˳� => �ɴν����ֵ�����ӹ�
                    //else
                    //{
                    //    this.OnKeyComExit?.Invoke();
                    //    // �˳�KeyCom
                    //    this.ExitKeyCom();
                    //}
                }
                #region Input.KeyCom
                // ����ν����ֺ� : ����Input.Build ����Input.Keycom
                else if (BInput.BuildKeyCom.KeyCom.IsPressed())
                {
                    this.HandelKeyCom();
                }
                // �˳��ν�����
                else if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
                {
                    this.ExitKeyCom();
                }
                #endregion
                // ȡ�� -> �ص�InteractMode
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
                // �ֻ�ActiveSocket
                else if (BInput.BuildPlaceMode.ChangeActiveSocket.WasPressedThisFrame())
                {
                    this.SelectedPartInstance.AlternativeActiveSocket();
                }
                // Style��Height�л�BPart
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
                // ���˵����ѡ��
                else if(BInput.BuildPlaceMode.ChangeOutLook.WasPressedThisFrame())
                {
                    this.EnterAppearancePanel();
                }
                // ȷ�� -> ��������λ��, CopyBPartInstacne -> LoopPlace
                else if (this.SelectedPartInstance.CanPlaceInPlaceMode && this.comfirmInputAction.WasPressedThisFrame())
                {
                    this.SelectedPartInstance.Mode = BuildingMode.None;
                    // ����һ��
                    var tmp = GameObject.Instantiate(this.SelectedPartInstance.gameObject).GetComponent<BuildingPart.BuildingPart>();

                    // �½������� -> ����ʵ��ID
                    this.SelectedPartInstance.InstanceID = BuildingManager.Instance.GetOneNewBPartInstanceID();
                    this.SelectedPartInstance.OnChangePlaceEvent(this.SelectedPartInstance.transform.position, this.SelectedPartInstance.transform.position);
                    this.OnPlaceModeSuccess?.Invoke(this.SelectedPartInstance);


                    this.SelectedPartInstance = tmp;
                }
            }
        }


        /// <summary>
        /// ���뽨����ѡ�����(PlaceMode)
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

            // ���� Input->Build
            this.PlaceControlFlow = 1;
            // �л�ģʽʱ�Զ�����
            // BInput.Build.Disable();
            // ���� Input->PlaceMode => ������ѡ�����
            BInput.BuildSelection.Enable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flow">�������̼�</param>
        protected void ExitBuildSelection(byte flow)
        {
            this.PlaceControlFlow = flow;

            // to-do : ������2����ʱ�������۲��ʸ��赱ǰѡ�е�BPartInstance
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

            // ���� Input.Build
            this.Mode = BuildingMode.Interact;
            // ���� Input.Place
            BInput.BuildPlaceMode.Disable();
            // ����BPart״̬
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
            // ʵʱ��������λ�ú���ת�Լ��Ƿ�ɷ���
            this.TransformSelectedPartInstance();
            // ������ʼ => �����¼����ã��뽨��ϵͳ�߼��޹�
            if (BInput.BuildPlaceMode.KeyCom.WasPressedThisFrame())
            {
                this.keyComStartTime = Time.time;
                this.OnKeyComStart?.Invoke();
            }
            // ����������
            else if (BInput.BuildPlaceMode.KeyCom.IsPressed())
            {
                float curT = Time.time - this.keyComStartTime;
                // �����¼�����
                if (curT < this.keyComTotalTime)
                {
                    this.OnKeyComInProgress?.Invoke(curT, this.keyComTotalTime);
                }
                // ������� => ����ν�����, ���������Input.Build�������ظ�ִ��
                else
                {
                    // ����ν����� => ������InteractMode
                    this.EnterKeyCom();
                }
            }
            // ��������
            else if (BInput.BuildPlaceMode.KeyCom.WasReleasedThisFrame())
            {
                float curT = Time.time - this.keyComStartTime;
                // ����ȡ��
                if (curT < this.keyComTotalTime)
                {
                    this.OnKeyComCancel?.Invoke(curT, this.keyComTotalTime);
                }
                //// �����˳� => �ɴν����ֵ�����ӹ�
                //else
                //{
                //    this.OnKeyComExit?.Invoke();
                //    // �˳�KeyCom
                //    this.ExitKeyCom();
                //}
            }
            #region Input.KeyCom
            // ����ν����ֺ� : ����Input.Build ����Input.Keycom
            else if (BInput.BuildKeyCom.KeyCom.IsPressed())
            {
                this.HandelKeyCom();
            }
            // �˳��ν�����
            else if (BInput.BuildKeyCom.KeyCom.WasReleasedThisFrame())
            {
                this.ExitKeyCom();
            }
            #endregion
            // ȡ�� -> �ص�ԭ״̬
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
            // �ֻ�ActiveSocket
            else if(BInput.BuildPlaceMode.ChangeActiveSocket.WasPressedThisFrame())
            {
                this.SelectedPartInstance.AlternativeActiveSocket();
            }
            // ȷ�� -> ��������λ��, �ص�InteractMode
            else if (this.SelectedPartInstance.CanPlaceInPlaceMode && this.comfirmInputAction.WasPressedThisFrame())
            {
                this.SelectedPartInstance.OnChangePlaceEvent(this._editOldPos, this.SelectedPartInstance.transform.position);

                this.ExitEditMode();
            }

        }

        /// <summary>
        /// ����Edit�ƶ�����ģʽ
        /// </summary>
        protected void EnterEditMode()
        {
            if (!this.SelectedPartInstance.CanEnterEditMode())
            {
                return;
            }
            // ���� Input.Build
            this.Mode = BuildingMode.Edit;
            // ���� Input.Edit
            this.EnableEditModeInput();

            // ��¼ԭ Transform
            this._editOldPos = this.SelectedPartInstance.transform.position;
            this._editOldRotation = this.SelectedPartInstance.transform.rotation;

            this.OnEditModeEnter?.Invoke(this.SelectedPartInstance);
        }

        protected void ExitEditMode()
        {
            // ���� Input.Build
            this.Mode = BuildingMode.Interact;
            // ���� Input.Edit
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
        /// ֱ������BPart
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

            // to-do :�������ܻ�������ٵ���
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
            // ȷ��ѡ��Ĳ���
            if (this.comfirmInputAction.WasPressedThisFrame())
            {
                //this.ReplaceSelectedAppearance();
                this.ExitAppearancePanel();
            }
            // ȡ����۱༭
            else if(this.backInputAction.WasPressedThisFrame())
            {
                this.SelectedPartInstance.SetCopiedMaterial(this._aMat);
                this.ExitAppearancePanel();
            }
            // �л�����
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
        /// �л�ѡ�е���۲���
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
        ///// ΪSelectedBPart�滻ѡ�е���۲���
        ///// </summary>
        //protected void ReplaceSelectedAppearance()
        //{
        //    this.SelectedPartInstance.SetCopiedMaterial(this._aCurrentMatPackages[this._aCurrentIndex]);
        //}

        /// <summary>
        /// �������ѡ�����
        /// </summary>
        protected void EnterAppearancePanel()
        {
            // PlaceMode
            if (this.Mode == BuildingMode.Place)
            {
                this.PlaceControlFlow = 2;

                // ���� Input.PlaceMode
                BInput.BuildPlaceMode.Disable();
                // ���� Input.Appearance
                BInput.BuildingAppearance.Enable();
            }
            else if (this.Mode == BuildingMode.Interact)
            {
                // ���� Input.Build
                BInput.Build.Disable();
                // ���� Input.Appearance
                BInput.BuildingAppearance.Enable();
            }

            // �������ѡ��״̬
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

                // ���� Input.PlaceMode
                this.EnablePlaceModeInput();
                // ���� Input.Appearance
                BInput.BuildingAppearance.Disable();
            }
            else if (this.Mode == BuildingMode.Interact)
            {
                // ���� Input.Build
                BInput.Build.Enable();
                // ���� Input.Appearance
                BInput.BuildingAppearance.Disable();
            }

            this.SelectedPartInstance.Mode = this._aMode;
            // �˳����ѡ��״̬
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

        #region �ν�����
        protected void HandelKeyCom()
        {
            if (this.SelectedPartInstance == null)
            {
                return;
            }
            // CopyBuild
            if (BInput.BuildKeyCom.CopyBuild.WasPressedThisFrame())
            {
                // ��������
                if (this.Mode == BuildingMode.Place || this.Mode == BuildingMode.Edit)
                {
                    this.IsEnableGridSupport = !IsEnableGridSupport;
                }
                // ���ƽ�����
                else
                {
                    // ���Ƶ�ǰѡ�еĿɽ��������PlaceMode

                    // ����һ�ݵ�ǰѡ�е�BPart(��һ����������ѡ������ѡ�������)

                    var bpart = BuildingManager.Instance.GetOneBPartCopyInstance(this.SelectedPartInstance);
                    if(bpart == this.SelectedPartInstance)
                    {
                        this.ExitKeyCom();
                        // ����PlaceMode�ķ�������
                        this.PlaceControlFlow = 3;
                    }
                    // to-do : ���̶�
                    // ���ܸ�����ֱ�ӽ���Editģʽ
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
                // ���Ƶ�ǰѡ�еĿɽ���������Ĳ���
                BuildingManager.Instance.CopyBPartMaterial(this.SelectedPartInstance);
            }
            // PasteOutLook
            else if (BInput.BuildKeyCom.PasteOutLook.WasPressedThisFrame())
            {
                // �������ƵĲ��ʵ���ǰѡ�еĿɽ���������
                BuildingManager.Instance.PasteBPartMaterial(this.SelectedPartInstance);
            }
        }

        protected void EnterKeyCom()
        {
            if (this.Mode == BuildingMode.Interact)
            {
                // ���� Input.Build
                BInput.Build.Disable();
            }

            else
            {
                BInput.BuildPlaceMode.Disable();
            }
            // ���� Input.KeyCom
            BInput.BuildKeyCom.Enable();
            this.OnKeyComComplete?.Invoke();

        }

        protected void ExitKeyCom()
        {
            // ����û���ɿ�KeyCom, �ٴν���ʱֱ�Ӵ�
            this.keyComStartTime = Time.time;

            if(this.Mode == BuildingMode.Interact)
            {
                // ���� Input.Build
                BInput.Build.Enable();
            }
            else
            {
                BInput.BuildPlaceMode.Enable();
            }

            // ���� Input.KeyCom
            BInput.BuildKeyCom.Disable();
            this.OnKeyComExit?.Invoke();
        }
        #endregion

        #endregion

    }
}

