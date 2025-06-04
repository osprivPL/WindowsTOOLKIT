using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FontAwesome.WPF;

namespace WindowsTOOLKIT
{
    public partial class BackupRemover : Window
    {
        public class restorePoint
        {
            public int Day;
            public int Month;
            public int Year;
            public string Description;
        }

        bool _closable = true;
        List<restorePoint> _restorePoints = new List<restorePoint>();

        public BackupRemover()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_closable)
            {
                this.Close();
            }
            else
            {
                MessageBox.Show("Zamknięcie okna będzie możliwe dopiero po zakończeniu operacji");
            }
        }

        private async void WinBackupRemover_Loaded(object sender, RoutedEventArgs e)
        {
            _closable = false;

            GBackups.Children.Add(
                new ImageAwesome
                {
                    Icon = FontAwesomeIcon.Spinner,
                    Spin = true,
                    Width = 32,
                    Height = 32,
                    Foreground = Brushes.Gray
                }
            );


            string[] output;

            await Task.Run(() =>
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "Get-ComputerRestorePoint",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                proc.Start();
                output = proc.StandardOutput.ReadToEnd().Split('\n');
                proc.WaitForExit();

                List<int> begins = new List<int>();
                List<List<int>> allEnds = new List<List<int>>();

                foreach (string line in output)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var ends = new List<int>();
                    if (line.StartsWith("-"))
                    {
                        // wyznacz begins raz
                        begins.Clear();
                        bool inDash = false;
                        for (int i = 0; i < line.Length; i++)
                        {
                            if (!inDash && line[i] == '-')
                            {
                                begins.Add(i);
                                inDash = true;
                            }
                            else if (line[i] != '-')
                            {
                                inDash = false;
                            }
                        }
                    }
                    else if (char.IsDigit(line[0]))
                    {
                        // dla każdej linii danych, wyznacz ends
                        ends = new List<int>();

                        for (int i = 0; i < begins.Count; i++)
                        {
                            int start = begins[i];
                            int end;

                            if (i < begins.Count - 1)
                            {
                                end = begins[i + 1] - 1;
                                while (end > start && line[end] == ' ')
                                    end--;
                            }
                            else
                            {
                                end = line.Length - 1;
                                while (end > start && line[end] == ' ')
                                    end--;
                            }

                            ends.Add(end);
                        }

                        int day;
                        var s = line;

                        if (line[1] == '.')
                        {
                            day = int.Parse(line[0].ToString());
                            s = '0' + line;
                        }
                        else
                        {
                            day = int.Parse(line.Substring(0, 2));
                        }

                        restorePoint rp = new restorePoint
                        {
                            Day = day,
                            Month = int.Parse(s.Substring(3, 2)),
                            Year = int.Parse(s.Substring(6, 4)),
                            Description = s.Substring(begins[1], ends[1] - begins[1] + 2) //.Trim()
                        };
                        _restorePoints.Add(rp);
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var elem in GBackups.Children)
                    {
                        if (elem is ImageAwesome ia)
                        {
                            GBackups.Children.Remove(ia);
                            break;
                        }
                    }

                    var index = 0;
                    foreach (var rp in _restorePoints)
                    {
                        var date = new Label
                        {
                            Content = $"{rp.Day:00}.{rp.Month:00}.{rp.Year}",
                            Margin = new Thickness(10, 5, 10, 5),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 16,
                            Foreground = Brushes.Black
                        };

                        var descript = new Label
                        {
                            Content = rp.Description,
                            Margin = new Thickness(10, 5, 10, 5),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center,
                            FontSize = 16,
                            Foreground = Brushes.Black
                        };

                        Grid.SetColumn(date, 0);
                        Grid.SetColumn(descript, 1);
                        Grid.SetRow(date, index);
                        Grid.SetRow(descript, index);

                        GBackups.Children.Add(date);
                        GBackups.Children.Add(descript);

                        index++;
                    }
                });
            });
            _closable = true;
        }

        private void winBackupRemover_closing(object sender, CancelEventArgs e)
        {
            if (!_closable)
            {
                e.Cancel = true;
                MessageBox.Show("Zamknięcie okna będzie możliwe po zakończeniu operacji");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}