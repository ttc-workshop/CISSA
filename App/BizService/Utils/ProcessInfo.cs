using System;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.BizService.Utils
{
    [DataContract]
    public class ProcessInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Guid UserId { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string OrgName { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public TimeSpan Duration { get; set; }
    }
}