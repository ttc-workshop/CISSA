using System;
using System.Collections.Generic;
using System.Linq;
using Intersoft.CISSA.DataAccessLayer.Core;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;
using Intersoft.CISSA.DataAccessLayer.Model.Maps;
using Intersoft.CISSA.DataAccessLayer.Repository;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Sql
{
    public class SqlQueryDocSource : SqlQuerySource
    {
        private DocDef Source { get; set; }
        private IAppServiceProvider Provider { get; set; }
        // public IDataContext DataContext { get; private set; }

        private readonly IDocDefRepository _docDefRepo;
        private readonly IDocumentTableMapRepository _mapper;
        private readonly IUserRepository _userRepo;

        public SqlQueryDocSource(IAppServiceProvider provider, /*IDataContext dataContext, */DocDef docDef, string alias)
            : base(alias)
        {
            Source = docDef;
            Provider = provider;
            // DataContext = dataContext;

            _docDefRepo = Provider.Get<IDocDefRepository>();
            _mapper = Provider.Get<IDocumentTableMapRepository>();
            _userRepo = Provider.Get<IUserRepository>();
        }

        public SqlQueryDocSource(IAppServiceProvider provider, /*IDataContext dataContext, */DocDef docDef, string alias, Guid userId)
            : this(provider, docDef, alias)
        {
            UserId = userId;
        }

        public SqlQueryDocSource(IAppServiceProvider provider, /*IDataContext dataContext,*/ Guid docDefId, string alias)
            : base(alias)
        {
            Provider = provider;
            //DataContext = dataContext;
            //using (var docDefRepo = new DocDefRepository(DataContext))
            
            _docDefRepo = Provider.Get<IDocDefRepository>();
            _mapper = Provider.Get<IDocumentTableMapRepository>();
            _userRepo = Provider.Get<IUserRepository>();

            Source = _docDefRepo.DocDefById(docDefId);
        }

        public SqlQueryDocSource(IAppServiceProvider provider, /*IDataContext dataContext,*/ Guid docDefId, string alias, Guid userId)
            : this(provider, docDefId, alias)
        {
            UserId = userId;
        }

        private IDocumentTableMapRepository Mapper
        {
            get { return _mapper /*?? (_mapper = new DocumentTableMapRepository(DataContext))*/; }
        }

        public override DocDef GetDocDef()
        {
            return Source;
        }

        public override AttrDef FindAttributeDef(Guid attrDefId)
        {
            return Source.Attributes.FirstOrDefault(a => a.Id == attrDefId);
        }

        public override AttrDef FindAttributeDef(string attrDefName)
        {
            return
                Source.Attributes.FirstOrDefault(
                    a => String.Equals(a.Name, attrDefName, StringComparison.OrdinalIgnoreCase));
        }

        protected override SqlQuerySourceAttribute AddAttribute(Guid attrDefId)
        {
            var attrDef = FindAttributeDef(attrDefId);
            if (attrDef == null)
                throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден!", attrDefId));

            var alias = GetAttributeAlias(attrDef.Name);
            AttributeAliases.Add(alias.ToUpper());
            var attribute = new SqlQueryDocSourceAttribute(attrDef, alias);

            Attributes.Add(attribute);

            return attribute;
        }

        protected override SqlQuerySourceAttribute AddAttribute(string attrDefName)
        {
            string alias;
            SqlQuerySourceAttribute attribute;

            if (attrDefName.Length > 0 && attrDefName[0] == '&')
            {
                var ident = SystemIdentConverter.Convert(attrDefName);
                alias = GetAttributeAlias(attrDefName);
                attribute = new SqlQueryDocSourceAttribute(ident, alias);
            }
            else
            {
                var attrDef = FindAttributeDef(attrDefName);
                if (attrDef == null)
                    throw new ApplicationException(String.Format("Атрибут \"{0}\" не найден!", attrDefName));
                alias = GetAttributeAlias(attrDefName);
                attribute = new SqlQueryDocSourceAttribute(attrDef, alias);
            }
            AttributeAliases.Add(alias.ToUpper());

            Attributes.Add(attribute);

            return attribute;
        }

        protected override SqlQuerySourceAttribute AddAttribute(SystemIdent attrIdent)
        {
            var alias = GetAttributeAlias(SystemIdentConverter.Convert(attrIdent));
            SqlQuerySourceAttribute attribute = new SqlQueryDocSourceAttribute(attrIdent, alias);
            AttributeAliases.Add(alias.ToUpper());

            Attributes.Add(attribute);

            return attribute;
        }

        /*private const string SourceInnerJoinDocListAttr = @"(
SELECT dla.Document_Id, dla.Value FROM DocumentList_Attributes dla {2}
WHERE dla.Def_Id = '{0}' AND dla.Expired = '99991231') AS dla{1} ON dla{1}.Document_Id = d.Id ";*/
        private const string SourceInnerJoinDocListAttr2 = @"DocumentList_Attributes dla{1} {2}
ON (dla{1}.Document_Id = d.Id AND dla{1}.Def_Id = '{0}' AND dla{1}.Expired = '99991231'{3}) ";
        /*private const string SourceOrgRestrictCondition = @"(
EXISTS(SELECT [ObjDef_Id] FROM [OrgUnits_ObjectDefs] {2}
       WHERE [OrgUnit_Id] = '{0}' AND [ObjDef_Id] = o.[Type_Id]) OR
d.Organization_Id = '{1}')";*/

        public override SqlBuilder BuildSql(ICollection<SqlQueryCondition> conditions, bool isMain = false)
        {
            return BuildSql(Attributes, conditions, isMain);
        }

        public override SqlBuilder BuildSql(ICollection<SqlQuerySourceAttribute> attrs,
            ICollection<SqlQueryCondition> conditions, bool isMain = false)
        {
            var builder = new SqlBuilder();

            var tableMap = Mapper != null ? Mapper.Find(Source.Id) : null;
            var needToJoinDocument = isMain || (tableMap != null && NeedToJoinDocument(tableMap));

            var withOption = WithNoLock ? "WITH(NOLOCK)" : "";
            var fromAlias = tableMap != null ? "t" : "d";
            var orgAlias = (tableMap != null && !needToJoinDocument) ? "t" : "d";
            var createdAlias = (tableMap != null && tableMap.FindIdentField("Created") != null) ? "t" : "d";
            var modifiedAlias = (tableMap != null && tableMap.FindIdentField("Last_Modified") != null) ? "t" : "d";
            var userAlias = (tableMap != null && tableMap.FindIdentField("User_Id") != null) ? "t" : "d";
            var defAlias = (tableMap != null && tableMap.FindIdentField("Def_Id") != null) ? "t" : "d";
            var tableMapToDocJoin = tableMap != null && tableMap.IsVirtual ? "LEFT OUTER" : "INNER";
            var from = tableMap == null
                ? "Documents d " + withOption
                : needToJoinDocument
                    ? String.Format("{0} {1} JOIN Documents d {2} ON t.Id = d.Id", GetTableMapSql(tableMap, "t", withOption), tableMapToDocJoin, withOption)
                    : String.Format("{0}", GetTableMapSql(tableMap, "t", withOption));

//                                 ? String.Format("[{0}] t {1} INNER JOIN Documents d {1} ON t.Id = d.Id", tableMap.TableName, withOption)
//                                 : String.Format("[{0}] t {1}", tableMap.TableName, withOption);
//            var from = "Documents d";
            var orgRestriction = "";

//            if (WithNoLock) from += " " + withOption;

            if (UserId != Guid.Empty && !Source.IsPublic)
            {
                /*using (*/
                //var userRepo = new UserRepository(DataContext);/*)*/
                {
/*                    var userInfo = userRepo.GetUserInfo(UserId);

                    from += " inner join Organizations o WITH(NOLOCK) on o.Id = d.Organization_Id";
                    orgRestriction = String.Format(SourceOrgRestrictCondition, userInfo.OrgUnitTypeId,
                                                   userInfo.OrganizationId, withOption);*/
                    var userOrgs = new List<Guid>(_userRepo.GetUserAccessOrgs(UserId));
                    if (userOrgs.Count > 0)
                    {
                        foreach(var orgId in userOrgs)
                        {
                            if (orgRestriction.Length > 0) orgRestriction += ", ";
                            orgRestriction += String.Format("'{0}'", orgId);
                        }

                        orgRestriction =
                            String.Format(
                                userOrgs.Count == 1 ? "{0}.Organization_Id = {1}" : "{0}.Organization_Id IN ({1})",
                                orgAlias, orgRestriction);
                    }
                }
            }
            builder.SetFrom(from);

            builder.AddSelect(fromAlias + ".Id");
            var usedConditions = new List<SqlQueryCondition>();
            var attrIndexDictionary = new Dictionary<SqlQuerySourceAttribute, int>();
            var no = 1;

            if (attrs != null && Attributes != null)
            {
                foreach (var attr in attrs)
                {
                    var i = no;
                    if (!Attributes.Contains(attr)) continue;

                    if (attrIndexDictionary.ContainsKey(attr)) continue;

                    attrIndexDictionary.Add(attr, i);

                    var attributeConditions =
                        new List<SqlQueryCondition>(GetQuerySourceAttributeConditions(attr, conditions));
                    var attrConditionStr = SqlQueryCondition.BuildConditions(attributeConditions, "[{0}{1}].[{2}]");
                    var hasCondition = HasQuerySourceAttributeConditions(attr, conditions);

                    var sb = new ScriptStringBuilder();
                    var attribute = attr;
                    if (attr.Def != null)
                    {
                        var attrTableField = tableMap != null
                                                 ? tableMap.FindField(attribute.Def.Id) /*tableMap.Fields.FirstOrDefault(
                                                     f => f.AttrDefId == attribute.Def.Id)*/
                                                 : null;
                        var attrDef = attr.Def;
                        if (attrDef.Type.Id == (short)CissaDataType.DocList)
                        {
                            if (String.IsNullOrEmpty(attrConditionStr))
                                builder.AddJoin(
                                    SqlSourceJoinType.LeftOuter,
                                    String.Format(SourceInnerJoinDocListAttr2, attrDef.Id, i, withOption, String.Empty));
                            else
                                builder.AddJoin(
                                    SqlSourceJoinType.Inner,
                                    String.Format(SourceInnerJoinDocListAttr2, attr.Def.Id, i, withOption,
                                                  String.Format(" and " + attrConditionStr, "dla", i, "Value")));

                            sb.AppendFormat("[dla{0}].[Value] as [{1}]", i, attr.AliasName);
                        }
                        else if (attrDef.Type.Id == (short)CissaDataType.Blob)
                        {
                            var tableName = attr.GetAttrDefTableName();

                            sb.AppendFormat("[a{0}].[File_Name] as [{1}]", i, attr.AliasName);

                            if (String.IsNullOrEmpty(attrConditionStr))
                                builder.AddJoin(
                                    SqlSourceJoinType.LeftOuter,
                                    String.Format(
                                        "{0} a{1} {2} on (a{1}.Document_Id = {4}.Id and a{1}.Def_Id = '{3}' and a{1}.Expired = '99991231')",
                                        tableName, i, withOption, attr.Def.Id, fromAlias));
                            else
                            {
                                var valueFieldName = attr.Def.Type.Id == (short)CissaDataType.Text ? "Upper_" : "";
                                builder.AddJoin(
                                    SqlSourceJoinType.Inner,
                                    String.Format(
                                        "{0} a{1} {2} on (a{1}.Document_Id = {5}.Id and a{1}.Def_Id = '{3}' and a{1}.Expired = '99991231' and {4})",
                                        tableName, i, withOption, attr.Def.Id,
                                        String.Format(attrConditionStr, "a", i, valueFieldName + "Value"), fromAlias));
                            }
                        }
                        else if (attrDef.Type.Id == (short)CissaDataType.OrganizationOfDocument)
                        {
                            if (String.IsNullOrEmpty(attrConditionStr))
                            {
                                sb.AppendFormat("(select od{0}.Full_Name", i);
                                sb.AppendFormat(
                                    " from Organizations o{0} {1} inner join Object_Defs od{0} {1} on od{0}.Id = o{0}.Id\n",
                                    i,
                                    withOption);
                                sb.AppendFormat(" where o{0}.Id = {2}.Organization_Id) as [{1}]", i,
                                                attr.AliasName, orgAlias);
                            }
                            else
                            {
                                sb.AppendFormat("[od{0}].[Full_Name] as [{1}]", i, attr.AliasName);
                                builder.AddJoin(
                                    SqlSourceJoinType.Inner,
                                    String.Format("Organizations o{0} {1} on o{0}.[Id] = {2}.Organization_Id", i,
                                                  withOption, orgAlias));
                                builder.AddJoin(
                                    SqlSourceJoinType.Inner,
                                    String.Format("Object_Defs od{0} {1} on (od{0}.[Id] = o{0}.[Id] and {2})", i,
                                                  withOption,
                                                  String.Format(attrConditionStr, "od", i, "Full_Name")));
                            }
                        }
                            /*
                                                else if (attrDef.Type.Id == (short)CissaDataType.AuthorOfDocument)
                                                {
                                                    sb.AppendFormat("(select top 1 od{0}.Full_Name from Workers w{0} ", i);
                                                    sb.AppendFormat("inner join Object_Defs od{0} on od{0}.Id = w{0}.Id ", i);
                                                    sb.AppendFormat(
                                                        " where o{0}.Id = d.Organization_Id) as [{1}]",
                                                        i, attr.AliasName);
                                                }
                                                else if (attrDef.Type.Id == (short)CissaDataType.Organization)
                                                {
                                                    var tableName = attr.GetAttrDefTableName();
                                                    sb.AppendFormat("(select top 1 od{0}.Value from {1} oa{0} \n", i, tableName);
                        //                            sb.AppendFormat(" inner join Organizations o{0} on o{0}.Id = oa{0}.Value\n", i);
                        //                            sb.AppendFormat(" inner join Object_Defs od{0} on od{0}.Id = o{0}.Id\n", i);
                                                    sb.AppendFormat(
                                                        " where oa{0}.Document_Id = d.Id and oa{0}.Def_Id = '{1}' and oa{0}.Expired = convert(date, '31/12/9999', 103)) as [{2}]",
                                                        i, attrDef.Id, attr.AliasName);
                                                }
                                                else if (attrDef.Type.Id == (short)CissaDataType.DocumentState)
                                                {
                                                    var tableName = attr.GetAttrDefTableName();
                                                    sb.AppendFormat("(select top 1 od{0}.Full_Name from {1} da{0} \n", i, tableName);
                                                    sb.AppendFormat(" inner join Document_State_Types dst{0} on dst{0}.Id = da{0}.Value\n", i);
                                                    sb.AppendFormat(" inner join Object_Defs od{0} on od{0}.Id = dst{0}.Id\n", i);
                                                    sb.AppendFormat(
                                                        " where da{0}.Document_Id = d.Id and da{0}.Def_Id = '{1}' and da{0}.Expired = convert(date, '31/12/9999', 103)) as [{2}]",
                                                        i, attrDef.Id, attr.AliasName);
                                                }
                        */
                        else
                        {
                            var tableName = attr.GetAttrDefTableName();

                            if (attrTableField != null)
                            {
                                if (attrTableField.Type.HasFlag(AttributeFieldType.View))
                                    sb.AppendFormat("[{0}].[{1}] as [{2}]", fromAlias, attrTableField.FieldName,
                                                    attr.AliasName);
                                else
                                    sb.AppendFormat(
                                        "(select [a{0}].[Value] from [{1}] [a{0}] {2}\n" +
                                        "where [a{0}].Document_Id = {3}.Id and [a{0}].Def_Id = '{4}' and [a{0}].Expired = '99991231') as [{5}]",
                                        i, tableName, withOption, fromAlias, attr.Def.Id, attr.AliasName);


                                if (!String.IsNullOrEmpty(attrConditionStr))
                                {
                                    if (attrTableField.Type.HasFlag(AttributeFieldType.Search))
                                        builder.AddCondition(ExpressionOperation.And,
                                                             String.Format(attrConditionStr, fromAlias, String.Empty,
                                                                           attrTableField.FieldName));
                                    else
                                    {
                                        var valueFieldName = attr.Def.Type.Id == (short)CissaDataType.Text ? "Upper_" : "";
                                        builder.AddJoin(
                                            SqlSourceJoinType.Inner,
                                            String.Format(
                                                "{0} a{1} {2} on (a{1}.Document_Id = {5}.Id and a{1}.Def_Id = '{3}' and a{1}.Expired = '99991231' and {4})",
                                                tableName, i, withOption, attr.Def.Id,
                                                String.Format(attrConditionStr, "a", i, valueFieldName + "Value"), fromAlias));
                                    }
                                }
                            }
                            else
                            {
                                sb.AppendFormat("[a{0}].[Value] as [{1}]", i, attr.AliasName);

                                if (String.IsNullOrEmpty(attrConditionStr))
                                    builder.AddJoin(
                                        SqlSourceJoinType.LeftOuter,
                                        String.Format(
                                            "{0} a{1} {2} on (a{1}.Document_Id = {4}.Id and a{1}.Def_Id = '{3}' and a{1}.Expired = '99991231')",
                                            tableName, i, withOption, attr.Def.Id, fromAlias));
                                else
                                {
                                    var valueFieldName = attr.Def.Type.Id == (short)CissaDataType.Text ? "Upper_" : "";
                                    builder.AddJoin(
                                        SqlSourceJoinType.Inner,
                                        String.Format(
                                            "{0} a{1} {2} on (a{1}.Document_Id = {5}.Id and a{1}.Def_Id = '{3}' and a{1}.Expired = '99991231' and {4})",
                                            tableName, i, withOption, attr.Def.Id,
                                            String.Format(attrConditionStr, "a", i, valueFieldName + "Value"), fromAlias));
                                }
                            }
                        }
                    }
                    else if (attr.Ident == SystemIdent.State || attr.Ident == SystemIdent.InState)
                    {
                        var stateTableField = tableMap != null ? tableMap.FindIdentField("State") : null;

                        if (stateTableField != null)
                        {
                            sb.AppendFormat("[{0}].[State] as [{1}]", fromAlias, attr.AliasName);
                            if (!String.IsNullOrEmpty(attrConditionStr))
                                builder.AddCondition(ExpressionOperation.And,
                                    String.Format(attrConditionStr, fromAlias, String.Empty,
                                        stateTableField.FieldName));
                        }
                        else
                        {
                            sb.AppendFormat("[ds{0}].[State_Type_Id] as [{1}]", i, attr.AliasName);
                            if (String.IsNullOrEmpty(attrConditionStr))
                            {
                                //sb.AppendFormat("(select ds{0}.State_Type_Id from Document_States ds{0} {1}\n", i,
                                //    withOption);
                                //sb.AppendFormat(
                                //    " where ds{0}.Document_Id = {2}.Id and ds{0}.Expired = '99991231') as [{1}]",
                                //    i, attr.AliasName, fromAlias);
                                builder.AddJoin(
                                    SqlSourceJoinType.LeftOuter,
                                    String.Format(
                                        "[Document_States] ds{0} {1} on (ds{0}.Document_Id = {2}.Id and ds{0}.Expired = '99991231')",
                                        i, withOption, fromAlias));
                            }
                            else
                            {
                                builder.AddJoin(
                                    SqlSourceJoinType.Inner,
                                    String.Format(
                                        "[Document_States] ds{0} {1} on (ds{0}.Document_Id = {3}.Id and ds{0}.Expired = '99991231' and {2})",
                                        i, withOption,
                                        String.Format(attrConditionStr, "ds", i, "State_Type_Id"), fromAlias));
                            }
                        }
                    }
                    else if (attr.Ident == SystemIdent.StateDate)
                    {
                        if (String.IsNullOrEmpty(attrConditionStr))
                        {
                            sb.AppendFormat("(select ds{0}.Created from Document_States ds{0} {1}\n", i, withOption);
                            sb.AppendFormat(
                                " where ds{0}.Document_Id = {2}.Id and ds{0}.Expired = '99991231') as [{1}]",
                                i, attr.AliasName, fromAlias);
                        }
                        else
                        {
                            sb.AppendFormat("[ds{0}].[Created] as [{1}]", i, attr.AliasName);
                            builder.AddJoin(
                                SqlSourceJoinType.Inner,
                                String.Format(
                                    "[Document_States] ds{0} {1} on (ds{0}.Document_Id = {3}.Id and ds{0}.Expired = '99991231' and {2})",
                                    i, withOption,
                                    String.Format(attrConditionStr, "ds", i, "Created"), fromAlias));
                        }
                    }
                    else if (attr.Ident == SystemIdent.Id)
                    {
                        sb.AppendFormat("{0}.Id as [{1}]", fromAlias, attr.AliasName);
                        if (!String.IsNullOrEmpty(attrConditionStr))
                            builder.AddCondition(ExpressionOperation.And,
                                                 String.Format(attrConditionStr, fromAlias, String.Empty, "Id"));
                    }
                    else if (attr.Ident == SystemIdent.OrgId)
                    {
                        sb.AppendFormat("{0}.Organization_Id as [{1}]", orgAlias, attr.AliasName);
                        if (!String.IsNullOrEmpty(attrConditionStr))
                            builder.AddCondition(ExpressionOperation.And,
                                                 String.Format(attrConditionStr, orgAlias, "", "Organization_Id"));
                    }
                    else if (attr.Ident == SystemIdent.Created)
                    {
                        sb.AppendFormat("{0}.Created as [{1}]", createdAlias, attr.AliasName);
                        if (!String.IsNullOrEmpty(attrConditionStr))
                            builder.AddCondition(ExpressionOperation.And,
                                                 String.Format(attrConditionStr, createdAlias, "", "Created"));
                    }
                    else if (attr.Ident == SystemIdent.Modified)
                    {
                        sb.AppendFormat("{0}.Last_Modified as [{1}]", modifiedAlias, attr.AliasName);
                        if (!String.IsNullOrEmpty(attrConditionStr))
                            builder.AddCondition(ExpressionOperation.And,
                                                 String.Format(attrConditionStr, modifiedAlias, "", "Last_Modified"));
                    }
                    else if (attr.Ident == SystemIdent.UserId || attr.Ident == SystemIdent.UserName)
                    {
                        sb.AppendFormat("{0}.UserId as [{1}]", userAlias, attr.AliasName);
                        if (!String.IsNullOrEmpty(attrConditionStr))
                            builder.AddCondition(ExpressionOperation.And,
                                                 String.Format(attrConditionStr, userAlias, "", "UserId"));
                    }
                    else if (attr.Ident == SystemIdent.DefId)
                    {
                        sb.AppendFormat("{0}.Def_Id as [{1}]", defAlias, attr.AliasName);
                        if (!String.IsNullOrEmpty(attrConditionStr))
                            builder.AddCondition(ExpressionOperation.And,
                                                 String.Format(attrConditionStr, defAlias, "", "Def_Id"));
                    }
                    /*else if (attr.Ident == SystemIdent.UserName)
                    {
                        if (String.IsNullOrEmpty(attrConditionStr))
                        {
                            sb.AppendFormat("(select w{0}.User_Name from Workers w{0} {1}\n", i, withOption);
                            sb.AppendFormat(" where w{0}.Id = {2}.UserId) as [{1}]", i, attr.AliasName, userAlias);
                        }
                        else
                        {
                            sb.AppendFormat("[w{0}].[User_Name] as [{1}]", i, attr.AliasName);
                            builder.AddJoin(
                                SqlSourceJoinType.Inner,
                                String.Format("Workers w{0} {1} on (w{0}.Id = {3}.UserId and {2})", i, withOption,
                                              String.Format(attrConditionStr, "w", i, "User_Name"), userAlias));
                        }
                    }*/
                    else if (attr.Ident == SystemIdent.OrgCode)
                    {
                        if (String.IsNullOrEmpty(attrConditionStr))
                        {
                            sb.AppendFormat("(select o{0}.Code from Organizations o{0} {1}\n", i, withOption);
                            sb.AppendFormat(" where o{0}.Id = {2}.Organization_Id) as [{1}]", i,
                                            attr.AliasName, orgAlias);
                        }
                        else
                        {
                            sb.AppendFormat("[o{0}].[Code] as [{1}]", i, attr.AliasName);
                            builder.AddJoin(
                                SqlSourceJoinType.Inner,
                                String.Format("Organizations o{0} {1} on (o{0}.Id = {3}.Organization_Id and {2})", i, withOption,
                                              String.Format(attrConditionStr, "o", i, "Code"), orgAlias));
                        }
                    }
                    else if (attr.Ident == SystemIdent.OrgName)
                    {
                        if (String.IsNullOrEmpty(attrConditionStr))
                        {
                            sb.AppendFormat(
                                "(select od{0}.Full_Name\n" +
                                " from Organizations o{0} {1} inner join Object_Defs od{0} {1} on od{0}.Id = o{0}.Id\n",
                                i, withOption);
                            sb.AppendFormat(" where o{0}.Id = {2}.Organization_Id) as [{1}]", i,
                                            attr.AliasName, orgAlias);
                        }
                        else
                        {
                            sb.AppendFormat("[od{0}].[Full_Name] as [{1}]", i, attr.AliasName);
                            builder.AddJoin(
                                SqlSourceJoinType.Inner,
                                String.Format("Organizations o{0} {1} on o{0}.[Id] = {2}.Organization_Id", i,
                                              withOption, orgAlias));
                            builder.AddJoin(
                                SqlSourceJoinType.Inner,
                                String.Format("Object_Defs od{0} {1} on od{0}.[Id] = o{0}.[Id] and {2}", i,
                                              withOption,
                                              String.Format(attrConditionStr, "od", i, "Full_Name")));
                        }
                    }
                    else
                        throw new ApplicationException("Не известный атрибут в условии запроса!");

                    no++;
                    builder.AddSelect(sb);
                    usedConditions.AddRange(attributeConditions);
                }
            }
            //if (builder.Select == null || builder.Select.Count == 0) builder.AddSelect("d.Id");

            if (tableMap == null)
            {
                var descendants = new List<Guid>(_docDefRepo.GetDocDefDescendant(Source.Id));
                if (descendants.Count < 2)
                    builder.AddCondition(ExpressionOperation.And,
                        String.Format("{0}.Def_Id = '{1}'", fromAlias, Source.Id));
                else
                {
                    var s = String.Empty;
                    foreach (var defId in descendants)
                    {
                        if (!String.IsNullOrEmpty(s)) s += ",";
                        s += String.Format("'{0}'", defId);
                    }
                    builder.AddCondition(ExpressionOperation.And, String.Format("{0}.Def_Id IN ({1})", fromAlias, s));
                }
            }
            if (!String.IsNullOrEmpty(orgRestriction))
                builder.AddCondition(ExpressionOperation.And, orgRestriction);
            if (isMain || (needToJoinDocument && (tableMap == null || !tableMap.IsVirtual)))
                builder.AddCondition(ExpressionOperation.And, "([d].[Deleted] is null OR [d].[Deleted] = 0)");

            BuildConditions(builder, conditions, attrIndexDictionary, tableMap, needToJoinDocument, usedConditions);

            return builder;
        }

        private string GetTableMapSql(DocumentTableMap tableMap, string alias, string option)
        {
            if (tableMap.Ancestor == null)
                return String.Format("[{0}] {1} {2}", tableMap.TableName, alias, option);

            var sql = new SqlBuilder();
            BuildTableMapSql(sql, tableMap, option, 1);

            return String.Format("({0}) {1}", sql, alias);
        }

        private void BuildTableMapSql(SqlBuilder sql, DocumentTableMap tableMap, string option, int no)
        {
            if (no == 1)
                sql.SetFrom("[{0}] [tm{1}] {2}", tableMap.TableName, no, option);
            else
                sql.AddJoin(SqlSourceJoinType.Inner, "[{0}] [tm{1}] {2} on [tm{1}].[Id] = [tm1].[Id]", tableMap.TableName, no, option);

            if (no == 1) sql.AddSelect("[tm{0}].[Id]", no);

            foreach (var field in tableMap.Fields)
            {
                sql.AddSelect("[tm{0}].[{1}]", no, field.FieldName);
            }

            if (tableMap.Ancestor != null)
                BuildTableMapSql(sql, tableMap.Ancestor, option, no + 1);
        }

        private void BuildConditions(SqlBuilder builder, ICollection<SqlQueryCondition> conditions, Dictionary<SqlQuerySourceAttribute, int> attrIndexDictionary, 
            DocumentTableMap tableMap, bool needToJoinDocument, ICollection<SqlQueryCondition> usedConditions)
        {
            if (conditions != null && conditions.Count > 0)
            {
                foreach (var cond in conditions)
                {
                    if (usedConditions.Contains(cond)) continue;

                    if (cond.Condition == ConditionOperation.Exp)
                    {
                        if (cond.Conditions == null) continue;

                        var sources = GetConditionListSources(cond.Conditions);
                        if (sources.Count == 1 && sources[0] == this)
                        {
                            builder.AddWhereExp(cond.Operation);
                            try
                            {
                                BuildConditions(builder, cond.Conditions, attrIndexDictionary, tableMap,
                                    needToJoinDocument, usedConditions);
                            }
                            finally
                            {
                                builder.EndWhereExp();
                            }
                        }
                    }
                    else if (cond.Source == this)
                    {
                        //builder.AddJoin(SqlSourceJoinType.Inner, cond.GetConditionSubQuery(i++));
                        var sql = cond.BuildSourceCondition(attrIndexDictionary, tableMap, needToJoinDocument);
                        builder.AddCondition(cond.Operation, sql);
                    }
                }
            }
        }

        private static IEnumerable<SqlQueryCondition> GetQuerySourceAttributeConditions(SqlQuerySourceAttribute attribute,
            IEnumerable<SqlQueryCondition> conditions)
        {
            if (conditions == null) yield break;

            foreach (var condition in conditions)
            {
                var b = false;

                if ((condition.Condition == ConditionOperation.Exp ||
                     condition.Condition == ConditionOperation.Include) &&
                    condition.Conditions != null)
                {
                    b = condition.Conditions.All(
                        c => c.Condition != ConditionOperation.Exp && c.Condition != ConditionOperation.Include &&
                             c.Left.Attributes != null && c.Left.Attributes.All(a => a.Attribute == attribute) &&
                             (!c.Right.IsAttribute || c.Right.Attributes.All(a => a.Attribute == attribute))) &&
                        !condition.Conditions.Any(
                            c => c.Condition == ConditionOperation.IsNull || c.Condition == ConditionOperation.IsNotNull);
                }
                else if (condition.Attributes != null)
                {
                    b = condition.Left.Attributes.All(attr => attr.Attribute == attribute) &&
                        (!condition.Right.IsAttribute ||
                         condition.Right.Attributes.All(a => a.Attribute == attribute)) &&
                        !(condition.Condition == ConditionOperation.IsNull ||
                          condition.Condition == ConditionOperation.IsNotNull);
                }

/*
                    if (condition.SubQuery != null)
                    {
                        //condition.Left.SubQuery.Attribute.Attributes
                        continue;
                    }
*/
                if (b) yield return condition;
            }
        }

        private static bool HasQuerySourceAttributeConditions(SqlQuerySourceAttribute attribute,
            IEnumerable<SqlQueryCondition> conditions)
        {
            if (conditions == null) return false;

            foreach (var condition in conditions)
            {
                var b = false;

                if ((condition.Condition == ConditionOperation.Exp ||
                     condition.Condition == ConditionOperation.Include) &&
                    condition.Conditions != null)
                {
                    b = condition.Conditions.All(
                        c => c.Left.Attributes != null && c.Left.Attributes.All(a => a.Attribute == attribute) &&
                             (!c.Right.IsAttribute || c.Right.Attributes.All(a => a.Attribute == attribute)));
                }
                else if (condition.Attributes != null)
                {
                    b = condition.Left.Attributes.All(attr => attr.Attribute == attribute) &&
                        (!condition.Right.IsAttribute ||
                         condition.Right.Attributes.All(a => a.Attribute == attribute));
                }
                if (b) return true;
            }
            return false;
        }

        private bool NeedToJoinDocument(DocumentTableMap tableMap)
        {
            var orgField = tableMap.FindIdentField("Organization_Id");

            if (orgField == null && UserId != Guid.Empty && !Source.IsPublic) return true;

            var createdField = tableMap.FindIdentField("Created");
            var lastModifiedField = tableMap.FindIdentField("Last_Modified");
            var userField = tableMap.FindIdentField("User_Id");

            foreach (var attribute in Attributes)
            {
                if (attribute.Def == null)
                {
                    switch (attribute.Ident)
                    {
                        case SystemIdent.DefId:
                            return true;
                        case SystemIdent.Modified:
                            if (lastModifiedField == null) return true;
                            break;
                        case SystemIdent.Created:
                            if (createdField == null) return true;
                            break;
                        case SystemIdent.OrgId:
                        case SystemIdent.OrgCode:
                        case SystemIdent.OrgName:
                            if (orgField == null) return true;
                            break;
                        case SystemIdent.UserId:
                        case SystemIdent.UserName:
                            if (userField == null) return true;
                            break;
                    }
                }
                /*else
                {
                    var fieldMap = tableMap.FindField(attribute.Def.Id);
                    if (fieldMap == null) return true;
                }*/
            }
            return false;
        }

    }
}