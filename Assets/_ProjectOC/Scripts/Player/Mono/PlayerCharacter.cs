using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.FSM;
using Sirenix.OdinInspector;
using ProjectOC.Player.Terrain;
using UnityEngine.InputSystem;
using ML.Engine.InteractSystem;
using ML.Engine.UI;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace ProjectOC.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerCharacter : MonoBehaviour, ML.Engine.Timer.ITickComponent
    {
        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        #endregion

        #region ����|�ű�����
        [LabelText("��ɫ�ӵ�"), SerializeField, ReadOnly, FoldoutGroup("����|�ű�����")]
        protected Transform playerViewer;
        [LabelText("��ɫģ��"), SerializeField, ReadOnly, FoldoutGroup("����|�ű�����")]
        protected Transform playerModel;
        [LabelText("��ɫ���μ����"), SerializeField, ReadOnly, FoldoutGroup("����|�ű�����")]
        protected Transform playerTerrainDetect;
        [LabelText("�ӽ���ת"), SerializeField, FoldoutGroup("����|�ű�����")]
        protected ThirdPersonRotateComp thirdPersonRotateComp;

        /// <summary>
        /// PlayerInputActions
        /// </summary>
        protected ProjectOC.Input.PlayerInput.PlayerActions playerInputActions => Input.InputManager.PlayerInput.Player;

        public ML.Engine.InteractSystem.InteractComponent interactComponent;

        #endregion

        #region �����ӽǺ��ƶ�����
        [LabelText("��ɫ�����ƶ�����"), SerializeField, FoldoutGroup("�����ӽǺ��ƶ�����")]
        public PlayerNormalMove moveAbility;
        #endregion

        #region �ƶ�״̬��
        [LabelText("�ƶ�״̬��������"), SerializeField, FoldoutGroup("�ƶ�״̬��"), ReadOnly]
        public StateController moveStateController;
        [LabelText("�ƶ�״̬����������"), SerializeField, FoldoutGroup("�ƶ�״̬��")]
        public PlayerMoveStateMachine.MoveStateParams moveStateParams = new PlayerMoveStateMachine.MoveStateParams();
        private PlayerMoveStateMachine moveStateMachine;
        #endregion

        #region ģ��״̬��
        public PlayerModelStateController playerModelStateController;
        #endregion

        #region UI
        [FoldoutGroup("UI")]
        public RectTransform playerUIBotPanel;
        [FoldoutGroup("UI")]
        public UI.PlayerUIPanel playerUIPanel;
        public ProjectOC.ResonanceWheelSystem.UI.BeastPanel beastPanel;
        #endregion

        #region ���� to-do : ��ʱ����ʹ��
        [ShowInInspector, ReadOnly]
        public ML.Engine.InventorySystem.IInventory Inventory;
        #endregion


        #region Init
        /// <summary>
        /// ������������
        /// </summary>
        private void Awake()
        {
            this.playerViewer = this.transform.Find("PlayerViewer");
            this.playerModel = this.transform.Find("PlayerModel");
            this.playerTerrainDetect = this.transform.Find("PlayerTerrainDetect");
            this.InternalInit();
        }

        /// <summary>
        /// �����ʼ��
        /// </summary>
        private void Start()
        {
            this.moveStateParams.RuntimeInit(this.playerInputActions.Acc, this.playerInputActions.Crouch);
            
        }
        private void InternalInit()
        {
            this.playerModelStateController = new PlayerModelStateController(0, this.playerModel.GetComponentInChildren<Animator>(), this.GetComponent<Animator>(), this);
            
            this.playerTerrainDetect.gameObject.AddComponent<PlayerTerrainDetect>().SetMoveSetting(this.moveAbility.moveSetting);

            if(this.thirdPersonRotateComp == null)
            {
                this.thirdPersonRotateComp = new ThirdPersonRotateComp();
            }
            this.thirdPersonRotateComp.RegisterTick(0);
            this.thirdPersonRotateComp.RuntimeSetMouseInput(this.playerInputActions.MouseX, this.playerInputActions.MouseY,this.playerInputActions.MouseScroll);


            this.moveStateController = new StateController(0);
            this.moveStateMachine = new PlayerMoveStateMachine(this.moveAbility.moveSetting, this.moveStateParams);
            this.moveStateMachine.SetMoveAnimator(this.GetComponent<Animator>());

            this.moveStateController.SetStateMachine(moveStateMachine);

            this.interactComponent = this.GetComponentInChildren<ML.Engine.InteractSystem.InteractComponent>();

            this.playerInputActions.Enable();

            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterFixedTick(0, this);
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);

            // ���¶�Ӧ�����Ż�ѹ��ջ

            var botui = GameObject.Instantiate(this.playerUIBotPanel.gameObject, ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).GetComponent<UI.PlayerUIBotPanel>();
            botui.player = this;
            ML.Engine.Manager.GameManager.Instance.UIManager.ChangeBotUIPanel(botui);

            this.Inventory = new ML.Engine.InventorySystem.InfiniteInventory(this.transform, 999);


            // ���뽨��ϵͳҪ���ƶ�
            ML.Engine.BuildingSystem.BuildingManager.Instance.Placer.OnBuildingModeEnter += () =>
            {
                // to-do : ���Ż�
                this.GetComponentInChildren<InteractComponent>().Disable();
            };
            ML.Engine.BuildingSystem.BuildingManager.Instance.Placer.OnBuildingModeExit += () =>
            {
                // to-do : ���Ż�
                this.GetComponentInChildren<InteractComponent>().Enable();
            };

            //StartCoroutine(__DelayInit__());

            this.enabled = false;
        }

        private void OnDestroy()
        {
            (this.thirdPersonRotateComp as ML.Engine.Timer.ITickComponent).DisposeTick();
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
            this.playerModelStateController.Stop();
        }
        #endregion

        #region Tick
        public void Tick(float deltatime)
        {
            this.moveAbility.UpdateJump(deltatime, this.playerInputActions.Jump);

            if(Input.InputManager.PlayerInput.Player.OpenBotUI.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(GameObject.Instantiate(this.playerUIPanel.gameObject, ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false).GetComponent<ML.Engine.UI.UIBasePanel>());
                (ML.Engine.Manager.GameManager.Instance.UIManager.GetTopUIPanel() as UI.PlayerUIPanel).player = this;
            }


            //// In-Window
            //if (Application.isFocused)
            //{
            //    // OpenUI => LockMode = None
            //    if (this.inventoryOwner.IsOpen)
            //    {
            //        if(Cursor.lockState != CursorLockMode.None)
            //            Cursor.lockState = CursorLockMode.None;
            //    }
            //    else if(Mouse.current.position.value.x >= 0 && Mouse.current.position.value.y >= 0 && Mouse.current.position.value.x <= Screen.width && Mouse.current.position.value.y <= Screen.height && Input.anyKeyDown)
            //    {
            //        Cursor.lockState = CursorLockMode.Locked;
            //    }
            //}
            //// Out-Window
            //else
            //{
            //    // LockMode = None
            //    if(Cursor.lockState != CursorLockMode.None)
            //        Cursor.lockState = CursorLockMode.None;
            //}
        }

        public void FixedTick(float deltatime)
        {
            this.moveAbility.UpdatePosition(deltatime, this.playerInputActions.Move.ReadValue<Vector2>());
        }

        #endregion


        #region to-delete
        [Button("������: AddAllItems")]
        private void __TEST__AddInventoryAllItem__()
        {
            // to-do : to-delete
            // ��������
            foreach (var id in ML.Engine.InventorySystem.ItemManager.Instance.GetAllItemID())//ML.Engine.InventorySystem.ItemManager.Instance.GetCanStack(id) ? UnityEngine.Random.Range(1, 999) : 1
            {
                ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(id, 500).ForEach(item => Inventory.AddItem(item));
            }
        }

        [Button("������: AddItem")]
        private void __TEST__AddInventoryItem__(string id, int amount)
        {

            ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(id, amount).ForEach(item => Inventory.AddItem(item));
        }
        #endregion
#if UNITY_EDITOR
        private void OnValidate()
        {
            GetComponent<Rigidbody>().mass = moveAbility.moveSetting.Mass;
            CharacterController cc = GetComponent<CharacterController>();
            cc.stepOffset = moveAbility.moveSetting.MaxStepHeight;
            cc.slopeLimit = moveAbility.moveSetting.WalkerbleFloorAngle;
        }
#endif
    }
}
