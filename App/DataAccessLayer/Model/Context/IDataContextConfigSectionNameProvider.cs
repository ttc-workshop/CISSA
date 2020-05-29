namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public interface IDataContextConfigSectionNameProvider
    {
        string GetSectionName();
    }

    public class DataContextConfigSectionNameProvider : IDataContextConfigSectionNameProvider
    {
        public string SectionName { get; private set; }

        public DataContextConfigSectionNameProvider(string sectionName)
        {
            SectionName = sectionName;
        }

        public string GetSectionName()
        {
            return SectionName;
        }
    }
}