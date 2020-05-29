using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Intersoft.CISSA.DataAccessLayer.Model.Controls
{
    [Flags]
    public enum BizControlOptionFlags
    {
        Hidden = 1,
        ReadOnly = 2,
        Disabled = 4
    }

    [DataContract]
    public class BizControlOption
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string AttributeName { get; set; }

        [DataMember]
        public BizControlOptionFlags Flags { get; set; }

        [DataMember]
        public string Caption { get; set; }
    }

    [DataContract]
    public class BizFormOptions
    {
        [DataMember]
        public Guid Id { get; set; }

        private List<BizControlOption> _options = new List<BizControlOption>();

        [DataMember]
        public List<BizControlOption> Options
        {
            get { return _options; }
            set
            {
                if (_options == null) _options = new List<BizControlOption>();
                _options.Clear();
                _options.AddRange(new List<BizControlOption>(value));
            }
        }

        [DataMember]
        public string Caption { get; set; }
    }
}
