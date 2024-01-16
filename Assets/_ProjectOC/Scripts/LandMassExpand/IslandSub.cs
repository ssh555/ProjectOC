using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public class IslandSub : IslandBase
    {
        public override void GenerateColliderBox(Vector2Int centerPos)
        {
            base.GenerateColliderBox(centerPos + islandMapPos);
        }
    }
}