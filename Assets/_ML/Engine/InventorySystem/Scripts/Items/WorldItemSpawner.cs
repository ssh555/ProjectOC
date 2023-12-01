using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine.InventorySystem
{
    public class WorldItemSpawner : MonoBehaviour
    {
        //[Button("Éú³É WorldItem")]
        public WorldItem SpawnerWorldItem(int id)
        {
            var ret = ItemSpawner.Instance.SpawnWorldItem(ItemSpawner.Instance.SpawnItem(id), this.transform.position, this.transform.rotation);
            ret.transform.SetParent(this.transform);
            return ret;
        }

    }
}

