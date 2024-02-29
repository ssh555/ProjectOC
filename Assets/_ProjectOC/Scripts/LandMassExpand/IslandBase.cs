using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectOC.LandMassExpand
{
    public class IslandBase : MonoBehaviour
    {
        private int island_Index;
        [SerializeField,LabelText("岛屿大地图位置")] 
        public Vector2Int islandMapPos;
        [SerializeField,LabelText("岛屿大地图网格")]
        protected Vector2Int[] islandMapRanges;
        [SerializeField,LabelText("岛屿模型")]
        private Transform islandMesh;

        private IslandManager islandManager;        
        Vector2Int bigMapSize => islandManager.maxSize;
        [SerializeField,LabelText("岛屿区域")] 
        public List<IslandFieldPart> islandFieldParts;
        private void Start()
        {
            islandManager = GameManager.Instance.GetLocalManager<IslandManager>();
            //ChangeIslandGrids(islandMapRanges, true);
        }

        public virtual void OnUnlock()
        { }
        
        
        
        public virtual void IslandMove()
        {
            if (!JudgeVaild_IslandMapRanges())
            {
                Debug.LogError(this.gameObject.name + " :岛屿大地图网格中含有重复元素");
                return;
            }
            
            Vector2Int[] islandMapRangeData = new Vector2Int[islandMapRanges.Length];
            Array.Copy(islandMapRanges, islandMapRangeData, islandMapRanges.Length);
            int mapSize = islandManager.mapGridSize;
            ChangeIslandGrids(islandMapRangeData,false);
            
            
            #region 检查是否超出边界或有重合
            for(int i = 0;i<islandMapRanges.Length;i++)
            {
                //从[-halfMapSize,halfMapSize]转到[0,bigMapSize-1]
                Vector2Int realBigMapPos = LocalToWorldMapPos(islandMapRanges[i]);
                if (realBigMapPos.x < 0 || realBigMapPos.x >= bigMapSize.x || realBigMapPos.y < 0 || realBigMapPos.y >= bigMapSize.y )
                {
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError(this.gameObject.name + "Map pos beyond bounder in array:  " + i +";" + realBigMapPos);
                    return;
                }
                else if(islandManager.islandGrids[realBigMapPos.x,realBigMapPos.y] != null)
                {
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError("Map pos overlap in array:  " + i +" " + this.gameObject.name + 
                        " GridStore:" + islandManager.islandGrids[realBigMapPos.x,realBigMapPos.y]);
                    return;
                }
            }
            #endregion
            //移动岛屿父物体
            Vector2Int realCenterPos = islandMapPos;
            if(typeof(IslandSub) == this.GetType())
            {
                realCenterPos += (this as IslandSub).islandMain.islandMapPos;
            }
            transform.position= new Vector3(realCenterPos.x,transform.position.y,realCenterPos.y)*mapSize;
            ChangeIslandGrids(islandMapRanges,true);
        }

        [Button(name: "旋转岛屿"),PropertyOrder(-1)]
        public void RotateIsland()
        {
            if (!JudgeVaild_IslandMapRanges())
            {
                Debug.LogError(this.gameObject.name + " :岛屿大地图网格中含有重复元素");
                return;
            }
            
            Vector2Int[] islandMapRangeData = new Vector2Int[islandMapRanges.Length];
            Array.Copy(islandMapRanges, islandMapRangeData, islandMapRanges.Length);
            ChangeIslandGrids(islandMapRangeData,false);
            
            #region 检查是否超出边界或有重合
            for (int i = 0; i < islandMapRanges.Length; i++)
            {
                
                islandMapRanges[i] = new Vector2Int(islandMapRanges[i].y, -islandMapRanges[i].x);
                Vector2Int realBigMapPos = LocalToWorldMapPos(islandMapRanges[i]);
                
                //从[-halfMapSize,halfMapSize]转到[0,bigMapSize-1]
                if (realBigMapPos.x < 0 || realBigMapPos.x >= bigMapSize.x || realBigMapPos.y < 0 ||
                    realBigMapPos.y >= bigMapSize.y)
                {
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError(this.gameObject.name + "Map pos beyond bounder in array:  " + i +";" + realBigMapPos);
                    return;
                }

                if (islandManager.islandGrids[realBigMapPos.x, realBigMapPos.y] != null)
                {
                    //更改本地范围数据，更改世界范围数据
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError("Map pos overlap in grid:  " + realBigMapPos);
                    return;
                }
            }
            #endregion
            //顺时针旋转
            islandMesh.rotation = Quaternion.Euler(islandMesh.eulerAngles + Vector3.up*90);
            ChangeIslandGrids(islandMapRanges,true);
        }

        public bool JudgeVaild_IslandMapRanges()
        {
            HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();

            foreach (Vector2Int vector in islandMapRanges)
            {
                if (!hashSet.Add(vector))
                {
                    return false; // 发现重复元素
                }
            }

            return true; // 没有发现重复元素
        }
        
        
        
        void ChangeIslandGrids(Vector2Int[] islandMapRanges,bool isRange)
        {
            //islandMapRanges.局部坐标
            foreach (var islandMapRange in islandMapRanges)
            {
                IslandBase _islandBase;
                if (isRange)
                    _islandBase = this;
                else
                    _islandBase = null;
                Vector2Int worldIslandMapRange = LocalToWorldMapPos(islandMapRange);
                islandManager.islandGrids[worldIslandMapRange.x, worldIslandMapRange.y] = _islandBase;
            }
        }
        
        //从岛屿本地网格坐标转换到全局网格坐标，即IslandManager.islandGrids
        Vector2Int LocalToWorldMapPos(Vector2Int localMapPos)
        {
            Vector2Int halfMapSize = (islandManager.maxSize - Vector2Int.one) / 2;
            Vector2Int realCenterPos = islandMapPos;
            if(typeof(IslandSub) == this.GetType())
            {
                realCenterPos += (this as IslandSub).islandMain.islandMapPos;
            }
            Vector2Int realBigMapPos = realCenterPos + localMapPos + halfMapSize;
            return realBigMapPos;
        }
        
        void OnDrawGizmosSelected()
        {
            if(this.GetType() == typeof(IslandMain))
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            IslandManager islandManager = GetComponentInParent<IslandManager>();
            for (int i = 0; i < islandMapRanges.Length; i++)
            {
                Gizmos.DrawWireCube(new Vector3(islandMapRanges[i].x, 0, islandMapRanges[i].y) * islandManager.mapGridSize + transform.position,
                    Vector3.one * islandManager.mapGridSize);
            }
            
            //Draw 区域包围盒Bounds
            Gizmos.color = Color.blue;
            foreach (var islandFieldPart in islandFieldParts)
            {
                foreach (var _bound in islandFieldPart.fieldBounds)
                {
                    Gizmos.DrawWireCube(transform.position+_bound.center,_bound.size);   
                }   
            }
        }

        #region 区域划分
        [Serializable]
        public struct IslandFieldPart
        {
            public List<Bounds> fieldBounds;
            public NavMeshSurface navMeshSurface;
            public Transform buildingPartsTransf;
        }
        #endregion


    }
}