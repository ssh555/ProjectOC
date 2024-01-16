using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.LandMassExpand
{
  public class IslandMain : IslandBase
  {
    //附属岛屿
    public List<IslandSub> affiliatedIslands;

    public override void GenerateColliderBox(Vector2Int centerPos)
    {
        base.GenerateColliderBox(centerPos);
        //同时修改子岛的碰撞、位置
        foreach (var affiliatedIsland in affiliatedIslands)
        {
          affiliatedIsland.GenerateColliderBox(centerPos);
        }
    }
  }
}