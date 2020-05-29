using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Intersoft.CISSA.DataAccessLayer.Cache
{
    public class ObjectCacheItem<TKey, TObject>
    {
        private Int64 created;

        public DateTime Created
        {
            get { return new DateTime(Interlocked.Read(ref created)); } 
            private set { Interlocked.Exchange(ref created, value.Ticks); }
        }
        public TKey Key { get; private set; }
        public TObject CachedObject { get; private set; }

        public ObjectCacheItem(TKey key, TObject obj)
        {
            Created = DateTime.Now;
            Key = key;
            CachedObject = obj;
        }

        public void Used()
        {
            Interlocked.Exchange(ref created, DateTime.Now.Ticks);
        }
    }

    public class ObjectCache<T> 
    {
        private readonly SortedList<Guid, ObjectCacheItem<Guid, T>> _items = new SortedList<Guid, ObjectCacheItem<Guid, T>>();
        //private readonly object _lockObject = new object();
        private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
        private const int LockTimeout = 500000;
        private readonly int _timeOut = 0;
        private readonly int _limit = 0;
        //public object Lock { get { return _lockObject; } }

        public ObjectCache(int timeOut = 0, int limit = 0)
        {
            _timeOut = timeOut;
            _limit = 0;
        }

        public void Add(T obj, Guid id)
        {
            // lock(_lockObject)
            _rwLock.AcquireWriterLock(LockTimeout);
            try
            {
                _items.Remove(id);
                var item = new ObjectCacheItem<Guid, T>(id, obj);
                if (_limit > 0 && _items.Count > _limit)
                    LimitTo(_limit - 1);
                _items.Add(id, item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public ObjectCacheItem<Guid, T> Find(Guid id)
        {
            //lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                ObjectCacheItem<Guid, T> item;
                if (_items.TryGetValue(id, out item))
                {
                    if (_timeOut > 0 && (DateTime.Now - item.Created).TotalSeconds >= _timeOut)
                        return null;
                    item.Used();
                    return item;
                }
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
            return null;
        }

        public bool Remove(Guid id)
        {
            // lock (_lockObject)
            _rwLock.AcquireWriterLock(LockTimeout);
            try
            {
                return _items.Remove(id);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void AddOrSet(T obj, Guid id)
        {
            Remove(id);
            Add(obj, id);
        }

        public void Clear()
        {
            // lock (_lockObject)
            _rwLock.AcquireWriterLock(LockTimeout);
            try
            {
                _items.Clear();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        protected IEnumerable<ObjectCacheItem<Guid, T>> GetItems()
        {
            if (_timeOut <= 0) return _items.Values;

            return _items.Values.Where(item => (DateTime.Now - item.Created).TotalSeconds < _timeOut);
        }

        public ObjectCacheItem<Guid, T> FirstOrDefault(Func<ObjectCacheItem<Guid, T>, bool> predicate)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                return GetItems().FirstOrDefault(predicate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        public ObjectCacheItem<Guid, T> LastOrDefault(Func<ObjectCacheItem<Guid, T>, bool> predicate)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                return GetItems().LastOrDefault(predicate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        public IEnumerable<ObjectCacheItem<Guid, T>> Where(Func<ObjectCacheItem<Guid, T>, bool> predicate)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                return GetItems().Where(predicate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        protected void LimitTo(int limit)
        {
            var list = new SortedList<DateTime, ObjectCacheItem<Guid, T>>();
            var existList = new SortedList<Guid, ObjectCacheItem<Guid, T>>();

            foreach (var item in _items.Values)
            {
                list.Add(item.Created, item);
            }

            int i = 0;
            foreach (var item in list.Reverse())
            {
                if (i > limit) break;
                existList.Add(item.Value.Key, item.Value);
                i++;
            }
            _items.Clear();
            _items.Union(existList);
        }

        public int Count 
        {
            get
            {
                //lock (_lockObject)
                _rwLock.AcquireReaderLock(LockTimeout);
                try
                {
                    return _items.Count;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }
    }

    public class ObjectKeyCache<TKey, TObject>
    {
        private readonly SortedList<TKey, ObjectCacheItem<TKey, TObject>> _items =
            new SortedList<TKey, ObjectCacheItem<TKey, TObject>>();

        // private readonly object _lockObject = new object();
        private readonly ReaderWriterLock _rwLock = new ReaderWriterLock();
        private const int LockTimeout = 500000;
        private readonly int _timeOut = 0;
        private readonly int _limit = 0;

        // public object Lock { get { return _lockObject; } }

        public ObjectKeyCache(int timeOut = 0, int limit = 0)
        {
            _timeOut = timeOut;
            _limit = 0;
        }

        public void Add(TObject obj, TKey id)
        {
            var item = new ObjectCacheItem<TKey, TObject>(id, obj);
            // lock (_lockObject)
            _rwLock.AcquireWriterLock(LockTimeout);
            try
            {
                _items.Remove(id);
                if (_limit > 0 && _items.Count > _limit)
                    LimitTo(_limit - 1);
                _items.Add(id, item);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public ObjectCacheItem<TKey, TObject> Find(TKey id)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                ObjectCacheItem<TKey, TObject> item;
                if (_items.TryGetValue(id, out item))
                {
                    if (_timeOut > 0 && (DateTime.Now - item.Created).TotalSeconds >= _timeOut)
                        return null;
                    item.Used();
                    return item;
                }
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
            return null;
        }

        public bool Remove(TKey id)
        {
            // lock (_lockObject)
            _rwLock.AcquireWriterLock(LockTimeout);
            try
            {
                return _items.Remove(id);
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        public void AddOrSet(TObject obj, TKey id)
        {
            Remove(id);
            Add(obj, id);
        }

        protected void LimitTo(int limit)
        {
            var list = new SortedList<DateTime, ObjectCacheItem<TKey, TObject>>();
            var existList = new SortedList<TKey, ObjectCacheItem<TKey, TObject>>();
            
            foreach (var item in _items.Values)
            {
                list.Add(item.Created, item);
            }

            int i = 0;
            foreach (var item in list.Reverse())
            {
                if (i > limit) break;
                existList.Add(item.Value.Key, item.Value);
                i++;
            }
            _items.Clear();
            _items.Union(existList);
        }

        public void Clear()
        {
            // lock (_lockObject)
            _rwLock.AcquireWriterLock(LockTimeout);
            try
            {
                _items.Clear();
            }
            finally
            {
                _rwLock.ReleaseWriterLock();
            }
        }

        protected IEnumerable<ObjectCacheItem<TKey, TObject>> GetItems()
        {
            if (_timeOut <= 0) return _items.Values;

            return _items.Values.Where(item => (DateTime.Now - item.Created).TotalSeconds < _timeOut);
        }

        public ObjectCacheItem<TKey, TObject> FirstOrDefault(Func<ObjectCacheItem<TKey, TObject>, bool> predicate)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                return GetItems().FirstOrDefault(predicate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        public ObjectCacheItem<TKey, TObject> LastOrDefault(Func<ObjectCacheItem<TKey, TObject>, bool> predicate)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                return GetItems().LastOrDefault(predicate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }

        public IEnumerable<ObjectCacheItem<TKey, TObject>> Where(Func<ObjectCacheItem<TKey, TObject>, bool> predicate)
        {
            // lock (_lockObject)
            _rwLock.AcquireReaderLock(LockTimeout);
            try
            {
                return GetItems().Where(predicate);
            }
            finally
            {
                _rwLock.ReleaseReaderLock();
            }
        }
        public int Count
        {
            get
            {
                // lock (_lockObject)
                _rwLock.AcquireReaderLock(LockTimeout);
                try
                {
                    return _items.Count;
                }
                finally
                {
                    _rwLock.ReleaseReaderLock();
                }
            }
        }
    }

    public static class ObjectCacheContext
    {
        
    }
}
