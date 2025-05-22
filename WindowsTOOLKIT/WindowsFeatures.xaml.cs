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
    public partial class WindowsFeatures : Window
    {
        private List<(string Name, string State)> _featuresBefore = new List<(string Name, string State)>();

        public WindowsFeatures()
        {
            InitializeComponent();
        }

        private async void winWindowsFeatures_Loaded(object sender, RoutedEventArgs e)
        {
            WFBTNsave.IsEnabled = false;
            var loading = new Label
            {
                Content = "Wczytywanie...",
                Margin = new Thickness(5)
            };

            GFeatures.Children.Add(loading);
            //string output = "";
            string output = await Task.Run(() =>
                // await, by zdazyla sie pokazac informacja o wczytywaniu, na innym watku sie to wykona!!! (notatka bo pewnie zapomne co to robi)
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
                string temp = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                return temp;
            });

            GFeatures.Children.Remove(loading);

            bool show = false;

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


            foreach (var line in lines)
            {
                if (!(line.StartsWith("Feature Name") || line.StartsWith("---") ||
                      line == "The operation completed successfully.") && show)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string name = parts[0];
                        string state = parts[parts.Length - 1];
                        _featuresBefore.Add((name, state));
                    }
                }
                else if (!show && line.StartsWith("---"))
                {
                    show = true;
                }
            }

            _featuresBefore = _featuresBefore.OrderBy(feature => feature.Name).ToList();
            int row = 0;

            foreach (var feature in _featuresBefore)
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
            WFBTNsave.IsEnabled = true;
        }

        private void WFBTNback_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void WFBTNSave_click(object sender, RoutedEventArgs e)
        {
            WFBTNsave.IsEnabled = false;
            var lblIndex = _featuresBefore.Count - 1;
            var cbIndex = _featuresBefore.Count - 1;
            List<(string Name, string State)> featuresAfter = new List<(string Name, string State)>();

            for (int i = 0; i < _featuresBefore.Count; i++)
            {
                featuresAfter.Add((null, null));
            }

            for (int i = GFeatures.Children.Count - 1; i >= 0; i--)
            {
                var elem = GFeatures.Children[i];
                if (elem is Label)
                {
                    var valueTuple = featuresAfter[lblIndex];
                    valueTuple.Name = ((Label)elem).Content.ToString();
                    featuresAfter[lblIndex] = valueTuple;
                    lblIndex--;
                }
                else if (elem is CheckBox)
                {
                    var valueTuple = featuresAfter[cbIndex];
                    valueTuple.State = ((CheckBox)elem).IsChecked == true ? "Enabled" : "Disabled";
                    featuresAfter[cbIndex] = valueTuple;
                    cbIndex--;
                }

                GFeatures.Children.Remove(elem);
            }

            Label processing = new Label();

            GFeatures.Children.Add(processing);

            for (int i = 0; i < featuresAfter.Count; i++)
            {
                if ((featuresAfter[i].Name == null && featuresAfter[i].State == null) || _featuresBefore[i].State=="Pending" )
                {
                    continue;
                }

                if (featuresAfter[i].Name == "IIS-HostableWebCore")
                {
                    i = i;
                }

                if (featuresAfter[i].Name == _featuresBefore[i].Name &&
                    featuresAfter[i].State != _featuresBefore[i].State) {
                    processing.Content = "Zmiana stanu: " + featuresAfter[i].Name;
                    string output = await Task.Run(() =>
                    {
                        if (featuresAfter[i].State == "Enabled")
                        {
                            Process proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "dism",
                                    Arguments = "/online /enable-feature /NoRestart /featurename:" + featuresAfter[i].Name,
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };

                            proc.Start();
                            string temp = proc.StandardOutput.ReadToEnd();
                            proc.WaitForExit();

                            return "";
                        }
                        else
                        {
                            Process proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "dism",
                                    Arguments = "/online /disable-feature /NoRestart /featurename:" + featuresAfter[i].Name,
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };

                            proc.Start();
                            string temp = proc.StandardOutput.ReadToEnd();
                            proc.WaitForExit();

                            return "";
                        }
                    });
                }
            }

            this.Topmost = true;
            this.Activate();
            MessageBox.Show("Zapisano zmiany. Zrestartuj komputer, aby zmiany zostały zastosowane");
            this.Topmost = false;
            
            this.Close();

            //     foreach (var child in GFeatures.Children)
            //     {
            //         if (child is CheckBox checkbox)
            //         {
            //             string featureName = ((Label)GFeatures.Children[GFeatures.Children.IndexOf(checkbox) - 1]).Content
            //                 .ToString();
            //             string state = checkbox.IsChecked == true ? "Enabled" : "Disabled";
            //
            //             // MessageBox.Show($"Feature: {featureName} - State: {state}");
            //
            //             if (_featuresBefore[row].State == state)
            //             {
            //                 row++;
            //                 continue;
            //             }
            //
            //             if (state == "Enabled")
            //             {
            //                 Process proc = new Process
            //                 {
            //                     StartInfo = new ProcessStartInfo
            //                     {
            //                         FileName = "dism",
            //                         Arguments = $"/online /enable-feature /featurename:{featureName}",
            //                         RedirectStandardOutput = true,
            //                         UseShellExecute = false,
            //                         CreateNoWindow = true
            //                     }
            //                 };
            //                 
            //                 proc.Start();
            //                 proc.WaitForExit();
            //             }
            //             else
            //             {
            //                 Process proc = new Process
            //                 {
            //                     StartInfo = new ProcessStartInfo
            //                     {
            //                         FileName = "dism",
            //                         Arguments = $"/online /enable-feature /featurename:{featureName}",
            //                         RedirectStandardOutput = true,
            //                         UseShellExecute = false,
            //                         CreateNoWindow = true
            //                     }
            //                 };
            //                 proc.Start();
            //                 proc.WaitForExit();
            //             }
            //         }
            //     }
            //
            //     MessageBox.Show("Zapisano zmiany. Zrestartuj komputer, aby zmiany zostały zastosowane");
            // }
        }
    }
}