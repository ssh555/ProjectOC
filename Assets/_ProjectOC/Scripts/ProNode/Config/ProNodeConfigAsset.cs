using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ProNodeNS
{
    [CreateAssetMenu(fileName = "ProNodeConfigAsset", menuName = "OC/ProNode/ProNodeConfigAsset", order = 1)]
    public class ProNodeConfigAsset : SerializedScriptableObject
    {
        [LabelText("≈‰÷√ ˝æ›"), ShowInInspector]
        public ProNodeConfig Config;
    }
}