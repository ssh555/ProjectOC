using ML.Engine.TextContent;
using System.Collections.Generic;
using UnityEngine;
using static ProjectOC.ClanNS.UI.UIClanBed;
using Sirenix.OdinInspector;


namespace ProjectOC.ClanNS.UI
{
    public class UIClanBed : ML.Engine.UI.UIBasePanel<ClanBedPanel>
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
            BotKeyTips = transform.Find("BotKeyTips").Find("KeyTips");

            ChangeClan.gameObject.SetActive(false);
            IsInit = true;
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            SortClans(true);
            base.Enter();
        }
        protected override void Exit()
        {
            ClearTemp();
            base.Exit();
        }
        #endregion

        #region Internal
        private enum Mode
        {
            Bed = 0,
            ChangeClan = 1
        }
        [ShowInInspector]
        private Mode CurMode;
        private Clan Clan => Bed.Clan;
        public ClanBed Bed;

        #region Clans
        [ShowInInspector]
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

        public void SortClans(bool needClear = false)
        {
            if (needClear)
            {
                this.Clans.Clear();
                this.Clans.AddRange(ManagerNS.LocalGameManager.Instance.ClanManager.Clans);
            }
            this.Clans.Sort(new Clan.Sort());
            if (this.Bed.HaveClan)
            {
                this.Clans.Remove(this.Clan);
                this.Clans.Insert(0, this.Clan);
            }
            index = 0;
        }

        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Disable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.SetEmpty.performed -= SetEmpty_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Pre.performed -= Pre_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Post.performed -= Post_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Disable();
        }

        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Enable();
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.SetEmpty.performed += SetEmpty_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Pre.performed += Pre_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Post.performed += Post_performed;
            ProjectOC.Input.InputManager.PlayerInput.UIBed.Enable();
        }

        private void SetEmpty_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (CurMode == Mode.Bed && this.Bed.HaveClan)
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
                index = 0;
                SortClans(true);
            }
            else if (CurMode == Mode.ChangeClan)
            {
                if (this.CurClan != null && this.Bed.CanSetClan && (!Bed.HaveClan || Bed.Clan != CurClan))
                {
                    if (CurClan.HasBed)
                    {
                        string text = PanelTextContent.textConfirmPrefix + CurClan.Name + PanelTextContent.textConfirmSuffix;
                        ML.Engine.Manager.GameManager.Instance.UIManager.PushNoticeUIInstance(ML.Engine.UI.UIManager.NoticeUIType.PopUpUI, new ML.Engine.UI.UIManager.PopUpUIData(text, null, null, () => { Bed.SetClan(this.CurClan);}));
                    }
                    else
                    {
                        Bed.SetClan(this.CurClan);
                        SortClans(false);
                    }
                }
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
                this.Clans.Clear();
                index = 0;
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
        private Transform BotKeyTips;
        #endregion
        public override void Refresh()
        {
            // 加载完成JSON数据 & 查找完所有引用
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }

            ChangeClan.gameObject.SetActive(CurMode == Mode.ChangeClan);
            if (CurMode == Mode.Bed)
            {
                Text_Title.text = PanelTextContent.textBed;
                Bed_Clan.gameObject.SetActive(Bed.HaveClan);
                Bed_Clan.Find("TopTitle").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = Clan?.Name ?? "";

            }
            else if (CurMode == Mode.ChangeClan)
            {
                ChangeClan_Clan.gameObject.SetActive(Bed.HaveClan);
                ChangeClan_Clan.Find("TopTitle").Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = Clan?.Name ?? "";

                if (CurClan != null)
                {
                    ChangeClan.Find("Background1").gameObject.SetActive(CurClan.HasBed);
                    ChangeClan.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = CurClan.Name ?? "";
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

            #region BotKeyTips
            BotKeyTips.Find("KT_Confirm").gameObject.SetActive(CurMode == Mode.ChangeClan);
            BotKeyTips.Find("KT_Back").gameObject.SetActive(true);
            #endregion
        }
        #endregion

        #region TextContent
        [System.Serializable]
        public struct ClanBedPanel
        {
            public TextContent textBed;
            public TextContent textConfirmPrefix;
            public TextContent textConfirmSuffix;
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
            this.abpath = "OCTextContent/Building";
            this.abname = "BedPanel";
            this.description = "BedPanel数据加载完成";
        }
        #endregion
    }
}