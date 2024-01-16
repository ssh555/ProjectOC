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
        [SerializeField] protected Transform colliderTransform;
        [SerializeField] protected Vector2Int islandMapPos;
        [SerializeField] protected Vector2Int[] islandMapRange;
        public virtual void OnUnlock()
        { }

        //可视化方便策划观察
        [Button(name: "生成碰撞体"), PropertyOrder(-1)]
        public void ButtonGenerateCollider()
        {
            GenerateColliderBox(islandMapPos);
        }
        
        
        public virtual void GenerateColliderBox(Vector2Int centerPos)
        {
            //检查是否有重复等合法性
            //对islandMapRange排序
            
            for (int i = colliderTransform.childCount-1; i>-1;i--)
            {
                DestroyImmediate(colliderTransform.GetChild(i).gameObject);
            }
            
            //根据islandMapRange、pos 生成碰撞、位置
            IslandManager islandManager = FindObjectOfType<IslandManager>().GetComponent<IslandManager>();
            int mapSize = islandManager.mapGridSize;
            transform.localPosition = new Vector3(islandMapPos.x,0,islandMapPos.y)*mapSize;
            for(int i = 0;i<islandMapRange.Length;i++)
            {
                //检查是否超出边界
                Vector2Int bigMapPos = centerPos + islandMapRange[i];
                if (Mathf.Abs(bigMapPos.x) >islandManager.maxSize.x || Mathf.Abs(bigMapPos.y) > islandManager.maxSize.y)
                {
                    Debug.LogError("Map pos beyond bounder in array:  " + i +" " + this.gameObject.name);
                }
                
                
                GameObject colliderGo = new GameObject("collider"+i);
                colliderGo.transform.SetParent(colliderTransform);
                BoxCollider boxCollid = colliderGo.AddComponent<BoxCollider>();
                boxCollid.size = Vector3.one * mapSize;
                colliderGo.transform.localPosition = new Vector3(islandMapRange[i].x,0,islandMapRange[i].y)*mapSize;
            }
            
        }
    }
}