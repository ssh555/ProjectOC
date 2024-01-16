using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;
using UnityEngine.Serialization;


namespace ProjectOC.LandMassExpand
{
    public class IslandManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        public static IslandManager instance = null;

        public static IslandManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new IslandManager();
                    ML.Engine.Manager.GameManager.Instance.RegisterLocalManager(instance);
                }

                return instance;
            }
        }
        
        void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }    
        }
        
        public int mapGridSize = 100;
        public Vector2Int maxSize;
        [SerializeField]private List<IslandMain> islandMains;
        private IslandBase[,] islandGrids;
        bool UnlockIsland(int island_Index)
        {
            return true;
        }

        //初始地图生产
        public void IslandRandomGeneration()
        {
            //to-do
        }

        private void OnValidate()
        {
            islandGrids = new IslandBase[maxSize.x,maxSize.y];
        }
    }
}