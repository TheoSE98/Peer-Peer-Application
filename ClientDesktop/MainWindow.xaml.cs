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
        public MainWindow()
        {
            InitializeComponent();

            client = new RestClient(URL);

           // networkingThread = new Thread(NetworkingThreadFunction);
           // serverThread.Start();

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

        //private async Task NetworkingThreadFunction()
        //{

        // }

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
                    Thread.Sleep(1000); //Avoid using too much CPU 
                }
            }
            catch (Exception e)
            {
                //Handle any errors when attempting to start the server 
                Console.WriteLine("An error has occured when attempting to start server: " + e.Message);
            }
            finally
            {
                //Check host is open before closing 
                if (host != null && host.State == CommunicationState.Opened)
                {
                    try
                    {
                        host.Close();
                    }
                    //Handles Errors when server is closed
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

                // Display the file contents in the PythonCodeTextBox or send it to the server NOT SURE YET 
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
                NetTcpBinding tcpBinding = new NetTcpBinding();
                string serverAddress = "net.tcp://localhost:8100/ClientService";
                ChannelFactory<IServerService> factory = new ChannelFactory<IServerService>(tcpBinding, new EndpointAddress(serverAddress));

                // Create a channel to the server
                IServerService serverChannel = factory.CreateChannel();

                string result = serverChannel.PostJob(pythonCode);
                MessageBox.Show(result, "Job Submitted");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error submitting job: " + ex.Message, "Error");
            }
        }
    }
}