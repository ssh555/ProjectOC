using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PlayerCharacterNS
{
    public interface IController:ITickComponent
    {
        List<ICharacter> SpawnedCharacters { get; set; }
        IControllerState State { get;set;  }
        IStartPoint startPoint { get; set; }
        ICharacter SpawnCharacter();
        void ReSpawn();
        void Dispose(ICharacter character);
        void DisposeAll();
    }
}