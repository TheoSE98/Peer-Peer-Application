using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APIModels;

namespace ClientDesktop
{
    public static class ServiceSingleton
    {
        private static BlockingCollection<JobModel> jobQueue = new BlockingCollection<JobModel>();
        public static async Task<string> PostJob(string jobCode)
        {
            try
            {
                // Create a Python runtime
                ScriptRuntimeSetup runtimeSetup = Python.CreateRuntimeSetup(null);
                ScriptRuntime runtime = new ScriptRuntime(runtimeSetup);
                ScriptEngine engine = Python.GetEngine(runtime);
                ScriptScope scope = engine.CreateScope();

                await Task.Run(() => engine.Execute(jobCode, scope));

                // Retrieve the result
                dynamic result = scope.GetVariable("result");

                return result.ToString();
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during job execution
                return "Error: " + ex.Message;
            }
        }

        public static async Task AddJobToQueue(JobModel job)
        {
            await Task.Run(() => jobQueue.Add(job));
            Console.WriteLine("job adding to queue");
        }

        public static async Task<List<JobModel>> GetJobs()
        {
            List<JobModel> jobs = new List<JobModel>();

            // Use Take to dequeue items
            foreach (var job in jobQueue.GetConsumingEnumerable())
            {
                await Task.Run(() => jobs.Add(job));
            }

            return jobs;
        }
    }
}