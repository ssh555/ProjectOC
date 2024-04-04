using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public interface IController:ITickComponent
    {
        List<ICharacter> SpawnedCharacters{ get; set; }
        IControllerState State { get;set;  }
        IStartPoint startPoint { get; set; }
        ICharacter SpawnCharacter(int _index = 0,Transform _transf = null) ;
        void ReSpawn();
        void Dispose(ICharacter character);
        void DisposeAll();
    }
}