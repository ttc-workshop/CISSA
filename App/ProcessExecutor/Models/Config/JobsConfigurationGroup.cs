using System.Configuration;

namespace ProcessExecutor.Models.Config
{
    public class JobsConfigurationGroup : ConfigurationSectionGroup
    {
        [ConfigurationProperty("Processes")]
        public ProcessesConfigurationSection Processes
        {
            get { return (ProcessesConfigurationSection) Sections["processes"]; }
        }
    }
}