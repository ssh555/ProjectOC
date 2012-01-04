using ML.Engine.BuildingSystem;
using ML.Engine.TextContent;
using ML.Engine.UI;
using Newtonsoft.Json;
using ProjectOC.TechTree.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ML.Engine.BuildingSystem.Test_BuildingManager;

namespace ProjectOC.Player.UI
{
    public class PlayerUIBotPanel : UIBasePanel//, ML.Engine.Timer.ITickComponent
    {
        public PlayerCharacter player;

        //public int tickPriority { get; set; }
        //public int fixedTickPriority { get; set; }
        //public int lateTickPriority { get; set; }

        //private void Start()
        //{
        //    ML.Engine.Manager.GameManager.Instance.TickManager.RegisterTick(0, this);
        //}

        //public void Tick(float deltatime)
        //{

        //}

        public override void OnEnter()
        {
            base.OnEnter();
            Input.InputManager.PlayerInput.Player.Enable();
        }

        public override void OnPause()
        {
            base.OnPause();
            Input.InputManager.PlayerInput.Player.Disable();
        }

        public override void OnRecovery()
        {
            base.OnRecovery();
            Input.InputManager.PlayerInput.Player.Enable();
        }

        public override void OnExit()
        {
            base.OnExit();
            Input.InputManager.PlayerInput.Player.Disable();
        }
    }
}

