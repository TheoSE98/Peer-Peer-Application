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
    public partial class MainWindow : Window
    {
        private Thread networkingThread;
        private Thread serverThread;
        private RestClient client;
        private const string URL = "http://localhost:5002";
        private static bool ProcessJobFlag { get; set; }
        private int completedJobCount = 0;
        private int serverPort;
        private bool IsWorkingOnJob { get; set; } = false;


        public MainWindow()
        {
            InitializeComponent();

            client = new RestClient(URL);

            networkingThread = new Thread(NetworkingThreadFunction);
            networkingThread.Start();

            serverThread = new Thread(ServerThreadFunction);
            serverThread.Start();

            Task.Run(UpdateJobStatus);
        }

        private int GenerateRandomPort()
        {
            Random random = new Random();
            int minPort = 49152; // The minimum valid port number
            int maxPort = 65535; // The maximum valid port number
            return random.Next(minPort, maxPort + 1);
        }

        private async Task UpdateJobStatus()
        {
            while (true)
            {
                Dispatcher.Invoke(() =>
                {
                    JobStatusLabel.Content = IsWorkingOnJob ? "Working on a job" : "Idle";
                });

                await Task.Delay(1000);
            }
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

                RestRequest request = new RestRequest("/Client/ClientList", Method.Get);
                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Available clients retrieved successfully.");

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
                    foreach (var job in jobs)
                    {
                        IsWorkingOnJob = true;

                        string result = serverChannel.PostJob(job.JobCode);
                        Console.WriteLine($"Job execution result: {result}");

                        JobResultModel jobResult = new JobResultModel
                        {
                            ClientPort = client.Port,
                            ExecutionResult = result.ToString(),
                        };

                        await PostJobResult(jobResult);

                        completedJobCount++;

                        Dispatcher.Invoke(() =>
                        {
                            CompletedJobsLabel.Content = completedJobCount.ToString();
                        });

                        IsWorkingOnJob = false;
                    }

                    // Add the client to the updatedClients list after processing its jobs
                    updatedClients.Add(client);
                }
         
                factory.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to client {client.IP}:{client.Port}: {ex.Message}");
                IsWorkingOnJob = false;                
            }
        }

        private async void NetworkingThreadFunction()
        {
            while (true)
            {
                try
                {
                    List<ClientModel> availableClients = null;

                    int currentServerPort = serverPort;

                    // Get available clients
                    Task.Run(async () =>
                    {
                        availableClients = await GetAvailableClients();
                    }).Wait();

                    if (availableClients.Count == 1 && availableClients[0].Port == currentServerPort)
                    {
                        await ConnectAndExecuteTasks(availableClients[0], new List<ClientModel>());
                    }
                    else if (availableClients.Count > 0)
                    {
                        List<ClientModel> updatedClients = new List<ClientModel>();

                        availableClients = availableClients.Where(client => client.Port != currentServerPort).ToList();

                        foreach (var client in availableClients)
                        {
                            // Connect to the client and perform tasks
                            Console.WriteLine($"attempting to connect {client.Port}");
                            
                            Task.Run(async () =>
                            {
                                await ConnectAndExecuteTasks(client, updatedClients);
                            }).Wait();
                        }

                    }

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
            serverPort = GenerateRandomPort();

            Console.WriteLine($"Server has started on port {serverPort}");

            RegisterClient(serverPort);

            ServiceHost host = null;

            try
            {
                NetTcpBinding tcp = new NetTcpBinding();

                IServerService service = new ClientService();

                host = new ServiceHost(typeof(ClientService));

                string serviceAddress = $"net.tcp://localhost:{serverPort}/ClientService";

                host.AddServiceEndpoint(typeof(IServerService), tcp, serviceAddress);
                host.Open();

                while (true)
                {
                    if (ProcessJobFlag)
                    { 
                        string pythonCode = JobData.PythonCode;
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
                Console.WriteLine("An error has occured when attempting to start server: " + e.Message);
            }
            finally
            {
                if (host != null && host.State == CommunicationState.Opened)
                {
                    try
                    {
                        host.Close();
                    }
                    catch (Exception ex) { Console.WriteLine("An error occured when attempting to close server " + ex.Message); }
                }
            }
        }

        public async Task PostJobResult(JobResultModel jobResult)
        {
            try
            {
                Console.WriteLine("Posting job result: ");
                Console.WriteLine($"ClientPort: {jobResult.ClientPort}");
                Console.WriteLine($"ExecutionResult: {jobResult.ExecutionResult}");

                RestRequest request = new RestRequest("/Client/PostJobResult", Method.Post);
                request.AddJsonBody(jobResult);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Job result posted successfully.");
                }
                else
                {
                    Console.WriteLine("Error posting job result: " + response.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error posting job result: " + ex.Message);
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