using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class PlayerController : RoleController
    {
        public PlayerController()
        {
            State = new PlayerControllerState(this);
        }
    }
}