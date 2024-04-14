using ML.Engine.Timer;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ProjectOC.Player;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

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