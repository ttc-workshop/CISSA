using System.Configuration;

namespace ProcessService.Models.Config
{
    public class ProcessesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("processes")]
        public ProcessElementCollection Processes
        {
            get { return (ProcessElementCollection) base["processes"]; }
        }
    }
}