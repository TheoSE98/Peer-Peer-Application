using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientInterface
{
    [ServiceContract]
    internal interface IServerService
    {
        [OperationContract]
        void SubmitJob(string code);

        [OperationContract]
        List<string> GetAvailableJobs();
    }
}