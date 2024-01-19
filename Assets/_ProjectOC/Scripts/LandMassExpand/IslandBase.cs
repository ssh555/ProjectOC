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
        protected Vector2Int islandMapPos;
        [SerializeField,LabelText("������ͼ����")]
        protected Vector2Int[] islandMapRanges;
        [SerializeField,LabelText("����ģ��")]
        private Transform islandMesh;
        public virtual void OnUnlock()
        { }

        public void GenerateColliderBox()
        {
            IslandMove(islandMapPos);
        }
        public virtual void IslandMove(Vector2Int centerPos)
        {
            //����Ƿ����ظ��ȺϷ���
            //��islandMapRange����
            
            //����islandMapRange��pos ������ײ��λ��
            IslandManager islandManager = FindObjectOfType<IslandManager>().GetComponent<IslandManager>();
            int mapSize = islandManager.mapGridSize;
            
            //�ƶ����츸����
            transform.position= new Vector3(centerPos.x,0,centerPos.y)*mapSize;
            
            for(int i = 0;i<islandMapRanges.Length;i++)
            {
                //����Ƿ񳬳��߽�

                #region ����Ƿ񳬳��߽�����غ�
                Vector2Int bigMapPos = centerPos + islandMapRanges[i];
                //��[-halfMapSize,halfMapSize]ת��[0,bigMapSize-1]
                Vector2Int bigMapSize = islandManager.maxSize;
                Vector2Int halfMapSize = (islandManager.maxSize - Vector2Int.one) / 2;

                Vector2Int realBigMapPos = bigMapPos + halfMapSize;
                if (realBigMapPos.x < 0 || realBigMapPos.x >= bigMapSize.x || realBigMapPos.y < 0 || realBigMapPos.y >= bigMapSize.y )
                {
                    Debug.LogError("Map pos beyond bounder in array:  " + i +" " + this.gameObject.name);
                }
                else if(islandManager.islandGrids[realBigMapPos.x,realBigMapPos.y] != null)
                {
                    Debug.LogError("Map pos overlap in array:  " + i +" " + this.gameObject.name + 
                                   "GridStore:" + islandManager.islandGrids[realBigMapPos.x,realBigMapPos.y].gameObject.name);
                }
                else
                {
                    islandManager.islandGrids[realBigMapPos.x, realBigMapPos.y] = this;
                }
                #endregion
                

            }
            
            
        }

        [Button(name: "��ת����"),PropertyOrder(-1)]
        public void RotateIsland()
        {
            //˳ʱ����ת
            islandMesh.rotation = Quaternion.Euler(islandMesh.eulerAngles + Vector3.up*90);
            for (int i = 0; i < islandMapRanges.Length; i++)
            {
                islandMapRanges[i] = new Vector2Int(islandMapRanges[i].y, -islandMapRanges[i].x);
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