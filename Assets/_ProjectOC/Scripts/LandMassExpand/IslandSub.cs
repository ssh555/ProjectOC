using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public class IslandSub : IslandBase
    {
        public IslandMain islandMain;
        private void Awake()
        {
            islandMain = GetComponentInParent<IslandMain>();
        }

        public override void IslandMove(Vector2Int centerPos)
        {
            base.IslandMove(centerPos + islandMapPos);
        }
    }
}