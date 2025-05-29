using System.Windows;

namespace WindowsTOOLKIT
{
    public partial class Backup : Window
    {
        public Backup()
        {
            InitializeComponent();
        }

        private void BtnBack_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}