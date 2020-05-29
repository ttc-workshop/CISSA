using System;

namespace Intersoft.CISSA.DataAccessLayer.Model.Security
{
    public class BizObjectPermission
    {
        public Guid ObjectId { get; set; }
        public BizObjectType BizObjectType { get; set; }
        public BizPermission Permission { get; set; }

        public BizObjectPermission()
        {
            Permission = new BizPermission();
        }
    }
}