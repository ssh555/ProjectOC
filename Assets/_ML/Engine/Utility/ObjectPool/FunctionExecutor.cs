using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting;
namespace ML.Engine.Utility
{
    public class FunctionExecutor<T> where T : List<AsyncOperationHandle>
    {
        public List<Func<T>> FunctionList = new List<Func<T>>();
        private Action onAllFunctionsCompleted; // 所有函数执行完毕后的回调函数
        public bool IsFinished { get { return isFinished; } }
        private bool isFinished;
        // 构造函数
        public FunctionExecutor(List<Func<T>> FunctionList, Action onAllFunctionsCompleted)
        {
            this.FunctionList = FunctionList;
            this.onAllFunctionsCompleted = onAllFunctionsCompleted;
        }

        public FunctionExecutor()
        {
            this.FunctionList = new List<Func<T>>();
            this.isFinished = false;
        }

        //执行
        public IEnumerator Execute()
        {
            List<IEnumerator> enumerators = new List<IEnumerator>();

            foreach (Func<T> func in this.FunctionList)
            {
                foreach (IEnumerator enumerator in func.Invoke())
                {
                    enumerators.Add(enumerator);
                }
            }
            
            for (int i = 0; i < enumerators.Count; i++)
            {
                yield return enumerators[i];
            }
            yield return null;
            this.onAllFunctionsCompleted.Invoke();
            this.isFinished = true;
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
            if(this.onAllFunctionsCompleted != null)
            {
                this.onAllFunctionsCompleted += action;
            }
            else { this.onAllFunctionsCompleted = action; }

        }
    }
}

