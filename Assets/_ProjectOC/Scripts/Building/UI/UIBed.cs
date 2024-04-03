using ML.Engine.TextContent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.Building.UI.UIBed;
using ProjectOC.ClanNS;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;

namespace ProjectOC.Building.UI
{
    public class UIBed : ML.Engine.UI.UIBasePanel<BedPanel>
    {
        #region Unity
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();

            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Bed_Clan = transform.Find("Bed").Find("Clan");
            ChangeClan = transform.Find("ChangeClan");
            ChangeClan_Clan = ChangeClan.Find("Clan");
            ConfirmClan = transform.Find("ConfirmClan");
            BotKeyTips = transform.Find("BotKeyTips").Find("KeyTips");

            ChangeClan.gameObject.SetActive(false);
            IsInit = true;
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

        protected override void Enter()
        {
            this.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Enable();
            base.Enter();
        }

        protected override void Exit()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Disable();
            this.UnregisterInput();
            base.Exit();
        }
        #endregion

        #region Internal
        private enum Mode
        {
            Bed = 0,
            ChangeClan = 1,
            ConfirmClan = 2
        }
        [ShowInInspector]
        private Mode CurMode;
        private Clan Clan => Bed.Clan;
        public Bed Bed;

        #region Clans
        private List<Clan> Clans = new List<Clan>();
        /// <summary>
        /// 当前选中的Index
        /// </summary>
        private int index = 0;
        private int Index
        {
            get => index;
            set
            {
                if (Clans != null && Clans.Count > 0)
                {
                    index = value;
                    if (index == -1)
                    {
                        index = Clans.Count - 1;
                    }
                    else if (index == Clans.Count)
                    {
                        index = 0;
                    }
                }
                else
                {
                    index = 0;
                }
                this.Refresh();
            }
        }
        /// <summary>
        /// 当前选中的Clan
        /// </summary>
        private Clan CurClan
        {
            get
            {
                if (Clans != null && Index < Clans.Count)
                {
                    return Clans[Index];
                }
                return null;
            }
        }
        #endregion

        protected override void UnregisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.SetEmpty.performed -= SetEmpty_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Pre.performed -= Pre_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Post.performed -= Post_performed;
        }

        protected override void RegisterInput()
        {
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.SetEmpty.performed += SetEmpty_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Pre.performed += Pre_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Post.performed += Post_performed;
        }

        private void SetEmpty_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Bed && this.Bed.HasClan)
            {
                this.Bed.SetEmpty();
                Refresh();
            }
        }
        private void Pre_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.CurMode == Mode.ChangeClan)
            {
                Index -= 1;
            }
        }
        private void Post_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (this.CurMode == Mode.ChangeClan)
            {
                Index += 1;
            }
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Bed)
            {
                this.CurMode = Mode.ChangeClan;
            }
            else if (CurMode == Mode.ChangeClan)
            {
                if (this.CurClan != null && this.Bed.CanSetClan)
                {
                    this.CurMode = Mode.ConfirmClan;
                }
            }
            else if (CurMode == Mode.ConfirmClan)
            {
                this.Bed.SetClan(this.CurClan);
                this.CurMode = Mode.ChangeClan;
            }
            Refresh();
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Bed)
            {
                UIMgr.PopPanel();
            }
            else if (CurMode == Mode.ChangeClan)
            {
                CurMode = Mode.Bed;
                Refresh();
            }
            else if(CurMode == Mode.ConfirmClan)
            {
                CurMode = Mode.ChangeClan;
                Refresh();
            }
        }
        #endregion

        #region UI
        #region Temp
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();

        private void ClearTemp()
        {
            foreach (var s in tempSprite)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s.Value);
            }
        }
        #endregion

        #region UI对象引用
        private TMPro.TextMeshProUGUI Text_Title;
        private Transform Bed_Clan;
        private Transform ChangeClan;
        private Transform ChangeClan_Clan;
        private Transform ConfirmClan;
        private Transform BotKeyTips;
        #endregion
        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            if (CurMode == Mode.Bed)
            {
                ChangeClan.gameObject.SetActive(false);
                ConfirmClan.gameObject.SetActive(false);

                Text_Title.text = PanelTextContent.textBed;
                Bed_Clan.gameObject.SetActive(Bed.HasClan);
                Bed_Clan.Find("TopTitle").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = Clan?.Name ?? "";

            }
            else if (CurMode == Mode.ChangeClan)
            {
                this.Clans = LocalGameManager.Instance.ClanManager.Clans;
                ChangeClan.gameObject.SetActive(true);
                ConfirmClan.gameObject.SetActive(false);

                ChangeClan_Clan.gameObject.SetActive(Bed.HasClan);
                ChangeClan_Clan.Find("TopTitle").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = Clan?.Name ?? "";

                if (CurClan != null)
                {
                    ChangeClan.Find("Background1").gameObject.SetActive(CurClan.HasBed);
                    ChangeClan.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = CurClan?.Name ?? "";
                    ChangeClan.Find("Background3").gameObject.SetActive(true);
                    ChangeClan.Find("Name").gameObject.SetActive(true);
                    ChangeClan.Find("Icon").gameObject.SetActive(true);
                }
                else
                {
                    ChangeClan.Find("Background1").gameObject.SetActive(false);
                    ChangeClan.Find("Background3").gameObject.SetActive(false);
                    ChangeClan.Find("Name").gameObject.SetActive(false);
                    ChangeClan.Find("Icon").gameObject.SetActive(false);
                }
            }
            else if (CurMode == Mode.ConfirmClan)
            {
                ChangeClan.gameObject.SetActive(true);
                ConfirmClan.gameObject.SetActive(true);
            }

            #region BotKeyTips
            if (CurMode == Mode.Bed)
            {
                BotKeyTips.Find("KT_Confirm").gameObject.SetActive(false);
                BotKeyTips.Find("KT_Back").gameObject.SetActive(true);
            }
            else if(CurMode == Mode.ChangeClan)
            {
                BotKeyTips.Find("KT_Confirm").gameObject.SetActive(false);
                BotKeyTips.Find("KT_Back").gameObject.SetActive(true);
            }
            #endregion
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct BedPanel
        {
            public TextContent textBed;
            public KeyTip SetEmpty;
            public KeyTip ConfirmClan;
            public KeyTip Pre;
            public KeyTip Post;
            public KeyTip Confirm;
            public KeyTip Back;
            public KeyTip Confirm1;
            public KeyTip Back1;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OC/Json/TextContent/Building";
            this.abname = "BedPanel";
            this.description = "BedPanel数据加载完成";
        }
        #endregion
    }
}