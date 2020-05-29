using System.Configuration;

namespace ProcessExecutor.Models.Config
{
    public class ParamElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string) base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("value")]
        public string Value
        {
            get { return (string) base["value"]; }
            set { base["value"] = value; }
        }
    }
}