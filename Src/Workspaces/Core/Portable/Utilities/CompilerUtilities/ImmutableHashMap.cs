﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Roslyn.Utilities;
using Contract = System.Diagnostics.Contracts.Contract;

namespace Roslyn.Collections.Immutable
{
    /// <summary>
    /// An immutable unordered hash map implementation.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableHashMap<,>.DebuggerProxy))]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    internal sealed class ImmutableHashMap<TKey, TValue> : IImmutableDictionary<TKey, TValue>
    {
        private static readonly ImmutableHashMap<TKey, TValue> EmptySingleton = new ImmutableHashMap<TKey, TValue>();

        /// <summary>
        /// The root node of the tree that stores this map.
        /// </summary>
        private readonly Bucket root;

        /// <summary>
        /// The comparer used to sort keys in this map.
        /// </summary>
        private readonly IEqualityComparer<TKey> keyComparer;

        /// <summary>
        /// The comparer used to detect equivalent values in this map.
        /// </summary>
        private readonly IEqualityComparer<TValue> valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="comparer">The comparer.</param>
        /// <param name="valueComparer">The value comparer.</param>
        private ImmutableHashMap(Bucket root, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer)
            : this(comparer, valueComparer)
        {
            Contract.Requires(root != null);
            Contract.Requires(comparer != null);
            Contract.Requires(valueComparer != null);

            this.root = root;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        /// <param name="valueComparer">The value comparer.</param>
        internal ImmutableHashMap(IEqualityComparer<TKey> comparer = null, IEqualityComparer<TValue> valueComparer = null)
        {
            this.keyComparer = comparer ?? EqualityComparer<TKey>.Default;
            this.valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        /// <summary>
        /// Gets an empty map with default equality comparers.
        /// </summary>
        public static ImmutableHashMap<TKey, TValue> Empty
        {
            get { return EmptySingleton; }
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        public ImmutableHashMap<TKey, TValue> Clear()
        {
            return this.IsEmpty ? this : Empty.WithComparers(this.keyComparer, this.valueComparer);
        }

        #region Public methods

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        [Pure]
        public ImmutableHashMap<TKey, TValue> Add(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            Contract.Ensures(Contract.Result<ImmutableHashMap<TKey, TValue>>() != null);
            var vb = new ValueBucket(key, value, this.keyComparer.GetHashCode(key));
            if (this.root == null)
            {
                return this.Wrap(vb);
            }
            else
            {
                return this.Wrap(this.root.Add(0, vb, this.keyComparer, this.valueComparer, false));
            }
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        [Pure]
        public ImmutableHashMap<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            Requires.NotNull(pairs, "pairs");
            Contract.Ensures(Contract.Result<ImmutableHashMap<TKey, TValue>>() != null);

            return this.AddRange(pairs, overwriteOnCollision: false, avoidToHashMap: false);
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        [Pure]
        public ImmutableHashMap<TKey, TValue> SetItem(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs(key, "key");
            Contract.Ensures(Contract.Result<ImmutableHashMap<TKey, TValue>>() != null);
            Contract.Ensures(!Contract.Result<ImmutableHashMap<TKey, TValue>>().IsEmpty);
            var vb = new ValueBucket(key, value, this.keyComparer.GetHashCode(key));
            if (this.root == null)
            {
                return this.Wrap(vb);
            }
            else
            {
                return this.Wrap(this.root.Add(0, vb, this.keyComparer, this.valueComparer, true));
            }
        }

        /// <summary>
        /// Applies a given set of key=value pairs to an immutable dictionary, replacing any conflicting keys in the resulting dictionary.
        /// </summary>
        /// <param name="items">The key=value pairs to set on the map.  Any keys that conflict with existing keys will overwrite the previous values.</param>
        /// <returns>An immutable dictionary.</returns>
        [Pure]
        public ImmutableHashMap<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull(items, "items");
            Contract.Ensures(Contract.Result<ImmutableDictionary<TKey, TValue>>() != null);

            return this.AddRange(items, overwriteOnCollision: true, avoidToHashMap: false);
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        [Pure]
        public ImmutableHashMap<TKey, TValue> Remove(TKey key)
        {
            Requires.NotNullAllowStructs(key, "key");
            Contract.Ensures(Contract.Result<ImmutableHashMap<TKey, TValue>>() != null);
            if (this.root != null)
            {
                return this.Wrap(this.root.Remove(this.keyComparer.GetHashCode(key), key, this.keyComparer));
            }

            return this;
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        [Pure]
        public ImmutableHashMap<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
        {
            Requires.NotNull(keys, "keys");
            Contract.Ensures(Contract.Result<ImmutableHashMap<TKey, TValue>>() != null);
            var map = this.root;
            if (map != null)
            {
                foreach (var key in keys)
                {
                    map = map.Remove(this.keyComparer.GetHashCode(key), key, this.keyComparer);
                    if (map == null)
                    {
                        break;
                    }
                }
            }

            return this.Wrap(map);
        }

        /// <summary>
        /// Returns a hash map that uses the specified key and value comparers and has the same contents as this map.
        /// </summary>
        /// <param name="keyComparer">The key comparer.  A value of <c>null</c> results in using the default equality comparer for the type.</param>
        /// <param name="valueComparer">The value comparer.  A value of <c>null</c> results in using the default equality comparer for the type.</param>
        /// <returns>The hash map with the new comparers.</returns>
        /// <remarks>
        /// In the event that a change in the key equality comparer results in a key collision, an exception is thrown.
        /// </remarks>
        [Pure]
        public ImmutableHashMap<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (keyComparer == null)
            {
                keyComparer = EqualityComparer<TKey>.Default;
            }

            if (valueComparer == null)
            {
                valueComparer = EqualityComparer<TValue>.Default;
            }

            if (this.keyComparer == keyComparer)
            {
                if (this.valueComparer == valueComparer)
                {
                    return this;
                }
                else
                {
                    // When the key comparer is the same but the value comparer is different, we don't need a whole new tree
                    // because the structure of the tree does not depend on the value comparer.
                    // We just need a new root node to store the new value comparer.
                    return new ImmutableHashMap<TKey, TValue>(this.root, this.keyComparer, valueComparer);
                }
            }
            else
            {
                var set = new ImmutableHashMap<TKey, TValue>(keyComparer, valueComparer);
                set = set.AddRange(this, overwriteOnCollision: false, avoidToHashMap: true);
                return set;
            }
        }

        /// <summary>
        /// Returns a hash map that uses the specified key comparer and current value comparer and has the same contents as this map.
        /// </summary>
        /// <param name="keyComparer">The key comparer.  A value of <c>null</c> results in using the default equality comparer for the type.</param>
        /// <returns>The hash map with the new comparers.</returns>
        /// <remarks>
        /// In the event that a change in the key equality comparer results in a key collision, an exception is thrown.
        /// </remarks>
        [Pure]
        public ImmutableHashMap<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer)
        {
            return this.WithComparers(keyComparer, this.valueComparer);
        }

        /// <summary>
        /// Determines whether the ImmutableSortedMap&lt;TKey,TValue&gt;
        /// contains an element with the specified value.
        /// </summary>
        /// <param name="value">
        /// The value to locate in the ImmutableSortedMap&lt;TKey,TValue&gt;.
        /// The value can be null for reference types.
        /// </param>
        /// <returns>
        /// true if the ImmutableSortedMap&lt;TKey,TValue&gt; contains
        /// an element with the specified value; otherwise, false.
        /// </returns>
        [Pure]
        public bool ContainsValue(TValue value)
        {
            return this.Values.Contains(value, this.valueComparer);
        }

        #endregion

        #region IImmutableDictionary<TKey, TValue> Members

        /// <summary>
        /// Gets the number of elements in this collection.
        /// </summary>
        public int Count
        {
            get { return this.root != null ? this.root.Count : 0; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return this.Count == 0; }
        }

        /// <summary>
        /// Gets the keys in the map.
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                if (this.root == null)
                {
                    yield break;
                }

                var stack = new Stack<IEnumerator<Bucket>>();
                stack.Push(this.root.GetAll().GetEnumerator());
                while (stack.Count > 0)
                {
                    var en = stack.Peek();
                    if (en.MoveNext())
                    {
                        var vb = en.Current as ValueBucket;
                        if (vb != null)
                        {
                            yield return vb.Key;
                        }
                        else
                        {
                            stack.Push(en.Current.GetAll().GetEnumerator());
                        }
                    }
                    else
                    {
                        stack.Pop();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the values in the map.
        /// </summary>
        public IEnumerable<TValue> Values
        {
#pragma warning disable 618
            get { return this.GetValueBuckets().Select(vb => vb.Value); }
#pragma warning restore 618
        }

        /// <summary>
        /// Gets the <typeparamref name="TValue"/> with the specified key.
        /// </summary>
        public TValue this[TKey key]
        {
            get
            {
                TValue value;
                if (this.TryGetValue(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key contains key; otherwise, <c>false</c>.
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            if (this.root != null)
            {
                var vb = this.root.Get(this.keyComparer.GetHashCode(key), key, this.keyComparer);
                return vb != null;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this map contains the specified key-value pair.
        /// </summary>
        /// <param name="keyValuePair">The key value pair.</param>
        /// <returns>
        ///   <c>true</c> if this map contains the key-value pair; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            if (this.root != null)
            {
                var vb = this.root.Get(this.keyComparer.GetHashCode(keyValuePair.Key), keyValuePair.Key, this.keyComparer);
                return vb != null && this.valueComparer.Equals(vb.Value, keyValuePair.Value);
            }

            return false;
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.root != null)
            {
                var vb = this.root.Get(this.keyComparer.GetHashCode(key), key, this.keyComparer);
                if (vb != null)
                {
                    value = vb.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// See the <see cref="IImmutableDictionary&lt;TKey, TValue&gt;"/> interface.
        /// </summary>
        public bool TryGetKey(TKey equalKey, out TKey actualKey)
        {
            if (this.root != null)
            {
                var vb = this.root.Get(this.keyComparer.GetHashCode(equalKey), equalKey, this.keyComparer);
                if (vb != null)
                {
                    actualKey = vb.Key;
                    return true;
                }
            }

            actualKey = equalKey;
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this.GetValueBuckets().Select(vb => new KeyValuePair<TKey, TValue>(vb.Key, vb.Value)).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder("ImmutableHashMap[");
            bool needComma = false;
            foreach (var kv in this)
            {
                builder.Append(kv.Key);
                builder.Append(":");
                builder.Append(kv.Value);
                if (needComma)
                {
                    builder.Append(",");
                }

                needComma = true;
            }

            builder.Append("]");
            return builder.ToString();
        }

        /// <summary>
        /// Exchanges a key for the actual key instance found in this map.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="existingKey">Receives the equal key found in the map.</param>
        /// <returns>A value indicating whether an equal and existing key was found in the map.</returns>
        internal bool TryExchangeKey(TKey key, out TKey existingKey)
        {
            var vb = this.root != null ? this.root.Get(this.keyComparer.GetHashCode(key), key, this.keyComparer) : null;
            if (vb != null)
            {
                existingKey = vb.Key;
                return true;
            }
            else
            {
                existingKey = default(TKey);
                return false;
            }
        }

        /// <summary>
        /// Attempts to discover an <see cref="ImmutableHashMap&lt;TKey, TValue&gt;"/> instance beneath some enumerable sequence
        /// if one exists.
        /// </summary>
        /// <param name="sequence">The sequence that may have come from an immutable map.</param>
        /// <param name="other">Receives the concrete <see cref="ImmutableHashMap&lt;TKey, TValue&gt;"/> typed value if one can be found.</param>
        /// <returns><c>true</c> if the cast was successful; <c>false</c> otherwise.</returns>
        private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, out ImmutableHashMap<TKey, TValue> other)
        {
            other = sequence as ImmutableHashMap<TKey, TValue>;
            if (other != null)
            {
                return true;
            }

            return false;
        }

        private ImmutableHashMap<TKey, TValue> Wrap(Bucket root)
        {
            if (root == null)
            {
                return this.Clear();
            }

            if (this.root != root)
            {
                return root.Count == 0 ? this.Clear() : new ImmutableHashMap<TKey, TValue>(root, this.keyComparer, this.valueComparer);
            }

            return this;
        }

        /// <summary>
        /// Bulk adds entries to the map.
        /// </summary>
        /// <param name="pairs">The entries to add.</param>
        /// <param name="overwriteOnCollision"><c>true</c> to allow the <paramref name="pairs"/> sequence to include duplicate keys and let the last one win; <c>false</c> to throw on collisions.</param>
        /// <param name="avoidToHashMap"><c>true</c> when being called from ToHashMap to avoid StackOverflow.</param>
        [Pure]
        private ImmutableHashMap<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs, bool overwriteOnCollision, bool avoidToHashMap)
        {
            Contract.Requires(pairs != null);
            Contract.Ensures(Contract.Result<ImmutableHashMap<TKey, TValue>>() != null);

            // Some optimizations may apply if we're an empty list.
            if (this.IsEmpty && !avoidToHashMap)
            {
                // If the items being added actually come from an ImmutableHashMap<TKey, TValue>
                // then there is no value in reconstructing it.
                ImmutableHashMap<TKey, TValue> other;
                if (TryCastToImmutableMap(pairs, out other))
                {
                    return other.WithComparers(this.keyComparer, this.valueComparer);
                }
            }

            var map = this;
            foreach (var pair in pairs)
            {
                map = overwriteOnCollision
                    ? map.SetItem(pair.Key, pair.Value)
                    : map.Add(pair.Key, pair.Value);
            }

            return map;
        }

        private IEnumerable<ValueBucket> GetValueBuckets()
        {
            if (this.root == null)
            {
                yield break;
            }

            var stack = new Stack<IEnumerator<Bucket>>();
            stack.Push(this.root.GetAll().GetEnumerator());
            while (stack.Count > 0)
            {
                var en = stack.Peek();
                if (en.MoveNext())
                {
                    var vb = en.Current as ValueBucket;
                    if (vb != null)
                    {
                        yield return vb;
                    }
                    else
                    {
                        stack.Push(en.Current.GetAll().GetEnumerator());
                    }
                }
                else
                {
                    stack.Pop();
                }
            }
        }

        private abstract class Bucket
        {
            internal abstract int Count { get; }

            internal abstract Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue);
            internal abstract Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer);
            internal abstract ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer);
            internal abstract IEnumerable<Bucket> GetAll();
        }

        private abstract class ValueOrListBucket : Bucket
        {
            /// <summary>
            /// The hash for this bucket.
            /// </summary>
            internal readonly int Hash;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;.ValueOrListBucket"/> class.
            /// </summary>
            /// <param name="hash">The hash.</param>
            protected ValueOrListBucket(int hash)
            {
                this.Hash = hash;
            }
        }

        private sealed class ValueBucket : ValueOrListBucket
        {
            internal readonly TKey Key;
            internal readonly TValue Value;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;.ValueBucket"/> class.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            /// <param name="hashcode">The hashcode.</param>
            internal ValueBucket(TKey key, TValue value, int hashcode)
                : base(hashcode)
            {
                this.Key = key;
                this.Value = value;
            }

            internal override int Count
            {
                get { return 1; }
            }

            internal override Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue)
            {
                if (this.Hash == bucket.Hash)
                {
                    if (comparer.Equals(this.Key, bucket.Key))
                    {
                        // Overwrite of same key.  If the value is the same as well, don't switch out the bucket.
                        if (valueComparer.Equals(this.Value, bucket.Value))
                        {
                            return this;
                        }
                        else
                        {
                            if (overwriteExistingValue)
                            {
                                return bucket;
                            }
                            else
                            {
                                throw new ArgumentException(Strings.DuplicateKey);
                            }
                        }
                    }
                    else
                    {
                        // two of the same hash will never be happy in a hash bucket
                        return new ListBucket(new ValueBucket[] { this, bucket });
                    }
                }
                else
                {
                    return new HashBucket(suggestedHashRoll, this, bucket);
                }
            }

            internal override Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer)
            {
                if (this.Hash == hash && comparer.Equals(this.Key, key))
                {
                    return null;
                }

                return this;
            }

            internal override ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer)
            {
                if (this.Hash == hash && comparer.Equals(this.Key, key))
                {
                    return this;
                }

                return null;
            }

            internal override IEnumerable<Bucket> GetAll()
            {
                return SpecializedCollections.SingletonEnumerable(this);
            }
        }

        private sealed class ListBucket : ValueOrListBucket
        {
            private readonly ValueBucket[] buckets;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;.ListBucket"/> class.
            /// </summary>
            /// <param name="buckets">The buckets.</param>
            internal ListBucket(ValueBucket[] buckets)
                : base(buckets[0].Hash)
            {
                Contract.Requires(buckets != null);
                Contract.Requires(buckets.Length >= 2);
                this.buckets = buckets;
            }

            internal override int Count
            {
                get { return this.buckets.Length; }
            }

            internal override Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue)
            {
                if (this.Hash == bucket.Hash)
                {
                    int pos = this.Find(bucket.Key, comparer);
                    if (pos >= 0)
                    {
                        // If the value hasn't changed for this key, return the original bucket.
                        if (valueComparer.Equals(bucket.Value, this.buckets[pos].Value))
                        {
                            return this;
                        }
                        else
                        {
                            if (overwriteExistingValue)
                            {
                                return new ListBucket(this.buckets.ReplaceAt(pos, bucket));
                            }
                            else
                            {
                                throw new ArgumentException(Strings.DuplicateKey);
                            }
                        }
                    }
                    else
                    {
                        return new ListBucket(this.buckets.InsertAt(this.buckets.Length, bucket));
                    }
                }
                else
                {
                    return new HashBucket(suggestedHashRoll, this, bucket);
                }
            }

            internal override Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer)
            {
                if (this.Hash == hash)
                {
                    int pos = this.Find(key, comparer);
                    if (pos >= 0)
                    {
                        if (this.buckets.Length == 1)
                        {
                            return null;
                        }
                        else if (this.buckets.Length == 2)
                        {
                            return pos == 0 ? this.buckets[1] : this.buckets[0];
                        }
                        else
                        {
                            return new ListBucket(this.buckets.RemoveAt(pos));
                        }
                    }
                }

                return this;
            }

            internal override ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer)
            {
                if (this.Hash == hash)
                {
                    int pos = this.Find(key, comparer);
                    if (pos >= 0)
                    {
                        return this.buckets[pos];
                    }
                }

                return null;
            }

            private int Find(TKey key, IEqualityComparer<TKey> comparer)
            {
                for (int i = 0; i < this.buckets.Length; i++)
                {
                    if (comparer.Equals(key, this.buckets[i].Key))
                    {
                        return i;
                    }
                }

                return -1;
            }

            internal override IEnumerable<Bucket> GetAll()
            {
                return this.buckets;
            }
        }

        private sealed class HashBucket : Bucket
        {
            private readonly int hashRoll;
            private readonly uint used;
            private readonly Bucket[] buckets;
            private readonly int count;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;.HashBucket"/> class.
            /// </summary>
            /// <param name="hashRoll">The hash roll.</param>
            /// <param name="used">The used.</param>
            /// <param name="buckets">The buckets.</param>
            /// <param name="count">The count.</param>
            private HashBucket(int hashRoll, uint used, Bucket[] buckets, int count)
            {
                Contract.Requires(buckets != null);
                Contract.Requires(buckets.Length == CountBits(used));

                this.hashRoll = hashRoll & 31;
                this.used = used;
                this.buckets = buckets;
                this.count = count;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ImmutableHashMap&lt;TKey, TValue&gt;.HashBucket"/> class.
            /// </summary>
            /// <param name="suggestedHashRoll">The suggested hash roll.</param>
            /// <param name="bucket1">The bucket1.</param>
            /// <param name="bucket2">The bucket2.</param>
            internal HashBucket(int suggestedHashRoll, ValueOrListBucket bucket1, ValueOrListBucket bucket2)
            {
                Contract.Requires(bucket1 != null);
                Contract.Requires(bucket2 != null);
                Contract.Requires(bucket1.Hash != bucket2.Hash);

                // find next hashRoll that causes these two to be slotted in different buckets
                var h1 = bucket1.Hash;
                var h2 = bucket2.Hash;
                int s1;
                int s2;
                for (int i = 0; i < 32; i++)
                {
                    this.hashRoll = (suggestedHashRoll + i) & 31;
                    s1 = this.ComputeLogicalSlot(h1);
                    s2 = this.ComputeLogicalSlot(h2);
                    if (s1 != s2)
                    {
                        this.count = 2;
                        this.used = (1u << s1) | (1u << s2);
                        this.buckets = new Bucket[2];
                        this.buckets[this.ComputePhysicalSlot(s1)] = bucket1;
                        this.buckets[this.ComputePhysicalSlot(s2)] = bucket2;
                        return;
                    }
                }

                throw new InvalidOperationException();
            }

            internal override int Count
            {
                get { return this.count; }
            }

            internal override Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue)
            {
                int logicalSlot = ComputeLogicalSlot(bucket.Hash);
                if (IsInUse(logicalSlot))
                {
                    // if this slot is in use, then add the new item to the one in this slot
                    int physicalSlot = ComputePhysicalSlot(logicalSlot);
                    var existing = this.buckets[physicalSlot];

                    // suggest hash roll that will cause any nested hash bucket to use entirely new bits for picking logical slot
                    // note: we ignore passed in suggestion, and base new suggestion off current hash roll.
                    var added = existing.Add(this.hashRoll + 5, bucket, keyComparer, valueComparer, overwriteExistingValue);
                    if (added != existing)
                    {
                        var newBuckets = this.buckets.ReplaceAt(physicalSlot, added);
                        return new HashBucket(this.hashRoll, this.used, newBuckets, this.count - existing.Count + added.Count);
                    }
                    else
                    {
                        return this;
                    }
                }
                else
                {
                    int physicalSlot = ComputePhysicalSlot(logicalSlot);
                    var newBuckets = this.buckets.InsertAt(physicalSlot, bucket);
                    var newUsed = InsertBit(logicalSlot, this.used);
                    return new HashBucket(this.hashRoll, newUsed, newBuckets, this.count + bucket.Count);
                }
            }

            internal override Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer)
            {
                int logicalSlot = ComputeLogicalSlot(hash);
                if (IsInUse(logicalSlot))
                {
                    int physicalSlot = ComputePhysicalSlot(logicalSlot);
                    var existing = this.buckets[physicalSlot];
                    Bucket result = existing.Remove(hash, key, comparer);
                    if (result == null)
                    {
                        if (this.buckets.Length == 1)
                        {
                            return null;
                        }
                        else if (this.buckets.Length == 2)
                        {
                            return physicalSlot == 0 ? this.buckets[1] : this.buckets[0];
                        }
                        else
                        {
                            return new HashBucket(this.hashRoll, RemoveBit(logicalSlot, this.used), this.buckets.RemoveAt(physicalSlot), this.count - existing.Count);
                        }
                    }
                    else if (this.buckets[physicalSlot] != result)
                    {
                        return new HashBucket(this.hashRoll, this.used, this.buckets.ReplaceAt(physicalSlot, result), this.count - existing.Count + result.Count);
                    }
                }

                return this;
            }

            internal override ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer)
            {
                int logicalSlot = ComputeLogicalSlot(hash);
                if (IsInUse(logicalSlot))
                {
                    int physicalSlot = ComputePhysicalSlot(logicalSlot);
                    return this.buckets[physicalSlot].Get(hash, key, comparer);
                }

                return null;
            }

            internal override IEnumerable<Bucket> GetAll()
            {
                return this.buckets;
            }

            private bool IsInUse(int logicalSlot)
            {
                return ((1 << logicalSlot) & this.used) != 0;
            }

            private int ComputeLogicalSlot(int hc)
            {
                uint uc = unchecked((uint)hc);
                uint rotated = RotateRight(uc, this.hashRoll);
                return unchecked((int)(rotated & 31));
            }

            [Pure]
            private static uint RotateRight(uint v, int n)
            {
                Contract.Requires(n >= 0 && n < 32);
                if (n == 0)
                {
                    return v;
                }

                return v >> n | (v << (32 - n));
            }

            private int ComputePhysicalSlot(int logicalSlot)
            {
                Contract.Requires(logicalSlot >= 0 && logicalSlot < 32);
                Contract.Ensures(Contract.Result<int>() >= 0);
                if (this.buckets.Length == 32)
                {
                    return logicalSlot;
                }

                if (logicalSlot == 0)
                {
                    return 0;
                }

                uint mask = uint.MaxValue >> (32 - logicalSlot); // only count the bits up to the logical slot #
                return CountBits(this.used & mask);
            }

            [Pure]
            private static int CountBits(uint v)
            {
                unchecked
                {
                    v = v - ((v >> 1) & 0x55555555u);
                    v = (v & 0x33333333u) + ((v >> 2) & 0x33333333u);
                    return (int)((v + (v >> 4) & 0xF0F0F0Fu) * 0x1010101u) >> 24;
                }
            }

            [Pure]
            private static uint InsertBit(int position, uint bits)
            {
                Contract.Requires(0 == (bits & (1u << position)));
                return bits | (1u << position);
            }

            [Pure]
            private static uint RemoveBit(int position, uint bits)
            {
                Contract.Requires(0 != (bits & (1u << position)));
                return bits & ~(1u << position);
            }
        }

        #region IImmutableDictionary<TKey,TValue> Members

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
        {
            return this.Clear();
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return this.Add(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return this.SetItem(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return this.SetItems(items);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return this.AddRange(pairs);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return this.RemoveRange(keys);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
        {
            return this.Remove(key);
        }

        #endregion

        /// <summary>
        /// A simple view of the immutable collection that the debugger can show to the developer.
        /// </summary>
        private class DebuggerProxy
        {
            /// <summary>
            /// The collection to be enumerated.
            /// </summary>
            private readonly ImmutableHashMap<TKey, TValue> map;

            /// <summary>
            /// The simple view of the collection.
            /// </summary>
            private KeyValuePair<TKey, TValue>[] contents;

            /// <summary>   
            /// Initializes a new instance of the <see cref="DebuggerProxy"/> class.
            /// </summary>
            /// <param name="map">The collection to display in the debugger</param>
            public DebuggerProxy(ImmutableHashMap<TKey, TValue> map)
            {
                Requires.NotNull(map, "map");
                this.map = map;
            }

            /// <summary>
            /// Gets a simple debugger-viewable collection.
            /// </summary>
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<TKey, TValue>[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = this.map.ToArray();
                    }

                    return this.contents;
                }
            }
        }

        private static class Requires
        {
            [DebuggerStepThrough]
            public static T NotNullAllowStructs<T>(T value, string parameterName)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                return value;
            }

            [DebuggerStepThrough]
            public static T NotNull<T>(T value, string parameterName) where T : class
            {
                if (value == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                return value;
            }

            [DebuggerStepThrough]
            public static Exception FailRange(string parameterName, string message = null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    throw new ArgumentOutOfRangeException(parameterName);
                }

                throw new ArgumentOutOfRangeException(parameterName, message);
            }

            [DebuggerStepThrough]
            public static void Range(bool condition, string parameterName, string message = null)
            {
                if (!condition)
                {
                    Requires.FailRange(parameterName, message);
                }
            }
        }

        private static class Strings
        {
            public static string DuplicateKey
            {
                get
                {
                    return Microsoft.CodeAnalysis.WorkspacesResources.DuplicateKey;
                }
            }
        }
    }
}
