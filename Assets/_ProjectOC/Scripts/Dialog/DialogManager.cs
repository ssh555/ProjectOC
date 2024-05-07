using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ML.Engine.Manager;
using ProjectOC.NPC;
using ProjectOC.Player;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

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

        
        private DialogTableData[] DialogDatas;
        private OptionTableData[] OptionDatas;
        private UIDialogPanel DialogPanel;
        private int CurDialogIndex = -1;
        private DialogTableData CurDialog => DialogDatas[CurDialogIndex];
        

        public NPCCharacter CurrentChatNpcModel;


        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<DialogTableData[]> ABJAProcessor = new ML.Engine.ABResources.
                ABJsonAssetProcessor<DialogTableData[]>(abPath, dialogPath,
                    (datas) =>
                    {
                        DialogDatas = new DialogTableData[datas.Length];
                        datas.CopyTo(DialogDatas,0);
                        //异步加载表，顺序不对需要重新排序
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
                    }, "对话选项");
            
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
            CurDialogIndex = _dialogIndex;
            
            DialogPanel.ShowDialogText(CurDialog.Content.GetText(),CurDialog.Name.GetText());
            //播放Action、Mood、Audio todo
            CurrentChatNpcModel.PlayAction(CurDialog.ActionID);
            CurrentChatNpcModel.PlayMood(CurDialog.MoodID);
            
            if (CurDialog.OptionID != "")
            {
                DialogPanel.ShowOption(StringToOption(CurDialog.OptionID));
            }
        }

        public void LoadDialogue(int _optionIndex)
        {
            string nextDialogID = CurDialog.NextID;
            
            if (_optionIndex == -1)
            {
                if (nextDialogID == "")
                {
                    EndDialogMode();
                    return;
                }
                else
                {
                    LoadDialogue(nextDialogID);   
                }
            }
            else
            {
                LoadDialogue(StringToOption(CurDialog.OptionID).Options[_optionIndex].NextID);
            }
        }
        


        private OptionTableData StringToOption(string _OptionID)
        {
            string[] _opetionDatas = _OptionID.Split('_');
            int _dialogIndex = int.Parse(_opetionDatas[2]);
            OptionTableData _option = OptionDatas[_dialogIndex];
            return _option;
        }
        
        

        
        private void EndDialogMode()
        {
            DialogPanel.PopPanel();
            //CurrentChatNpc.EndDialogMode();
            GameObject.Destroy(CurrentChatNpcModel.gameObject);
            
            DialogPanel = null;
            CurrentChatNpcModel = null;
            CurDialogIndex = -1;
        }

        #endregion


    }   
}
