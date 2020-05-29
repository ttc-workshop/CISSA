namespace Intersoft.CISSA.DataAccessLayer.Model.Query
{
    public enum ConditionOperation
    {
        Equal, 
        NotEqual, 
        GreatThen, 
        GreatEqual, 
        LessThen, 
        LessEqual, 
        Contains,
        NotContains,
        Like, 
        NotLike,
        Between,
        NotBetween,
        In,
        NotIn,
        IsNull, 
        IsNotNull,
        Include,
        Is,
        NotIs,
        Exp,
        Levenstein
    }
}
