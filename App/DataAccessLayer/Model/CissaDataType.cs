using System;

namespace Intersoft.CISSA.DataAccessLayer.Model
{
    public enum CissaDataType
    {
        Int = 1,
        Currency, // 2
        Text, // 3
        Float, // 4
        Enum, // 5
        Doc, // 6
        DocList, // 7
        Bool, // 8
        DateTime, // 9
        Auto, // 10
        Blob, // 11
        Organization, // 12
        DocumentState, // 13
        // ссылки на ObjectDef - Object_Def_Attributes
        User, // 14
        OrgPosition, // 15
        OrgUnit, // 16
        DocumentDef, // 17
        EnumDef, // 18
        Form, // 19
        Process, // 20

        // Виртуальные атрибуты
        OrganizationOfDocument, // 21 Организация - создатель документа, поле Organization_Id в документе
        StateOfDocument, // 22 Текущий статус документа
        ClassOfDocument, // 23 - Тип документа
        AuthorOfDocument, // 24 - Пользователь-автор документа
        OrgUnitOfDocument, // 25 - Позиция автора документа
        CreateTimeOfDocument, // 26 - Дата создания документа
        DocumentId // 27 - Id документа
    }

    public enum BaseDataType
    {
        Unknown,
        Int,
        Currency,
        Text,
        Float,
        Bool,
        DateTime,
        Guid,
        Blob
    }

    public static class CissaDataTypeHelper
    {
        public static BaseDataType ConvertToBase(CissaDataType dataType)
        {
            switch (dataType)
            {
                case CissaDataType.Currency:
                    return BaseDataType.Currency;
                case CissaDataType.CreateTimeOfDocument:
                case CissaDataType.DateTime:
                    return BaseDataType.DateTime;
                case CissaDataType.Bool:
                    return BaseDataType.Bool;
                case CissaDataType.Float:
                    return BaseDataType.Float;
                case CissaDataType.Int:
                    return BaseDataType.Int;
                case CissaDataType.Text:
                    return BaseDataType.Text;
                case CissaDataType.Doc:
                case CissaDataType.DocList:
                case CissaDataType.DocumentId:
                case CissaDataType.DocumentState:
                case CissaDataType.Enum:
                case CissaDataType.User:
                case CissaDataType.DocumentDef:
                case CissaDataType.EnumDef:
                case CissaDataType.OrgPosition:
                case CissaDataType.OrgUnit:
                case CissaDataType.OrgUnitOfDocument:
                case CissaDataType.Organization:
                case CissaDataType.OrganizationOfDocument:
                case CissaDataType.StateOfDocument:
                    return BaseDataType.Guid;
                case CissaDataType.Blob:
                    return BaseDataType.Blob;
                default:
                    return BaseDataType.Unknown;
                // throw new ApplicationException("Не могу преобразовать тип данных");
            }
        }

        public static bool IsValid(short dataType)
        {
            return dataType >= (short) CissaDataType.Int && dataType <= (short) CissaDataType.DocumentId;
        }

        public static void CheckValidity(short dataType)
        {
            if (!IsValid(dataType))
                throw new ApplicationException("Недействительный код типа данных!");
        }

        public static BaseDataType ConvertToBase(short dataType)
        {
            CheckValidity(dataType);

            return ConvertToBase((CissaDataType) dataType);
        }

        public static string GetTableName(short dataType)
        {
            CheckValidity(dataType);
            return GetTableName((CissaDataType) dataType);
        }

        public static string GetTableName(CissaDataType dataType)
        {
            switch (dataType)
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
                case CissaDataType.Blob:
                    return "Image_Attributes";
                case CissaDataType.User:
                case CissaDataType.OrgPosition:
                case CissaDataType.OrgUnit:
                case CissaDataType.DocumentDef:
                case CissaDataType.EnumDef:
                case CissaDataType.Form:
                case CissaDataType.Process:
                    return "Object_Def_Attributes";
                case CissaDataType.Auto:
                case CissaDataType.OrganizationOfDocument:
                case CissaDataType.StateOfDocument:
                case CissaDataType.ClassOfDocument:
                case CissaDataType.AuthorOfDocument:
                case CissaDataType.OrgUnitOfDocument:
                case CissaDataType.CreateTimeOfDocument:
                case CissaDataType.DocumentId:
                    return String.Empty; // Виртуальные атрибуты
            }
            return String.Empty;
        }
    }
}
