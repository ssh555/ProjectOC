using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.StoreNS
{
    [CreateAssetMenu(fileName = "StoreConfigAsset", menuName = "OC/Store/StoreConfigAsset", order = 1)]
    public class StoreConfigAsset : SerializedScriptableObject
    {
        [LabelText("��������")]
        public StoreConfig Config;
    }
}