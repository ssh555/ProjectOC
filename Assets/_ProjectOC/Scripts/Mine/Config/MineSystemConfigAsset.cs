using Sirenix.OdinInspector;
using UnityEngine;

namespace ProjectOC.ProNodeNS
{
    [CreateAssetMenu(fileName = "MineSystemConfigAsset", menuName = "OC/MineSystem/MineSystemConfigAsset", order = 1)]
    public class MineSystemConfigAsset : SerializedScriptableObject
    {
        [LabelText("��������"), ShowInInspector]
        public MineSystemConfig Config;
    }
}