using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.Input;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
using ML.Engine.Timer;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using ProjectOC.WorkerEchoNS;
using ProjectOC.WorkerNS;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub1;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheel_sub2;
using static ProjectOC.ResonanceWheelSystem.UI.ResonanceWheelUI;


namespace ProjectOC.ResonanceWheelSystem.UI
{
    public class BeastPanel : ML.Engine.UI.UIBasePanel
    {

        #region Unity
        public bool IsInit = false;
        private void Start()
        {
            InitUITextContents();
            InitUITexture2D();
            InitPrefabs();
            //KeyTips
            UIKeyTipComponents = this.transform.GetComponentsInChildren<UIKeyTipComponent>(true);
            foreach (var item in UIKeyTipComponents)
            {
                uiKeyTipDic.Add(item.InputActionName, item);
            }

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

        IsInit = true;

          

            Refresh();
        }

        private List<AsyncOperationHandle<GameObject>> goHandle = new List<AsyncOperationHandle<GameObject>>();
        private AsyncOperationHandle spriteAtlasHandle;
        private void OnDestroy()
        {
            GM.ABResourceManager.Release(spriteAtlasHandle);
            foreach(var handle in goHandle)
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
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Enable();
            UikeyTipIsInit = false;
            this.Refresh();
        }

        private void Exit()
        {

            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();
        }

        private void UnregisterInput()
        {



            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.SwitchBeast.started -= SwitchBeast_started;
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.SwitchBeast.canceled -= SwitchBeast_canceled;
            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed -= Expel_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }



        private void RegisterInput()
        {

            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.SwitchBeast.started += SwitchBeast_started;
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.SwitchBeast.canceled += SwitchBeast_canceled;

            //驱逐
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed += Expel_performed;

            // 返回
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }

        #region SwitchBeast_performed
        private float TimeInterval = 0.2f;
        CounterDownTimer timer = null;
        private int curIndex=-1, lastIndex=-1;

        #endregion
        private void SwitchBeast_started(InputAction.CallbackContext obj)
        {
            Debug.Log("SwitchBeast_started");
            if(timer == null)
            {
                timer = new CounterDownTimer(TimeInterval, true, true, 1, 2);
                timer.OnEndEvent += () =>
                {
                    Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();
                    if (Workers.Count == 0) return;


                    if (obj.ReadValue<float>() > 0)//上
                    {
                        CurrentBeastIndex = (CurrentBeastIndex + Workers.Count - 1) % Workers.Count;
                    }
                    else//下
                    {
                        CurrentBeastIndex = (CurrentBeastIndex + 1) % Workers.Count;
                    }
                    lastIndex = curIndex;
                    curIndex = CurrentBeastIndex;

                    this.Refresh();

                };
            }
            
        }

        private void SwitchBeast_canceled(InputAction.CallbackContext obj)
        {
            Debug.Log("SwitchBeast_canceled");
            GameManager.Instance.CounterDownTimerManager.RemoveTimer(timer);
            timer = null;
        }


        private void Expel_performed(InputAction.CallbackContext obj)
        {

            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();

            if (Workers.Count == 0) return;
            LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[CurrentBeastIndex]);


            CurrentBeastIndex = (CurrentBeastIndex + Workers.Count - 1) % Workers.Count;


            this.Refresh();


        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        #endregion

        #region UI
        #region temp
        private Dictionary<string, UIKeyTipComponent> uiKeyTipDic = new Dictionary<string, UIKeyTipComponent>();
        private bool UikeyTipIsInit;
        private InputManager inputManager => GameManager.Instance.InputManager;
        private Sprite icon_genderfemaleSprite, icon_gendermaleSprite;

        private void ClearTemp()
        {
            uiKeyTipDic = null;
        }

        #endregion

        #region UI对象引用
        private UIKeyTipComponent[] UIKeyTipComponents;

        private int CurrentBeastIndex = 0;

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
            if (ABJAProcessorJson_BeastPanel == null || !ABJAProcessorJson_BeastPanel.IsLoaded || !IsInit)
            {
                return;
            }

            if (UikeyTipIsInit == false)
            {
                KeyTip[] keyTips = inputManager.ExportKeyTipValues(PanelTextContent_BeastPanel);
                foreach (var keyTip in keyTips)
                {
                    InputAction inputAction = inputManager.GetInputAction((keyTip.keymap.ActionMapName, keyTip.keymap.ActionName));
                    inputManager.GetInputActionBindText(inputAction);

                    UIKeyTipComponent uIKeyTipComponent = uiKeyTipDic[keyTip.keyname];
                    if (uIKeyTipComponent.keytip != null)
                    {
                        uIKeyTipComponent.keytip.text = inputManager.GetInputActionBindText(inputAction);
                    }
                    if (uIKeyTipComponent.description != null)
                    {
                        uIKeyTipComponent.description.text = keyTip.description.GetText();
                    }

                }
                UikeyTipIsInit = true;
            }


            var Content = this.transform.Find("HiddenBeastInfo1").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");

            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();
            
            if (Workers.Count == 0) CurrentBeastIndex = -1;

            for (int i = 0; i < Content.childCount; i++)
            {
                ML.Engine.Manager.GameManager.DestroyObj(Content.GetChild(i).gameObject);
            }

            Debug.Log("Workers " + Workers.Count);

            for (int i = 0; i < Workers.Count; i++)
            {

                var tPrefab = GameObject.Instantiate(this.beastBioPrefab, Content);
                //worker.TimeArrangement[10] = TimeStatus.Relax;
                if (i == CurrentBeastIndex)
                {
                    tPrefab.transform.Find("Bio").Find("Selected").gameObject.SetActive(true);
                }
                else
                {
                    tPrefab.transform.Find("Bio").Find("Selected").gameObject.SetActive(false);
                }

                tPrefab.transform.Find("Bio").Find("mask").GetComponent<Image>().fillAmount = (float)Workers[i].APCurrent / Workers[i].APMax;
            }

            if(CurrentBeastIndex != -1)
            {
                this.transform.Find("HiddenBeastInfo2").Find("Info").gameObject.SetActive(true);
                this.transform.Find("HiddenBeastInfo3").Find("Info").gameObject.SetActive(true);

                //BeastInfo

                Speed.text = PanelTextContent_BeastPanel.Speed;
                Cook.text = PanelTextContent_BeastPanel.Cook;
                HandCraft.text = PanelTextContent_BeastPanel.HandCraft;
                Industry.text = PanelTextContent_BeastPanel.Industry;
                Magic.text = PanelTextContent_BeastPanel.Magic;
                Transport.text = PanelTextContent_BeastPanel.Transport;
                Collect.text = PanelTextContent_BeastPanel.Collect;

                Worker worker = Workers[CurrentBeastIndex];
                List<float> datas = new List<float>();


                Dictionary<WorkType,Skill> skillDic = worker.Skill;
                foreach (var skill in skillDic)
                {
                    datas.Add(skillDic[skill.Key].Level / 10f);
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




                SpeedNumText.text = worker.WalkSpeed.ToString();

                var Info = this.transform.Find("HiddenBeastInfo3").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");
                for (int i = 0; i < Info.childCount; i++)
                {
                    ML.Engine.Manager.GameManager.DestroyObj(Info.GetChild(i).gameObject);
                }

                foreach (var feature in worker.Features)
                {


                    var tPrefab = GameObject.Instantiate(this.descriptionPrefab, Info);
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

                #region 更新滑动窗口

                if (lastIndex != -1 && curIndex != -1)
                {
                    var cur = Content.GetChild(curIndex);
                    var last = Content.GetChild(lastIndex);

                    if (cur != null && last != null)
                    {
                        // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                        RectTransform uiRectTransform = cur.GetComponent<RectTransform>();
                        RectTransform scrollRectTransform = cur.transform.parent.parent.parent.GetComponent<RectTransform>();
                        // 获取 ScrollRect 组件
                        ScrollRect scrollRect = scrollRectTransform.GetComponent<ScrollRect>();
                        // 获取 Content 的 RectTransform 组件
                        RectTransform contentRect = scrollRect.content;

                        // 获取 UI 元素的四个角点
                        Vector3[] corners = new Vector3[4];
                        uiRectTransform.GetWorldCorners(corners);
                        bool allCornersVisible = true;
                        for (int i = 0; i < 4; ++i)
                        {
                            // 将世界空间的点转换为屏幕空间的点
                            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
                            // 判断 ScrollRect 是否包含这个点
                            if (!RectTransformUtility.RectangleContainsScreenPoint(scrollRectTransform, screenPoint, null))
                            {
                                allCornersVisible = false;
                                break;
                            }
                        }

                        // 当前激活的TP四个边点有一个不位于窗口内 -> 更新窗口滑动
                        if (!allCornersVisible)
                        {
                            // 将当前选中的这个放置于上一个激活TP的位置

                            // 设置滑动位置

                            // 获取点 A 和点 B 在 Content 中的位置
                            Vector2 positionA = (last.transform as RectTransform).anchoredPosition;
                            Vector2 positionB = (cur.transform as RectTransform).anchoredPosition;

                            // 计算点 B 相对于点 A 的偏移量
                            Vector2 offset = positionB - positionA;

                            // 根据偏移量更新 ScrollRect 的滑动位置
                            Vector2 normalizedPosition = scrollRect.normalizedPosition;
                            normalizedPosition += new Vector2(offset.x / (contentRect.rect.width - (contentRect.parent as RectTransform).rect.width), offset.y / (contentRect.rect.height - (contentRect.parent as RectTransform).rect.height));
                            scrollRect.normalizedPosition = normalizedPosition;
                        }


                    }
                }

                
                #endregion







            }
            else
            {
                this.transform.Find("HiddenBeastInfo2").Find("Info").gameObject.SetActive(false);
                this.transform.Find("HiddenBeastInfo3").Find("Info").gameObject.SetActive(false);

                //Debug.Log("无隐兽");
            }  
        }
        #endregion



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


            //BotKeyTips
            public KeyTip Back;
        }
        public BeastPanelStruct PanelTextContent_BeastPanel => ABJAProcessorJson_BeastPanel.Datas;
        public ML.Engine.ABResources.ABJsonAssetProcessor<BeastPanelStruct> ABJAProcessorJson_BeastPanel;
        private void InitUITextContents()
        {
            ABJAProcessorJson_BeastPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<BeastPanelStruct>("OC/Json/TextContent/ResonanceWheel", "BeastPanel", (datas) =>
            {
                Refresh();
                this.enabled = false;
            }, "UIBeastPanel数据");
            ABJAProcessorJson_BeastPanel.StartLoadJsonAssetData();
        }

        #endregion

        #region Texture2D
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private string ResonanceWheelSpriteAtlasPath = "OC/UI/ResonanceWheel/Texture/SA_ResonanceWheel_UI.spriteatlasv2";
        private void InitUITexture2D()
        {
            GM.ABResourceManager.LoadAssetAsync<SpriteAtlas>(ResonanceWheelSpriteAtlasPath).Completed += (handle) =>
                {
                    this.spriteAtlasHandle = handle;
                    SpriteAtlas resonanceWheelAtlas = handle.Result as SpriteAtlas;
                    icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite("icon_genderfemale");
                    icon_genderfemaleSprite = resonanceWheelAtlas.GetSprite("icon_gendermale");
                };
        }

        #endregion

        #region Prefab
        private GameObject descriptionPrefab;
        private GameObject beastBioPrefab;
        private void InitPrefabs()
        {
            GM.ABResourceManager.InstantiateAsync("OC/UI/ResonanceWheel/Prefabs/BeastBio.prefab").Completed += (handle) =>
            {
                this.goHandle.Add(handle);
                this.beastBioPrefab = handle.Result;
            };

            GM.ABResourceManager.InstantiateAsync("OC/UI/ResonanceWheel/Prefabs/Description.prefab").Completed += (handle) =>
            {
                this.goHandle.Add(handle);
                this.descriptionPrefab = handle.Result;
            };
        }

        #endregion


    }


}
