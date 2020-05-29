using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Query.DefDatas;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [DataContract]
    [KnownType(typeof(BizDataControl))]
    [KnownType(typeof(BizButton))]
    [KnownType(typeof(BizComboBox))]
    [KnownType(typeof(BizText))]
    [KnownType(typeof(BizImage))]
    [KnownType(typeof(BizDataImage))]
    [KnownType(typeof(BizRadioItem))]
    [KnownType(typeof(BizTableForm))]
    [KnownType(typeof(BizPanel))]
    [KnownType(typeof(BizEdit))]
    [KnownType(typeof(BizComboBox))]
    [KnownType(typeof(BizForm))]
    [KnownType(typeof(BizDetailForm))]
    [KnownType(typeof(BizTableForm))]
    [KnownType(typeof(BizGrid))]
    [KnownType(typeof(BizDocumentControl))]
    [KnownType(typeof(BizDocumentListForm))]
    [KnownType(typeof(BizDynamicDocumentListForm))]
    [KnownType(typeof(BizEditText))]
    [KnownType(typeof(BizEditCurrency))]
    [KnownType(typeof(BizEditFloat))]
    [KnownType(typeof(BizEditInt))]
    [KnownType(typeof(BizEditDateTime))]
    [KnownType(typeof(BizEditBool))]
    [KnownType(typeof(BizMenu))]
    [KnownType(typeof(BizTableColumn))]
    [KnownType(typeof(BizTabControl))]
    [KnownType(typeof(BizEditVar))]
    [KnownType(typeof(BizEditSysIdent))]
    [KnownType(typeof(BizEditFile))]

    [XmlInclude(typeof(BizDataControl))]
    [XmlInclude(typeof(BizButton))]
    [XmlInclude(typeof(BizComboBox))]
    [XmlInclude(typeof(BizText))]
    [XmlInclude(typeof(BizImage))]
    [XmlInclude(typeof(BizDataImage))]
    [XmlInclude(typeof(BizRadioItem))]
    [XmlInclude(typeof(BizTableForm))]
    [XmlInclude(typeof(BizPanel))]
    [XmlInclude(typeof(BizEdit))]
    [XmlInclude(typeof(BizComboBox))]
    [XmlInclude(typeof(BizForm))]
    [XmlInclude(typeof(BizDetailForm))]
    [XmlInclude(typeof(BizTableForm))]
    [XmlInclude(typeof(BizGrid))]
    [XmlInclude(typeof(BizDocumentControl))]
    [XmlInclude(typeof(BizDocumentListForm))]
    [XmlInclude(typeof(BizDynamicDocumentListForm))]
    [XmlInclude(typeof(BizEditText))]
    [XmlInclude(typeof(BizEditCurrency))]
    [XmlInclude(typeof(BizEditFloat))]
    [XmlInclude(typeof(BizEditInt))]
    [XmlInclude(typeof(BizEditDateTime))]
    [XmlInclude(typeof(BizEditBool))]
    [XmlInclude(typeof(BizMenu))]
    [XmlInclude(typeof(BizTableColumn))]
    [XmlInclude(typeof(BizTabControl))]
    [XmlInclude(typeof(BizEditVar))]
    [XmlInclude(typeof(BizEditSysIdent))]
    [XmlInclude(typeof(BizEditFile))]
    public class BizControl
    {
        [DataMember]
        public Guid Id { get; set; }
       
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Style { get; set; }

        [DataMember]
        public List<BizControl> Children { get; set; }

        [DataMember]
        public bool Invisible { get; set; }

        [DataMember]
        public CompareOperation Operation { get; set; }

        [DataMember]
        public SortType SortType { get; set; }

        [DataMember]
        public PermissionSet Permissions { get; set; }

        [DataMember]
        public string DefaultCaption { get; set; }

        [DataMember]
        public int LanguageId { get; set; }

        [DataMember]
        public BizControlOptionFlags Options { get; set; }

        [DataMember]
        public List<QueryItemDefData> QueryItems { get; set; }

        [DataMember]
        public List<Guid> Masters { get; set; }

        [DataMember]
        public List<Guid> Dependents { get; set; }
    }
}