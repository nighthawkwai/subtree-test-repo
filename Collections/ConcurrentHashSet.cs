using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Services.Core.Collections
{
    /// <summary>
    /// An implementation of a ConcurrentHashset
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentHashSet<T> : ISet<T>
    {
        private readonly ConcurrentDictionary<T, object> _backingDictionary;

        public IEqualityComparer<T> Comparer { get; } = EqualityComparer<T>.Default;

        public ConcurrentHashSet()
        {
            _backingDictionary = new ConcurrentDictionary<T, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentHashSet{T}"/> class.
        /// </summary>
        /// <param name="equalityComparer">The equality comparer.</param>
        public ConcurrentHashSet(IEqualityComparer<T> equalityComparer)
        {
            Contract.Requires<ArgumentNullException>(equalityComparer != null, "equalityComparer cannot be null");
            _backingDictionary = new ConcurrentDictionary<T, object>(equalityComparer);
            Comparer = equalityComparer;
        }

        public ConcurrentHashSet(IEnumerable<T> elements)
            : this()
        {
            Contract.Requires<ArgumentNullException>(elements != null, "elements cannot be null");
            foreach (var elem in elements)
            {
                Add(elem);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _backingDictionary.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if(other == this)
                return;

            foreach (var elem in other)
            {
                Add(elem);
            }
        }
        
        public void IntersectWith(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (other == this)
                return;

            if (!this.Any())
                return;

            if (!other.Any())
            {
                Clear();
                return;
            }

            var otherAsConcurrentHashSet = other as ConcurrentHashSet<T>;

            if (otherAsConcurrentHashSet != null && AreEqualityComparersEqual(this, otherAsConcurrentHashSet))
            {
                foreach (var elem in this)
                {
                    if (!otherAsConcurrentHashSet.Contains(elem))
                    {
                        Remove(elem);
                    }
                }

                return;
            }

            var otherSet = new HashSet<T>(other, Comparer);

            foreach (var elem in this)
            {
                if (!otherSet.Contains(elem))
                {
                    Remove(elem);
                }
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (other == this)
            {
                Clear();
                return;
            }

            foreach (var elem in other)
            {
                if (Contains(elem))
                {
                    Remove(elem);
                }
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (other == this)
            {
                Clear();
                return;
            }

            if (!other.Any())
                return;

            if (!this.Any())
            {
                UnionWith(other);
                return;
            }

            HashSet<T> intersection = new HashSet<T>(other, Comparer);
            
            foreach (var elem in other)
            {
                if (Contains(elem))
                {
                    intersection.Add(elem);
                }
            }

            foreach (var elem in intersection)
            {
                Remove(elem);
            }

            foreach (var elem in other)
            {
                if (!intersection.Contains(elem))
                    Add(elem);
            }
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (other == this)
                return true;

            var otherAsConcurrentHashSet = other as ConcurrentHashSet<T>;

            if (otherAsConcurrentHashSet != null && AreEqualityComparersEqual(this, otherAsConcurrentHashSet))
            {
                if (this.Count > otherAsConcurrentHashSet.Count)
                    return false;

                foreach (var elem in this)
                {
                    if (!otherAsConcurrentHashSet.Contains(elem))
                        return false;
                }

                return true;
            }

            var otherSet = new HashSet<T>(other, Comparer);

            foreach (var elem in this)
            {
                if (!otherSet.Contains(elem))
                    return false;
            }

            return true;
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (other == this)
                return true;

            foreach (var elem in other)
            {
                if (!Contains(elem))
                    return false;
            }

            return true;
        }
        
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (!this.Any())
                return false;

            if (!other.Any())
                return true;

            var otherAsConcurrentHashSet = other as ConcurrentHashSet<T>;

            if (otherAsConcurrentHashSet != null && AreEqualityComparersEqual(this, otherAsConcurrentHashSet))
            {
                return this.Count > otherAsConcurrentHashSet.Count && IsSupersetOf(otherAsConcurrentHashSet);
            }

            bool uniqueElemInThis = false; //Indicates if there is an element in this concurrent hashset that is not present in other
            var otherSet = new HashSet<T>(other, Comparer);

            foreach (var elem in this)
            {
                if (!otherSet.Contains(elem))
                {
                    uniqueElemInThis = true;
                    break;
                }
            }

            return uniqueElemInThis && IsSupersetOf(otherSet);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            if (!other.Any())
                return false;

            if (!this.Any())
                return true;

            var otherAsConcurrentHashSet = other as ConcurrentHashSet<T>;

            if (otherAsConcurrentHashSet != null && AreEqualityComparersEqual(this, otherAsConcurrentHashSet))
            {
                return this.Count < otherAsConcurrentHashSet.Count && IsSubsetOf(otherAsConcurrentHashSet);
            }

            bool uniqueElemInOther = false; //Indicates if there is an element in other that is not present in this concurrent hash set

            foreach (var elem in other)
            {
                if (!Contains(elem))
                {
                    uniqueElemInOther = true;
                    break;
                }
            }

            return uniqueElemInOther && IsSubsetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            foreach (var elem in other)
            {
                if (this.Contains(elem))
                    return true;
            }
            return false;
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            Contract.Requires<ArgumentNullException>(other != null, "other");

            return IsSubsetOf(other) && IsSupersetOf(other);
        }

        public bool Add(T item)
        {
            return _backingDictionary.TryAdd(item, null);
        }

        void ICollection<T>.Add(T item)
        {

            Add(item);
        }

        public void Clear()
        {
            _backingDictionary.Clear();
        }

        public bool Contains(T item)
        {
            return _backingDictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            T[] sourceArray = _backingDictionary.Keys.ToArray();
            Array.ConstrainedCopy(sourceArray, 0, array, arrayIndex, sourceArray.Length);
        }

        public bool Remove(T item)
        {
            object o;
            return _backingDictionary.TryRemove(item, out o);
        }

        public int Count => _backingDictionary.Count;

        public bool IsReadOnly => false;

        private static bool AreEqualityComparersEqual(ConcurrentHashSet<T> hashset1, ConcurrentHashSet<T> hashSet2)
        {
            return hashset1.Comparer.Equals(hashSet2.Comparer);
        }
    }
}
