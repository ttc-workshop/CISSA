using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Query.Helpers;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Def
{
    [DataContract]
    public class QueryConditionDef : QueryItemDef
    {
        [DataMember]
        public ExpressionOperation Operation { get; set; }

        [DataMember]
        public ConditionOperation Condition { get; set; }

        [DataMember]
        public List<QueryConditionDef> Conditions { get; private set; }

        [DataMember]
        public QueryConditionPartDef Left { get; set; }
        [DataMember]
        public QueryConditionPartDef Right { get; set; }

        /*
                [DataMember]
                public QuerySourceDef Source { get; set; }

                [DataMember]
                public string AttributeName { get; set; }
                [DataMember]
                public Guid AttributeId { get; set; }

                [DataMember]
                public string Exp { get; set; }

                [DataMember]
                public string SourceAlias { get; set; }
        */

       /* [DataMember]
        public object[] Values { get; set; }

        [DataMember]
        public QueryDef SubQueryDef { get; set; }

        [DataMember]
        public string SubQueryAttribute { get; set; }*/

        public QueryConditionDef()
        {
            Conditions = new List<QueryConditionDef>();
            Left = new QueryConditionPartDef();
            Right = new QueryConditionPartDef();
        }

        public string AttributeName
        {
            get
            {
                return Left != null && Left.Attribute != null
                    ? new QueryAttributeDefHelper(Left.Attribute).AttributeName
                    : String.Empty;
            }
        }
        public QuerySourceDef AttributeSourceDef
        {
            get
            {
                return Left != null && Left.Attribute != null
                    ? new QueryAttributeDefHelper(Left.Attribute).GetSourceDef()
                    : null;
            }
        }

        public Guid AttributeId
        {
            get
            {
                return Left != null && Left.Attribute != null
                    ? new QueryAttributeDefHelper(Left.Attribute).AttributeId
                    : Guid.Empty;
            }
        }

        public QueryDef SubQueryDef
        {
            get { return Right != null && Right.SubQuery != null ? Right.SubQuery.QueryDef : null; }
        }

        public string SubQueryAttribute
        {
            get
            {
                return Right != null && Right.SubQuery != null
                    ? new QueryAttributeDefHelper(Right.SubQuery.Attribute).AttributeName
                    : String.Empty;
            }
        }

        public object[] Values
        {
            get
            {
                return Right != null ? Right.Values : null;
            }
        }

        public object Value { get { return Right != null && Right.Values != null && Right.Values.Length > 0 ? Right.Values[0] : null; } }
        public object Value2 { get { return Right != null && Right.Values != null && Right.Values.Length > 1 ? Right.Values[1] : null; } }
    }
}
