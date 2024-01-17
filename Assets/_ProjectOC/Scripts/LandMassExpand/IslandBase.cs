using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace ProjectOC.LandMassExpand
{
    public class IslandBase : MonoBehaviour
    {
        private int island_Index;
        [SerializeField] protected Vector2Int islandMapPos;
        [SerializeField] protected Vector2Int[] islandMapRange;
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
            
            for(int i = 0;i<islandMapRange.Length;i++)
            {
                //����Ƿ񳬳��߽�

                #region ����Ƿ񳬳��߽�����غ�
                Vector2Int bigMapPos = centerPos + islandMapRange[i];
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
            
            IslandManager islandManager = FindObjectOfType<IslandManager>().GetComponent<IslandManager>();
            for (int i = 0; i < islandMapRange.Length; i++)
            {
                Vector2Int bigMapSize = islandManager.maxSize;
                Gizmos.DrawWireCube(new Vector3(islandMapRange[i].x, 0, islandMapRange[i].y) * islandManager.mapGridSize + transform.position,
                    Vector3.one * islandManager.mapGridSize);
            }
        }
    }
}