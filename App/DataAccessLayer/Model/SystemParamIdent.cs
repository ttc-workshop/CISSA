using System;

namespace Intersoft.CISSA.DataAccessLayer.Model
{
    public enum SystemParamIdent
    {
        UserId,
        UserOrgId,
        UserPositionId,
        UserOrgTypeId,
        UserName,
        UserOrgName,
        UserPositionName,
        Today,
        Now
    }

    public static class SystemParamIdentConverter
    {
        public static bool TryConvert(string attrName, out SystemParamIdent ident)
        {
            ident = SystemParamIdent.UserId;

            if (String.Equals(attrName, "&UserId", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserId;
            else if (String.Equals(attrName, "&UserOrgId", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserOrgId;
            else if (String.Equals(attrName, "&UserPositionId", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserPositionId;
            else if (String.Equals(attrName, "&UserOrgTypeId", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserOrgTypeId;
            else if (String.Equals(attrName, "&Today", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.Today;
            else if (String.Equals(attrName, "&Now", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.Now;
            else if (String.Equals(attrName, "&UserId", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserId;
            else if (String.Equals(attrName, "&UserName", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserName;
            else if (String.Equals(attrName, "&UserOrgName", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserOrgName;
            else if (String.Equals(attrName, "&UserPositionName", StringComparison.OrdinalIgnoreCase))
                ident = SystemParamIdent.UserPositionName;
            else
                return false;

            return true;
        }

        public static SystemParamIdent Convert(string name)
        {
            SystemParamIdent ident;

            if (!TryConvert(name, out ident))
                throw new ApplicationException(String.Format("Неизвестный системный параметр {0}", name));

            return ident;
        }
        public static string Convert(SystemParamIdent ident)
        {
            switch (ident)
            {
                case SystemParamIdent.UserId:
                    return "&UserId";
                case SystemParamIdent.UserOrgId:
                    return "&UserOrgId";
                case SystemParamIdent.UserPositionId:
                    return "&UserPositionId";
                case SystemParamIdent.UserOrgTypeId:
                    return "&UserOrgTypeId";
                case SystemParamIdent.UserName:
                    return "&UserName";
                case SystemParamIdent.UserOrgName:
                    return "&UserOrgName";
                case SystemParamIdent.UserPositionName:
                    return "&UserPositionName";
                case SystemParamIdent.Today:
                    return "&Today";
                case SystemParamIdent.Now:
                    return "&Now";
            }
            return String.Empty;
        }

        public static BaseDataType ToBaseType(SystemParamIdent ident)
        {
            switch (ident)
            {
                case SystemParamIdent.UserId:
                case SystemParamIdent.UserOrgId:
                case SystemParamIdent.UserPositionId:
                case SystemParamIdent.UserOrgTypeId:
                    return BaseDataType.Guid;
                case SystemParamIdent.Today:
                case SystemParamIdent.Now:
                    return BaseDataType.DateTime;
                case SystemParamIdent.UserName:
                case SystemParamIdent.UserOrgName:
                case SystemParamIdent.UserPositionName:
                    return BaseDataType.Text;
            }
            return BaseDataType.Unknown;
        }
    }
}
