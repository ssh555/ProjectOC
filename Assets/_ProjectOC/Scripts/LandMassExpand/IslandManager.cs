using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ML.Engine.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;


namespace ProjectOC.LandMassExpand
{
    public class IslandManager : MonoBehaviour, ML.Engine.Manager.LocalManager.ILocalManager
    {
        private static IslandManager Instance = null;
        [SerializeField] 
        public IslandBase currentIsland;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            
            islandGrids = new IslandBase[maxSize.x,maxSize.y];
        }

        private void Start()
        {
            GameManager.Instance.RegisterLocalManager(this);
            this.enabled = false;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                if(ML.Engine.Manager.GameManager.Instance != null)
                    ML.Engine.Manager.GameManager.Instance.UnregisterLocalManager<BuildPowerIslandManager>();
                Instance = null;
            }    
        }
        [LabelText("单个网格大小")]
        public int mapGridSize;
        [LabelText("大地图网格范围")]
        public Vector2Int maxSize;
        [SerializeField,LabelText("主岛屿")]
        private List<IslandMain> islandMains;
        public IslandBase[,] islandGrids;
        bool UnlockIsland(int island_Index)
        {
            return true;
        }

        
        //初始地图生产
        public void IslandRandomGeneration()
        {
            //to-do
        }
        
        [Button(name: "移动岛屿"),PropertyOrder(-1)]
        public void AllIslandMove()
        {
            foreach (var islandMain in islandMains)
            {
                islandMain.IslandMove();
            }
        }
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Vector3.zero,new Vector3(maxSize.x,0,maxSize.y)*mapGridSize);
            
        }
    }
}