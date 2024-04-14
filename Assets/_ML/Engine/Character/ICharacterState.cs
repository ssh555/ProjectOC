using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public interface ICharacterState : IState
    { 
        ICharacter character { get; set; }
    }
}