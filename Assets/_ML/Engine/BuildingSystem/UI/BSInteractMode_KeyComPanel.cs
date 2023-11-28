using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSInteractMode_KeyComPanel : Engine.UI.UIBasePanel
    {
        public override void OnExit()
        {
            Destroy(this.gameObject);
        }
    }

}
