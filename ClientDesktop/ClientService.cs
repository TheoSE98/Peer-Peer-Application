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
        public void AddJobToQueue(JobModel job)
        {
            ServiceSingleton.AddJobToQueue(job);
        }

        public List<JobModel> GetJobs()
        {
            return ServiceSingleton.GetJobs();
        }

        public async Task<string> PostJob(string jobCode)
        {
            return await ServiceSingleton.PostJob(jobCode);
        }
    }
}