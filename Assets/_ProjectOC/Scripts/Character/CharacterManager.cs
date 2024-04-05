using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private List<IStartPoint> playerStartPoints;
        int CurrentControllerIndex;

        //暂定Scene1 的触发时间
        public void Scene1Init()
        {
            playerControllers = new List<PlayerController>();
            playerStartPoints = new List<IStartPoint>();
            AddPlayerController(new PlayerController());
            CurrentControllerIndex = 0;
            
            //Player生成
            playerStartPoints = GameObject.Find("SpawnPoints").GetComponentsInChildren<IStartPoint>().ToList();
            GetController().SpawnCharacter(0,playerStartPoints[Random.Range(0,playerStartPoints.Count)]);
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