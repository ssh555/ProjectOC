using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public interface IControllerState : IState
    {
        IController controller { get; set; }
    }
}