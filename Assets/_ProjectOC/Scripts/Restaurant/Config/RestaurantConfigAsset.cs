using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.RestaurantNS
{
    [CreateAssetMenu(fileName = "RestaurantConfigAsset", menuName = "OC/Restaurant/RestaurantConfigAsset", order = 1)]
    public class RestaurantConfigAsset : SerializedScriptableObject
    {
        [LabelText("��������"), ShowInInspector]
        public RestaurantConfig Config;
    }
}