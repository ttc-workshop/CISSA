// Type: System.Collections.Hashtable
// Assembly: mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll

using System;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace System.Collections
{
    [DebuggerTypeProxy(typeof (Hashtable.HashtableDebugView))]
    [DebuggerDisplay("Count = {Count}")]
    [ComVisible(true)]
    [Serializable]
    public class Hashtable : IDictionary, ICollection, IEnumerable, ISerializable, IDeserializationCallback, ICloneable
    {
        public Hashtable();
        public Hashtable(int capacity);
        public Hashtable(int capacity, float loadFactor);

        [Obsolete("Please use Hashtable(int, float, IEqualityComparer) instead.")]
        public Hashtable(int capacity, float loadFactor, IHashCodeProvider hcp, IComparer comparer);

        public Hashtable(int capacity, float loadFactor, IEqualityComparer equalityComparer);

        [Obsolete("Please use Hashtable(IEqualityComparer) instead.")]
        public Hashtable(IHashCodeProvider hcp, IComparer comparer);

        public Hashtable(IEqualityComparer equalityComparer);

        [Obsolete("Please use Hashtable(int, IEqualityComparer) instead.")]
        public Hashtable(int capacity, IHashCodeProvider hcp, IComparer comparer);

        public Hashtable(int capacity, IEqualityComparer equalityComparer);
        public Hashtable(IDictionary d);
        public Hashtable(IDictionary d, float loadFactor);

        [Obsolete("Please use Hashtable(IDictionary, IEqualityComparer) instead.")]
        public Hashtable(IDictionary d, IHashCodeProvider hcp, IComparer comparer);

        public Hashtable(IDictionary d, IEqualityComparer equalityComparer);

        [Obsolete("Please use Hashtable(IDictionary, float, IEqualityComparer) instead.")]
        public Hashtable(IDictionary d, float loadFactor, IHashCodeProvider hcp, IComparer comparer);

        public Hashtable(IDictionary d, float loadFactor, IEqualityComparer equalityComparer);
        protected Hashtable(SerializationInfo info, StreamingContext context);

        [Obsolete("Please use EqualityComparer property.")]
        protected IHashCodeProvider hcp { get; set; }

        [Obsolete("Please use KeyComparer properties.")]
        protected IComparer comparer { get; set; }

        protected IEqualityComparer EqualityComparer { get; }

        #region ICloneable Members

        public virtual object Clone();

        #endregion

        #region IDeserializationCallback Members

        public virtual void OnDeserialization(object sender);

        #endregion

        #region IDictionary Members

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual void Add(object key, object value);

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public virtual void Clear();

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual bool Contains(object key);

        public virtual void CopyTo(Array array, int arrayIndex);

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        IEnumerator IEnumerable.GetEnumerator();

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public virtual IDictionaryEnumerator GetEnumerator();

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public virtual void Remove(object key);

        public virtual object this[object key] { get; [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        set; }

        public virtual bool IsReadOnly { get; }
        public virtual bool IsFixedSize { get; }
        public virtual bool IsSynchronized { get; }

        public virtual ICollection Keys { [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        get; }

        public virtual ICollection Values { [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        get; }

        public virtual object SyncRoot { get; }
        public virtual int Count { get; }

        #endregion

        #region ISerializable Members

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context);

        #endregion

        public virtual bool ContainsKey(object key);
        public virtual bool ContainsValue(object value);

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected virtual int GetHash(object key);

        protected virtual bool KeyEquals(object item, object key);
        public static Hashtable Synchronized(Hashtable table);
    }
}
