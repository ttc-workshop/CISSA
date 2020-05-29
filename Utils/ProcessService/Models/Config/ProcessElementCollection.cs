using System.Configuration;

namespace ProcessService.Models.Config
{
    [ConfigurationCollection(typeof(ProcessElement))]
    public class ProcessElementCollection : ConfigurationElementCollection
    {
        public ProcessElement this[string name]
        {
            get { return (ProcessElement)BaseGet(name); }
        }

        public ProcessElement this[int index]
        {
            get { return (ProcessElement)BaseGet(index); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProcessElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProcessElement)element).Id;
        }

        protected override string ElementName
        {

            get { return "process"; }

        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
    }
}