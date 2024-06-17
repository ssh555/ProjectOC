using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ProjectOC.ClanNS.UI.UISelectClan;

namespace ProjectOC.ClanNS.UI
{
    public class UISelectClan : ML.Engine.UI.UIBasePanel<SelectClanPanel>
    {
        #region Str
        private const string str = "";
        #endregion
        #region Data
        private int Index = 0;
        private Dictionary<string, Sprite> tempSprite = new Dictionary<string, Sprite>();
        private TMPro.TextMeshProUGUI Text_Title;
        private Transform Clan;
        private Transform ClanL1;
        private Transform ClanL2;
        private Transform ClanR1;
        private Transform ClanR2;
        private Transform KeyTips;
        public bool IsInit = false;
        protected override void Start()
        {
            base.Start();
            InitTextContentPathData();
            Text_Title = transform.Find("TopTitle").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            Clan = transform.Find("Clans").Find("Up").Find("Clan");
            ClanL1 = transform.Find("Clans").Find("Up").Find("ClanL1");
            ClanL2 = transform.Find("Clans").Find("Up").Find("ClanL2");
            ClanR1 = transform.Find("Clans").Find("Up").Find("ClanR1");
            ClanR2 = transform.Find("Clans").Find("Up").Find("ClanR2");
            KeyTips = transform.Find("BotKeyTips").Find("KeyTips");
            IsInit = true;
        }
        [System.Serializable]
        public struct SelectClanPanel
        {
            public ML.Engine.TextContent.KeyTip Confirm;
            public ML.Engine.TextContent.KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            abpath = "OCTextContent/Clan";
            abname = "SelectClanPanel";
            description = "SelectClanPanel数据加载完成";
        }
        #endregion

        #region Override
        protected override void Exit()
        {
            foreach (var s in tempSprite.Values)
            {
                ML.Engine.Manager.GameManager.DestroyObj(s);
            }
            tempSprite.Clear();
            base.Exit();
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.SelectClan.Alter.performed -= Alter_started;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
        }
        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.SelectClan.Alter.performed += Alter_started;
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
            ML.Engine.Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            //ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(strPrefab_Mine_UI_SelectMineralSourcesPanel).Completed += (handle) =>
            //{
            //    UISelectMineralSourcesPanel selectMineralSourcesPanel = handle.Result.GetComponent<UISelectMineralSourcesPanel>();
            //    selectMineralSourcesPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.NormalPanel, false);
            //    selectMineralSourcesPanel.ProNodeId = ProNode.GetUID();
            //    selectMineralSourcesPanel.UIMineProNode = this;
            //    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(selectMineralSourcesPanel);
            //    MM.ChangeCurMineralMapData(MineralCircleData.SmallMapTuple.Item1);
            //    GameManager.Instance.StartCoroutine(PushSmallMapPanel(selectMineralSourcesPanel));
            //};
        }
        private void Alter_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            var f_offset = obj.ReadValue<Vector2>();
            var offset = new Vector2Int(Mathf.RoundToInt(f_offset.x), Mathf.RoundToInt(f_offset.y));
            int len = ManagerNS.LocalGameManager.Instance.ClanManager.Clans.Count;
            if (offset.x > 0)
            {
                Index = (Index + 1) % len;
            }
            else if (offset.x < 0)
            {
                Index = (Index - 1) % len;
            }
            Refresh();
        }
        #endregion

        #region UI
        public void SetClanUI(Clan clan, Transform transform)
        {
            bool isNotNull = clan != null;
            transform.Find("Name").GetComponent<TMPro.TextMeshProUGUI>().text = isNotNull ? clan.Name : str;
            transform.Find("Icon").gameObject.SetActive(isNotNull);
        }

        public override void Refresh()
        {
            if (ABJAProcessorJson == null || !ABJAProcessorJson.IsLoaded || !IsInit) { return; }
            var clans = ManagerNS.LocalGameManager.Instance.ClanManager.Clans;
            if (clans.Count > 0)
            {
                Index = Index % clans.Count;
                Text_Title.text = clans[Index].Name;
                SetClanUI(clans[Index], Clan);
                HashSet<int> sets = new HashSet<int>() { Index };
                int[] indexs = { Index - 1, Index - 2, Index + 1, Index + 2 };
                Transform[] uiElements = { ClanL1, ClanL2, ClanR1, ClanR2 };
                for (int i = 0; i < indexs.Length; i++)
                {
                    int index = indexs[i];
                    if (0 <= index && index < clans.Count && !sets.Contains(index))
                    {
                        SetClanUI(clans[index], uiElements[i]);
                        sets.Add(index);
                    }
                    else { SetClanUI(null, uiElements[i]); }
                }
                for (int i = 0; i < indexs.Length; i++)
                {
                    indexs[i] = indexs[i] % clans.Count;
                }
                for (int i = 0; i < indexs.Length; i++)
                {
                    if (!sets.Contains(indexs[i]))
                    {
                        SetClanUI(clans[indexs[i]], uiElements[i]);
                        sets.Add(indexs[i]);
                    }
                }
            }
            else
            {
                SetClanUI(null, Clan);
                SetClanUI(null, ClanL1);
                SetClanUI(null, ClanL2);
                SetClanUI(null, ClanR1);
                SetClanUI(null, ClanR2);
                Text_Title.text = "";
                KeyTips.Find("KT_Confirm").gameObject.SetActive(false);
            }
        }
        #endregion
    }
}