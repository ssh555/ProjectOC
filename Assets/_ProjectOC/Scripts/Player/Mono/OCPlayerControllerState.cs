using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;

namespace ProjectOC.Player
{
    public class OCPlayerControllerState : PlayerControllerState
    {

        public OCPlayerControllerState(OCPlayerController _controller) : base(_controller)
        {
        }
        
        
        public ML.Engine.InventorySystem.IInventory Inventory;
    }
}