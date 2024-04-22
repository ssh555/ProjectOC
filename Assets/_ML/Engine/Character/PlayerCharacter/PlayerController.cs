using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using UnityEngine;


namespace ML.PlayerCharacterNS
{
    [System.Serializable]
    public class PlayerController : RoleController

    {
        public PlayerController()
        {
            State = new PlayerControllerState(this);
        }
    }
}