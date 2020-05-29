using Intersoft.CISSA.DataAccessLayer.Model.Query;

namespace Intersoft.CISSA.DataAccessLayer.Model
{
    public enum CompareOperation
    {
        Equal = 0,
        Great = 1, 
        Less = 2,
        GreatEqual = 3,
        LessEqual = 4,
        NotEqual = 5, 
        Like = 6, 
        StartWith = 7,
        EndWith = 8,
        NotLike = 9,
        Levenstein = 10,
        IsNull = 11,
        IsNotNull = 12,
        NotApplied = 13
    }

    public static class CompareOperationConverter
    {
        public static ConditionOperation CompareToCondition(CompareOperation compare)
        {
            switch (compare)
            {
                case CompareOperation.Equal:
                    return ConditionOperation.Equal;
                case CompareOperation.NotEqual:
                    return ConditionOperation.NotEqual;
                case CompareOperation.Great:
                    return ConditionOperation.GreatThen;
                case CompareOperation.GreatEqual:
                    return ConditionOperation.GreatEqual;
                case CompareOperation.Less:
                    return ConditionOperation.LessThen;
                case CompareOperation.LessEqual:
                    return ConditionOperation.LessEqual;
                case CompareOperation.Like:
                    return ConditionOperation.Like;
                case CompareOperation.NotLike:
                    return ConditionOperation.NotLike;
                case CompareOperation.Levenstein:
                    return ConditionOperation.Levenstein;
                case CompareOperation.IsNull:
                    return ConditionOperation.IsNull;
                case CompareOperation.IsNotNull:
                    return ConditionOperation.IsNotNull;
                default:
                    return ConditionOperation.Equal;
            }
        }

        public static ConditionOperation CompareToCondition(short compare)
        {
            switch (compare)
            {
                case (short) CompareOperation.Equal:
                    return ConditionOperation.Equal;
                case (short) CompareOperation.NotEqual:
                    return ConditionOperation.NotEqual;
                case (short)CompareOperation.Great:
                    return ConditionOperation.GreatThen;
                case (short) CompareOperation.GreatEqual:
                    return ConditionOperation.GreatEqual;
                case (short) CompareOperation.Less:
                    return ConditionOperation.LessThen;
                case (short) CompareOperation.LessEqual:
                    return ConditionOperation.LessEqual;
                case (short) CompareOperation.Like:
                    return ConditionOperation.Like;
                case (short) CompareOperation.NotLike:
                    return ConditionOperation.NotLike;
                case (short) CompareOperation.Levenstein:
                    return ConditionOperation.Levenstein;
                case (short) CompareOperation.IsNull:
                    return ConditionOperation.IsNull;
                case (short) CompareOperation.IsNotNull:
                    return ConditionOperation.IsNotNull;
                default:
                    return ConditionOperation.Equal;
            }
        }
    }
}