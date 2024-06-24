using ProjectOC.ManagerNS;
using ProjectOC.ResonanceWheelSystem.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Dialog
{
    public class DialogPhoneProcess : DialogProcess
    {
        private IDialogPanel uiCommunicationPanel;
        private DialogManager DialogManager;
        public DialogPhoneProcess(string id, IDialogPanel uICommunicationPanel)
        {
            this.uiCommunicationPanel = uICommunicationPanel;
            DialogManager = LocalGameManager.Instance.DialogManager;
            DialogManager.LoadDialogue(id);
        }
        public void LoadDialogue(DialogTableData _singleData)
        {
            
        }

        public void EndDialogMode()
        {
            
        }
    }
}

