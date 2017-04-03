using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace GATUtils.Types
{
    public class ThreadSafeQueue<K, T> : IEnumerable<T>
    {
        public ThreadSafeQueue()
        {
            locker = new object();
            this.queues = new Dictionary<K, Queue<T>>();
            this.enumeratedCollection = new Queue<T>();
        }

        #region Public Methods
        // Public Methods
        // --------------

        public void Add(K key, T value)
        {
            lock (locker)
            {
                if (!queues.ContainsKey(key))
                    queues.Add(key, new Queue<T>());
                queues[key].Enqueue(value);
            }
        }

        public bool IsEmpty()
        {
            lock (locker)
            {
                if (enumeratedCollection.Count > 0)
                    return false;

                foreach (Queue<T> q in queues.Values)
                {
                    if (q.Count > 0)
                        return false;
                }
                return true;
            }
        }

        public int Count()
        {     
            lock (locker)
            {
                int countSum = enumeratedCollection.Count;

                foreach (Queue<T> q in queues.Values)
                {
                    if (q.Count > 0)
                        countSum += q.Count;
                }
                return countSum;
            } 
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (locker) // session can be removed from list during for each
            {
                extractCurrentQueuesToEnumeratedQueue();
            }

            while (enumeratedCollection.Count > 0)
                yield return enumeratedCollection.Dequeue();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Private 

        /// <summary>
        /// Extracts the current queues to enumerated queue.
        /// </summary>
        private void extractCurrentQueuesToEnumeratedQueue()
        {
            lock (locker)
            {
                int notEmpty;
                do
                {
                    notEmpty = queues.Keys.Count;
                    foreach (var key in queues.Keys)
                    {
                        if (queues[key].Count > 0)
                            this.enumeratedCollection.Enqueue(queues[key].Dequeue());
                        else
                            notEmpty--;
                    }
                } while (notEmpty > 0);
            }
        }

        private readonly Queue<T> enumeratedCollection;
        private readonly Dictionary<K, Queue<T>> queues;
        private object locker;
        #endregion
    }
}
