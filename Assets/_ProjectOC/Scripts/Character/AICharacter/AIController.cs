using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.PlayerCharacterNS
{
    public class AIController : IController
    {
        public List<ICharacter> SpawnedCharacters { get; set; }
        public IControllerState State { get; set; }
        public IStartPoint startPoint { get; set; }
        public ICharacter SpawnCharacter()
        {
            ICharacter character = null;
            return character;
        }

        public ICharacter SpawnCharacter(int _index = 0,Transform _transf = null)
        {
            return null;
        }

        public void ReSpawn(ICharacter _character,Transform _transf = null)
        {
            int _index = _character.prefabIndex;
            Dispose(_character);
            SpawnCharacter(_index,_transf);
        }
        
        public void Dispose(ICharacter character,bool _destoryModel = true)
        {
            if (_destoryModel)
            {
                //Ïú»ÙÎïÌå
                ML.Engine.Manager.GameManager.DestroyObj(character.transform.gameObject);
            }
            this.SpawnedCharacters.Remove(character);
        }
        public void DisposeAll()
        {
            foreach (var _character in SpawnedCharacters)
            {
                Dispose(_character);
            }
        }
        #region ITickComponent
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public virtual void Tick(float deltatime)
        {

        }
        public virtual void FixedTick(float deltatime)
        {

        }
        public virtual void LateTick(float deltatime)
        {

        }
        #endregion
    }
}