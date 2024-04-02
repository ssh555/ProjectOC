using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PlayerCharacterNS
{
    public interface ICharacter
    {
        int prefabIndex { get; set; }
        ICharacterState State { get; set; }
        IController Controller { get; set; }
        void onSpawn(IController controller);
        void onDestroy(IController controller);
        void onUpdate(IController controller);
    }
}