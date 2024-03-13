using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;
namespace ML.Engine.UI
{
    public class FunctionExecutor<T> where T : IEnumerator
    {
        private List<Func<T>> FunctionList = new List<Func<T>>();
        private Action onAllFunctionsCompleted; // ���к���ִ����Ϻ�Ļص�����

        // ���캯��
        public FunctionExecutor(List<Func<T>> FunctionList, Action onAllFunctionsCompleted)
        {
            this.FunctionList = FunctionList;
            this.onAllFunctionsCompleted = onAllFunctionsCompleted;
        }

        public FunctionExecutor()
        {
            this.FunctionList = new List<Func<T>>();
        }

        //ִ��
        public IEnumerator Execute()
        {
            IEnumerator[] enumerators = new IEnumerator[FunctionList.Count];
            for (int i = 0; i < FunctionList.Count; i++)
            {
                enumerators[i] = FunctionList[i].Invoke();
            }
            for (int i = 0; i < FunctionList.Count; i++)
            {
                yield return enumerators[i];
            }


            this.onAllFunctionsCompleted.Invoke();
        }

        public void AddFunction(Func<T> func)
        {
            this.FunctionList.Add(func);
        }

        public void AddFunction(List<Func<T>> funclist)
        {
            foreach (Func<T> func in funclist) { this.FunctionList.Add(func);}
        }
        public void SetOnAllFunctionsCompleted(Action action)
        {
            this.onAllFunctionsCompleted = action;
        }
    }
}

