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
using System.Windows.Shapes;

namespace TBD
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool isToConnect; 

        public SettingsWindow()
        {
            InitializeComponent();

            InitializeTextBoxes();
        }

        private void InitializeTextBoxes()
        {
            TextBoxServerName.Text = (string)Application.Current.Properties["ServerName"];
            TextBoxDatabaseName.Text = (string)Application.Current.Properties["ServerDatabase"];
            TextBoxUserName.Text = (string)Application.Current.Properties["ServerUser"];
            PasswordBoxPassword.Password = (string)Application.Current.Properties["ServerPassword"];

        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            isToConnect = false;
            this.Close();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Properties["ServerName"] = TextBoxServerName.Text;
            Application.Current.Properties["ServerDatabase"] = TextBoxDatabaseName.Text;
            Application.Current.Properties["ServerUser"] = TextBoxUserName.Text;
            Application.Current.Properties["ServerPassword"] = PasswordBoxPassword.Password;

            isToConnect = true;

            this.Close();
        }
    }
}
