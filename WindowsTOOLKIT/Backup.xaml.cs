using System.ComponentModel;
using FontAwesome.WPF;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsTOOLKIT
{
    public partial class Backup
    {
        private bool _closable = true; 
        public Backup()
        {
            InitializeComponent();
        }

        private async void btnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
            _closable = false;
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "vssadmin",
                    Arguments = "List shadowstorage",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            if (output.Contains("No items found that satisfy the query."))
            {
                var result = MessageBox.Show(
                    "Brak zarezerwowanej przestrzeni dyskowej na kopie zapasowe. Czy chcesz ją utworzyć?",
                    "Brak miejsca na kopie",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "vssadmin",
                            Arguments = "resize shadowstorage /for=C: /on=C: /maxsize=15GB",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            CreateNoWindow = false
                        }
                    };

                    proc.Start();
                    // output = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit();
                    // MessageBox.Show(output);
                }
                else
                {
                    _closable = true;
                    return;
                }
            } // ochrona systemu wylaczona

            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "reg",
                    Arguments =
                        "add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore \" " +
                        "/v \"SystemRestorePointCreationFrequency\" /t REG_DWORD /d 0 /f",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            BtnCreateBackup.IsEnabled = false;
            BtnDeleteBackup.IsEnabled = false;
            BtnBack.IsEnabled = false;
            

            Image loading = new ImageAwesome
            {
                Icon = FontAwesomeIcon.Spinner,
                Spin = true,
                Width = 128,
                Height = 128,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Gray
            };

            Gbackup.Children.Clear();
            Gbackup.RowDefinitions.Clear();
            Gbackup.ColumnDefinitions.Clear();
            Gbackup.RowDefinitions.Add(new RowDefinition());
            Gbackup.ColumnDefinitions.Add(new ColumnDefinition());
            
            Gbackup.Children.Add(loading);

            await Task.Run(() =>
            {
                proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments =
                            @"Checkpoint-Computer -Description 'WINDOWS TOOLKIT' -RestorePointType ""MODIFY_SETTINGS"" ",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true
                    }
                };

                // MessageBox.Show(proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);

                proc.Start();
                output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();
                this.Topmost = true;
                this.Activate();
                MessageBox.Show("Kopia zapasowa została utworzona pomyślnie.\n" + output);
            });

            proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "reg",
                    Arguments =
                        "add \"HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore \" " +
                        "/v \"SystemRestorePointCreationFrequency\" /t REG_DWORD /d 86400 /f",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            _closable = true;
            this.Close();
        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void winBackup_Closing(object sender, CancelEventArgs e)
        {
            if (!_closable)
            {
                e.Cancel = true;
                MessageBox.Show("Zamknięcie okna będzie możliwe po zakończeniu operacji");    
            }
        }

        private void btnDeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            BackupRemover window = new BackupRemover();
            window.ShowDialog();
        }
    }
}