using System.Configuration;

namespace ProcessService.Models.Config
{
    [ConfigurationCollection(typeof(ParamElement))]
    public class ParamElementCollection : ConfigurationElementCollection
    {
        public ParamElement this[string name]
        {
            get { return (ParamElement) BaseGet(name); }
        }

        public ParamElement this[int index]
        {
            get { return (ParamElement) BaseGet(index); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ParamElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ParamElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "param"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }
    }
}
