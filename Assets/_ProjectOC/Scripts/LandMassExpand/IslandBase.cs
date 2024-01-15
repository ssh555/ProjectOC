using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace ML.Engine.LandMassExpand
{
    public class IslandBase : MonoBehaviour
    {
        private int island_Index;
        [SerializeField] private Transform colliderTransform;
        [SerializeField] private Vector2Int[] islandMapRange;
        public virtual void OnUnlock()
        { }

        //可视化方便策划观察
        [Button(name: "生成碰撞体"),PropertyOrder(-1)]
        private void GenerateColliderBox()
        {
            //检查是否有重复等合法性
            //对islandMapRange排序
            for (int i = colliderTransform.childCount-1; i>-1;i--)
            {
                DestroyImmediate(colliderTransform.GetChild(i).gameObject);
            }
            
            for(int i = 0;i<islandMapRange.Length;i++)
            {
                GameObject colliderGo = new GameObject("collider"+i);
                colliderGo.transform.SetParent(colliderTransform);
                BoxCollider boxCollid = colliderGo.AddComponent<BoxCollider>();
                int mapSize = FindObjectOfType<IslandManager>().GetComponent<IslandManager>().mapSize;
                boxCollid.size = Vector3.one * mapSize;
                colliderGo.transform.localPosition = new Vector3(islandMapRange[i].x,0,islandMapRange[i].y)*mapSize;
            }
        }
    }
}