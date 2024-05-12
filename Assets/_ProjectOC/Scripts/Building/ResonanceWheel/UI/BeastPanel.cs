using ML.Engine.Input;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using ProjectOC.ManagerNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using System;
using static ProjectOC.ResonanceWheelSystem.UI.BeastPanel;
using ML.Engine.Utility;
namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class BeastPanel : ML.Engine.UI.UIBasePanel<BeastPanelStruct>
    {
        #region Unity
        public bool IsInit = false;

        protected override void Awake()
        {
            base.Awake();

            //BeastInfo
            var Info1 = this.transform.Find("HiddenBeastInfo2").Find("Info");
            Speed = Info1.Find("MovingSpeed").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            SpeedNumText = Info1.Find("MovingSpeed").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            GenderImage = Info1.Find("Icon").Find("GenderImage").GetComponent<Image>();


            var GInfo = Info1.Find("SkillGraph").Find("Viewport").Find("Content").Find("Ring");
            Cook = GInfo.Find("Cook").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            HandCraft = GInfo.Find("HandCraft").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Industry = GInfo.Find("Industry").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Magic = GInfo.Find("Magic").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Transport = GInfo.Find("Transport").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Collect = GInfo.Find("Collect").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();

            //需要调接口显示的隐兽信息
            BeastName = Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();
        }
        protected override void Start()
        {
            IsInit = true;
            Refresh();
            base.Start();
        }

        #endregion

        #region Override
        protected override void Exit()
        {
            base.Exit();
            ClearTemp();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.BeastList.EnableBtnList();
            this.ScheduleList.EnableBtnList();
        }

        public override void OnExit()
        {
            base.OnExit();
            this.BeastList.DisableBtnList();
            this.ScheduleList.DisableBtnList();
        }
        #endregion

        #region Internal
        protected override void UnregisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();

            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed -= Expel_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            this.BeastList.RemoveAllListener();
            this.BeastList.DeBindInputAction();
        }
        protected override void RegisterInput()
        {
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Enable();

            // 驱逐
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed += Expel_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

            this.BeastList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
        }

        private void Expel_performed(InputAction.CallbackContext obj)
        {
            LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[CurrentBeastIndex]);
            this.BeastList.DeleteButton(CurrentBeastIndex, () => { this.Refresh(); });
            
        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        #endregion

        #region UI
        #region temp
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;

        private void ClearTemp()
        {
            GameManager.DestroyObj(icon_genderfemaleSprite);
            GameManager.DestroyObj(icon_gendermaleSprite);
        }

        #endregion

        #region UI对象引用
        [ShowInInspector]
        private int CurrentBeastIndex { get { return this.BeastList.GetCurSelectedPos1(); } }

        [ShowInInspector]
        List<Worker> Workers = new List<Worker>();

        //BeastInfo
        private TMPro.TextMeshProUGUI Speed;
        private TMPro.TextMeshProUGUI SpeedNumText;

        private Image GenderImage;

        private TMPro.TextMeshProUGUI Cook;
        private TMPro.TextMeshProUGUI HandCraft;
        private TMPro.TextMeshProUGUI Industry;
        private TMPro.TextMeshProUGUI Magic;
        private TMPro.TextMeshProUGUI Transport;
        private TMPro.TextMeshProUGUI Collect;

        //需要调接口显示的隐兽信息
        private TMPro.TextMeshProUGUI BeastName;

        #endregion

        public override void Refresh()
        {
            if (this.ABJAProcessorJson == null || !this.ABJAProcessorJson.IsLoaded || !IsInit)
            {
                return;
            }
            var Content = this.transform.Find("HiddenBeastInfo1").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");

            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();

            if(CurrentBeastIndex != -1)
            {
                this.transform.Find("HiddenBeastInfo2").Find("Info").gameObject.SetActive(true);
                this.transform.Find("HiddenBeastInfo3").Find("Info").gameObject.SetActive(true);

                //BeastInfo

                Speed.text = this.PanelTextContent.Speed;
                Cook.text = this.PanelTextContent.Cook;
                HandCraft.text = this.PanelTextContent.HandCraft;
                Industry.text = this.PanelTextContent.Industry;
                Magic.text = this.PanelTextContent.Magic;
                Transport.text = this.PanelTextContent.Transport;
                Collect.text = this.PanelTextContent.Collect;

                Worker worker = Workers[CurrentBeastIndex];
                List<float> datas = new List<float>();

                Dictionary<SkillType,Skill> skillDic = worker.Skill;
                foreach (var skill in skillDic)
                {
                    datas.Add(skillDic[skill.Key].LevelCurrent / 10f);
                }

                var radar = this.transform.Find("HiddenBeastInfo2").Find("Info").Find("SkillGraph").Find("Viewport").Find("Content").Find("Radar").GetComponent<UIPolygon>();
                radar.DrawPolygon(datas);
                
                //性别
                if (worker.Gender == Gender.Male)
                {
                    GenderImage.sprite = icon_gendermaleSprite;
                }
                else
                {
                    GenderImage.sprite = icon_genderfemaleSprite;
                }

                this.transform.Find("HiddenBeastInfo2").Find("Info").Find("Icon").Find("mask").GetComponent<Image>().fillAmount = (float)worker.APCurrent / worker.APMax; ;
                BeastName.text = worker.Name;
                SpeedNumText.text = worker.RealWalkSpeed.ToString();

                var Info = this.transform.Find("HiddenBeastInfo3").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");

                foreach (var feature in worker.GetSortFeature())
                {

                    var tPrefab = this.objectPool.GetNextObject("DescriptionPool", Info);
                    tPrefab.transform.Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                    tPrefab.transform.Find("Text2").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Description;
                    tPrefab.transform.Find("Text3").GetComponent<TMPro.TextMeshProUGUI>().text =
                "<color=#6FB502><b><sprite name=\"Triangle\" index=0 tint=1>" + feature.EffectsDescription + "</b></color>";
                    
                }
                //更新日程表
                var schedule = this.transform.Find("BeastDuty").Find("Part1").Find("Info").Find("Schedule").Find("Time");
                TimeStatus[] workerTimeStatus = worker.TimeArrangement.Status;
                
                for (int i = 0; i < workerTimeStatus.Length; i++)
                {
                    //Debug.Log("34 " + i);
                    Image img = schedule.Find("T" + i.ToString()).GetComponent<Image>();
                    switch (workerTimeStatus[i])
                    {
                        case TimeStatus.Relax:
                            //Debug.Log("workerTimeStatus Relax");
                            img.color = UnityEngine.Color.green;
                            break;
                        case TimeStatus.Work_Transport:
                            //Debug.Log("workerTimeStatus Work_Transport");
                            img.color = UnityEngine.Color.blue;
                            break;
                        case TimeStatus.Work_OnDuty:
                            //Debug.Log("workerTimeStatus Work_OnDuty");
                            img.color = UnityEngine.Color.yellow;
                            break;
                    }
                }
            }
            else
            {
                this.transform.Find("HiddenBeastInfo2").Find("Info").gameObject.SetActive(false);
                this.transform.Find("HiddenBeastInfo3").Find("Info").gameObject.SetActive(false);
            }  
        }
        #endregion

        #region Resource

        #region TextContent
        [System.Serializable]
        public struct BeastPanelStruct
        {
            //BeastInfo
            public TextContent Speed;

            public TextContent Cook;
            public TextContent HandCraft;
            public TextContent Industry;
            public TextContent Magic;
            public TextContent Transport;
            public TextContent Collect;

            public KeyTip Expel;
            public KeyTip Back;
        }
        protected override void InitTextContentPathData()
        {
            this.abpath = "OCTextContent/ResonanceWheel";
            this.abname = "BeastPanel";
            this.description = "BeastPanel数据加载完成";
        }
        #endregion

        protected override void InitObjectPool()
        {
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Texture2D, "Texture2DPool", 1,
            "SA_ResonanceWheel_UI", (handle) =>
            {
                string Pre = "Tex2D_Resonance_UI_";
                SpriteAtlas resonanceWheelAtlas = handle.Result as SpriteAtlas;
                icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite(Pre + "icon_genderfemale");
                icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite(Pre + "icon_gendermale");
            }
            );
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "BeastBioPool", LocalGameManager.Instance.WorkerManager.GetWorkers().Count, "Prefab_ResonanceWheel_UIPrefab/Prefab_ResonanceWheel_UI_BeastBio.prefab");
            this.objectPool.RegisterPool(UIObjectPool.HandleType.Prefab, "DescriptionPool", 5, "Prefab_ResonanceWheel_UIPrefab/Prefab_ResonanceWheel_UI_Description.prefab");
            base.InitObjectPool();
        }
        [ShowInInspector]
        private UIBtnList BeastList;
        [ShowInInspector]
        private UIBtnList ScheduleList;
        protected override void InitBtnInfo()
        {
            BeastList = new UIBtnList(this.transform.Find("HiddenBeastInfo1").Find("Info").Find("Scroll View").GetComponentInChildren<UIBtnListInitor>());
            ScheduleList = new UIBtnList(this.transform.Find("BeastDuty").Find("Part1").Find("Info").Find("Schedule").Find("Time").GetComponentInChildren<UIBtnListInitor>());
        }
        protected override void InitBtnInfoAfterInitObjectPool()
        {
            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();

            this.objectPool.ResetAllObject();

            for (int i = 0; i < Workers.Count; i++)
            {
                var tPrefab = this.objectPool.GetNextObject("BeastBioPool");

                tPrefab.transform.Find("Bio").Find("mask").GetComponent<Image>().fillAmount = (float)Workers[i].APCurrent / Workers[i].APMax;
                BeastList.AddBtn(tPrefab);
            }
            this.BeastList.OnSelectButtonChanged += () => { this.Refresh(); };
            this.ScheduleList.SetAllBtnAction(() => {

                if (CurrentBeastIndex == -1) return;
                Worker worker = Workers[CurrentBeastIndex];
                int time = ScheduleList.GetCurSelectedPos1();
                if (worker != null && time != -1) 
                {
                    if (worker.TimeArrangement.Status[time] == TimeStatus.Relax)
                    {
                        worker.SetTimeStatus(time, TimeStatus.Work_Transport);
                    }
                    else if (worker.TimeArrangement.Status[time] == TimeStatus.Work_Transport)
                    {
                        worker.SetTimeStatus(time, TimeStatus.Relax);
                    }
                    this.Refresh();
                }
                
            });
            
        }
        #endregion
    }


}
