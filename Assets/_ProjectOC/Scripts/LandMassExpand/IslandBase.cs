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
        [SerializeField,LabelText("������ͼλ��")] 
        public Vector2Int islandMapPos;
        [SerializeField,LabelText("������ͼ����")]
        protected Vector2Int[] islandMapRanges;
        [SerializeField,LabelText("����ģ��")]
        private Transform islandMesh;

        private IslandManager islandManager;        
        Vector2Int bigMapSize => islandManager.maxSize;
        [SerializeField,LabelText("��������")] 
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
                Debug.LogError(this.gameObject.name + " :������ͼ�����к����ظ�Ԫ��");
                return;
            }
            
            Vector2Int[] islandMapRangeData = new Vector2Int[islandMapRanges.Length];
            Array.Copy(islandMapRanges, islandMapRangeData, islandMapRanges.Length);
            int mapSize = islandManager.mapGridSize;
            ChangeIslandGrids(islandMapRangeData,false);
            
            
            #region ����Ƿ񳬳��߽�����غ�
            for(int i = 0;i<islandMapRanges.Length;i++)
            {
                //��[-halfMapSize,halfMapSize]ת��[0,bigMapSize-1]
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
            //�ƶ����츸����
            Vector2Int realCenterPos = islandMapPos;
            if(typeof(IslandSub) == this.GetType())
            {
                realCenterPos += (this as IslandSub).islandMain.islandMapPos;
            }
            transform.position= new Vector3(realCenterPos.x,transform.position.y,realCenterPos.y)*mapSize;
            ChangeIslandGrids(islandMapRanges,true);
        }

        [Button(name: "��ת����"),PropertyOrder(-1)]
        public void RotateIsland()
        {
            if (!JudgeVaild_IslandMapRanges())
            {
                Debug.LogError(this.gameObject.name + " :������ͼ�����к����ظ�Ԫ��");
                return;
            }
            
            Vector2Int[] islandMapRangeData = new Vector2Int[islandMapRanges.Length];
            Array.Copy(islandMapRanges, islandMapRangeData, islandMapRanges.Length);
            ChangeIslandGrids(islandMapRangeData,false);
            
            #region ����Ƿ񳬳��߽�����غ�
            for (int i = 0; i < islandMapRanges.Length; i++)
            {
                
                islandMapRanges[i] = new Vector2Int(islandMapRanges[i].y, -islandMapRanges[i].x);
                Vector2Int realBigMapPos = LocalToWorldMapPos(islandMapRanges[i]);
                
                //��[-halfMapSize,halfMapSize]ת��[0,bigMapSize-1]
                if (realBigMapPos.x < 0 || realBigMapPos.x >= bigMapSize.x || realBigMapPos.y < 0 ||
                    realBigMapPos.y >= bigMapSize.y)
                {
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError(this.gameObject.name + "Map pos beyond bounder in array:  " + i +";" + realBigMapPos);
                    return;
                }

                if (islandManager.islandGrids[realBigMapPos.x, realBigMapPos.y] != null)
                {
                    //���ı��ط�Χ���ݣ��������緶Χ����
                    islandMapRanges = islandMapRangeData;
                    Debug.LogError("Map pos overlap in grid:  " + realBigMapPos);
                    return;
                }
            }
            #endregion
            //˳ʱ����ת
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
                    return false; // �����ظ�Ԫ��
                }
            }

            return true; // û�з����ظ�Ԫ��
        }
        
        
        
        void ChangeIslandGrids(Vector2Int[] islandMapRanges,bool isRange)
        {
            //islandMapRanges.�ֲ�����
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
        
        //�ӵ��챾����������ת����ȫ���������꣬��IslandManager.islandGrids
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
            
            //Draw �����Χ��Bounds
            Gizmos.color = Color.blue;
            foreach (var islandFieldPart in islandFieldParts)
            {
                foreach (var _bound in islandFieldPart.fieldBounds)
                {
                    Gizmos.DrawWireCube(transform.position+_bound.center,_bound.size);   
                }   
            }
        }

        #region ���򻮷�
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