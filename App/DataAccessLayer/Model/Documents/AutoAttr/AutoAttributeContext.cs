using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Documents.AutoAttr
{
    public class AutoAttributeContext
    {
        public Doc CurrentDoc { get; set; }
        public Guid CurrentUserId { get; set; }
    }
}
