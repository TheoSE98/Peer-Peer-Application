using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientServerInterface
{
    [ServiceContract]
    public interface IServerService
    {
        [OperationContract]
        string PostJob(string jobCode);
    }
}