using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using APIModels;

namespace ClientServerInterface
{
    [ServiceContract]
    public interface IServerService
    {
        [OperationContract]
        string PostJob(string jobCode);
        [OperationContract]
        List<JobModel> GetJobs();
        [OperationContract]
        void AddJobToQueue(JobModel job);
    }
}