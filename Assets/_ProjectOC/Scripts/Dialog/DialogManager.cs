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

        private string abPath = "OCTableData";
        private string dialogPath = "Dialog";
        private string optionPath = "Option";

        
        private DialogTableData[] DialogDatas;
        private OptionTableData[] OptionDatas;
        private int CurDialogIndex = -1;
        private DialogTableData CurDialog => DialogDatas[CurDialogIndex];
        

        public NPCCharacter CurrentChatNpcModel;

        
        public PlayerCharacter playerCharacter=> (GameManager.Instance.CharacterManager.GetLocalController() as OCPlayerController)
            .currentCharacter;

        private void LoadTableData()
        {
            ML.Engine.ABResources.ABJsonAssetProcessor<DialogTableData[]> ABJAProcessor = new ML.Engine.ABResources.
                ABJsonAssetProcessor<DialogTableData[]>(abPath, dialogPath,
                    (datas) =>
                    {
                        DialogDatas = new DialogTableData[datas.Length];
                        datas.CopyTo(DialogDatas,0);
                        //�첽���ر�˳�򲻶���Ҫ��������
                        Array.Sort(DialogDatas, (a, b) 
                            => DialogIDToIndex(a.ID).CompareTo (DialogIDToIndex(b.ID)));
                    }, "�Ի���");
            
            ABJAProcessor.StartLoadJsonAssetData();
            
            ML.Engine.ABResources.ABJsonAssetProcessor<OptionTableData[]> ABJAProcessorDialog = new ML.Engine.ABResources.
                ABJsonAssetProcessor<OptionTableData[]>(abPath, optionPath,
                    (datas) =>
                    {
                        OptionDatas = new OptionTableData[datas.Length];
                        datas.CopyTo(OptionDatas,0);
                        
                        Array.Sort(OptionDatas, (a, b) 
                            => DialogIDToIndex(a.ID).CompareTo (DialogIDToIndex(b.ID)));
                    }, "�Ի�ѡ��");
            
            ABJAProcessorDialog.StartLoadJsonAssetData();

        }
        //Option_BeginnerTutorial_0 -> 0
        private int DialogIDToIndex(string str)
        {
            return int.Parse(str.Split("_")[2]);
        }
        #endregion

        
        #region ProcessDialog

        public enum DialogType
        {
            Chat,
            Phone
        }

        public DialogType CurDialogType;
        public DialogProcess CurDialogProcess;
        public void StartDialogMode(DialogType _dialogType,string _ID,IDialogPanel dialogPanel = null)
        {
            switch (_dialogType)
            {
                case DialogType.Chat:
                    CurDialogProcess = new DialogChatProcess(_ID);
                    break;
                case DialogType.Phone:
                    CurDialogProcess = new DialogPhoneProcess(_ID, dialogPanel);
                    break;
            }
        }
        //todo:��CharacterManager��CharacterID��Ӧ�Զ������쳡��
        /// <summary>
        /// ����һ��
        /// </summary>
        /// <param name="_ID">�Ի�ID�����Ϊ���򴥷���һ��</param>
        public void LoadDialogue(string _ID)
        {
            string[] dialogDatas = _ID.Split('_');
            int _dialogIndex = int.Parse(dialogDatas[2]);
            CurDialogIndex = _dialogIndex;
            
            CurDialogProcess.LoadDialogue(CurDialog);
        }
        /// <summary>
        /// �����-1��ת����һ�����������������Index ��ת����һ��
        /// </summary>
        /// <param name="_optionIndex"></param>
        public void LoadDialogueOption(int _optionIndex)
        {
            if (_optionIndex == -1)
            {
                LoadNextDialogue();
            }
            else
            {
                LoadDialogue(StringToOption(CurDialog.OptionID).Options[_optionIndex].NextID);    
            }
        }

        public void LoadNextDialogue()
        {
            string nextDialogID = CurDialog.NextID;
            if (nextDialogID == "")
            {
                EndDialogMode();
            }
            else
            {
                LoadDialogue(nextDialogID);   
            }
        }


        public OptionTableData StringToOption(string _OptionID)
        {
            string[] _opetionDatas = _OptionID.Split('_');
            int _dialogIndex = int.Parse(_opetionDatas[2]);
            OptionTableData _option = OptionDatas[_dialogIndex];
            return _option;
        }
        
        private void EndDialogMode()
        {
            CurDialogProcess.EndDialogMode();
            //���ݸ�ԭ
            CurrentChatNpcModel = null;
            CurDialogIndex = -1;
        }
        #endregion
    }   
}
