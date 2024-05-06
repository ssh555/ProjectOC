using System;
using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.LandMassExpand
{
  public class IslandMain : IslandBase
  {
    private void Start()
    {
      IslandModelManager islandModelManager = LocalGameManager.Instance.IslandManager;
      islandModelManager.islandMain = this;
      islandModelManager.SetCurIsland(this);
      
      this.enabled = false;
    }

    public override void IslandMove()
    {
        
    }
  }
}