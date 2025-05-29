using System.Diagnostics;
using System.Windows;

namespace WindowsTOOLKIT
{
    public partial class Backup : Window
    {
        public Backup()
        {
            InitializeComponent();
        }

        private void btnCreateBackup_Click(object sender, RoutedEventArgs e)
        {
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
                    "Brak zarezerwowanej przestrzeni dyskowej na kopie zapasowe. Czy chcesz ją utworzyć", 
                    "Potwierdzenie", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Kliknięto TAK");
                }
                else
                {
                    MessageBox.Show("Kliknięto NIE");
                }

            }
            



        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}