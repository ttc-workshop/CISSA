using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Context;
using Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class Doc 
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string DataContextName { get; set; }

        [DataMember]
//        [System.Xml.Serialization.XmlIgnore()]
        public DocDef DocDef { get; set; }

        [DataMember]
        public DateTime CreationTime { get; set; }

        [DataMember]
        public Guid? OrganizationId { get; set; }

        [DataMember]
        public Guid? PositionId { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public DateTime ModifiedTime { get; set; }

        [DataMember]
        public List<AttributeBase> Attributes { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public object this[string attributeName]
        {
            get
            {
                var attribute = GetAttributeByName(attributeName);

                return attribute.ObjectValue;
            }
            set
            {
                var attribute = GetAttributeByName(attributeName);

                attribute.ObjectValue = value;
            }
        }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore]
        public bool IsNew { get; set; }

        [DataMember]
        [System.Xml.Serialization.XmlIgnore]
        public DocState State { get; set; }

        public AttributeBase GetAttributeByName(string attributeName)
        {
            var query = from attr in Attributes
                        where String.Equals(attr.AttrDef.Name, attributeName, StringComparison.CurrentCultureIgnoreCase) 
                        select attr;

            var attributeBases = query as AttributeBase[] ?? query.ToArray();
            if (!attributeBases.Any())
                throw new ApplicationException(
                    string.Format("Атрибута с именем {0} не существует.", attributeName));

            return attributeBases.First();
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<IntAttribute> AttrInt 
        {
            get
            {
                return Attributes
                    .Where(a => a.GetType() == typeof (IntAttribute))
                    .Select(a=>(IntAttribute)a);
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<FloatAttribute> AttrFloat
        {
            get
            {
                return Attributes.OfType<FloatAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<TextAttribute> AttrText
        {
            get
            {
                return Attributes.OfType<TextAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<BlobAttribute> AttrBlob
        {
            get
            {
                return Attributes.OfType<BlobAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<AutoAttribute> AttrAuto
        {
            get
            {
                return Attributes.OfType<AutoAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<CurrencyAttribute> AttrCurrency
        {
            get
            {
                return Attributes.OfType<CurrencyAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<DateTimeAttribute> AttrDateTime
        {
            get
            {
                return Attributes.OfType<DateTimeAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<BoolAttribute> AttrBool
        {
            get
            {
                return Attributes
                    .Where(a => a.GetType() == typeof(BoolAttribute))
                    .Select(a => (BoolAttribute)a);
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<EnumAttribute> AttrEnum
        {
            get
            {
                return Attributes.OfType<EnumAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<DocAttribute> AttrDoc
        {
            get
            {
                return Attributes.OfType<DocAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<DocListAttribute> AttrDocList
        {
            get
            {
                return Attributes.OfType<DocListAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<OrganizationAttribute> AttrOrg
        {
            get
            {
                return Attributes.OfType<OrganizationAttribute>();
            }
        }

        [System.Xml.Serialization.XmlIgnore]
        public IEnumerable<DocumentStateAttribute> AttrDocState
        {
            get
            {
                return Attributes.OfType<DocumentStateAttribute>();
            }
        }
/*
        [System.Xml.Serialization.XmlIgnore]
        [Obsolete("Не правильное свойство! Необходимо удалить")]
        public bool IsStored
        {
            get
            {
                using (var dataContext = new DataContext())
                    return dataContext.Documents.FirstOrDefault(d => d.Id == Id) != null;
            }
        }*/

        public T Get<T>(string name) where T: AttributeBase
        {
            var attr = GetAttributeByName(name);

            if (attr is T) return (T) attr;

            throw new Exception(String.Format("Ошибка в типе атрибута \"{0}\"", name));
        }

        public T Find<T>(string name) where T : AttributeBase
        {
            var attr = GetAttributeByName(name);

            if (attr == null || attr is T) return (T)attr;

            throw new Exception(String.Format("Ошибка в типе атрибута \"{0}\"", name));
        }

        public object GetValue(string name)
        {
            var attr = GetAttributeByName(name);

            if (attr != null) return attr.ObjectValue;

            throw new Exception(String.Format("Атрибут не найден \"{0}\"", name));
        }

        public T GetValue<T>(string name)
        {
            var value = GetValue(name);

            if (value is T) return (T)value;

            throw new Exception(String.Format("Ошибка в типе атрибута \"{0}\"", name));
        }

        public T FindValue<T>(string name)
        {
            var value = this[name];

            if (value != null) return (T)value;

            return (T)(object)null;
        }

//        public 
    }
}
