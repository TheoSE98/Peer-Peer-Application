using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestSharp;
using Newtonsoft.Json;
using IronPython;
using System.Threading;
using System.ServiceModel;
using ClientServerInterface;
using APIModels;
using System.Net;

namespace ClientDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread networkingThread;
        private Thread serverThread;
        private RestClient client;
        private const string URL = "http://localhost:5002";
        private static bool ProcessJobFlag { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            client = new RestClient(URL);

            networkingThread = new Thread(NetworkingThreadFunction);
            networkingThread.Start();

            serverThread = new Thread(ServerThreadFunction);
            serverThread.Start();
        }

        private int GenerateRandomPort()
        {
            Random random = new Random();
            int minPort = 49152; // The minimum valid port number
            int maxPort = 65535; // The maximum valid port number
            return random.Next(minPort, maxPort + 1);
        }

        private void RegisterClient(int port)
        {
            try
            {
                RestRequest request = new RestRequest("/Client/Register", Method.Post);

                request.AddJsonBody(new ClientModel { Port = port, IP = "localhost" });

                RestResponse response = client.Execute(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Error registering client with the webdbserver.");
                }
                else
                {
                    Console.WriteLine("Client registered with the webdbserver.");
                }
            }
            catch (Exception ex)
            {              
                Console.WriteLine("An error occurred while registering client: " + ex.Message);
            }
        }

        private async Task<List<ClientModel>> GetAvailableClients()
        {
            try
            {
                Console.WriteLine("Requesting available clients from the server...");

                RestRequest request = new RestRequest("/Client/GetOtherClients", Method.Get);
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Available clients retrieved successfully.");
                    // Deserialize the response to get available clients
                    var availableClients = JsonConvert.DeserializeObject<List<ClientModel>>(response.Content);
                    Console.WriteLine($"Available clients count: {availableClients.Count}");
                    return availableClients;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting available clients: " + ex.Message);
            }

            return new List<ClientModel>();
        }

        private async Task ConnectAndExecuteTasks(ClientModel client, List<ClientModel> updatedClients)
        {
            try
            {
                NetTcpBinding tcpBinding = new NetTcpBinding();
                string serverAddress = $"net.tcp://{client.IP}:{client.Port}/ClientService";
                ChannelFactory<IServerService> factory = new ChannelFactory<IServerService>(tcpBinding, new EndpointAddress(serverAddress));

                // Create a channel to the client
                IServerService serverChannel = factory.CreateChannel();

                // Query if any jobs exist and download them
                List<JobModel> jobs = serverChannel.GetJobs();
                Console.WriteLine($"Number of jobs: {jobs.Count}");

                if (jobs.Count > 0)
                {
                    Console.WriteLine($"Connected to client {client.IP}:{client.Port}");
                    Console.WriteLine($"Found {jobs.Count} job(s) to execute:");
                    // Process the jobs
                    var tasks = jobs.Select(async job =>
                    {
                        // Execute the job using IronPython
                        string result = await serverChannel.PostJob(job.JobCode);
                        Console.WriteLine($"Job execution result: {result}");
                    });

                    await Task.WhenAll(tasks);

                    updatedClients.Add(client);
                }

                // Close the factory after use
                factory.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to client {client.IP}:{client.Port}: {ex.Message}");
            }
        }

        private void NetworkingThreadFunction()
        {
            while (true)
            {
                try
                {
                    List<ClientModel> availableClients = null;

                    // Get available clients
                    Task.Run(async () =>
                    {
                        availableClients = await GetAvailableClients();
                    }).Wait();

                    if (availableClients.Count > 0)
                    {
                        List<ClientModel> updatedClients = new List<ClientModel>();

                        foreach (var client in availableClients)
                        {
                            // Connect to the client and perform tasks
                            Console.WriteLine($"attempting to connect {client.Port}");
                            
                            Task.Run(async () =>
                            {
                                await ConnectAndExecuteTasks(client, updatedClients);
                            }).Wait();
                        }

                        availableClients = updatedClients;
                    }

                    // Wait for some time before checking again
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Networking thread error: " + ex.Message);
                }
            }
        }

        private void ServerThreadFunction()
        {
            int port = GenerateRandomPort();

            Console.WriteLine($"Server has started on port {port}");

            RegisterClient(port);

            ServiceHost host = null;

            try
            {
                NetTcpBinding tcp = new NetTcpBinding();

                IServerService service = new ClientService();

                host = new ServiceHost(typeof(ClientService));

                string serviceAddress = $"net.tcp://localhost:{port}/ClientService";

                host.AddServiceEndpoint(typeof(IServerService), tcp, serviceAddress);
                host.Open();

                while (true)
                {
                    if (ProcessJobFlag)
                    {
                        // This probably needs to be sent to the WebServer instead of this shit 
                        string pythonCode = JobData.PythonCode;
                        //string result = service.PostJob(pythonCode);
                        Console.WriteLine($"Adding a job to the queue: {pythonCode}");
                        JobModel job = new JobModel { JobCode = pythonCode};
                        service.AddJobToQueue(job);
                        Console.WriteLine();

                        // Reset the flag
                        ProcessJobFlag = false;
                    }
                    Thread.Sleep(1000); //Avoid using too much CPU 
                }
            }
            catch (Exception e)
            {
                // Handle any errors when attempting to start the server 
                Console.WriteLine("An error has occured when attempting to start server: " + e.Message);
            }
            finally
            {
                // Check host is open before closing 
                if (host != null && host.State == CommunicationState.Opened)
                {
                    try
                    {
                        host.Close();
                    }
                    // Handles Errors when server is closed
                    catch (Exception ex) { Console.WriteLine("An error occured when attempting to close server " + ex.Message); }
                }
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".py", 
                Filter = "Python Files|*.py|All Files|*.*",
            };
    
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                string fileContents = System.IO.File.ReadAllText(selectedFilePath);

                PythonCodeTextBox.Text = fileContents;
            }
        }

        private void SubmitCodeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pythonCode = PythonCodeTextBox.Text;

                if (string.IsNullOrEmpty(pythonCode))
                {
                    MessageBox.Show("Python code is empty. Please enter code before submitting.", "Error");
                    return; 
                }

                // Create a channel factory to communicate with the server's WCF service
                JobData.PythonCode = pythonCode;

                ProcessJobFlag = true;

                MessageBox.Show("Job submitted.", "Job Submitted");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting job: " + ex.Message, "Error");
            }
        }
    }
}