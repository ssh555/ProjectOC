using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting;
namespace ML.Engine.UI
{
    public class FunctionExecutor<T> where T : List<AsyncOperationHandle>
    {
        public List<Func<T>> FunctionList = new List<Func<T>>();
        private Action onAllFunctionsCompleted; // 所有函数执行完毕后的回调函数

        // 构造函数
        public FunctionExecutor(List<Func<T>> FunctionList, Action onAllFunctionsCompleted)
        {
            this.FunctionList = FunctionList;
            this.onAllFunctionsCompleted = onAllFunctionsCompleted;
        }

        public FunctionExecutor()
        {
            this.FunctionList = new List<Func<T>>();
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

