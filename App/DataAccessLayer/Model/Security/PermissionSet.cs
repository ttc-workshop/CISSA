using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Intersoft.CISSA.DataAccessLayer.Model.Security
{
    [DataContract]
    public class PermissionSet : IEnumerable<Guid>
    {
        [DataMember]
        public HashSet<Guid> Items { get; set; }

        public PermissionSet()
        {
            Items = new HashSet<Guid>();
        }

        public PermissionSet(IEnumerable<Guid> collection)
        {
            Items = new HashSet<Guid>(collection);
        }

        public void UnionWith(IEnumerable<Guid> other)
        {
            Items.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<Guid> other)
        {
            var set = new HashSet<Guid>(other);
            Items.IntersectWith(set);
        }

        public void ExceptWith(IEnumerable<Guid> other)
        {
            var set = new HashSet<Guid>(other);
            Items.ExceptWith(set);
        }

        public bool Overlaps(IEnumerable<Guid> other)
        {
            var set = new HashSet<Guid>(other);
            return Items.Overlaps(set);
        }

        public bool IsSupersetOf(IEnumerable<Guid> other)
        {
            return other == null || Items.IsSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<Guid> other)
        {
            return other != null && Items.IsSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<Guid> other)
        {
            return Items.IsProperSupersetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<Guid> other)
        {
            return Items.IsProperSubsetOf(other);
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(object o)
        {
            var value = Guid.Parse(o.ToString());
                
            Items.Add(value);
        }
    }
}
