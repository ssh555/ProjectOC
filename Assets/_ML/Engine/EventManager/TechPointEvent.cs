using ML.Engine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.Event
{
    public sealed partial class EventManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        #region ¿Æ¼¼Ê÷Event
        public void ProNodeUpgrade(string p1,int p2)
        {
            Debug.Log($"ProNodeUpgrade {p1} {p2}");
        }

        public void InteractUpgrade(string p1, int p2)
        {
            Debug.Log($"InteractUpgrade {p1} {p2}");
        }

        #endregion
    }
}





