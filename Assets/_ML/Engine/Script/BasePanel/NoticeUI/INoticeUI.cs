using System.Collections;
using System.Collections.Generic;
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

