using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class PlayerControllerState : IControllerState
    {
        public IController controller { get; set; }

        public enum PlayerState
        {
            normal,
            death
        }
    }
}