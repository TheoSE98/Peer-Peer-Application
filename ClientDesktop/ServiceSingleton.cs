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
    public class ServiceSingleton
    {
        private static ServiceSingleton instance;
        private static List<JobModel> jobQueue = new List<JobModel>();
        private static readonly object LockObject = new object();


        private ServiceSingleton()
        {
        }

        // Property to access the singleton instance
        public static ServiceSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceSingleton();
                }
                return instance;
            }
        }
        public string PostJob(string jobCode)
        {
            try
            {
                // Create a Python runtime
                ScriptRuntimeSetup runtimeSetup = Python.CreateRuntimeSetup(null);
                ScriptRuntime runtime = new ScriptRuntime(runtimeSetup);
                ScriptEngine engine = Python.GetEngine(runtime);
                ScriptScope scope = engine.CreateScope();

                engine.Execute(jobCode, scope);

                // Retrieve the result
                dynamic result = scope.GetVariable("result");

                if (result != null)
                {
                    return "Job Completed successfully";
                }
                else
                {
                    foreach (var variable in scope.GetVariableNames())
                    {
                        Console.WriteLine($"LOOK HERE: {variable} = {scope.GetVariable(variable)}");
                    }
                    return "Job encountered an error";
                }
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during job execution
                return "Error: " + ex.Message;
            }
        }

        public void AddJobToQueue(JobModel job)
        {
            jobQueue.Add(job);
            Console.WriteLine("Job added to queue");
            Console.WriteLine($"total jobs in job queue: {jobQueue.Count}");
        }

        public List<JobModel> GetJobs()
        {
            List<JobModel> jobs;

            lock (LockObject) // Add locking to ensure thread safety
            {
                jobs = new List<JobModel>(jobQueue);
                jobQueue.Clear();
            }

            return jobs;
        }
    }
}