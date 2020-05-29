using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Intersoft.CISSA.UserApp.ServiceReference;

namespace Intersoft.CISSA.UserApp.Models.Application.ContextStates
{
    public interface IFormContextState
    {
        Guid GetFormId();
        BizForm GetForm();

        BizControl FindControl(Guid id);
    }
}
