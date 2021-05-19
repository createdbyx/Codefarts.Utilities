// --------------------------------------------------------------------------------------------------------------------
// <copyright company="" file="ObservableDictionary.cs">
//   
// </copyright>
// <summary>
//   Provides a generic observable dictionary implementation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace System.Collections.ObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// Provides a generic observable dictionary implementation.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type used as the key.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type used as the value.
    /// </typeparam>   
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged, IDictionary
    {
        /// <summary>
        /// Holds the name of the <see cref="Count"/> property.
        /// </summary>
        private const string CountString = "Count";

        /// <summary>
        /// Holds the name of the <see cref="this"/> property.
        /// </summary>
        private const string IndexerName = "Item[]";

        /// <summary>
        /// Holds the name of the <see cref="Keys"/> property.
        /// </summary>
        private const string KeysName = "Keys";

        /// <summary>
        /// Holds the name of the <see cref="Values"/> property.
        /// </summary>
        private const string ValuesName = "Values";

        /// <summary>
        /// Holds the dictionary keys.
        /// </summary>
        private readonly List<TKey> keysList = new List<TKey>();

        /// <summary>
        /// Holds the dictionary values.
        /// </summary>
        private readonly List<TValue> valuesList = new List<TValue>();

        /// <summary>
        /// Gets an <see cref="System.Collections.ICollection"/> object containing the keys of the <see cref="System.Collections.IDictionary"/> object.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.ICollection"/> object containing the keys of the <see cref="System.Collections.IDictionary"/> object.
        /// </returns>
        ICollection IDictionary.Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets an <see cref="System.Collections.ICollection"/> object containing the values in the <see cref="System.Collections.IDictionary"/> object.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.ICollection"/> object containing the values in the <see cref="System.Collections.IDictionary"/> object.
        /// </returns>
        ICollection IDictionary.Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                return new ReadOnlyCollection<TKey>(this.keysList);
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                return new ReadOnlyCollection<TValue>(this.valuesList);
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key. 
        /// </summary>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        /// <returns>
        /// The element with the specified key.
        /// </returns>
        public TValue this[TKey key]
        {
            get
            {
                return this.valuesList[this.keysList.IndexOf(key)];
            }

            set
            {
                this.Insert(key, value, false);
            }
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="IDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">
        /// The object to use as the key of the element to add.
        /// </param>
        /// <param name="value">
        /// The object to use as the value of the element to add.
        /// </param>
        public void Add(TKey key, TValue value)
        {
            this.Insert(key, value, true);
        }

        /// <summary>
        /// Determines whether the <see cref="IDictionary{TKey,TValue}"/> contains an element with the specified key.
        /// </summary>
        /// <param name="key">
        /// The key to locate in the <see cref="IDictionary{TKey,TValue}"/>.
        /// </param>
        /// <returns>
        /// true if the <see cref="IDictionary{TKey,TValue}"/> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            return this.keysList.Contains(key);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>. (Inherited from <see cref="ICollection{T}"/>.)
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ICollection{T}"/>.
        /// </param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="ICollection{T}"/>; otherwise, false. This method also returns false if item is not found in the original <see cref="ICollection{T}"/>.
        /// </returns>
        public bool Remove(TKey item)
        {
            var index = this.keysList.IndexOf(item);
            if (index == -1)
            {
                throw new IndexOutOfRangeException("key does not exist in the dictionary.");
            }

            var value = this.valuesList[index];
            this.valuesList.RemoveAt(index);
            this.keysList.Remove(item);

            var removed = this.keysList.Remove(item);
            if (removed)
            {
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(item, value), index);
            }

            return removed;
        }

        /// <summary>
        /// Gets the value associated with the specified key. 
        /// </summary>
        /// <param name="key">
        /// The key whose value to get.
        /// </param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// true if the object that implements <see cref="IDictionary{TKey,TValue}"/> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = this.keysList.IndexOf(key);
            if (index == -1)
            {
                value = default(TValue);
                return false;
            }

            value = this.valuesList[index];
            return true;
        }

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see cref="IDictionary{TKey,TValue}"/>. 
        /// </summary>
        public int Count
        {
            get
            {
                return this.keysList.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        bool ICollection.IsSynchronized
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="System.Collections.ICollection"/>.
        /// </returns>
        object ICollection.SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether is read only.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="System.Collections.IDictionary"/> object has a fixed size.
        /// </summary>
        /// <returns>
        /// true if the <see cref="System.Collections.IDictionary"/> object has a fixed size; otherwise, false.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        bool IDictionary.IsFixedSize
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a value indicating whether is read only.
        /// </summary>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        bool IDictionary.IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <returns>
        /// The element with the specified key, or null if the key does not exist.
        /// </returns>
        /// <param name="key">
        /// The key of the element to get or set. 
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="key"/> is null. 
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The property is set and the <see cref="System.Collections.IDictionary"/> object is read-only.-or- The property is set, <paramref name="key"/> does not exist in the collection, and the <see cref="System.Collections.IDictionary"/> has a fixed size. 
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        object IDictionary.this[object key]
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Removes all keys and values from the <see cref="IDictionary{TKey,TValue}"/>. 
        /// </summary>
        public void Clear()
        {
            if (this.keysList.Count > 0)
            {
                this.keysList.Clear();
                this.valuesList.Clear();
                this.OnCollectionChanged();
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="System.Collections.ICollection"/> to an <see cref="System.Array"/>, starting at a particular <see cref="System.Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="System.Array"/> that is the destination of the elements copied from <see cref="System.Collections.ICollection"/>. The <see cref="System.Array"/> must have zero-based indexing. 
        /// </param>
        /// <param name="index">
        /// The zero-based index in <paramref name="array"/> at which copying begins. 
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="array"/> is null. 
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. 
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// <paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>.-or-The type of the source <see cref="System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.
        /// </exception>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The add.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The contains.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The copy to.
        /// </summary>
        /// <param name="array">
        /// The array.
        /// </param>
        /// <param name="arrayIndex">
        /// The array index.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The remove.
        /// </summary>
        /// <param name="item">
        /// The item.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// Not implemented.
        /// </exception>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="System.Collections.IDictionary"/> object.
        /// </summary>
        /// <param name="key">
        /// The <see cref="System.Object"/> to use as the key of the element to add. 
        /// </param>
        /// <param name="value">
        /// The <see cref="System.Object"/> to use as the value of the element to add. 
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="key"/> is null. 
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// An element with the same key already exists in the <see cref="System.Collections.IDictionary"/> object. 
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="System.Collections.IDictionary"/> is read-only.-or- The <see cref="System.Collections.IDictionary"/> has a fixed size. 
        /// </exception>
        void IDictionary.Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the <see cref="System.Collections.IDictionary"/> object contains an element with the specified key.
        /// </summary>
        /// <returns>
        /// true if the <see cref="System.Collections.IDictionary"/> contains an element with the key; otherwise, false.
        /// </returns>
        /// <param name="key">
        /// The key to locate in the <see cref="System.Collections.IDictionary"/> object.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="key"/> is null. 
        /// </exception>
        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an <see cref="System.Collections.IDictionaryEnumerator"/> object for the <see cref="System.Collections.IDictionary"/> object.
        /// </summary>
        /// <returns>
        /// An <see cref="System.Collections.IDictionaryEnumerator"/> object for the <see cref="System.Collections.IDictionary"/> object.
        /// </returns>
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="System.Collections.IDictionary"/> object.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove. 
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// <paramref name="key"/> is null. 
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        /// The <see cref="System.Collections.IDictionary"/> object is read-only.-or- The <see cref="System.Collections.IDictionary"/> has a fixed size. 
        /// </exception>
        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="IDictionary{TKey,TValue}"/>. 
        /// </summary>
        /// <returns>A <see cref="IDictionary{TKey,TValue}"/>.Enumerator structure for the <see cref="IDictionary{TKey,TValue}"/>.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var key in this.keysList)
            {
                yield return new KeyValuePair<TKey, TValue>(key, this.valuesList[this.keysList.IndexOf(key)]);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="IDictionary{TKey,TValue}"/>. 
        /// </summary>
        /// <returns>A <see cref="IDictionary{TKey,TValue}"/>.Enumerator structure for the <see cref="IDictionary{TKey,TValue}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (var key in this.keysList)
            {
                yield return new KeyValuePair<TKey, TValue>(key, this.valuesList[this.keysList.IndexOf(key)]);
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that changed.
        /// </param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Adds or inserts a value.
        /// </summary>
        /// <param name="key">
        /// The access key
        /// </param>
        /// <param name="value">
        /// The item to be inserted.
        /// </param>
        /// <param name="add">
        /// true to perform a add; otherwise false to replace an existing item.
        /// </param>
        private void Insert(TKey key, TValue value, bool add)
        {
            if (!(key is ValueType) && key == null)
            {
                throw new ArgumentNullException("key");
            }

            TValue item;
            if (this.TryGetValue(key, out item))
            {
                if (add)
                {
                    throw new ArgumentException("An item with the same key has already been added.");
                }

                // ReSharper disable once RedundantNameQualifier
                if (object.Equals(item, value))
                {
                    return;
                }

                var index = this.keysList.IndexOf(key);
                this.valuesList[index] = value;

                this.OnCollectionChanged(
                    NotifyCollectionChangedAction.Replace,
                    new KeyValuePair<TKey, TValue>(key, value),
                    new KeyValuePair<TKey, TValue>(key, item),
                    index);
            }
            else
            {
                this.keysList.Add(key);
                this.valuesList.Add(value);

                this.OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value), this.keysList.Count - 1);
            }
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        private void OnCollectionChanged()
        {
            this.OnPropertyChanged();
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="action">
        /// The action type that took place.
        /// </param>
        /// <param name="changedItem">
        /// The item that was changed.
        /// </param>
        /// <param name="index">
        /// The index where the change takes place.
        /// </param>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem, int index)
        {
            this.OnPropertyChanged();
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(action, changedItem, index));
            }
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="action">
        /// The action type that took place.
        /// </param>
        /// <param name="newItem">
        /// The new item that was added.
        /// </param>
        /// <param name="oldItem">
        /// The old item that was replace.
        /// </param>
        /// <param name="index">
        /// The index where the change takes place.
        /// </param>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem,
            int index)
        {
            this.OnPropertyChanged();
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
            }
        }

        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="action">
        /// The action type that took place.
        /// </param>
        /// <param name="changedItems">
        /// A list of items that are part of the change.
        /// </param>
        /// <param name="index">
        /// The index where the change takes place.
        /// </param>
        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList changedItems, int index)
        {
            this.OnPropertyChanged();
            var handler = this.CollectionChanged;
            if (handler != null)
            {
                handler(this, new NotifyCollectionChangedEventArgs(action, changedItems, index));
            }
        }

        /// <summary>
        /// Raises <see cref="PropertyChanged"/> events for each property.
        /// </summary>
        private void OnPropertyChanged()
        {
            this.OnPropertyChanged(CountString);
            this.OnPropertyChanged(IndexerName);
            this.OnPropertyChanged(KeysName);
            this.OnPropertyChanged(ValuesName);
        }
    }
}