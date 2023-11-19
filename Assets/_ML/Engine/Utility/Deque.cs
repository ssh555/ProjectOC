using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Engine
{
    /// <summary>
    /// Ë«Ïò¶ÓÁÐ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Deque<T>
    {
        private LinkedList<T> list = new LinkedList<T>();

        public void EnqueueFront(T item)
        {
            list.AddFirst(item);
        }

        public void EnqueueBack(T item)
        {
            list.AddLast(item);
        }

        public T DequeueFront()
        {
            if (list.Count == 0)
            {
                throw new System.InvalidOperationException("Deque is empty");
            }
            T value = list.First.Value;
            list.RemoveFirst();
            return value;
        }

        public T DequeueBack()
        {
            if (list.Count == 0)
            {
                throw new System.InvalidOperationException("Deque is empty");
            }
            T value = list.Last.Value;
            list.RemoveLast();
            return value;
        }

        public T PeekFront()
        {
            if (list.Count == 0)
            {
                throw new System.InvalidOperationException("Deque is empty");
            }
            return list.First.Value;
        }

        public T PeekBack()
        {
            if (list.Count == 0)
            {
                throw new System.InvalidOperationException("Deque is empty");
            }
            return list.Last.Value;
        }

        public int Count
        {
            get { return list.Count; }
        }
    }

}
