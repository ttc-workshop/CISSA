using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocDef
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public String Caption { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public bool WithHistory { get; set; }

        [DataMember]
        public Guid? AncestorId { get; set; }

//        [DataMember]
//        public BizPermission Permissions { get; set; }

        [DataMember]
        public List<AttrDef> Attributes { get; set; }

        [DataMember]
        public bool IsInline { get; set; }

        [DataMember]
        public bool IsPublic { get; set; }

        [DataMember]
        public Guid? CreatePermissionId { get; set; }
        [DataMember]
        public Guid? SelectPermissionId { get; set; }
        [DataMember]
        public Guid? UpdatePermissionId { get; set; }
        [DataMember]
        public Guid? DeletePermissionId { get; set; }

        [DataMember]
        public PermissionSet Permissions { get; set; }

        public AttrDef GetByName(string name)
        {
            var attr = FindByName(name);

            if (attr != null) return attr;

            throw new Exception(String.Format("Атрибута с именем \"{0}\" не существует", name));
        }

        public AttrDef FindByName(string name)
        {
            if (Attributes != null)
            {
                var attr = Attributes.FirstOrDefault(a => String.Compare(a.Name, name, StringComparison.OrdinalIgnoreCase) == 0);

                return attr;
            }
            return null;
        }

        public AttrDef GetById(Guid id)
        {
            var attr = FindById(id);

            if (attr != null) return attr;

            throw new Exception(String.Format("Атрибута с идентификатором \"{0}\" не существует", id));
        }

        public AttrDef FindById(Guid id)
        {
            if (Attributes != null)
            {
                var attr = Attributes.FirstOrDefault(a => a.Id == id);

                return attr;
            }
            return null;
        }
    }
}
