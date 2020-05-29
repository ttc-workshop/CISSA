using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Intersoft.CISSA.BizService.Defs
{
    [ServiceContract]
    interface IPresentationManager
    {
        [OperationContract]
        BizObject GetBizObject(Guid userId, Guid objectId);
        [OperationContract]
        BizObjectType GetBizObjectType(Guid userId, Guid objectId);
        [OperationContract]
        List<BizObject> GetBizObjectChildren(Guid userId, Guid objectId);

        [OperationContract]
        List<BizObject> GetMainFormObjects(Guid userId);

        [OperationContract]
        BizControlData GetBizControlData(Guid userId, Guid controlId);

        [OperationContract]
        BizResult ExecuteBizAction(Guid userId, Guid actionId);

/*        [OperationContract]
        BizDocumentAttributeData GetBizDocumentAttributeData(Guid userId, Guid documentId, Guid attributeId);*/
    }
}