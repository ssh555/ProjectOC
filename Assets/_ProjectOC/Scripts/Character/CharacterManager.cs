using System.Collections;
using System.Collections.Generic;
using ProjectOC.ManagerNS;
using ProjectOC.Player;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class CharacterManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        List<PlayerController> playerControllers;
        List<IPlayerCharacter> playerCharacters;
        List<IAICharacter> AICharacters;
        private List<IStartPoint> playerStartPoint;
        int CurrentControllerIndex;

        //暂定Scene1 的触发时间
        public void Scene1Init()
        {
            playerControllers = new List<PlayerController>();
            playerStartPoint = new List<IStartPoint>();
            //    
            AddPlayerController(new PlayerController());
            CurrentControllerIndex = 0;
            GetController().SpawnCharacter(0);
        }
        
        
        public PlayerController GetController(int index = -1)
        {
            if (index == -1)
                index = CurrentControllerIndex;
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