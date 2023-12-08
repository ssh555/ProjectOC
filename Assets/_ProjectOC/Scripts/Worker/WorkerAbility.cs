using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectOC.WorkerNS
{
    /// <summary>
    /// ����
    /// </summary>
    [System.Serializable]
    public class WorkerAbility
    {
        /// <summary>
        /// ����WorkerAbility_��������
        /// </summary>
        public string ID = "";
        /// <summary>
        /// ����
        /// </summary>
        public string Name = "";
        /// <summary>
        /// ��ţ���������
        /// </summary>
        public int Sort;
        /// <summary>
        /// ���ܵ�ͼ��
        /// </summary>
        public Texture Icon;
        /// <summary>
        /// ��������
        /// </summary>
        public WorkType Type;
        /// <summary>
        /// ��������
        /// </summary>
        public string Desciption = "";
        /// <summary>
        /// ����Ч������
        /// </summary>
        public string EffectsDescription = "";
        /// <summary>
        /// ��ǰ�ȼ�
        /// </summary>
        public int Level;
        /// <summary>
        /// ���ȼ�
        /// </summary>
        public int LevelMax;
        /// <summary>
        /// ��ǰ����
        /// </summary>
        private int Exp;
        /// <summary>
        /// Ч��
        /// </summary>
        public List<Effect> Effects = new List<Effect>();

        public WorkerAbility(string id)
        {
            //TODO:����
            this.ID = id;
            this.Name = "";
            this.Sort = 0;
            //this.Icon;
            //this.Type;
            this.Desciption = "";
            this.EffectsDescription = "";
            this.Level = 0;
            this.LevelMax = 10;
            this.Exp = 0;
            //Effects.Add(new Effect(effectID));
        }

        /// <summary>
        /// �ȼ�����
        /// </summary>
        public void AddLevel()
        {
            if (this.Level < this.LevelMax)
            {
                this.Level++;
                this.Exp = 0;
                //�ı�Ч��
            }
        }
        /// <summary>
        /// �ȼ�����
        /// </summary>
        /// <param name="clearExp">�Ƿ���վ���ֵ</param>
        public void MinusLevel(bool clearExp = false)
        {
            if (this.Level >= 0)
            {
                this.Level--;
                this.Exp = clearExp ? 0 : 100 * (this.Level + 1);
                //�ı�Ч��
            }
        }
        /// <summary>
        /// ���ȼ��;����Ƿ����
        /// </summary>
        public void CheckLevel()
        {
            if (this.Level >= this.LevelMax)
            {
                this.Level = this.LevelMax;
                this.Exp = 0;
            }
            if (this.Level < 0)
            {
                this.Level = 0;
                this.Exp = 0;
            }
        }

        /// <summary>
        /// �޸ľ���ֵ��
        /// ÿ������������µľ���ֵ������µļ���Ч����
        /// ����ʱ������ֵ�������������һ����
        /// �ﵽ��߼�ʱ�����Ӿ��顣
        /// </summary>
        /// <param name="value">����ֵ</param>
        public void AlterExp(int value)
        {
            while (value > 0 && this.Level < this.LevelMax)
            {
                int expNeeded = 100 * (this.Level + 1) - this.Exp;
                if (value >= expNeeded)
                {
                    value -= expNeeded;
                    this.AddLevel();
                }
                else
                {
                    this.Exp = this.Level >= this.LevelMax ? 0 : this.Exp + value;
                    value = 0;
                }
            }

            while (value < 0 && (this.Level >= 0 && this.Exp > 0))
            {
                if (value + this.Exp >= 0)
                {
                    this.Exp -= value;
                    value = 0;
                }
                else
                {
                    value += this.Exp;
                    this.MinusLevel();
                }
            }
            CheckLevel();
        }

        public void ApplyEffectToWorker(Worker worker)
        {
            if (worker != null)
            {
                foreach (Effect effect in this.Effects)
                {
                    effect.ApplyEffectToWorker(worker);
                }
            }
        }
    }
}

