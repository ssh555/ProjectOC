using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
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
        [ShowInInspector]
        public IslandBase[,] islandGrids;

        public void OnRegister()
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


            Vector3Int center = new Vector3Int(maxSize.x/2,0,maxSize.y/2);
            //岛屿Gizmos
            for (int i = 0; i < maxSize.x; i++)
            {
                for (int j = 0; j < maxSize.y; j++)
                {
                    IslandBase _grid = islandGrids[i, j];
                    if (_grid != null)
                    {
                        Gizmos.color = _grid.gizmosColor;
                        Gizmos.DrawWireCube((new Vector3(i,0,j) - center)*mapGridSize,mapGridSize*Vector3.one);
                    }
                }
            }
            
        }
    }
}