using ML.Engine.BuildingSystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.TechTree.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.Test_BuildingManager;

namespace ProjectOC.Player.UI
{
    public class PlayerUIPanel : UIBasePanel, ML.Engine.Timer.ITickComponent
    {
        public static bool IsLoaded = false;

        public PlayerCharacter player;

        public UITechPointPanel uITechPointPanel;

        private IUISelected CurSelected;

        private SelectedButton EnterBuildBtn;
        private SelectedButton EnterTechTreeBtn;

        private BuildingManager BM => BuildingManager.Instance;

        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }

        private void Start()
        {
            StartCoroutine(InitUITextContents());

            var btnList = this.transform.Find("ButtonList");
            this.EnterBuildBtn = btnList.Find("EnterBuild").GetComponent<SelectedButton>();
            this.EnterBuildBtn.OnInteract += () =>
            {
                if (ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame() && BM.Mode == BuildingMode.None)
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
            };



            this.EnterTechTreeBtn = btnList.Find("EnterTechTree").GetComponent<SelectedButton>();
            this.EnterTechTreeBtn.OnInteract += () =>
            {
                var panel = GameObject.Instantiate(uITechPointPanel);
                panel.transform.SetParent(this.transform.parent, false);
                panel.inventory = this.player.Inventory;
                ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(panel);
            };


            var btn = btnList.GetComponentsInChildren<SelectedButton>();
            
            for(int i = 0; i < btn.Length; ++i)
            {
                int last = (i - 1 + btn.Length) % btn.Length;
                int next = (i + 1 + btn.Length) % btn.Length;

                btn[i].UpUI = btn[last];
                btn[i].DownUI = btn[next];
            }

            this.CurSelected = EnterBuildBtn;
            this.CurSelected.OnSelectedEnter();
        }

        public void Tick(float deltatime)
        {
            if(ML.Engine.Input.InputManager.Instance.Common.Common.Comfirm.WasPressedThisFrame())
            {
                this.CurSelected.Interact();
            }
            if(Input.InputManager.PlayerInput.PlayerUI.AlterSelected.WasPressedThisFrame())
            {
                var vec2 = Input.InputManager.PlayerInput.PlayerUI.AlterSelected.ReadValue<Vector2>();
                if(vec2.y > 0.1f)
                {
                    this.CurSelected.OnSelectedExit();
                    this.CurSelected = this.CurSelected.UpUI;
                    this.CurSelected.OnSelectedEnter();
                }
                else if(vec2.y < -0.1f)
                {
                    this.CurSelected.OnSelectedExit();
                    this.CurSelected = this.CurSelected.DownUI;
                    this.CurSelected.OnSelectedEnter();
                }
            }
            if (ML.Engine.Input.InputManager.Instance.Common.Common.Back.WasPressedThisFrame())
            {
                ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
            }
        }

        private void OnDestroy()
        {
            (this as ML.Engine.Timer.ITickComponent).DisposeTick();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            Input.InputManager.PlayerInput.PlayerUI.Enable();
            UpdateText();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        }

        public override void OnPause()
        {
            base.OnPause();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            Input.InputManager.PlayerInput.PlayerUI.Disable();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
            Input.InputManager.PlayerInput.PlayerUI.Enable();
            UpdateText();
        }

        public override void OnExit()
        {
            base.OnExit();
            ML.Engine.Manager.GameManager.Instance.TickManager.UnregisterTick(this);
            Input.InputManager.PlayerInput.PlayerUI.Disable();
        }

        public const string TextContentABPath = "JSON/TextContent/Player";
        public const string TextContentName = "PlayerUIBotPanel";
        public Dictionary<string, TextTip> TipDict = new Dictionary<string, TextTip>();
        public bool IsLoadOvered = false;

        [System.Serializable]
        private struct TextTips
        {
            public ML.Engine.TextContent.TextTip[] tips;
        }
        private IEnumerator InitUITextContents()
        {
            if(IsLoaded)
            {
                yield break;
            }
#if UNITY_EDITOR
            float startT = Time.realtimeSinceStartup;
#endif
            while (ML.Engine.Manager.GameManager.Instance.ABResourceManager == null)
            {
                yield return null;
            }
            var abmgr = ML.Engine.Manager.GameManager.Instance.ABResourceManager;
            AssetBundle ab;
            var crequest = abmgr.LoadLocalABAsync(TextContentABPath, null, out ab);
            yield return crequest;
            if (crequest != null)
            {
                ab = crequest.assetBundle;
            }

            var request = ab.LoadAssetAsync<TextAsset>(TextContentName);
            yield return request;
            ML.Engine.TextContent.TextTip[] Tips = JsonConvert.DeserializeObject<ML.Engine.TextContent.TextTip[]>((request.asset as TextAsset).text);
            foreach (var tip in Tips)
            {
                this.TipDict.Add(tip.name, tip);
            }
            IsLoadOvered = true;
#if UNITY_EDITOR
            Debug.Log("InitUITextContents cost time: " + (Time.realtimeSinceStartup - startT));
#endif
            this.UpdateText();

            IsLoaded = true;
        }


        private void UpdateText()
        {
            if(IsLoadOvered)
            {
                this.EnterBuildBtn.text.text = this.TipDict["enterbuild"].GetDescription();
                this.EnterTechTreeBtn.text.text = this.TipDict["techtree"].GetDescription();
            }
        }
    }
}

