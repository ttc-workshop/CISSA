using System;

namespace Intersoft.CISSA.DataAccessLayer.Model
{
    public static class SystemIdentConverter
    {
        public static bool TryConvert(string attrName, out SystemIdent ident)
        {
            ident = SystemIdent.Id;

            if (String.Equals(attrName, "&state", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.State;
            else if (String.Equals(attrName, "&Id", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.Id;
            else if (String.Equals(attrName, "&OrgId", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.OrgId;
            else if (String.Equals(attrName, "&Created", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.Created;
            else if (String.Equals(attrName, "&Modified", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.Modified;
            else if (String.Equals(attrName, "&UserId", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.UserId;
            else if (String.Equals(attrName, "&UserName", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.UserName;
            else if (String.Equals(attrName, "&OrgCode", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.OrgCode;
            else if (String.Equals(attrName, "&OrgName", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.OrgName;
            else if (String.Equals(attrName, "&DefId", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.DefId;
            else if (String.Equals(attrName, "&StateDate", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.StateDate;
            else if (String.Equals(attrName, "&InState", StringComparison.OrdinalIgnoreCase))
                ident = SystemIdent.InState;
            else
                return false;

            return true;
        }

        public static SystemIdent Convert(string attrName)
        {
            SystemIdent ident;

            if (!TryConvert(attrName, out ident))
                throw new ApplicationException(String.Format("Неизвестный системный идентификатор {0}", attrName));

            return ident;
        }

        public static string Convert(SystemIdent ident)
        {
            switch (ident)
            {
                case SystemIdent.Id:
                    return "&Id";
                case SystemIdent.DefId:
                    return "&DefId";
                case SystemIdent.Created:
                    return "&Created";
                case SystemIdent.Modified:
                    return "&Modified";
                case SystemIdent.OrgId:
                    return "&OrgId";
                case SystemIdent.State:
                    return "&State";
                case SystemIdent.StateDate:
                    return "&StateDate";
                case SystemIdent.UserName:
                    return "&UserName";
                case SystemIdent.UserId:
                    return "&UserId";
                case SystemIdent.OrgName:
                    return "&OrgCode";
                case SystemIdent.InState:
                    return "&InState";
            }
            return String.Empty;
        }

        public static BaseDataType ToBaseType(SystemIdent ident)
        {
            switch (ident)
            {
                case SystemIdent.Id:
                case SystemIdent.DefId:
                case SystemIdent.OrgId:
                case SystemIdent.State:
                case SystemIdent.UserId:
                    return BaseDataType.Guid;
                case SystemIdent.Created:
                case SystemIdent.Modified:
                case SystemIdent.StateDate:
                    return BaseDataType.DateTime;
                case SystemIdent.UserName:
                case SystemIdent.OrgName:
                    return BaseDataType.Text;
                case SystemIdent.InState:
                    return BaseDataType.Guid;
            }
            return BaseDataType.Unknown;
        }
    }
}