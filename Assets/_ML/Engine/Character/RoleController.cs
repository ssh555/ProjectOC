using System;
using System.Collections;
using System.Collections.Generic;
using ML.PlayerCharacterNS;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ML.PlayerCharacterNS
{
    public abstract class RoleController : IController
    {
        public int tickPriority { get; set; }
        public int fixedTickPriority { get; set; }
        public int lateTickPriority { get; set; }
        public List<ICharacter> SpawnedCharacters { get; set; } = new List<ICharacter>();
        public IControllerState State { get; set; }
        public List<int> CurrentCharacterIndexs { get; set; }

        //记得触发Character.OnSpawn()
        public List<ICharacter> GetCurrentCharacters()
        {
            List<ICharacter> _characters = new List<ICharacter>();
            foreach (var _index in CurrentCharacterIndexs)
            {
                ICharacter _character = SpawnedCharacters[_index];
                if (_character != null)
                {
                    _characters.Add(_character);    
                }
            }
            return _characters;
        }

        public ICharacter GetCharacter(int index)
        {
            return SpawnedCharacters[index];
        }

        public virtual ICharacter SpawnCharacter(int _index = 0, IStartPoint _startPoint = null)
        {
            //有对应的Controller具体实现
            return null;
        }

        public virtual void ReSpawn(ICharacter _character, IStartPoint _startPoint = null)
        {
            int _index = _character.prefabIndex;
            Dispose(_character);
            SpawnCharacter(_index, _startPoint);
        }

        public virtual void Dispose(ICharacter character)
        {
            character.OnDespose(this);
            this.SpawnedCharacters.Remove(character);
        }

        public virtual void DisposeAll()
        {
            foreach (var _character in SpawnedCharacters)
            {
                Dispose(_character);
            }
        }

        //根据_startPoint
        protected void SetCharacterTransform(Transform _transf, IStartPoint _startPoint)
        {
            CharacterController cc = _transf.GetComponent<CharacterController>();
            cc.enabled = false;
            _transf.position = _startPoint.transform.position;
            if (_startPoint.EnablePosRange)
            {
                _transf.position += new Vector3(Random.Range(0, _startPoint.PosRange), 0,
                    Random.Range(0, _startPoint.PosRange));
            }

            Vector3 _eulerAngles = _startPoint.transform.rotation.eulerAngles;
            if (_startPoint.EnableRotRange)
            {
                _eulerAngles += Vector3.up * Random.Range(-_startPoint.RotRange, _startPoint.RotRange);
            }

            _transf.rotation = Quaternion.Euler(_eulerAngles);
            cc.enabled = true;
        }
    }
}
