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

namespace ClientDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".py", // Default file extension
                Filter = "Python Files|*.py|All Files|*.*", // Filter files by extension
            };

            // Show open file dialog
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Get the selected file path
                string selectedFilePath = openFileDialog.FileName;

                // Read the contents of the selected file
                string fileContents = System.IO.File.ReadAllText(selectedFilePath);

                // Display the file contents in the PythonCodeTextBox or send it to the server
                PythonCodeTextBox.Text = fileContents;
            }
        }

        private void SubmitCodeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
