
using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Query.Interfaces
{
    public interface IQueryExpression
    {
//        IQuerySource GetSource(string attributePath);

//        IQueryCondition AndExp(string attribute);
//        IQueryCondition NotAndExp(string attribute);
//        IQueryCondition OrExp(string attribute);
//        IQueryCondition NotOrExp(string attribute);
//        IQueryExpression EndExp();
        IQueryCondition And(string attribute, string source = null);
        IQueryCondition Or(string attribute, string source = null);
        IQueryCondition AndNot(string attribute, string source = null);
        IQueryCondition OrNot(string attribute, string source = null);

        IQueryCondition And(Guid attributeId, string source = null);
        IQueryCondition Or(Guid attributeId, string source = null);
        IQueryCondition AndNot(Guid attributeId, string source = null);
        IQueryCondition OrNot(Guid attributeId, string source = null);

        IQueryCondition AndExp(string attribute, string source = null);
        IQueryCondition OrExp(string attribute, string source = null);
        IQueryCondition AndNotExp(string attribute, string source = null);
        IQueryCondition OrNotExp(string attribute, string source = null);

        IQueryExpression End();
/*
        IQueryExpression AndExp(string attribute, string exp = null);
        IQueryExpression OrExp(string attribute, string exp = null);
        IQueryExpression AndNotExp(string attribute, string exp = null);
        IQueryExpression OrNotExp(string attribute, string exp = null);
*/
    }
}