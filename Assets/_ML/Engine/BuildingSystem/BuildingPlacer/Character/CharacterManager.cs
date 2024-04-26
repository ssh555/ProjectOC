using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Reflection;
using ProjectOC.Player;
using Random = UnityEngine.Random;

namespace ML.PlayerCharacterNS
{
    [System.Serializable]
    public class CharacterManager : ML.Engine.Manager.GlobalManager.IGlobalManager
    {
        List<PlayerController> playerControllers = new List<PlayerController>();
        List<AIController> aiControllers = new List<AIController>();
        [HideInInspector]
        public List<IStartPoint> playerStartPoints = new List<IStartPoint>();
        int LocalPlayerControllerIndex = -1;
        int LocalAIControllerIndex = -1;
        
        [TypeFilter("GetAllPlayerControllerClass"), SerializeReference]
        public PlayerController playerController;
        public IEnumerable<Type> GetAllPlayerControllerClass()
        {
            var q = typeof(PlayerController).Assembly.GetTypes()
                .Where(x => !x.IsAbstract)                                          // Excludes BaseClass
                .Where(x => !x.IsGenericTypeDefinition)                             // Excludes C1<>
                .Where(x => typeof(PlayerController).IsAssignableFrom(x));          // Excludes classes not inheriting from BaseClass
            return q;
        }
        #region Unity

        
        public void OnRegister()
        {
            LocalPlayerControllerIndex = 0;
            AddPlayerController(playerController);
            //AddPlayerController(new OCPlayerController());
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