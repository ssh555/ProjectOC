using System;
using System.Collections;
using System.Collections.Generic;
using ML.Engine.Manager;
using ProjectOC.ManagerNS;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectOC.LandMassExpand
{
    public class IslandBase : MonoBehaviour
    {
        #region ������ͼ

        private int island_Index;
        [SerializeField,LabelText("������ͼλ��")] 
        public Vector2Int islandMapPos;
        [SerializeField,LabelText("������ͼ����")]
        protected Vector2Int[] islandMapRanges;
        [SerializeField,LabelText("����ģ��")]
        private Transform islandMesh;

        private IslandModelManager islandManager;        
        Vector2Int bigMapSize => islandManager.maxSize;
        [SerializeField,LabelText("��������")] 
        public List<IslandFieldPart> islandFieldParts;

        [LabelText("���������ƶ�")]
        public Vector2Int GridChange;
        private void Start()
        {
            islandManager = GameManager.Instance.GetLocalManager<IslandModelManager>();
            ChangeIslandGrids(islandMapRanges, true);
            this.enabled = false;
        }

        public virtual void OnUnlock()
        { }
        
        [Button(name: "�ƶ�����"), PropertyOrder(-2)]
        public void ButtonMoveIsland()
        {
            IslandMove();
        }
        
        public virtual void IslandMove()
        {
            if (!JudgeVaild_IslandMapRanges())
            {
                Debug.LogError(this.gameObject.name + " :������ͼ�����к����ظ�Ԫ��");
                return;
            }
            
            ChangeIslandGrids(islandMapRanges,false);
            if(typeof(IslandMain) == this.GetType())
            {
                IslandMain _island = this as IslandMain;
                foreach (var _sub in _island.affiliatedIslands)
                {
                    _sub.ChangeIslandGrids(_sub.islandMapRanges,false);
                }
            }
            
            Vector2Int centerData = islandMapPos;
            int mapSize = islandManager.mapGridSize;
            islandMapPos += GridChange;
            
            #region ����Ƿ񳬳��߽�����غ�
            for(int i = 0;i<islandMapRanges.Length;i++)
            {
                //��[-halfMapSize,halfMapSize]ת��[0,bigMapSize-1]
                Vector2Int realBigMapPos = LocalToWorldMapPos(islandMapRanges[i]);
                if (realBigMapPos.x < 0 || realBigMapPos.x >= bigMapSize.x || realBigMapPos.y < 0 || realBigMapPos.y >= bigMapSize.y )
                {
                    islandMapPos = centerData;
                    ChangeIslandGrids(islandMapRanges,true);
                    Debug.LogError(this.gameObject.name + "Map pos beyond bounder in array:  " + i +";" + realBigMapPos);
                    return;
                }
                else if(islandManager.islandGrids[realBigMapPos.x,realBigMapPos.y] != null)
                {
                    islandMapPos = centerData;
                    ChangeIslandGrids(islandMapRanges,true);
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
                    ChangeIslandGrids(islandMapRanges,true);
                    Debug.LogError(this.gameObject.name + "Map pos beyond bounder in array:  " + i +";" + realBigMapPos);
                    return;
                }

                if (islandManager.islandGrids[realBigMapPos.x, realBigMapPos.y] != null)
                {
                    //���ı��ط�Χ���ݣ��������緶Χ����
                    islandMapRanges = islandMapRangeData;
                    ChangeIslandGrids(islandMapRanges,true);
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
        
        
        
        void ChangeIslandGrids(Vector2Int[] _islandMapRanges,bool isRange)
        {
            if (_islandMapRanges == null)
            {
                _islandMapRanges = islandMapRanges;
            }
            
            //islandMapRanges.�ֲ�����
            foreach (var islandMapRange in _islandMapRanges)
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
        
        
        
        #endregion
        
        

        #region ���򻮷�
        [Serializable]
        public struct IslandFieldPart
        {
            public List<Bounds> fieldBounds;
            public Transform buildingPartsTransf;
        }

        



        #endregion


        #region Gizmos
#if UNITY_EDITOR        
        public Color gizmosColor;
#endif       
        
        void OnDrawGizmosSelected()
        {
            //Draw �����Χ��Bounds
            Gizmos.color = Color.blue;
            foreach (var islandFieldPart in islandFieldParts)
            {
                foreach (var _bound in islandFieldPart.fieldBounds)
                {
                    Gizmos.DrawWireCube(transform.position+_bound.center,_bound.size);   
                }   
            }
            
            if (!Application.isPlaying)
                return;
            
            // if(this.GetType() == typeof(IslandMain))
            // {
            //     Gizmos.color = Color.green;
            // }
            // else
            // {
            //     Gizmos.color = Color.red;
            // }
            //
            // IslandModelManager islandManager = GameManager.Instance.GetLocalManager<IslandModelManager>();
            // for (int i = 0; i < islandMapRanges.Length; i++)
            // {
            //     Gizmos.DrawWireCube(new Vector3(islandMapRanges[i].x, 0, islandMapRanges[i].y) * islandManager.mapGridSize + transform.position,
            //         Vector3.one * islandManager.mapGridSize);
            // }
            

        }
        #endregion
        
        

    }
}