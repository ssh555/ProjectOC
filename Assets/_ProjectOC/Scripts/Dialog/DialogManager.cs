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
                    }, "对话项");
            
            ABJAProcessor.StartLoadJsonAssetData();
            
            ML.Engine.ABResources.ABJsonAssetProcessor<OptionTableData[]> ABJAProcessorDialog = new ML.Engine.ABResources.
                ABJsonAssetProcessor<OptionTableData[]>(abPath, optionPath,
                    (datas) =>
                    {
                        OptionDatas = new OptionTableData[datas.Length];
                        datas.CopyTo(OptionDatas,0);
                    }, "对话项");
            
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
            //根据ID找对应的Dialog
            LoadDialogue(dialogID);
        }
        
        /// <summary>
        /// 加载一条
        /// </summary>
        /// <param name="_ID">对话ID，如果为空则触发下一条</param>
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
            //播放Action、Mood、Audio todo
            
            
            //跳转下一条,应该处理下空格，防止策划填错
            if (_currentDialog.OptionID != "")
            {
                ShowOption(_currentDialog.OptionID);
            }
            
                
            
            //更新Dialog
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
