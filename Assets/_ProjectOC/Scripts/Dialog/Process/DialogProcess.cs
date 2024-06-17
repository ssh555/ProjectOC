using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.Dialog
{
    public interface DialogProcess
    {
        public void LoadDialogue(DialogTableData _singleData);


        public void EndDialogMode();
    }
}