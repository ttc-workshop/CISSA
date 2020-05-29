using System.Configuration;

namespace ProcessService.Models.Config
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