using ML.Engine.Manager;
using ML.Engine.TextContent;
using System;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using ProjectOC.PinchFace;
using TMPro;
using UnityEngine;
using static ML.Engine.UI.PopUpUI;

namespace ML.Engine.UI
{
    public class UIFaceTemplatePanel : ML.Engine.UI.UIBasePanel<InputFieldUI.InputFieldStruct> ,INoticeUI
    {
        #region Unity

        private PinchFaceManager pinchFaceManager;
        private RacePinchData racePinchData;
        private bool isLoad;
        private List<PinchPart.PinchPartData> pinchPartDatas;
        private string templateName;
        private Transform btnTransfParent;

        protected override void Awake()
        {
            btnTransfParent = transform.Find("TextGroup");
            UIBtnList = new UIBtnList(transform.GetComponentInChildren<UIBtnListInitor>());
            pinchFaceManager = LocalGameManager.Instance.PinchFaceManager;
            base.Awake();
        }

        protected override void Start()
        {
            Refresh();
            base.Start();
            this.enabled = false;
        }
        /// <summary>
        /// TrueΪ����ģ�壬FalseΪ����ģ��
        /// </summary>
        /// <param name="_racePinchData"></param>
        /// <param name="isLoad"></param>
        public void Init(RacePinchData _racePinchData,bool _isLoad,List<PinchPart.PinchPartData> _pinchPartDatas = null,string _templateName = "")
        {
            racePinchData = _racePinchData;
            RefreshText();
            isLoad = _isLoad;
            pinchPartDatas = _pinchPartDatas;
            templateName = _templateName;
        }

        private void RefreshText()
        {
            var texts = btnTransfParent.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var _text in texts)
            {
                _text.text = "��";
            }
            for(int i = 0;i<racePinchData.PinchFaceTemplates.Length;i++)
            {
                if(racePinchData.PinchFaceTemplateNotNullIn(i))
                    texts[i].text = racePinchData.PinchFaceTemplates[i].faceTemplateName;
            }
        }
        #endregion

        #region Override
        protected override void Enter()
        {
            this.UIBtnList.EnableBtnList();
            Invoke("RegisterInput", 0.1f);
        }

        protected override void Exit()
        {
            this.UIBtnList.DisableBtnList();
            base.Exit();
        }


        #endregion

        #region Internal

        protected override void RegisterInput()
        {
            base.RegisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Enable();
            UIBtnList.InitBtnInfo();
            UIBtnList.BindNavigationInputAction(ML.Engine.Input.InputManager.Instance.Common.Common.SwichBtn, UIBtnListContainer.BindType.started);
            Input.InputManager.Instance.Common.Common.Confirm.performed += Confirm_performed;
            Input.InputManager.Instance.Common.Common.SubInteract.performed += Delete_performed;
            Input.InputManager.Instance.Common.Common.Back.performed += Back_performed;
        }

        protected override void UnregisterInput()
        {
            base.UnregisterInput();
            ProjectOC.Input.InputManager.PlayerInput.PlayerUI.Disable();
            UIBtnList.DisableBtnList();
            UIBtnList.DeBindInputAction();
            Input.InputManager.Instance.Common.Common.Confirm.performed -= Confirm_performed;
            Input.InputManager.Instance.Common.Common.SubInteract.performed -= Delete_performed;
            Input.InputManager.Instance.Common.Common.Back.performed -= Back_performed;
        }

        private void Confirm_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int index = UIBtnList.GetCurSelectedPos1();
            if (index != -1)
            {
                if (isLoad && racePinchData.PinchFaceTemplateNotNullIn(index))
                {
                    //�˳� FaceTemplate,Race��� ,����ģ������������
                    ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                    ML.Engine.Manager.GameManager.Instance.UIManager.PopPanel();
                    pinchFaceManager.GeneratePinchFaceUI(racePinchData,racePinchData.PinchFaceTemplates[index].PinchPartDatas);
                }
                else if(!isLoad)
                {
                    var _pinchFaceTemplateData = new RacePinchData.PinchFaceTemplateData(templateName, pinchPartDatas);
                    if (racePinchData.PinchFaceTemplateNotNullIn(index))
                    {
                        //�滻 ����   
                        GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, 
                            new UIManager.PopUpUIData($"ȷ�ϸ��Ǹ���λ�Ľ�ɫ������",$"�⽫ɾ����ɫ���� {racePinchData.PinchFaceTemplates[index].faceTemplateName} ��", null, 
                                () =>
                                {
                                    racePinchData.PinchFaceTemplates[index] = _pinchFaceTemplateData;
                                    RefreshText();
                                },null));
                    }
                    else
                    {
                        racePinchData.PinchFaceTemplates[index] = _pinchFaceTemplateData;
                        RefreshText();
                    }
                }
                
            }
            
        }
        private void Delete_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int index = UIBtnList.GetCurSelectedPos1();
            if (index != -1 && racePinchData.PinchFaceTemplateNotNullIn(index))
            {
                var templateData = racePinchData.PinchFaceTemplates[index];
                GameManager.Instance.UIManager.PushNoticeUIInstance(UIManager.NoticeUIType.PopUpUI, 
                    new UIManager.PopUpUIData($"ȷ��ɾ����ɫ���� {templateData.faceTemplateName} ��?","", null, 
                        () =>
                        {
                            racePinchData.PinchFaceTemplates[index] = null;
                            RefreshText();
                        },null));
            }
        }
        private void Back_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            GameManager.Instance.UIManager.PopPanel();
        }
        #endregion
        

        #region TextContent
        private UIBtnList UIBtnList;
        [System.Serializable]
        public struct InputFieldStruct
        {
            //BotKeyTips
            public KeyTip Confirm;
            public KeyTip Back;
        }

        // protected override void OnLoadJsonAssetComplete(InputFieldStruct datas)
        // {
        //     InitBtnData(datas);
        // }
        // private void InitBtnData(InputFieldStruct datas)
        // {
        //     
        // }
        protected override void InitTextContentPathData()
        {
            this.abpath = "TextContent";
            this.abname = "FaceTemplateUI";
            this.description = "FaceTemplateUI���ݼ������";
        }

        #endregion
    }

}
