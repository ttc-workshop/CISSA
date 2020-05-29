using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class DocListAttribute : AttributeBase
    {
        public DocListAttribute()
        {
//            ItemsDocId = new List<Guid>();
//            AddedDocIds = new List<Guid>();
//            AddedDocs = new List<Doc>();
        }

        public DocListAttribute(AttrDef attrDef)
        {
            AttrDef = attrDef;
            ItemsDocId = new List<Guid>();
            AddedDocIds = new List<Guid>();
            AddedDocs = new List<Doc>();
        }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public List<Guid> ItemsDocId { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public List<Guid> AddedDocIds { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore()]
        public List<Doc> AddedDocs { get; set; }

        [System.Xml.Serialization.XmlIgnore()]
        public override object ObjectValue
        {
            get { return ItemsDocId; }
            set { ItemsDocId = (List<Guid>)(value); }
        }
    }
}