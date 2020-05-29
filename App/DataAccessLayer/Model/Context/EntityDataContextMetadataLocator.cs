using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Intersoft.CISSA.DataAccessLayer.Model.Context
{
    public class EntityDataContextMetadataLocator
    {
        public static string[] GetPath()
        {
            return HttpRuntime.AppDomainAppId != null
                ? new[]
                {
                    @"~\Model\Data\DataModel.csdl",
                    @"~\Model\Data\DataModel.ssdl",
                    @"~\Model\Data\DataModel.msl"
                }
                : new[]
                {
                    @".\Model\Data\DataModel.csdl",
                    @".\Model\Data\DataModel.ssdl",
                    @".\Model\Data\DataModel.msl"
                };
        }
    }
}
