using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ML.Engine.FSM;
using Sirenix.OdinInspector;
using ProjectOC.Player.Terrain;
using UnityEngine.InputSystem;
using ML.Engine.InteractSystem;

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

        #region 物体|脚本引用
        [LabelText("角色视点"), SerializeField, ReadOnly, FoldoutGroup("物体|脚本引用")]
        protected Transform playerViewer;
        [LabelText("角色模型"), SerializeField, ReadOnly, FoldoutGroup("物体|脚本引用")]
        protected Transform playerModel;
        [LabelText("角色地形检测器"), SerializeField, ReadOnly, FoldoutGroup("物体|脚本引用")]
        protected Transform playerTerrainDetect;
        [LabelText("视角旋转"), SerializeField, FoldoutGroup("物体|脚本引用")]
        protected ThirdPersonRotateComp thirdPersonRotateComp;

        /// <summary>
        /// PlayerInputActions
        /// </summary>
        protected ProjectOC.Input.PlayerInput.PlayerActions playerInputActions => Input.InputManager.PlayerInput.Player;

        public ML.Engine.InteractSystem.InteractComponent interactComponent;

        #endregion

        #region 基础视角和移动参数
        [LabelText("角色基础移动参数"), SerializeField, FoldoutGroup("基础视角和移动参数")]
        public PlayerNormalMove moveAbility;
        #endregion

        #region 移动状态机
        [LabelText("移动状态机控制器"), SerializeField, FoldoutGroup("移动状态机"), ReadOnly]
        public StateController moveStateController;
        [LabelText("移动状态机参数配置"), SerializeField, FoldoutGroup("移动状态机")]
        public PlayerMoveStateMachine.MoveStateParams moveStateParams = new PlayerMoveStateMachine.MoveStateParams();
        private PlayerMoveStateMachine moveStateMachine;
        #endregion

        #region 模型状态机
        public PlayerModelStateController playerModelStateController;
        #endregion

        #region UI
        [FoldoutGroup("UI")]
        public RectTransform playerUIBotPanel;
        [FoldoutGroup("UI")]
        public UI.PlayerUIPanel playerUIPanel;
        public ProjectOC.ResonanceWheelSystem.UI.BeastPanel beastPanel;
        #endregion

        #region 背包 to-do : 临时测试使用
        [ShowInInspector, ReadOnly]
        public ML.Engine.InventorySystem.IInventory Inventory;
        #endregion


        #region Init
        /// <summary>
        /// 处理物体引用
        /// </summary>
        private void Awake()
        {
            this.playerViewer = this.transform.Find("PlayerViewer");
            this.playerModel = this.transform.Find("PlayerModel");
            this.playerTerrainDetect = this.transform.Find("PlayerTerrainDetect");


        }

        /// <summary>
        /// 处理初始化
        /// </summary>
        private void Start()
        {
            this.moveStateParams.RuntimeInit(this.playerInputActions.Acc, this.playerInputActions.Crouch);
            this.InternalInit();
        }

        private bool _lastInputEnable = true;
        private void InternalInit()
        {
            this.playerModelStateController = new PlayerModelStateController(0, this.playerModel.GetComponentInChildren<Animator>(), this.GetComponent<Animator>(), this);
            
            if(this.thirdPersonRotateComp == null)
            {
                this.thirdPersonRotateComp = new ThirdPersonRotateComp();
            }
            this.thirdPersonRotateComp.RegisterTick(0);
            this.thirdPersonRotateComp.RuntimeSetMouseInput(this.playerInputActions.MouseX, this.playerInputActions.MouseY);

            this.playerTerrainDetect.gameObject.AddComponent<PlayerTerrainDetect>().SetMoveSetting(this.moveAbility.moveSetting);

            this.moveStateController = new StateController(0);
            this.moveStateMachine = new PlayerMoveStateMachine(this.moveAbility.moveSetting, this.moveStateParams);
            this.moveStateMachine.SetMoveAnimator(this.GetComponent<Animator>());
            this.moveStateController.SetStateMachine(moveStateMachine);

            this.interactComponent = this.GetComponentInChildren<ML.Engine.InteractSystem.InteractComponent>();

            this.playerInputActions.Enable();

            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterFixedTick(0, this);
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);

            // 按下对应按键才会压入栈

            var botui = GameObject.Instantiate(this.playerUIBotPanel.gameObject, GameObject.Find("Canvas").transform, false).GetComponent<UI.PlayerUIBotPanel>();
            botui.player = this;
            ML.Engine.Manager.GameManager.Instance.UIManager.ChangeBotUIPanel(botui);

            this.Inventory = new ML.Engine.InventorySystem.InfiniteInventory(this.transform, 999);



            StartCoroutine(__DelayInit__());

            //this.enabled = false;
        }

        private IEnumerator __DelayInit__()
        {
            while(ML.Engine.BuildingSystem.BuildingManager.Instance == null || ML.Engine.BuildingSystem.BuildingManager.Instance.Placer == null)
            {
                yield return null;
            }
            // 进入建造系统要可移动
            ML.Engine.BuildingSystem.BuildingManager.Instance.Placer.OnBuildingModeEnter += () =>
            {
                _lastInputEnable = Input.InputManager.PlayerInput.Player.enabled;
                Input.InputManager.PlayerInput.Player.Enable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Disable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Disable();

                // to-do : 待优化
                this.GetComponentInChildren<InteractComponent>().Disable();
            };
            ML.Engine.BuildingSystem.BuildingManager.Instance.Placer.OnBuildingModeExit += () =>
            {
                ProjectOC.Input.InputManager.PlayerInput.Player.Crouch.Enable();
                ProjectOC.Input.InputManager.PlayerInput.Player.Jump.Enable();
                if (!_lastInputEnable)
                {
                    Input.InputManager.PlayerInput.Player.Disable();
                }
                // to-do : 待优化
                this.GetComponentInChildren<InteractComponent>().Enable();
            };
            while(!ML.Engine.InventorySystem.ItemManager.Instance.IsLoadOvered)
            {
                yield return null;
            }
            //// to-do : to-delete
            // 仅测试用
/*            foreach (var id in ML.Engine.InventorySystem.ItemManager.Instance.GetAllItemID())//ML.Engine.InventorySystem.ItemManager.Instance.GetCanStack(id) ? UnityEngine.Random.Range(1, 999) : 1
            {
               var item = ML.Engine.InventorySystem.ItemManager.Instance.SpawnItems(id, 20)[0];
                Inventory.AddItem(item);
            }*/

            this.enabled = false;
        }

        private void OnDestroy()
        {
            (this.thirdPersonRotateComp as ML.Engine.Timer.ITickComponent).DisposeTick();
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
        }
        #endregion

        #region Tick
        public void Tick(float deltatime)
        {
            this.moveAbility.UpdateJump(deltatime, this.playerInputActions.Jump);

            if(Input.InputManager.PlayerInput.Player.OpenBotUI.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(GameObject.Instantiate(this.playerUIPanel.gameObject, GameObject.Find("Canvas").transform, false).GetComponent<ML.Engine.UI.UIBasePanel>());
                (ML.Engine.Manager.GameManager.Instance.UIManager.GetTopUIPanel() as UI.PlayerUIPanel).player = this;
            }

            if (Input.InputManager.PlayerInput.Player.OpenBeastPanel.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(GameObject.Instantiate(this.beastPanel.gameObject, GameObject.Find("Canvas").transform, false).GetComponent<ML.Engine.UI.UIBasePanel>());
                
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
    }
}
