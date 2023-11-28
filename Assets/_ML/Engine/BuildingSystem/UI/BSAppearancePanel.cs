using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.BuildingSystem.UI
{
    public class BSAppearancePanel : Engine.UI.UIBasePanel
    {
        public override void OnExit()
        {
            Destroy(this.gameObject);
        }
    }

}
