using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class AIController : RoleController
    {
        public ICharacter SpawnCharacter()
        {
            ICharacter character = null;
            return character;
        }

        public ICharacter SpawnCharacter(int _index = 0,Transform _transf = null)
        {
            return null;
        }

        public override void ReSpawn(ICharacter _character, IStartPoint _startPoint = null)
        {
            base.ReSpawn(_character,_startPoint);
        }
        public override void Dispose(ICharacter _character)
        {
            base.Dispose(_character);
        }
        public override void DisposeAll()
        {
            base.DisposeAll();
        }
    }
}