using ML.Engine.BuildingSystem;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.UI;
using ProjectOC.InventorySystem.UI;
using ProjectOC.ManagerNS;
using ProjectOC.ResonanceWheelSystem.UI;
using ProjectOC.TechTree.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ProjectOC.Player.UI.PlayerUIPanel;
using static UnityEngine.Rendering.DebugUI;



namespace ProjectOC.Player.UI
{
    public class PlayerUIPanel : ML.Engine.UI.UIBasePanel<PlayerUIPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        protected override void Enter()
        {
            this.UIBtnList.EnableBtnList();
            base.Enter();
        }

        protected override void Exit()
        {
            this.UIBtnList.DisableBtnList();
            base.Exit();
        }
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private List<AsyncOperationHandle<GameObject>> goHandle = new List<AsyncOperationHandle<GameObject>>();

        protected override void OnDestroy()
        {
            foreach (var handle in goHandle)
            {
                GM.ABResourceManager.ReleaseInstance(handle);
            }
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            this.UIBtnList.RemoveAllListener();

            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();

            this.UIBtnList.DeBindInputAction();

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        protected override void RegisterInput()
        {

            //EnterBuild
            this.UIBtnList.SetBtnAction("EnterBuild",
            () =>
            {
                if (BM.Mode == BuildingMode.None)
                {
                    if (BM.GetRegisterBPartCount() > 0)
                    {
                        BM.Mode = BuildingMode.Interact;
                    }
                    else
                    {
                        Debug.LogWarning("当前建筑物数量为0，无法进入建造模??!");
                    }
                }
            }
            );
            //EnterTechTree
            this.UIBtnList.SetBtnAction("EnterTechTree",
            () =>
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefabs_TechTree_UI/Prefab_TechTree_UI_TechPointPanel.prefab", this.transform.parent, true).Completed += (handle) =>
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
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefabs_UIInfiniteInventoryPanel", this.transform.parent, true).Completed += (handle) =>
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
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync("Prefab_Worker_UI/Prefab_Worker_UI_BeastPanel.prefab", this.transform.parent, true).Completed += (handle) =>
                {
                    var panel = handle.Result.GetComponent<UIBeastPanel>();
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

            //AddItem
            this.UIBtnList.SetBtnAction("AddItem",
            () =>
            {
                foreach (var id in ML.Engine.InventorySystem.ItemManager.Instance.GetAllItemID())//ML.Engine.InventorySystem.ItemManager.Instance.GetCanStack(id) ? UnityEngine.Random.Range(1, 999) : 1
                {
                    int maxAmount = ItemManager.Instance.GetMaxAmount(id);
                    if (maxAmount < 500)
                    {
                        ItemManager.Instance.SpawnItems(id, maxAmount).ForEach(item => (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory.AddItem(item));
                    }
                    else
                    {
                        ItemManager.Instance.SpawnItems(id, 500).ForEach(item => (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController).OCState.Inventory.AddItem(item));
                    }
                }
            }
            );

            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();


            this.UIBtnList.BindNavigationInputAction(ProjectOC.Input.InputManager.PlayerInput.PlayerUI.AlterSelected, UIBtnListContainer.BindType.started);
            this.UIBtnList.BindButtonInteractInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.Confirm, UIBtnListContainer.BindType.started);

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }
        #endregion

        #region UI对象引用
        public PlayerCharacter player;
        private BuildingManager BM => BuildingManager.Instance;
        #endregion

        #region Resource
        #region TextContent
        [System.Serializable]
        public struct PlayerUIPanelStruct
        {
            public TextTip[] Btns;
        }
        protected override void OnLoadJsonAssetComplete(PlayerUIPanelStruct datas)
        {
            InitBtnData(datas);
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/PlayerUIPanel";
            this.abname = "PlayerUIPanel";
            this.description = "PlayerUIPanel数据加载完成";
        }

        private UIBtnList UIBtnList;
        protected override void InitBtnInfo()
        {
            UIBtnListInitor uIBtnListInitor = this.transform.GetComponentInChildren<UIBtnListInitor>(true);
            this.UIBtnList = new UIBtnList(uIBtnListInitor);
            CustomButton panelButton1 = this.UIBtnList.GetBtn("PanelBtn") as CustomButton;
            panelButton1.InitBindData(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, (obj) =>
            {
                var vector2 = obj.ReadValue<UnityEngine.Vector2>();
                panelButton1.transform.Find("Wheel").GetComponent<Image>().fillAmount -= vector2.x/10;
            });
            CustomButton panelButton2 = this.UIBtnList.GetBtn("SliderBtn") as CustomButton;
            panelButton2.InitBindData(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, (obj) =>
            {
                var vector2 = obj.ReadValue<UnityEngine.Vector2>();
                panelButton2.transform.Find("Slider").GetComponent<Slider>().value += vector2.x / 10;
            });
        }
        private void InitBtnData(PlayerUIPanelStruct datas)
        {
            foreach (var tt in datas.Btns)
            {
                this.UIBtnList.SetBtnText(tt.name, tt.description.GetText());
            }
        }

        #endregion
        #endregion
    }

}
