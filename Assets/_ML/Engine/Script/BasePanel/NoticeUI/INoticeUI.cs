using ML.Engine.Manager;
using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace ML.Engine.UI
{

    public interface INoticeUI
    {
        public virtual void SaveAsInstance()
        {
            
        }

        public virtual void CopyInstance<D>(D data)
        {

        }
    }

}   

