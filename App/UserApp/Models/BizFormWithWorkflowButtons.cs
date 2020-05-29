using System.Collections.Generic;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models
{
    public class BizFormWithWorkflowButtons
    {
        public BizForm BizForm { get; set; }
        public IList<UserAction> UserActions { get; set; }
    }
}