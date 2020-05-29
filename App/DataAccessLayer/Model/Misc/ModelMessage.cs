using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Controls;
using Intersoft.CISSA.DataAccessLayer.Model.Documents;

namespace Intersoft.CISSA.DataAccessLayer.Model.Misc
{
    [DataContract]
    public class ModelMessage
    {
        [DataMember]
        public Guid Key { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

/*    [DataContract]
    public class ModelMessages: List<ModelMessage>
    {
    }*/

    public class ModelMessageBuilder
    {
        private readonly List<ModelMessage> _messages;

        public List<ModelMessage> Messages { get { return _messages; } }

        public ModelMessageBuilder()
        {
            _messages = new List<ModelMessage>();
        }

        public ModelMessageBuilder(List<ModelMessage> list)
        {
            _messages = list;
        }

        public ModelMessage AddMessage(Guid key, string message)
        {
            var result = new ModelMessage {Key = key, Message = message};
            Messages.Add(result);
            return result;
        }

        public ModelMessage AddMessage(string name, string message)
        {
            var result = new ModelMessage { Name = name, Message = message };
            Messages.Add(result);
            return result;
        }

        public ModelMessage AddMessage(Guid key, string name, string message)
        {
            var result = new ModelMessage { Key = key, Name = name, Message = message };
            Messages.Add(result);
            return result;
        }

        public ModelMessage AddDocMessage(Doc doc, string name, string message)
        {
            if (doc != null)
            {
                var attr = doc.GetAttributeByName(name);
                if (attr != null && attr.AttrDef != null) 
                    return AddMessage(attr.AttrDef.Id, name, message);
            }

            return AddMessage(name, message);
        }

        public ModelMessage AddFormMessage(BizForm form, string name, string message)
        {
            if (form != null)
            {
                var finder = new ControlFinder(form);
                var ctrl =
                    finder.FirstOrDefault(
                        c =>
                            c is BizDataControl &&
                            String.Equals(((BizDataControl)c).AttributeName, name,
                                StringComparison.InvariantCultureIgnoreCase));
                if (ctrl != null)
                    return AddMessage(ctrl.Id, name, message);
            }

            return AddMessage(name, message);
        }

        public ModelMessage AddFormMessage(BizForm form, Guid attrId, string message)
        {
            if (form != null)
            {
                var finder = new ControlFinder(form);
                var ctrl =
                    finder.FirstOrDefault(
                        c =>
                            c is BizDataControl && ((BizDataControl) c).AttributeDefId == attrId);
                if (ctrl != null)
                    return AddMessage(ctrl.Id, message);
            }

            return AddMessage(attrId, message);
        }

    }
}
