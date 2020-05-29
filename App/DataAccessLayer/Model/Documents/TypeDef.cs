using System.Runtime.Serialization;
using Intersoft.CISSA.DataAccessLayer.Model.Data;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents
{
    [DataContract]
    public class TypeDef
    {
        public TypeDef() {}

        public TypeDef(Data_Type data)
        {
            if (data != null)
            {
                Id = data.Id;
                Name = data.Name;
            }
        }

        public TypeDef(Attribute_Def data)
        {
            if (data != null)
            {
                Id = data.Type_Id ?? 0;
                
                if (!data.Data_TypesReference.IsLoaded) data.Data_TypesReference.Load();
                if (data.Data_Types != null)
                    Name = data.Data_Types.Name;
            }
        }

        [DataMember]
        public short Id { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
