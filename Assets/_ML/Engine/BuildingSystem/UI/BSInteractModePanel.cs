using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.UI.BSInteractModePanel;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractModePanel : Engine.UI.UIBasePanel<BSInteractModePanelStruct>, Timer.ITickComponent
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private ProjectOC.Player.PlayerCharacter Player => (Manager.GameManager.Instance.CharacterManager.GetLocalController() as ProjectOC.Player.OCPlayerController).currentCharacter;

        private MonoBuildingManager monoBM;
        #region KeyTip
        private UIKeyTip keycom;
        private UIKeyTip movebuild;
        private UIKeyTip destroybuild;
        private UIKeyTip altermat;
        private UIKeyTip alterbpart;
        private UIKeyTip selectbpart;
        private UIKeyTip back;

        #endregion
        private GameObject interactPanel;

        #endregion

        #region TickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        public virtual void FixedTick(float deltatime)
        {
            this.Placer.CheckInteractBPart(deltatime);
        }

        #endregion

        #region Unity
        protected override void Awake()
        {
            this.enabled = false;
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            interactPanel = this.transform.Find("BPartInteractTip").gameObject;
        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            BM.Placer.OnDestroySelectedBPart += OnDestroySelectedBPart;
            S_LastSelectedCategory1Index = -1;
            S_LastSelectedCategory2Index = -1;
        }


        public override void OnPause()
        {
            base.OnPause();
            BM.Placer.OnDestroySelectedBPart -= OnDestroySelectedBPart;
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            BM.Placer.OnDestroySelectedBPart += OnDestroySelectedBPart;
        }
        public override void OnExit()
        {
            base.OnExit();
            BM.Placer.OnDestroySelectedBPart -= OnDestroySelectedBPart;
        }
        public override void Refresh()
        {

        }
        #endregion

        #region KeyFunction
        protected override void UnregisterInput()
        {
            this.Placer.BInput.Build.Disable();
            this.Placer.OnSelectBPart -= this.OnSelectBPart;

            Manager.GameManager.Instance.TickManager.UnregisterFixedTick(this);

            this.Placer.DisablePlayerInput();

            this.Placer.BInput.Build.KeyCom.performed -= Placer_EnterKeyCom;
            this.Placer.BInput.Build.ChangeOutLook.performed -= Placer_EnterAppearance;
            this.Placer.BInput.Build.MoveBuild.performed -= Placer_EnterEdit;
            this.Placer.BInput.Build.DestroyBuild.performed -= Placer_DestroyBPart;
            this.Placer.BInput.Build.ChangeInteractiveActor.performed -= Placer_ChangeInteractiveActor;
            this.Placer.BInput.Build.SelectBuild.performed -= Placer_EnterPlace;
            this.Placer.backInputAction.performed -= Placer_ExitBuild;
        }

        protected override void RegisterInput()
        {
            this.Placer.BInput.Build.Enable();
            this.Placer.OnSelectBPart += this.OnSelectBPart;

            Manager.GameManager.Instance.TickManager.RegisterFixedTick(0, this);

            this.Placer.EnablePlayerInput();

            this.Placer.BInput.Build.KeyCom.performed += Placer_EnterKeyCom;
            this.Placer.BInput.Build.ChangeOutLook.performed += Placer_EnterAppearance;
            this.Placer.BInput.Build.MoveBuild.performed += Placer_EnterEdit;
            this.Placer.BInput.Build.DestroyBuild.performed += Placer_DestroyBPart;
            this.Placer.BInput.Build.ChangeInteractiveActor.performed += Placer_ChangeInteractiveActor;
            this.Placer.BInput.Build.SelectBuild.performed += Placer_EnterPlace;
            this.Placer.backInputAction.performed += Placer_ExitBuild;
        }

        private void Placer_EnterKeyCom(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.Placer.SelectedPartInstance != null)
            {
                monoBM.PushPanel<BSInteractMode_KeyComPanel>();
            }
        }

        private void Placer_EnterAppearance(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.Placer.SelectedPartInstance != null)
            {
                monoBM.PushPanel<BSAppearancePanel>();
            }
        }

        private void Placer_EnterEdit(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.Placer.SelectedPartInstance != null && this.Placer.SelectedPartInstance.CanEnterEditMode())
            {
                monoBM.PushPanel<BSEditModePanel>();
            }
        }

        private void Placer_ChangeInteractiveActor(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.Placer.SelectedPartInstance != null)
            {
                this.Placer.SwitchInteractBPart(obj.ReadValue<float>() > 0 ? 1 : -1);
            }
        }

        private void Placer_DestroyBPart(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.Placer.SelectedPartInstance != null && this.Placer.SelectedPartInstance.CanEnterEditMode())
            {
                this.Placer.DestroySelectedBPart();
            }
        }


        private void Placer_EnterPlace(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            monoBM.PushPanel<BSSelectBPartPanel>();
        }

        private void Placer_ExitBuild(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //monoBM.PopPanel();
            this.Placer.Mode = BuildingMode.None;
        }

        #endregion

        #region OnEvent
        private void OnDestroySelectedBPart(IBuildingPart bpart)
        {
            if (bpart is ProjectOC.StoreNS.IWorldStore worldStore)
            {
                worldStore.Store.Destroy();
            }
            else if (bpart is ProjectOC.ProNodeNS.IWorldProNode worldProNode)
            {
                worldProNode.ProNode.Destroy();
            }
            else if (bpart is ProjectOC.RestaurantNS.WorldRestaurant worldRestaurant)
            {
                worldRestaurant.Restaurant.Destroy();
            }

            foreach (var formula in BuildingManager.Instance.GetRawAll(bpart.Classification.ToString()))
            {
                if (ItemManager.Instance.IsValidItemID(formula.id) && formula.num > 0)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(formula.id, formula.num);
                    foreach (var item in items)
                    {
                        Player.Inventory.AddItem(item);
                    }
                }
            }
        }

        private void OnSelectBPart(IBuildingPart BPart)
        {

            interactPanel.SetActive(BPart != null);
        }

        #endregion

        #region TextContent
        [System.Serializable]
        public struct BSInteractModePanelStruct
        {
            public KeyTip[] KeyTips;
        }

        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/BuildingSystem/UI";
            this.abname = "BSInteractModePanel";
            this.description = "BSInteractModePanel数据加载完成";
        }
        #endregion

        #region 临时存储
        public static int S_LastSelectedCategory1Index = -1;
        public static int S_LastSelectedCategory2Index = -1;
        #endregion
    }



}

