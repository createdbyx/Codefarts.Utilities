// <copyright file="SyncronizedQueryable.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

namespace Codefarts.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Linq.Expressions;

    public class SyncronizedQueryable<T> : IQueryable<T>, IDisposable, INotifyCollectionChanged
    {
        private IQueryable<T> wrappedCollection;
        [NonSerialized]
        private int _blockReentrancyCount;
        private SyncronizedQueryable<T>.SimpleMonitor _monitor;

        public SyncronizedQueryable(IQueryable<T> wrappedCollection)
        {
            this.wrappedCollection = wrappedCollection ?? throw new ArgumentNullException(nameof(wrappedCollection));
            if (wrappedCollection is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += this.WrappedCollectionChanged;
            }
        }

        private void WrappedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.CheckReentrancy();
            this.DoCollectionChanged(e);
        }

        private SyncronizedQueryable<T>.SimpleMonitor EnsureMonitorInitialized()
        {
            return this._monitor ?? (this._monitor = new SyncronizedQueryable<T>.SimpleMonitor(this));
        }

        //protected IDisposable BlockReentrancy()
        //{
        //    ++this._blockReentrancyCount;
        //    return (IDisposable)this.EnsureMonitorInitialized();
        //}

        protected void CheckReentrancy()
        {
            if (this._blockReentrancyCount <= 0)
            {
                return;
            }

            var collectionChanged = this.CollectionChanged;
            if ((collectionChanged != null ? (collectionChanged.GetInvocationList().Length > 1 ? 1 : 0) : 0) != 0)
            {
                throw new InvalidOperationException("SyncronizedCollection reentrancy not allowed!");
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        //public bool Contains(T item)
        //{
        //    return this.wrappedCollection.Contains(item);
        //}

        //public int Count
        //{
        //    get
        //    {
        //        return this.wrappedCollection.Count();
        //    }
        //}

        public IEnumerator<T> GetEnumerator()
        {
            return this.wrappedCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.wrappedCollection.GetEnumerator();
        }

        private void DoCollectionChanged(NotifyCollectionChangedAction action, T item)
        {
            var handler = this.CollectionChanged;
            if (handler != null)
            {
#if PORTABLE
                var args = new NotifyCollectionChangedEventArgs(action);
#else
                var args = new NotifyCollectionChangedEventArgs(action, item);
#endif
                ++this._blockReentrancyCount;
                try
                {
                    handler.Invoke(this, args);
                }
                finally
                {
                    --this._blockReentrancyCount;
                }
            }
        }

        private void DoCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }

        public void Dispose()
        {
            this.wrappedCollection = null;
        }

        [Serializable]
        private sealed class SimpleMonitor : IDisposable
        {
            internal int _busyCount;
            [NonSerialized]
            internal SyncronizedQueryable<T> _collection;

            public SimpleMonitor(SyncronizedQueryable<T> collection)
            {
                this._collection = collection;
            }

            public void Dispose()
            {
                --this._collection._blockReentrancyCount;
            }
        }

        public Type ElementType
        {
            get
            {
                return this.wrappedCollection.ElementType;
            }
        }

        public Expression Expression
        {
            get
            {
                return this.wrappedCollection.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get
            {
                return this.Provider;
            }
        }
    }
}