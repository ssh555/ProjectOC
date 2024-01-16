using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace ProjectOC.LandMassExpand
{
  public class IslandMain : IslandBase
  {
    //��������
    public List<IslandSub> affiliatedIslands;

    public override void GenerateColliderBox(Vector2Int centerPos)
    {
        base.GenerateColliderBox(centerPos);
        //ͬʱ�޸��ӵ�����ײ��λ��
        foreach (var affiliatedIsland in affiliatedIslands)
        {
          affiliatedIsland.GenerateColliderBox(centerPos);
        }
    }
  }
}