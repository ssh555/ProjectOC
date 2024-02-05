using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectOC.LandMassExpand
{
    public class IslandBase : MonoBehaviour
    {
        private int island_Index;
        [SerializeField,LabelText("������ͼλ��")] 
        public Vector2Int islandMapPos;
        [SerializeField,LabelText("������ͼ����")]
        protected Vector2Int[] islandMapRanges;
        [SerializeField,LabelText("����ģ��")]
        private Transform islandMesh;
        public virtual void OnUnlock()
        { }

        public void IslandMove()
        {
            IslandMove(islandMapPos);
        }
        public virtual void IslandMove(Vector2Int centerPos)
        {
            IslandManager islandManager = 
                ML.Engine.Manager.GameManager.Instance.GetLocalManager<IslandManager>();
            Vector2Int[] islandMapRangeData = new Vector2Int[islandMapRanges.Length];
            Array.Copy(islandMapRanges, islandMapRangeData, islandMapRanges.Length);
            int mapSize = islandManager.mapGridSize;
            Vector2Int bigMapSize = islandManager.maxSize;
            Vector2Int halfMapSize = (islandManager.maxSize - Vector2Int.one) / 2;
            
            #region ����Ƿ񳬳��߽�����غ�
            for(int i = 0;i<islandMapRanges.Length;i++)
            {
                //��[-halfMapSize,halfMapSize]ת��[0,bigMapSize-1]
                Vector2Int realBigMapPos = centerPos + islandMapRanges[i] + halfMapSize;
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
                        "GridStore:" + islandManager.islandGrids[realBigMapPos.x,realBigMapPos.y].gameObject.name);
                    return;
                }
                else
                {
                    islandManager.islandGrids[realBigMapPos.x, realBigMapPos.y] = this;
                }
                
            }
            #endregion
            //�ƶ����츸����
            transform.position= new Vector3(centerPos.x,transform.position.y,centerPos.y)*mapSize;
            ChangeIslandGrids(islandMapRangeData,false);
            ChangeIslandGrids(islandMapRanges,true);
            
            
        }

        [Button(name: "��ת����"),PropertyOrder(-1)]
        public void RotateIsland()
        {
            IslandManager islandManager = 
                ML.Engine.Manager.GameManager.Instance.GetLocalManager<IslandManager>();
            Vector2Int[] islandMapRangeData = new Vector2Int[islandMapRanges.Length];
            Array.Copy(islandMapRanges, islandMapRangeData, islandMapRanges.Length);
            Vector2Int bigMapSize = islandManager.maxSize;
            Vector2Int halfMapSize = (islandManager.maxSize - Vector2Int.one) / 2;
            Vector2Int realCenterPos = islandMapPos;
            if(typeof(IslandSub) == this.GetType())
            {
                realCenterPos += (this as IslandSub).islandMain.islandMapPos;
            }
            
            #region ����Ƿ񳬳��߽�����غ�
            for (int i = 0; i < islandMapRanges.Length; i++)
            {
                
                islandMapRanges[i] = new Vector2Int(islandMapRanges[i].y, -islandMapRanges[i].x);
                Vector2Int realBigMapPos = realCenterPos + islandMapRanges[i] + halfMapSize;
                
                Vector2Int _islandRangePos = islandMapPos + islandMapRanges[i];

                if (_islandRangePos.x < 0 || _islandRangePos.x >= bigMapSize.x || _islandRangePos.y < 0 ||
                    _islandRangePos.y >= bigMapSize.y)
                {
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError(this.gameObject.name + "Map pos beyond bounder in array:  " + i +";" + _islandRangePos);
                    return;
                }

                if (IslandManager.Instance.islandGrids[_islandRangePos.x, _islandRangePos.y] != null)
                {
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError("Map pos overlap in grid:  " + _islandRangePos);
                    return;
                }
                else
                {
                    islandManager.islandGrids[realBigMapPos.x, realBigMapPos.y] = this;
                }
            }
            #endregion
            //˳ʱ����ת
            islandMesh.rotation = Quaternion.Euler(islandMesh.eulerAngles + Vector3.up*90);
            ChangeIslandGrids(islandMapRangeData,false);
            ChangeIslandGrids(islandMapRanges,true);
        }

        public void JudgeInvalid()
        {
            
        }
        
        
        
        void ChangeIslandGrids(Vector2Int[] islandMapRanges,bool isRange)
        {
            foreach (var islandMapRange in islandMapRanges)
            {
                IslandBase _islandBase;
                if (isRange)
                    _islandBase = this;
                else
                    _islandBase = null;
                IslandManager.Instance.islandGrids[islandMapRange.x, islandMapRange.y] = _islandBase;
            }
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
                Vector2Int bigMapSize = islandManager.maxSize;
                Gizmos.DrawWireCube(new Vector3(islandMapRanges[i].x, 0, islandMapRanges[i].y) * islandManager.mapGridSize + transform.position,
                    Vector3.one * islandManager.mapGridSize);
            }
        }

    }
}