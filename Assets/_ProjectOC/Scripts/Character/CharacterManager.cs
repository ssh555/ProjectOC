using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.PlayerCharacterNS
{
    public class CharacterManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        List<PlayerController> playerControllers;
        List<IPlayerCharacter> playerCharacters;
        List<IAICharacter> AICharacters;
        int CurrentControllerIndex;
        public PlayerController GetCurrentController(int index)
        {
            return playerControllers[index];
        }

        public void AddPlayerController(PlayerController controller)
        {
            playerControllers.Add(controller);
        }
        public void AddPlayerCharacter(IPlayerCharacter character)
        {
            playerCharacters.Add(character);
        }
        public void AddAICharacter(IAICharacter character)
        {
            AICharacters.Add(character);
        }
        public void RemovePlayerCharacter(IPlayerCharacter character)
        {
            playerCharacters.Remove(character);
        }
        public void RemoveAICharacter(IAICharacter character)
        {
            AICharacters.Remove(character);
        }
    }
}