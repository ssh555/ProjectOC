using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PlayerCharacterNS
{
    public interface IControllerState : IState
    {
        IController controller { get; set; }
    }
}