using System;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Data;
using Intersoft.CISSA.DataAccessLayer.Utils;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    public class ActivityLink
    {
        public ActivityLink(Activity_Link link)
        {
            SourceId = link.Source_Id;
            TargetId = link.Target_Id;

            Condition = link.Condition;

            /* if (!link.User_ActionsReference.IsLoaded) link.User_ActionsReference.Load();

            UserAction = link.User_Actions == null
                             ? null
                             : new UserAction
                                   {
                                       Id = link.User_Action_Id,
                                       Name = link.User_Actions.Full_Name ?? link.User_Actions.Name
                                   };*/
            UserActionId = link.User_Action_Id;
        }

        [DataMember]
        public Guid? SourceId { get; private set; }
        [DataMember]
        public Guid? TargetId { get; private set; }
        [DataMember]
        public string Condition { get; private set; }
//        [DataMember]
//        public UserAction UserAction { get; private set; }
        [DataMember]
        public Guid? UserActionId { get; private set; }

        public virtual bool HasConditionScript()
        {
            if (String.IsNullOrEmpty(Condition)) return false;

            var parser = new CsParser(Condition);
            while (parser.NextToken() != TokenType.Eof)
            {
                if (parser.Token != TokenType.Comment &&
                    parser.Token != TokenType.LineComment &&
                    parser.Token != TokenType.Eof) return true;
            }
            return false;
        }

        public virtual bool HasCondition()
        {
            if (UserActionId != null) return true;
            return HasConditionScript();
        }

        public virtual bool CheckCondition(WorkflowContext context)
        {
            var result = true;

            if (/*!String.IsNullOrEmpty(Condition)*/ HasConditionScript()) result = Convert.ToBoolean(context.ExecuteExpression(Condition));

            if (!result) return false;

            return UserActionId == null || context.UserActionId == UserActionId;
        }
    }
}