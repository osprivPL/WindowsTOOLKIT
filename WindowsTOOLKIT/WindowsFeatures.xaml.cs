using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Shapes;

namespace WindowsTOOLKIT
{
    /// <summary>
    /// Interaction logic for WindowsFeatures.xaml
    /// </summary>
    public partial class WindowsFeatures : Window
    {
        public WindowsFeatures()
        {
            

            InitializeComponent();
        }

        private void winWindowsFeatures_Loaded(object sender, RoutedEventArgs e)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dism",
                    Arguments = "/online /get-features /format:table",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            bool show = false;
            var features = new List<(string Name, string State)>();
            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            

            foreach (var line in lines)
            {
                if (!(line.StartsWith("Feature Name") || line.StartsWith("---") || line == "The operation completed successfully.") && show)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string name = parts[0];
                        string state = parts[parts.Length - 1];
                        features.Add((name, state));
                    }
                }
                else if (!show && line.StartsWith("---"))
                {
                    show = true;
                }
            }

            foreach (var feature in features)
            {
                MessageBox.Show($"Feature: {feature.Name}, State: {feature.State}");
            }
        }

    }
}
