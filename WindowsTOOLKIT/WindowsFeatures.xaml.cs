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

            features = features.OrderBy(feature => feature.Name).ToList();
            int row = 0;

            foreach (var feature in features)
            {
                GFeatures.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var label = new Label
                {
                    Content = feature.Name,
                    Margin = new Thickness(5)
                };

                var checkbox = new CheckBox
                {
                    IsChecked = feature.State == "Enabled",
                    Margin = new Thickness(5)
                };

                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);

                Grid.SetRow(checkbox, row);
                Grid.SetColumn(checkbox, 1);

                GFeatures.Children.Add(label);
                GFeatures.Children.Add(checkbox);

                row++;
            }
        }

    }
}
