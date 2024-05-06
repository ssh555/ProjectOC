using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Dialog
{
    [System.Serializable]
    public class DialogManager : ML.Engine.Manager.LocalManager.ILocalManager
    {
        #region Register

        public void OnRegister()
        {
            LoadTableData();
        }

        public void OnUnregister()
        {
            
        }

        #endregion


        #region LoadData

        private string dialogPanelPath = "Prefab_Dialog/Prefab_Dialog_DialogPanel.prefab";
        private string abPath = "OCTableData";
        private string dialogPath = "Dialog";
        private string optionPath = "Option";

        private string dialogID = "Dialog_BeginnerTutorial_0";
        
        private List<DialogTableData> _dialogTableDatas;
        private DialogTableData[] DialogDatas;
        private string nextDialogID;
        
        private OptionTableData[] OptionDatas;
        private UIDialogPanel DialogPanel;
        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<DialogTableData[]> ABJAProcessor = new ML.Engine.ABResources.
                ABJsonAssetProcessor<DialogTableData[]>(abPath, dialogPath,
                    (datas) =>
                    {
                        DialogDatas = new DialogTableData[datas.Length];
                        datas.CopyTo(DialogDatas,0);
                    }, "�Ի���");
            
            ABJAProcessor.StartLoadJsonAssetData();
            
            ML.Engine.ABResources.ABJsonAssetProcessor<OptionTableData[]> ABJAProcessorDialog = new ML.Engine.ABResources.
                ABJsonAssetProcessor<OptionTableData[]>(abPath, optionPath,
                    (datas) =>
                    {
                        OptionDatas = new OptionTableData[datas.Length];
                        datas.CopyTo(OptionDatas,0);
                    }, "�Ի���");
            
            ABJAProcessorDialog.StartLoadJsonAssetData();
        }


        #endregion

        #region ProcessDialog
        
        public void StartDialogMode(string _ID)
        {
            if (DialogPanel == null)
            {
                ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(dialogPanelPath)
                    .Completed +=(handle) =>
                {
                    DialogPanel = handle.Result.GetComponent<UIDialogPanel>();
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(DialogPanel);
                };
            }
            //����ID�Ҷ�Ӧ��Dialog
            LoadDialogue(dialogID);
        }
        
        /// <summary>
        /// ����һ��
        /// </summary>
        /// <param name="_ID">�Ի�ID�����Ϊ���򴥷���һ��</param>
        public void LoadDialogue(string _ID = "")
        {
            if (nextDialogID == "")
            {
                EndDialogMode();
                return;
            }
            
            string[] dialogDatas = _ID.Split('_');
            int _dialogIndex = int.Parse(dialogDatas[2]);
            DialogTableData _currentDialog = DialogDatas[_dialogIndex];
            nextDialogID = _currentDialog.NextID;
            
            DialogPanel.ShowDialogText(_currentDialog.Content.GetText());
            //����Action��Mood��Audio todo
            
            
            //��ת��һ��,Ӧ�ô����¿ո񣬷�ֹ�߻����
            if (_currentDialog.OptionID != "")
            {
                ShowOption(_currentDialog.OptionID);
            }
            
                
            
            //����Dialog
        }
        
        
        
        
        private void ShowOption(string _OptionID)
        {
            if (_OptionID == "")
                return;
            
            string[] _opetionDatas = _OptionID.Split('_');
            int _dialogIndex = int.Parse(_opetionDatas[2]);
            OptionTableData _currentDialog = OptionDatas[_dialogIndex];
            

            DialogPanel.ShowOption(_currentDialog);
        }
        
        private void EndDialogMode()
        {
            DialogPanel = null;
            nextDialogID = "";
        }

        #endregion


    }   
}
