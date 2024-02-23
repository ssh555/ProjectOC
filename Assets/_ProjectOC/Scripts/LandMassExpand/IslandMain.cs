using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.LandMassExpand
{
  public class IslandMain : IslandBase
  {
    [LabelText("��������")]
    public List<IslandSub> affiliatedIslands;

    public override void IslandMove()
    {
        base.IslandMove();
        //ͬʱ�޸��ӵ�����ײ��λ��
        foreach (var affiliatedIsland in affiliatedIslands)
        {
          affiliatedIsland.IslandMove();
        }
    }
  }
}