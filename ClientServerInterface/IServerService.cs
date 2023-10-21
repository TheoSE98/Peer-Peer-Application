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
        Task<string> PostJob(string jobCode);
        [OperationContract]
        Task<List<JobModel>> GetJobs();
        [OperationContract]
        Task AddJobToQueue(JobModel job);
    }
}