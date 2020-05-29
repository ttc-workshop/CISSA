using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Enums;
using Intersoft.CISSA.DataAccessLayer.Model.Security;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class AttrDef
    {
        public AttrDef() {}

        /*public AttrDef(Attribute_Def source)
        {
            if (source == null) return;

            Id = source.Id;
            Name = source.Name ?? String.Empty;
            Caption = source.Full_Name ?? String.Empty;
            IsNotNull = source.Is_Not_Null ?? false;
            IsUnique = source.Is_Unique ?? false;
            MaxLength = source.Max_Length ?? 0;
            MaxValue = source.Max_Value;
            MinValue = source.Min_Value;
            DefaultValue = source.Default_Value ?? String.Empty;
            OrgTypeId = source.Org_Type_Id;

            if (!source.Data_TypesReference.IsLoaded) source.Data_TypesReference.Load();
            if (source.Data_Types != null)
            {
                Type = new TypeDef(source.Data_Types);
            }
            if (source.Enum_Id != null)
            {
                if (!source.Enum_DefsReference.IsLoaded) source.Enum_DefsReference.Load();

                EnumDefType = new EnumDef
                                  {
                                      Description = source.Enum_Defs.Description,
                                      Id = (Guid)source.Enum_Id,
                                      Name = source.Enum_Defs.Name ?? String.Empty,
                                      Caption = source.Enum_Defs.Full_Name ?? String.Empty
                                  };
            }
            if (source.Document_Id != null)
            {
                if (!source.Document_DefsReference.IsLoaded) source.Document_DefsReference.Load();
                DocDefType = new DocDef
                                 {
                                     Id = source.Document_Defs.Id,
                                     Name = source.Document_Defs.Name ?? String.Empty,
                                     Caption = source.Document_Defs.Full_Name ?? String.Empty,
                                     AncestorId = source.Document_Defs.Ancestor_Id,
                                     IsInline = source.Document_Defs.Is_Inline ?? false,
                                     IsPublic = source.Document_Defs.Is_Public ?? false
                                 };
            }
            Script = source.CalculateScript;
            BlobInfo = new BlobInfo
                           {
                               MaxHeight = source.BlobMaxHeight ?? 0,
                               MaxWidth = source.BlobMaxWidth ?? 0,
                               MaxSizeBytes = source.BlobMaxSizeBytes ?? 0,
                               IsImage = source.BlobIsImage ?? false
                           };
        }*/

        public AttrDef(AttrDef source)
        {
            if (source == null) return;

            Id = source.Id;
            Name = source.Name;
            Caption = source.Caption;
            IsNotNull = source.IsNotNull;
            IsUnique = source.IsUnique;
            MaxLength = source.MaxLength;
            MaxValue = source.MaxValue;
            MinValue = source.MinValue;
            DefaultValue = source.DefaultValue;
            OrgTypeId = source.OrgTypeId;

            if (source.Type != null)
                Type = new TypeDef {Id = source.Type.Id, Name = source.Type.Name};

            if (source.EnumDefType != null)
            {
                EnumDefType = new EnumDef
                {
                    Description = source.EnumDefType.Description,
                    Id = source.EnumDefType.Id,
                    Name = source.EnumDefType.Name,
                    Caption = source.EnumDefType.Caption
                };
            }
            if (source.DocDefType != null)
            {
                DocDefType = new DocDef
                {
                    Id = source.DocDefType.Id,
                    Name = source.DocDefType.Name,
                    Caption = source.DocDefType.Caption,
                    AncestorId = source.DocDefType.AncestorId,
                    IsInline = source.DocDefType.IsInline,
                    IsPublic = source.DocDefType.IsPublic
                };
            }
            Script = source.Script;
            BlobInfo = new BlobInfo
            {
                MaxHeight = source.BlobInfo != null ? source.BlobInfo.MaxHeight : 0,
                MaxWidth = source.BlobInfo != null ? source.BlobInfo.MaxWidth : 0,
                MaxSizeBytes = source.BlobInfo != null ? source.BlobInfo.MaxSizeBytes : 0,
                IsImage = source.BlobInfo != null && source.BlobInfo.IsImage
            };
        }

        [DataMember]
        public Guid Id { get; set; }
        
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public String Caption { get; set; }

        [Obsolete("Excluded")]
        [XmlIgnore]
        public DocDef OwnerDocDef { get; set; }

        [DataMember]
        public TypeDef Type { get; set; }

        [DataMember]
        public bool IsNotNull { get; set; }

        [DataMember]
        public bool IsUnique { get; set; }

        [DataMember]
        public EnumDef EnumDefType { get; set; }

        [DataMember]
        public DocDef DocDefType { get; set; }

//        [DataMember]
//        public Guid DocDefId { get; set; }

//        [DataMember]
//        public BizPermission Permissions { get; set; }

        [DataMember]
        public String Script { get; set; }

        [DataMember]
        public BlobInfo BlobInfo { get; set; }

        [DataMember]
        public PermissionSet Permissions { get; set; }

        [DataMember]
        public decimal? MaxValue { get; set; }

        [DataMember]
        public decimal? MinValue { get; set; }

        [DataMember]
        public int MaxLength { get; set; }

        [DataMember]
        public string DefaultValue { get; set; }

        [DataMember]
        public Guid? OrgTypeId { get; set; }
    }
}
