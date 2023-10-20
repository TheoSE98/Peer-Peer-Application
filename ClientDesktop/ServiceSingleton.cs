using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientDesktop
{
    public static class ServiceSingleton
    {
        public static string PostJob(string jobCode)
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

                return result.ToString();
            }
            catch (Exception ex)
            {
                // Handle any errors that occurred during job execution
                return "Error: " + ex.Message;
            }
        }
    }
}