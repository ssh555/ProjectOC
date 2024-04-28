using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public class IslandSub : IslandBase
    {
        //public IslandMain islandMain;

        private void Start()
        {
            LocalGameManager.Instance.IslandManager.islandSubs.Add(this);
            this.enabled = false;
        }
    }
}