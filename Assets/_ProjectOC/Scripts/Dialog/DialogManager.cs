using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ProjectOC.NPC;
using Sirenix.Utilities;
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

        
        
        private List<DialogTableData> _dialogTableDatas;
        private DialogTableData[] DialogDatas;
        private string nextDialogID;
        
        private OptionTableData[] OptionDatas;
        private UIDialogPanel DialogPanel;
       

        public NPCCharacter CurrentChatNpc;


        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<DialogTableData[]> ABJAProcessor = new ML.Engine.ABResources.
                ABJsonAssetProcessor<DialogTableData[]>(abPath, dialogPath,
                    (datas) =>
                    {
                        DialogDatas = new DialogTableData[datas.Length];
                        datas.CopyTo(DialogDatas,0);
                        //异步加载表，顺序不对
                        Array.Sort(DialogDatas, (a, b) 
                            => DialogIDToIndex(a.ID).CompareTo (DialogIDToIndex(b.ID)));
                    }, "对话项");
            
            ABJAProcessor.StartLoadJsonAssetData();
            
            ML.Engine.ABResources.ABJsonAssetProcessor<OptionTableData[]> ABJAProcessorDialog = new ML.Engine.ABResources.
                ABJsonAssetProcessor<OptionTableData[]>(abPath, optionPath,
                    (datas) =>
                    {
                        OptionDatas = new OptionTableData[datas.Length];
                        datas.CopyTo(OptionDatas,0);
                        
                        Array.Sort(OptionDatas, (a, b) 
                            => DialogIDToIndex(a.ID).CompareTo (DialogIDToIndex(b.ID)));
                    }, "对话项");
            
            ABJAProcessorDialog.StartLoadJsonAssetData();

        }
        //Option_BeginnerTutorial_0 -> 0
        private int DialogIDToIndex(string str)
        {
            return int.Parse(str.Split("_")[2]);
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
                    DialogPanel.transform.SetParent(ML.Engine.Manager.GameManager.Instance.UIManager.GetCanvas.transform, false);
                    ML.Engine.Manager.GameManager.Instance.UIManager.PushPanel(DialogPanel);
                    DialogPanel.FirstDialogueID = _ID;
                };
            }
        }
        //todo:用CharacterManager的CharacterID，应对多人聊天场景
        /// <summary>
        /// 加载一条
        /// </summary>
        /// <param name="_ID">对话ID，如果为空则触发下一条</param>
        public void LoadDialogue(string _ID)
        {
            string[] dialogDatas = _ID.Split('_');
            int _dialogIndex = int.Parse(dialogDatas[2]);
            DialogTableData _currentDialog = DialogDatas[_dialogIndex];
            nextDialogID = _currentDialog.NextID;
            
            DialogPanel.ShowDialogText(_currentDialog.Content.GetText(),_currentDialog.Name.GetText());
            //播放Action、Mood、Audio todo
            CurrentChatNpc.PlayAction(_currentDialog.ActionID);
            CurrentChatNpc.PlayMood(_currentDialog.MoodID);
                    
            Debug.Log($"Content:{_currentDialog.Content.GetText()}  Name:{_currentDialog.Name.GetText()}");
            Debug.Log($"ID:{_ID}  Option:{_currentDialog.OptionID}");
            if (_currentDialog.OptionID != "")
            {
                ShowOption(_currentDialog.OptionID);
            }
        }
        public void LoadDialogue()
        {
            if (nextDialogID == "")
            {
                EndDialogMode();
                return;
            }
            LoadDialogue(nextDialogID);
        }
        
        
        
        private void ShowOption(string _OptionID)
        {
            string[] _opetionDatas = _OptionID.Split('_');
            int _dialogIndex = int.Parse(_opetionDatas[2]);
            OptionTableData _currentDialog = OptionDatas[_dialogIndex];
            
            DialogPanel.ShowOption(_currentDialog);
        }
        
        private void EndDialogMode()
        {
            DialogPanel.PopPanel();
            //CurrentChatNpc.EndDialogMode();
            GameObject.Destroy(CurrentChatNpc.gameObject);
            
            DialogPanel = null;
            CurrentChatNpc = null;
            nextDialogID = "";
        }

        #endregion


    }   
}
