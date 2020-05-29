using System;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public abstract class SqlQuerySourceAttribute : SqlQueryItem
    {
        public string AliasName { get; private set; }

        protected SqlQuerySourceAttribute(string alias)
        {
            AliasName = alias;
        }

        public virtual AttrDef Def { get { return null; } }
        public abstract SystemIdent Ident { get; }

        public bool SameAttrDefId(Guid attrDefId)
        {
            var def = Def;
            return def != null && def.Id == attrDefId;
        }

        public virtual bool SameAttrName(string name)
        {
            if (name.Length <= 0 || name[0] != '&')
                return Def != null &&
                       String.Equals(Def.Name, name, StringComparison.OrdinalIgnoreCase);

            var ident = SystemIdentConverter.Convert(name);

            return Def == null && Ident == ident;
        }

        public string GetAttrDefTableName()
        {
            if (Def != null)
            {
                /*switch ((CissaDataType)Def.Type.Id)
                {
                    case CissaDataType.Int:
                        return "Int_Attributes";
                    case CissaDataType.Text:
                        return "Text_Attributes";
                    case CissaDataType.Float:
                        return "Float_Attributes";
                    case CissaDataType.Enum:
                        return "Enum_Attributes";
                    case CissaDataType.Bool:
                        return "Boolean_Attributes";
                    case CissaDataType.Currency:
                        return "Currency_Attributes";
                    case CissaDataType.DateTime:
                        return "Date_Time_Attributes";
                    case CissaDataType.Doc:
                        return "Document_Attributes";
                    case CissaDataType.DocList:
                        return "DocumentList_Attributes";
                    case CissaDataType.Organization:
                        return "Org_Attributes";
                    case CissaDataType.DocumentState:
                        return "Doc_State_Attributes";
                }*/
                var tableName = CissaDataTypeHelper.GetTableName((CissaDataType) Def.Type.Id);
                if (!String.IsNullOrEmpty(tableName))
                    return tableName;
            }
            throw new ApplicationException("Не поддерживаемый тип атрибута в SQL запросе!");
        }
    }
}