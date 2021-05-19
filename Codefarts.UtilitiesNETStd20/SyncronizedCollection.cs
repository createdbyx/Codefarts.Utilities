// <copyright file="SyncronizedCollection.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

using System.Linq;

namespace Codefarts.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    public class SyncronizedCollection<T> : IList<T>, IList, IDisposable, INotifyCollectionChanged
    {
        private IList<T> wrappedCollection;
#if !PORTABLE
        [NonSerialized]
#endif
        private int _blockReentrancyCount;
        private SimpleMonitor _monitor;

        public SyncronizedCollection(IList<T> wrappedCollection)
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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private SimpleMonitor EnsureMonitorInitialized()
        {
            return this._monitor ?? (this._monitor = new SimpleMonitor(this));
        }

        protected IDisposable BlockReentrancy()
        {
            ++this._blockReentrancyCount;
            return (IDisposable)this.EnsureMonitorInitialized();
        }

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

        public void Add(T item)
        {
            this.CheckReentrancy();
            this.wrappedCollection.Add(item);
            this.DoCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        public int IndexOf(T item)
        {
            return this.wrappedCollection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.wrappedCollection.Insert(index, item);
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            this.wrappedCollection.RemoveAt(index);
        }

        public bool IsFixedSize { get; }

        /// <summary>Gets or sets the element at the specified index.</summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index" /> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1" />.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1" /> is read-only.</exception>
        public T this[int index]
        {
            get
            {
                return this.wrappedCollection[index];
            }

            set
            {
                this.CheckReentrancy();
                this.wrappedCollection[index] = value;
                this.DoCollectionChanged(NotifyCollectionChangedAction.Replace, value);
            }
        }

        public int Add(object value)
        {
            var castedValue = (T)value;
            this.CheckReentrancy();
            this.wrappedCollection.Add(castedValue);
            this.DoCollectionChanged(NotifyCollectionChangedAction.Add, castedValue);
            return this.wrappedCollection.IndexOf(castedValue);
        }

        public void Clear()
        {
            this.CheckReentrancy();
            this.wrappedCollection.Clear();
            this.DoCollectionChanged(NotifyCollectionChangedAction.Reset, default(T));
        }

        public bool Contains(object value)
        {
            return this.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        public bool Contains(T item)
        {
            return this.wrappedCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.wrappedCollection.CopyTo(array, arrayIndex);
        }

        public void CopyTo(Array array, int index)
        {
            this.wrappedCollection.CopyTo(array.Cast<T>().ToArray(), index);
        }

        public int Count
        {
            get
            {
                return this.wrappedCollection.Count;
            }
        }

        public bool IsSynchronized { get; }

        public object SyncRoot { get; }

        public bool IsReadOnly
        {
            get
            {
                return this.wrappedCollection.IsReadOnly;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this.wrappedCollection[index];
            }

            set
            {
                this.wrappedCollection[index] = (T)value;
            }
        }

        public bool Remove(T item)
        {
            this.CheckReentrancy();
            if (this.wrappedCollection.Remove(item))
            {
                this.DoCollectionChanged(NotifyCollectionChangedAction.Remove, item);
                return true;
            }

            return false;
        }

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

#if !PORTABLE
        [Serializable]
#endif
        private sealed class SimpleMonitor : IDisposable
        {
            internal int _busyCount;
#if !PORTABLE
            [NonSerialized]
#endif
            internal SyncronizedCollection<T> _collection;

            public SimpleMonitor(SyncronizedCollection<T> collection)
            {
                this._collection = collection;
            }

            public void Dispose()
            {
                --this._collection._blockReentrancyCount;
            }
        }
    }
}