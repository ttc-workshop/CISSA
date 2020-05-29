using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Intersoft.CISSA.BizService.Defs
{
    [DataContract]
    public class BizResult
    {
        public enum BizResultType {Error, Warning, Message, User, Form, Report, Query, Action}

        [DataMember]
        public BizResultType Type { get; set; }
        [DataMember]
        public Guid Id { get; set; }
        [DataMember]
        public string Message { get; set; }
/*
        [DataMember]
        public BizObject Object { get; set; } */
    }
}