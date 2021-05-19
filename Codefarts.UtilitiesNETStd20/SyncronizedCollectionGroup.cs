// <copyright file="SyncronizedCollectionGroup.cs" company="Codefarts">
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

    public class SyncronizedCollectionGroup<T> : ICollection<T>, IDisposable, INotifyCollectionChanged
    {
        private IList<SyncronizedCollection<T>> collectionList;

        public IList<SyncronizedCollection<T>> CollectionList
        {
            get
            {
                return this.collectionList;
            }
        }

        public SyncronizedCollectionGroup(IEnumerable<SyncronizedCollection<T>> collections)
        {
            this.collectionList = new List<SyncronizedCollection<T>>(collections ?? throw new ArgumentNullException(nameof(collections)));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            foreach (var col in this.collectionList)
            {
                col.Add(item);
                var change = item as INotifyCollectionChanged;
                if (change != null)
                {
                    change.CollectionChanged += this.Change_CollectionChanged;
                }
            }

            this.DoCollectionChanged(NotifyCollectionChangedAction.Add, item);
        }

        private void Change_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var col in this.collectionList)
            {
                if (col == sender)
                {
                    continue;
                }

                var newItems = e.NewItems.OfType<T>();
                var oldItems = e.OldItems.OfType<T>();
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var item in newItems)
                        {
                            col.Add(item);
                        }
                        break;

#if !PORTABLE
                    case NotifyCollectionChangedAction.Move:
                        var i = 0;
                        foreach (var item in newItems)
                        {
                            var list = col as IList;
                            if (list != null)
                            {
                                list.RemoveAt(e.OldStartingIndex);
                                list.Insert(e.NewStartingIndex + i++, item);
                            }
                        }

                        break;
#endif

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var item in oldItems)
                        {
                            col.Remove(item);
                        }

                        break;

                    case NotifyCollectionChangedAction.Replace:
                        foreach (var item in newItems)
                        {
                            col.Add(item);
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                        col.Clear();
                        break;
                }
            }

            this.DoCollectionChanged(e);
        }

        public void Clear()
        {
            foreach (var col in this.collectionList)
            {
                col.Clear();
            }

            this.DoCollectionChanged(NotifyCollectionChangedAction.Reset, default);
        }

        public bool Contains(T item)
        {
            if (this.collectionList.Count == 0)
            {
                return false;
            }

            var first = this.collectionList[0];
            return first != null ? first.Contains(item) : false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (this.collectionList.Count == 0)
            {
                return;
            }

            var first = this.collectionList[0];
            first.CopyTo(array, arrayIndex);
        }

        public bool IsOutOfSync()
        {
            //TODO: the count of wach collection in the group could be the same but the collection values could be different
            return this.IsCountOutOfSyncInternal();
        }

        private bool IsCountOutOfSyncInternal()
        {
            if (this.collectionList.Count == 0)
            {
                return false;
            }

            var first = this.collectionList[0];
            var firstValue = first != null ? first.Count : 0;
            for (var i = 1; i < this.collectionList.Count; i++)
            {
                var x = this.collectionList[i];
                if (x.Count != firstValue)
                {
                    return true;
                }
            }

            return false;
        }

        public int Count
        {
            get
            {
                if (this.collectionList.Count == 0)
                {
                    return 0;
                }

                var first = this.collectionList[0];
                var firstValue = first != null ? first.Count : 0;
                for (var i = 1; i < this.collectionList.Count; i++)
                {
                    var x = this.collectionList[i];
                    if (x.Count != firstValue)
                    {
                        throw new CollectionsOutOfSyncException();
                    }
                }

                return firstValue;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (this.collectionList.Count == 0)
                {
                    return false;
                }

                var first = this.collectionList[0];
                var firstValue = first.IsReadOnly;
                for (var i = 1; i < this.collectionList.Count; i++)
                {
                    var x = this.collectionList[i];
                    if (x.IsReadOnly != firstValue)
                    {
                        throw new CollectionsOutOfSyncException();
                    }
                }

                return firstValue;
            }
        }

        public bool Remove(T item)
        {
            var wasRemoved = false;
            foreach (var col in this.collectionList)
            {
                col.Remove(item);
                wasRemoved = true;
            }

            this.DoCollectionChanged(NotifyCollectionChangedAction.Remove, item);
            return wasRemoved;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var first = this.collectionList.FirstOrDefault();
            return first != null ? first.GetEnumerator() : null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var first = this.collectionList.FirstOrDefault();
            return first != null ? first.GetEnumerator() : null;
        }

        private void DoCollectionChanged(NotifyCollectionChangedAction action, T item)
        {
            var handler = this.CollectionChanged;
            if (handler != null)
            {
#if PORTABLE
                var args = new NotifyCollectionChangedEventArgs(action);
                //var args = default(NotifyCollectionChangedEventArgs);
                //switch (action)
                //{
                //    case NotifyCollectionChangedAction.Add:
                //        args = new NotifyCollectionChangedEventArgs(action, item, this.Count);
                //        break;

                //    case NotifyCollectionChangedAction.Remove:
                //        break;

                //    case NotifyCollectionChangedAction.Replace:
                //        break;
                //    case NotifyCollectionChangedAction.Reset:
                //        break;
                //    default:
                //        throw new ArgumentOutOfRangeException(nameof(action));
                //}
#else
                var args = new NotifyCollectionChangedEventArgs(action, item);
#endif
                handler.Invoke(this, args);
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
            this.collectionList = null;
        }
    }
}