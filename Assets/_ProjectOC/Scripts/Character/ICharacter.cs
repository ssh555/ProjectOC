using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public interface ICharacter
    {
        Transform transform { get; }
        int prefabIndex { get;}
        ICharacterState State { get; set; }
        IController Controller { get; set; }
        void onSpawn(IController controller);
        void onDestroy(IController controller);
        void onUpdate(IController controller);
    }
}