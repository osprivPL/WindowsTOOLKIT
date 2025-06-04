using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;

namespace WindowsTOOLKIT
{
    public partial class WindowsFeatures : Window
    {
        private List<(string Name, string State)>
            _featuresBefore = new List<(string Name, string State)>(); // stan funkcji po pobrnaiu DISMEM

        private bool closedByProgram = true;

        public WindowsFeatures()
        {
            InitializeComponent();
        }

        private async void winWindowsFeatures_Loaded(object sender, RoutedEventArgs e)
        {
            closedByProgram = false;
            WfbtNsave.IsEnabled = false;
            WfbtNback.IsEnabled = false;
            var loading = new Label
            {
                Content = "Wczytywanie...",
                Margin = new Thickness(5)
            };

            GFeatures.Children.Add(loading);
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
                      line == "The operation completed successfully.") && show) // wykluczenie, by zostaly same funkcje
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        string name = parts[0];
                        string state = null;
                        int counter = 1;
                        try
                        {
                            while (state != "Enabled" && state != "Disabled")
                            {
                                state = parts[parts.Length - counter];
                                counter++;
                            }

                            _featuresBefore.Add((name, state));
                        }
                        catch (IndexOutOfRangeException){

                            continue;
                        }
                        
                    }
                }
                else if (!show && line.StartsWith("---"))
                {
                    show = true;
                }
            }

            _featuresBefore = _featuresBefore.OrderBy(feature => feature.Name).ToList(); // BY bylo alfabetycznie
            int row = 0;
            foreach (var feature in _featuresBefore)
            {
                GFeatures.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var label = new Label
                {
                    Content = feature.Name,
                    Margin = new Thickness(10, 5, 10, 5),
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brushes.Black
                };

                var checkbox = new CheckBox
                {
                    IsChecked = feature.State == "Enabled",
                    Margin = new Thickness(10, 5, 10, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    FontSize = 16
                };

                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);

                Grid.SetRow(checkbox, row);
                Grid.SetColumn(checkbox, 1);

                GFeatures.Children.Add(label);
                GFeatures.Children.Add(checkbox);

                row++;
            }


            WfbtNsave.IsEnabled = true;
            WfbtNback.IsEnabled = true;
            closedByProgram = true;
        }

        private void WFBTNback_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void WFBTNSave_click(object sender, RoutedEventArgs e)
        {
            closedByProgram = false;
            WfbtNsave.IsEnabled = false;
            WfbtNback.IsEnabled = false;
            var lblIndex = _featuresBefore.Count - 1;
            var cbIndex = _featuresBefore.Count - 1;
            List<(string Name, string State)> featuresAfter = new List<(string Name, string State)>();

            // Przygotowanie listy do porownania nowego stanu z poprzednim
            for (int i = 0; i < _featuresBefore.Count; i++)
            {
                featuresAfter.Add((null, null));
            }

            // kasuje elementy, i laczy Labele z checkboxami
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

            // wlaczanie/wylaczanie funkcji dla tych, ktorych checkboxy zmienily stan
            for (int i = 0; i < featuresAfter.Count; i++)
            {
                if ((featuresAfter[i].Name == _featuresBefore[i].Name &&
                     featuresAfter[i].State != _featuresBefore[i].State) &&
                    !(featuresAfter[i].Name == null || featuresAfter[i].State == null))
                {
                    processing.Content = "Zmiana stanu: " + featuresAfter[i].Name;

                    await Task.Run(() =>
                    {
                        Process proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "dism",
                                Arguments = "/online /" +
                                            (featuresAfter[i].State == "Enabled"
                                                ? "enable-feature"
                                                : "disable-feature") +
                                            " /NoRestart /featurename:" + featuresAfter[i].Name,
                                RedirectStandardOutput = false,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        proc.Start();
                        bool finished = proc.WaitForExit(1 * 60 * 1000); // 5 minut na wykonanie

                        if (!finished)
                        {
                            proc.Kill();
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.Topmost = true;
                                this.Activate();
                                MessageBox.Show("Błąd: Funkcja " + featuresAfter[i].Name +
                                                " nie została zmieniona. Zrestartuj komputer, i spróbuj ponownie");
                                this.Topmost = false;
                                
                            });
                        }
                    });
                }
            }

            this.Topmost = true;
            this.Activate();
            MessageBox.Show("Zapisano zmiany. Zrestartuj komputer, aby zmiany zostały zastosowane");
            closedByProgram = true;
            this.Topmost = false;
            this.Close();
        }

        private void winWindowsFeatures_Closing(object sender, CancelEventArgs e)
        {
            if (!closedByProgram)
            {
                e.Cancel = true;
                closedByProgram = false;
                MessageBox.Show("Zamknięcie okna będzie możliwe po zakończeniu operacji");    
            }
            
        }
    }
}