using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using ProjectOC.NPC;
using UnityEngine;

namespace ProjectOC.Dialog
{
    public class DialogChatProcess : DialogProcess
    {
        
        private string dialogPanelPath = "Prefab_Dialog/Prefab_Dialog_DialogPanel.prefab";
        
        private NPCCharacter CurrentChatNpcModel;
        private UIDialogPanel DialogPanel;
        private DialogManager DialogManager;
        public DialogChatProcess(string _ID)
        {
            DialogManager = LocalGameManager.Instance.DialogManager;
            ML.Engine.Manager.GameManager.Instance.ABResourceManager.InstantiateAsync(dialogPanelPath)
                .Completed += (handle) =>
            {
                DialogPanel = handle.Result.GetComponent<UIDialogPanel>();
                DialogPanel.transform.SetParent(GameManager.Instance.UIManager.NormalPanel, false);
                GameManager.Instance.UIManager.PushPanel(DialogPanel);
                DialogManager.LoadDialogue(_ID);

                DialogManager.CurrentChatNpcModel.NpcCMCamera.enabled = true;
                DialogManager.playerCharacter.GetPlayerCamera().enabled = false;
            };
        }
        
        public void LoadDialogue(DialogTableData _singleData)
        {
            DialogPanel.ShowDialogText(_singleData.Content.GetText(),_singleData.Name.GetText());
            //播放Action、Mood、Audio todo
            DialogManager.CurrentChatNpcModel.PlayAction(_singleData.ActionID);
            DialogManager.CurrentChatNpcModel.PlayMood(_singleData.MoodID);
            
            if (_singleData.OptionID != "")
            {
                DialogPanel.ShowOption(DialogManager.StringToOption(_singleData.OptionID));
            }
        }

        
        public void EndDialogMode()
        {
            DialogPanel.PopPanel();
            DialogManager.CurrentChatNpcModel.NpcCMCamera.enabled = false;
            DialogManager.playerCharacter.GetPlayerCamera().enabled = true;
            DialogManager.CurrentChatNpcModel.EndDialogMode();
            
            //数据归位
            DialogPanel = null;
        }
    }
}