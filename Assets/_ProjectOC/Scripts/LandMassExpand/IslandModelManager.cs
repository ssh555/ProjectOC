using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ProjectOC.LandMassExpand
{
    [System.Serializable]
    public class IslandModelManager :  ML.Engine.Manager.LocalManager.ILocalManager
    {
        public IslandBase currentIsland;
        

        public int mapGridSize;
        public Vector2Int maxSize;
        private List<IslandMain> islandMains;
        public IslandBase[,] islandGrids;
        
        public void Init()
        {
            mapGridSize = 100;
            maxSize = new Vector2Int(15, 15);
            islandGrids = new IslandBase[maxSize.x,maxSize.y];
            IslandBase mainIsland = GameObject.Find("IslandMainPrefab").GetComponent<IslandBase>();
            currentIsland = mainIsland;
            islandMains = new List<IslandMain>();
            islandMains.Add(mainIsland as IslandMain);
        }
        
        bool UnlockIsland(int island_Index)
        {
            return true;
        }

        
        //初始地图生产
        public void IslandRandomGeneration()
        {
            //to-do
        }

        public void AllIslandMove()
        {
            foreach (var islandMain in islandMains)
            {
                islandMain.IslandMove();
            }
        }
        
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero,new Vector3(maxSize.x,0,maxSize.y)*mapGridSize);
        }
    }
}