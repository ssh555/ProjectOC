using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PlayerCharacterNS
{
    public interface ICharacterState : IState
    { 
        ICharacter character { get; set; }
    }
}