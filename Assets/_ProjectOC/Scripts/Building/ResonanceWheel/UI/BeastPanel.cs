using ML.Engine.BuildingSystem.BuildingPart;
using ML.Engine.InventorySystem;
using ML.Engine.Manager;
using ML.Engine.TextContent;
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
using Unity.Burst.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Purchasing;
using UnityEngine.Rendering;
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
            StartCoroutine(InitUIPrefabs());
            StartCoroutine(InitUITexture2D());

            //BeastInfo
            var Info1 = this.transform.Find("HiddenBeastInfo2").Find("Info");
            Speed = Info1.Find("MovingSpeed").Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
            SpeedNumText = Info1.Find("MovingSpeed").Find("NumText").GetComponent<TMPro.TextMeshProUGUI>();
            GenderImage = Info1.Find("Icon").Find("GenderImage").GetComponent<Image>();


            var GInfo = Info1.Find("SkillGraph").Find("Viewport").Find("Content").Find("Ring");
            Cook = GInfo.Find("Skill1").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            HandCraft = GInfo.Find("Skill6").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Industry = GInfo.Find("Skill5").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Magic = GInfo.Find("Skill4").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Transport = GInfo.Find("Skill3").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();
            Collect = GInfo.Find("Skill2").Find("EmptyText").GetComponent<TMPro.TextMeshProUGUI>();


            var btn1 = this.transform.Find("HiddenBeastInfo3").Find("btn1");
            expel = new UIKeyTip();
            expel.keytip = btn1.Find("KeyTip").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            expel.description = btn1.Find("KeyTip").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();

            

            //��Ҫ���ӿ���ʾ��������Ϣ

            BeastName = Info1.Find("Icon").Find("Name").GetComponent<TMPro.TextMeshProUGUI>();


            //BotKeyTips
            var kt = this.transform.Find("BotKeyTips").Find("KeyTips");
            KT_Back = new UIKeyTip();

            KT_Back.keytip = kt.Find("KT_Back").Find("Image").Find("KeyText").GetComponent<TMPro.TextMeshProUGUI>();
            KT_Back.description = kt.Find("KT_Back").Find("Image").Find("KeyTipText").GetComponent<TMPro.TextMeshProUGUI>();





        IsInit = true;

          

            Refresh();
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
            this.Refresh();
        }

        private void Exit()
        {

            this.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Disable();
        }

        private void UnregisterInput()
        {



            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.SwitchBeast.performed -= SwitchBeast_performed;


            //����
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed -= Expel_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;

            

        }



        private void RegisterInput()
        {

            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.SwitchBeast.performed += SwitchBeast_performed;

            //����
            ProjectOC.Input.InputManager.PlayerInput.BeastPanel.Expel.performed += Expel_performed;

            // ����
            ML.Engine.Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;

        }


        private void SwitchBeast_performed(InputAction.CallbackContext obj)
        {
            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();
            if (Workers.Count == 0) return;
            if (obj.ReadValue<float>() > 0)//��
            {
                CurrentBeastIndex = (CurrentBeastIndex + Workers.Count - 1) % Workers.Count;
            }
            else//��
            {
                CurrentBeastIndex = (CurrentBeastIndex + 1) % Workers.Count;
            }
            this.Refresh();

        }


        private void Expel_performed(InputAction.CallbackContext obj)
        {

            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();

            if (Workers.Count == 0) return;
            LocalGameManager.Instance.WorkerManager.DeleteWorker(Workers[CurrentBeastIndex]);

            GameObject.Destroy(Workers[CurrentBeastIndex].gameObject);
            this.Refresh();


        }

        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            UIMgr.PopPanel();
        }
        #endregion

        #region UI


        #region temp
        private void ClearTemp()
        {

        }

        #endregion

        #region UI��������

        private GameObject BeastBioPrefab;//Ԥ����
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


        private GameObject DescriptionPrefab;//Ԥ����


        private UIKeyTip expel;

        //��Ҫ���ӿ���ʾ��������Ϣ
        private TMPro.TextMeshProUGUI BeastName;

        //BotKeyTips

        private UIKeyTip KT_Back;

        #endregion

        public override void Refresh()
        {
            if (ABJAProcessorJson_BeastPanel == null || !ABJAProcessorJson_BeastPanel.IsLoaded || !IsInit)
            {
                Debug.Log("ABJAProcessorJson is null");
                return;
            }




            var Content = this.transform.Find("HiddenBeastInfo1").Find("Info").Find("Scroll View").Find("Viewport").Find("Content");
            

            


            Workers = LocalGameManager.Instance.WorkerManager.GetWorkers();

            if (Workers.Count == 0) CurrentBeastIndex = -1;


            for (int i = 0; i < Content.childCount; i++)
            {
                Destroy(Content.GetChild(i).gameObject);
            }

            for (int i = 0;i < Workers.Count;i++)
            {
                
                var descriptionPrefab = Instantiate(BeastBioPrefab, Content);
                //Workers[i].TimeArrangement[10] = TimeStatus.Relax;
                if (i == CurrentBeastIndex)
                {
                    descriptionPrefab.transform.Find("Bio").Find("Selected").gameObject.SetActive(true);
                }
                else
                {
                    descriptionPrefab.transform.Find("Bio").Find("Selected").gameObject.SetActive(false);
                }
                descriptionPrefab.transform.Find("Bio").Find("mask").GetComponent<Image>().fillAmount = (float)Workers[i].APCurrent / Workers[i].APMax;

            }

            if(CurrentBeastIndex != -1)
            {
                //BeastInfo
                
                Speed.text = PanelTextContent_BeastPanel.Speed;
                Cook.text = PanelTextContent_BeastPanel.Cook;
                HandCraft.text = PanelTextContent_BeastPanel.HandCraft;
                Industry.text = PanelTextContent_BeastPanel.Industry;
                Magic.text = PanelTextContent_BeastPanel.Magic;
                Transport.text = PanelTextContent_BeastPanel.Transport;
                Collect.text = PanelTextContent_BeastPanel.Collect;

                expel.ReWrite(PanelTextContent_BeastPanel.expel);

                List<float> datas = new List<float>
                {
    /*                            // ���
                    worker.Skill[WorkType.Cook].Level / 10f,
                    // �Ṥ
                    worker.Skill[WorkType.HandCraft].Level / 10f,
                    // ����
                    worker.Skill[WorkType.Industry].Level / 10f,
                    // ����
                    worker.Skill[WorkType.Magic].Level / 10f,
                    // ����
                    worker.Skill[WorkType.Transport].Level / 10f,
                    // �ɼ�
                    worker.Skill[WorkType.Collect].Level / 10f*/
                    0.2f,0.3f,0.5f,0.7f,0.8f,0.1f
                };

                var radar = this.transform.Find("HiddenBeastInfo2").Find("Info").Find("SkillGraph").Find("Viewport").Find("Content").Find("Radar").GetComponent<UIPolygon>();
                radar.DrawPolygon(datas);

                Worker worker = Workers[CurrentBeastIndex];

                //�Ա�
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


                if (this.PrefabsAB == null) return;
                var Info = this.transform.Find("HiddenBeastInfo3").Find("Info");
                for (int i = 0; i < Info.childCount; i++)
                {
                    Destroy(Info.GetChild(i).gameObject);
                }

                foreach (var feature in worker.Features)
                {
                    
                    var descriptionPrefab = Instantiate(DescriptionPrefab, Info);
                    descriptionPrefab.transform.Find("Text1").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Name;
                    descriptionPrefab.transform.Find("Text2").GetComponent<TMPro.TextMeshProUGUI>().text = feature.Description;
                    descriptionPrefab.transform.Find("Text3").GetComponent<TMPro.TextMeshProUGUI>().text = feature.EffectsDescription;
                }


                //�����ճ̱�

                var schedule = this.transform.Find("BeastDuty").Find("Part1").Find("Info").Find("Schedule").Find("Time");
                TimeStatus[] workerTimeStatus = worker.TimeArrangement.Status;
                
                for (int i = 0; i < workerTimeStatus.Length; i++)
                {
                    
                    Image img = schedule.Find("T" + i.ToString()).GetComponent<Image>();
                    switch (workerTimeStatus[i])
                    {
                        case TimeStatus.None:
                            //Debug.Log("workerTimeStatus None");
                            img.color = UnityEngine.Color.red;
                            break;
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
                //Debug.Log("������");
            }
            //BotKeyTips

            KT_Back.ReWrite(PanelTextContent_BeastPanel.back);


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

            public KeyTip expel;


            //BotKeyTips
            public KeyTip back;
        }
        public static BeastPanelStruct PanelTextContent_BeastPanel => ABJAProcessorJson_BeastPanel.Datas;
        public static ML.Engine.ABResources.ABJsonAssetProcessor<BeastPanelStruct> ABJAProcessorJson_BeastPanel;
        private void InitUITextContents()
        {
            if (ABJAProcessorJson_BeastPanel == null)
            {
                ABJAProcessorJson_BeastPanel = new ML.Engine.ABResources.ABJsonAssetProcessor<BeastPanelStruct>("Json/TextContent/ResonanceWheel", "BeastPanel", (datas) =>
                {
                    Refresh();
                    this.enabled = false;
                }, null, "UIBeastPanel����");
                ABJAProcessorJson_BeastPanel.StartLoadJsonAssetData();
            }
        }

        #endregion



        #region Prefab
        public AssetBundle PrefabsAB;

        public IEnumerator InitUIPrefabs()
        {
            this.PrefabsAB = null;
            var abmgr = GameManager.Instance.ABResourceManager;
            var crequest = abmgr.LoadLocalABAsync("ui/resonancewheel/resonancewheelprefabs", null, out var PrefabsAB);

            if (crequest != null)
            {
                yield return crequest;
                Debug.Log("InitUIPrefabs ");
                PrefabsAB = crequest.assetBundle;
            }

            this.PrefabsAB = PrefabsAB;
            BeastBioPrefab = this.PrefabsAB.LoadAsset<GameObject>("BeastBio");
            DescriptionPrefab = this.PrefabsAB.LoadAsset<GameObject>("Description");
            this.Refresh();
        }
        #endregion


        #region temp
        public Sprite icon_genderfemaleSprite, icon_gendermaleSprite;
        #endregion


        #region Texture2D
        public static AssetBundle Texture2DAB;
        private ML.Engine.Manager.GameManager GM => ML.Engine.Manager.GameManager.Instance;
        private string ResonanceWheelTexture2DPath = "ui/resonancewheel/texture2d";
        private IEnumerator InitUITexture2D()
        {

            var crequest = GM.ABResourceManager.LoadLocalABAsync(ResonanceWheelTexture2DPath, null, out var Texture2DAB);

            if (crequest != null)
            {
                yield return crequest;
                Debug.Log("InitUITexture2D ");
                Texture2DAB = crequest.assetBundle;
            }
            Texture2D texture2D;

            texture2D = Texture2DAB.LoadAsset<Texture2D>("icon_genderfemale");
            icon_genderfemaleSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
            texture2D = Texture2DAB.LoadAsset<Texture2D>("icon_gendermale");
            icon_gendermaleSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));



        }

        #endregion

    }


}
