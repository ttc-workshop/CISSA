using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Intersoft.CISSA.BizService.Defs
{
    public enum BizObjectType 
    {
        // UI
        Form, Report, Query, Action,
        // System
        Document, DocumentDef, ReportDef, QueryDef,
        // Visual
        Panel, Text, Image, Bar, Button,
        Edit, ComboBox, List, Memo, RadioGroup, RadioItem, CheckBox,
        TableGrid, DetailGrid
    }

    [DataContract]
    public class BizObject
    {
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public BizObjectType Type { get; set; }
        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public List<BizObject> Children { get; set; }

        [DataMember]
        public Guid DefId { get; set;}
        [DataMember]
        public Guid ParentId { get; set; }
    }
}