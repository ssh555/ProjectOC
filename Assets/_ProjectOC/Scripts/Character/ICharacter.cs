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
        public void OnSpawn(IController controller);
        public void OnDespose(IController controller);
    }
}