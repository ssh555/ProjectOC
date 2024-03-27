using System.Collections;
using System.Collections.Generic;
using ProjectOC.PlayerCharacterNS;
using UnityEngine;

public class PlayerControllerState : IControllerState
{
    public IController controller { get; set; }

    public enum PlayerState
    {
        normal,
        death
    }

    private PlayerState playerState = PlayerState.normal;
}
