using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Workflow
{
    [DataContract]
    public class UserAction
    {
        [DataMember]
        public Guid Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string DefaultName { get; set; }
    }
}
