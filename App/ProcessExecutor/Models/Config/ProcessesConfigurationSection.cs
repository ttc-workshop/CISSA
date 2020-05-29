using System.Configuration;

namespace ProcessExecutor.Models.Config
{
    public class ProcessesConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ProcessElementCollection Processes
        {
            get { return (ProcessElementCollection) base[""]; }
        }
    }
}