using System;

namespace Intersoft.CISSA.UserApp.Models.Application
{
    public class StateBlobData
    {
        public Guid DocumentId { get; private set; }
        public Guid AttributeDefId { get; private set; }
        public byte[] Data { get; set; }
        public byte[] NewData { get; set; }
        public string FileName { get; set; }
        public string NewFileName { get; set; }

        public StateBlobData(Guid docId, Guid attrDefId, byte[] data, string fileName)
        {
            DocumentId = docId;
            AttributeDefId = attrDefId;
            Data = data;
            FileName = fileName;
        }
    }
}