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
        private ServiceSingleton serviceSingleton = ServiceSingleton.Instance;

        public void AddJobToQueue(JobModel job)
        {
            serviceSingleton.AddJobToQueue(job);
        }

        public List<JobModel> GetJobs()
        {
            return serviceSingleton.GetJobs();
        }

        public string PostJob(string jobCode)
        {
            return serviceSingleton.PostJob(jobCode);
        }
    }
}