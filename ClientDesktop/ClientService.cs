using APIModels;
using ClientServerInterface;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktop
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientService : IServerService
    {
        public async Task AddJobToQueue(JobModel job)
        {
            await ServiceSingleton.AddJobToQueue(job);
        }

        public async Task<List<JobModel>> GetJobs()
        {
            return await ServiceSingleton.GetJobs();
        }

        public async Task<string> PostJob(string jobCode)
        {
            return await ServiceSingleton.PostJob(jobCode);
        }
    }
}