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
        List<PlayerController> playerControllers = new List<PlayerController>();
        List<AIController> aiControllers = new List<AIController>();
        
        public List<IStartPoint> playerStartPoints = new List<IStartPoint>();
        int LocalPlayerControllerIndex = -1;
        int LocalAIControllerIndex = -1;

        #region Unity

        
        public void OnRegister()
        {
            LocalPlayerControllerIndex = 0;
            AddPlayerController(new OCPlayerController());
        }

        public PlayerController GetLocalController()
        {
            return playerControllers[LocalPlayerControllerIndex];
        }
        
        public AIController GetCurrentAIController()
        {
            return aiControllers[LocalAIControllerIndex];
        }

        public void AddPlayerController(PlayerController controller)
        {
            playerControllers.Add(controller);
        }

        public void AddAIController(AIController controller)
        {
            aiControllers.Add(controller);
        }

        #endregion
        
        public void SceneInit()
        {
            GetLocalController().SpawnCharacter(0,playerStartPoints[Random.Range(0,playerStartPoints.Count)]);
        }
    }
}