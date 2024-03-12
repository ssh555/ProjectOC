using ML.Engine.BuildingSystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.TechTree.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;



namespace ProjectOC.Player.UI
{
    public class PlayerUIPanel : ML.Engine.UI.UIBasePanel
    {
        #region Unity
        public bool IsInit = false;

        private void Awake()
        {

            //InitPrefabs();
            btnList = this.transform.Find("ButtonList");

        }
        protected override void Start()
        {
            InitUITextContents();

            IsInit = true;
            Refresh();
            base.Start();
        }
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private List<AsyncOperationHandle<GameObject>> goHandle = new List<AsyncOperationHandle<GameObject>>();
        private AsyncOperationHandle spriteAtlasHandle;
        private void OnDestroy()
        {
            GM.ABResourceManager.Release(spriteAtlasHandle);
            foreach (var handle in goHandle)
            {
                GM.ABResourceManager.ReleaseInstance(handle);
            }
        }
        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();

            this.Enter();



        }

        public override void OnExit()
        {
            base.OnExit();

            this.Exit();

            ClearTemp();


        }

        public override void OnPause()
        {
            base.OnPause();
            this.Exit();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.Enter();
        }

        #endregion

        #region Internal

        private void Enter()
        {
            this.RegisterInput();
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Enable();
            this.Refresh();
        }

        private void Exit()
        {
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.Disable();
            this.UnregisterInput();

        }

        private void UnregisterInput()
        {


            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started -= SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;



        }

        private void RegisterInput()
        {

            //切换按钮
            ML.Engine.Input.InputManager.Instance.Common.StartMenu.SwichBtn.started += SwichBtn_started;

            //确认
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }

        private void SwichBtn_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var vec2 = obj.ReadValue<Vector2>();
            if (vec2.y > 0.1f)
            {
                this.UIBtnList.MoveUPIUISelected();
            }
            else if (vec2.y < -0.1f)
            {
                this.UIBtnList.MoveDownIUISelected();
            }
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {

        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            this.UIBtnList.GetCurSelected().Interact();
        }
        #endregion

        #region UI
        #region Temp

        private void ClearTemp()
        {

        }

        #endregion

        #region UI对象引用
        public PlayerCharacter player;
        private BuildingManager BM => BuildingManager.Instance;
        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_PlayerUIPanelPanel == null || !ABJAProcessorJson_PlayerUIPanelPanel.IsLoaded || !IsInit)
            {
                return;
            }

        }
        #endregion



        #region TextContent
        [System.Serializable]
        public struct PlayerUIPanelStruct
        {
            public TextTip[] Btns;
        }

        public PlayerUIPanelStruct PanelTextContent_PlayerUIPanelPanel => ABJAProcessorJson_PlayerUIPanelPanel.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<PlayerUIPanelStruct> ABJAProcessorJson_PlayerUIPanelPanel;
        private void InitUITextContents()
        {
            ABJAProcessorJson_PlayerUIPanelPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<PlayerUIPanelStruct>("OC/Json/TextContent/PlayerUIPanel", "PlayerUIPanel", (datas) =>
            {
                InitBtnData(datas);

            }, "PlayerUIPanel数据");
            ABJAProcessorJson_PlayerUIPanelPanel.StartLoadJsonAssetData();

        }

        private Transform btnList;
        private UIBtnList UIBtnList;
        private void InitBtnData(PlayerUIPanelStruct datas)
        {
            UIBtnList = new UIBtnList(parent: btnList, BtnType: 1);
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }

            //EnterBuild
            this.UIBtnList.SetBtnAction("EnterBuild",
            () =>
            {
                if (ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.WasPressedThisFrame() && BM.Mode == BuildingMode.None)
                {
                    if (BM.GetRegisterBPartCount() > 0)
                    {
                        BM.Mode = BuildingMode.Interact;
                    }
                    else
                    {
                        Debug.LogWarning("当前建筑物数量为0，无法进入建造模式!");
                    }
                }
            }
            );
            //EnterTechTree
            this.UIBtnList.SetBtnAction("EnterTechTree",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/TechPointPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UITechPointPanel>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //EnterInventory
            this.UIBtnList.SetBtnAction("EnterInventory",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/UIInfiniteInventoryPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UIInfiniteInventory>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    panel.inventory = this.player.Inventory as ML.Engine.InventorySystem.InfiniteInventory;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //EnterBeastPanel
            this.UIBtnList.SetBtnAction("EnterBeastPanel",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("OC/UIPanel/BeastPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<BeastPanel>();
                    panel.transform.localScale = Vector3.one;
                    panel.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
                };
            }
            );
            //CreateWorker
            this.UIBtnList.SetBtnAction("CreateWorker",
            () =>
            {
                ProjectOC.ManagerNS.LocalGameManager.Instance.WorkerManager.SpawnWorker(player.transform.position, player.transform.rotation);
            }
            );
        }

        #endregion

    }

}
