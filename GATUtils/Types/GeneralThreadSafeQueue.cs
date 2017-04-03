using System.Collections.Generic;

namespace GATUtils.Types
{
    public class GeneralThreadSafeQueue<T>
    {
        public GeneralThreadSafeQueue()
        {
            _queue = new Queue<T>();
            _locker = new object();
        }

        public void Enqueue(T item)
        {
            lock (_locker)
            {
                _queue.Enqueue(item);
            }
        }

        public T Dequeue()
        {
            lock (_locker)
            {
                return _queue.Dequeue();
            }
        }

        /// <summary>
        /// Tries to dequeue safely from the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>True if dequeue succeeded</returns>
        public bool TryDequeue(out T item)
        {
            item = default(T);
            lock (_locker)
            {                
                if (_queue.Count == 0) return false;
                item = _queue.Dequeue();
                return true;
            }
        }

        /// <summary>
        /// Tries to dequeue safely from the queue, and gets the queue count.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="count">The count.</param>
        /// <returns>True if dequeue succeeded</returns>
        public bool TryDequeueWithCount(out T item, out int count)
        {
            item = default(T);
            lock (_locker)
            {
                count = _queue.Count;
                if (count == 0) return false;
                item = _queue.Dequeue();
                return true;
            }
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _queue.Count;
                }
            }
        }

        private readonly Queue<T> _queue;
        private readonly object _locker;
    }
}
