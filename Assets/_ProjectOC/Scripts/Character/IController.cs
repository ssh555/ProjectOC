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
        ICharacter SpawnCharacter(int _index = 0,IStartPoint _startPoint = null);
        void ReSpawn(ICharacter _character,IStartPoint _startPoint = null);
        void Dispose(ICharacter character);
        void DisposeAll();
    }
}