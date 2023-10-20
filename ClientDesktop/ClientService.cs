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
        public string PostJob(string jobCode)
        {
            string executionResult = ServiceSingleton.PostJob(jobCode);

            return executionResult;
        }
    }
}