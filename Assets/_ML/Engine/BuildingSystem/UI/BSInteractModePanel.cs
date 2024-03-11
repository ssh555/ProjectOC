using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem.CompositeSystem;
using ML.Engine.InventorySystem;
using ML.Engine.TextContent;
using ProjectOC.ProNodeNS;
using ProjectOC.StoreNS;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractModePanel : Engine.UI.UIBasePanel, Timer.ITickComponent
    {
        #region Property|Field
        private BuildingManager BM => BuildingManager.Instance;
        private BuildingPlacer.BuildingPlacer Placer => BM.Placer;
        private ProjectOC.Player.PlayerCharacter Player => GameObject.Find("PlayerCharacter")?.GetComponent<ProjectOC.Player.PlayerCharacter>();
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
        private void Awake()
        {
            this.enabled = false;
            monoBM = ML.Engine.Manager.GameManager.Instance.GetLocalManager<MonoBuildingManager>();
            Transform keytip = this.transform.Find("BPartInteractTip");

            keycom = new UIKeyTip();
            keycom.root = keytip.Find("KT_KeyCom") as RectTransform;
            keycom.img = keycom.root.Find("Image").GetComponent<Image>();
            keycom.keytip = keycom.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            keycom.description = keycom.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            keycom.ReWrite(monoBM.KeyTipDict["keycom"]);

            movebuild = new UIKeyTip();
            movebuild.root = keytip.Find("KT_MoveBuild") as RectTransform;
            movebuild.img = movebuild.root.Find("Image").GetComponent<Image>();
            movebuild.keytip = movebuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            movebuild.description = movebuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            movebuild.ReWrite(monoBM.KeyTipDict["movebuild"]);

            destroybuild = new UIKeyTip();
            destroybuild.root = keytip.Find("KT_DestroyBuild") as RectTransform;
            destroybuild.img = destroybuild.root.Find("Image").GetComponent<Image>();
            destroybuild.keytip = destroybuild.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            destroybuild.description = destroybuild.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            destroybuild.ReWrite(monoBM.KeyTipDict["destroybuild"]);

            altermat = new UIKeyTip();
            altermat.root = keytip.Find("KT_AlterMat") as RectTransform;
            altermat.img = altermat.root.Find("Image").GetComponent<Image>();
            altermat.keytip = altermat.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            altermat.description = altermat.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            altermat.ReWrite(monoBM.KeyTipDict["altermat"]);

            keytip = this.transform.Find("InteractTip");
            alterbpart = new UIKeyTip();
            alterbpart.root = keytip.Find("KT_AlterBPart") as RectTransform;
            alterbpart.img = alterbpart.root.Find("Image").GetComponent<Image>();
            alterbpart.keytip = alterbpart.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            alterbpart.description = alterbpart.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            alterbpart.ReWrite(monoBM.KeyTipDict["alterbpart"]);

            selectbpart = new UIKeyTip();
            selectbpart.root = keytip.Find("KT_SelectBPart") as RectTransform;
            selectbpart.img = selectbpart.root.Find("Image").GetComponent<Image>();
            selectbpart.keytip = selectbpart.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            selectbpart.description = selectbpart.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            selectbpart.ReWrite(monoBM.KeyTipDict["selectbpart"]);

            back = new UIKeyTip();
            back.root = keytip.Find("KT_Back") as RectTransform;
            back.img = back.root.Find("Image").GetComponent<Image>();
            back.keytip = back.img.transform.Find("KeyText").GetComponent<TextMeshProUGUI>();
            back.description = back.img.transform.Find("KeyTipText").GetComponent<TextMeshProUGUI>();
            back.ReWrite(monoBM.KeyTipDict["back"]);

        }

        #endregion

        #region Override
        public override void OnEnter()
        {
            base.OnEnter();
            this.RegisterInput();
            BM.Placer.OnDestroySelectedBPart += OnDestroySelectedBPart;
        }


        public override void OnPause()
        {
            base.OnPause();
            this.UnregisterInput();
            BM.Placer.OnDestroySelectedBPart -= OnDestroySelectedBPart;
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            this.RegisterInput();
            BM.Placer.OnDestroySelectedBPart += OnDestroySelectedBPart;
        }

        public override void OnExit()
        {
            base.OnExit();
            this.UnregisterInput();
        }

        public override void Refresh()
        {
            keycom.ReWrite(monoBM.KeyTipDict["keycom"]);
            movebuild.ReWrite(monoBM.KeyTipDict["movebuild"]);
            destroybuild.ReWrite(monoBM.KeyTipDict["destroybuild"]);
            altermat.ReWrite(monoBM.KeyTipDict["altermat"]);
            selectbpart.ReWrite(monoBM.KeyTipDict["selectbpart"]);
            back.ReWrite(monoBM.KeyTipDict["back"]);
        }
        #endregion

        #region KeyFunction
        private void UnregisterInput()
        {
            this.Placer.BInput.Build.Disable();

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

        private void RegisterInput()
        {
            this.Placer.BInput.Build.Enable();

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

        private void OnDestroySelectedBPart(IBuildingPart bpart)
        {
            if (bpart is WorldStore worldStore)
            {
                worldStore.Store.Destroy(Player);
            }
            else if (bpart is WorldProNode worldProNode)
            {
                worldProNode.ProNode.Destroy(Player);
            }

            bool flag = false;
            List<Item> resItems = new List<Item>();
            foreach (Formula formula in CompositeManager.Instance.GetCompositonFomula(BuildingManager.Instance.GetID(bpart.Classification.ToString())))
            {
                if (ItemManager.Instance.IsValidItemID(formula.id) && formula.num > 0)
                {
                    List<Item> items = ItemManager.Instance.SpawnItems(formula.id, formula.num);
                    foreach (var item in items)
                    {
                        if (flag)
                        {
                            resItems.Add(item);
                        }
                        else
                        {
                            if (!Player.Inventory.AddItem(item))
                            {
                                flag = true;
                            }
                        }
                    }
                }
            }
            // 没有加到玩家背包的都变成WorldItem
            foreach (Item item in resItems)
            {
                // 不需要，就异步同时执行
#pragma warning disable CS4014
                ItemManager.Instance.SpawnWorldItem(item, bpart.transform.position, bpart.transform.rotation);
#pragma warning restore CS4014
            }
        }
    }
}

