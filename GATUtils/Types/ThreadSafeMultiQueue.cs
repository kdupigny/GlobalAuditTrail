using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace GATUtils.Types
{
    public class ThreadSafeMultiQueue<K, T> : IEnumerable<T>
    {
        public ThreadSafeMultiQueue()
        {
            _locker = new object();
            _queues = new Dictionary<K, Queue<T>>();
            _enumeratedCollection = new Queue<T>();
        }

        #region Public Methods
        // Public Methods
        // --------------

        public void Add(K key, T value)
        {
            lock (_locker)
            {
                if (!_queues.ContainsKey(key))
                    _queues.Add(key, new Queue<T>());
                _queues[key].Enqueue(value);
            }
        }

        public bool IsEmpty()
        {
            lock (_locker)
            {
                if (_enumeratedCollection.Count > 0)
                    return false;

                return _queues.Values.All(q => q.Count <= 0);
            }
        }

        public int Count()
        {     
            lock (_locker)
            {
                return _enumeratedCollection.Count + _queues.Values.Where(q => q.Count > 0).Sum(q => q.Count);
            } 
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_locker) // session can be removed from list during for each
            {
                _ExtractCurrentQueuesToEnumeratedQueue();
            }

            while (_enumeratedCollection.Count > 0)
                yield return _enumeratedCollection.Dequeue();
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
        private void _ExtractCurrentQueuesToEnumeratedQueue()
        {
            lock (_locker)
            {
                int notEmpty;
                do
                {
                    notEmpty = _queues.Keys.Count;
                    foreach (var key in _queues.Keys)
                    {
                        if (_queues[key].Count > 0)
                            _enumeratedCollection.Enqueue(_queues[key].Dequeue());
                        else
                            notEmpty--;
                    }
                } while (notEmpty > 0);
            }
        }

        private readonly Queue<T> _enumeratedCollection;
        private readonly Dictionary<K, Queue<T>> _queues;
        private object _locker;
        #endregion
    }
}
