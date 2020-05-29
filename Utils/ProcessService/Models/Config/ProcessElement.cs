using System;
using System.Configuration;

namespace ProcessService.Models.Config
{
    public class ProcessElement : ConfigurationElement
    {
        [ConfigurationProperty("id", IsKey = true, IsRequired = true)]
        public Guid Id
        {
            get { return (Guid)base["id"]; }
            set { base["id"] = value; }
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("params")]
        public ParamElementCollection Params
        {
            get { return (ParamElementCollection)base["params"]; }
        }
    }
}