using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace TBD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DEFAULT_SERVERNAME     = "localhost";
        const string DEFAULT_SERVERDATABASE = "TBD";
        const string DEFAULT_SERVERUSER     = "sa";
        const string DEFAULT_SERVERPASSWORD = "systemadmin";

        SqlConnection sqlConnection;

        DataSet dataSet;


        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            InitializeProperties();

            ConnectToDatabase();

            CallBackgroundTask();

        }

        private void InitializeProperties()
        {
            Application.Current.Properties["ServerName"]     = DEFAULT_SERVERNAME;
            Application.Current.Properties["ServerDatabase"] = DEFAULT_SERVERDATABASE;
            Application.Current.Properties["ServerUser"]     = DEFAULT_SERVERUSER;
            Application.Current.Properties["ServerPassword"] = DEFAULT_SERVERPASSWORD;
        }

        async private void ConnectToDatabase()
        {
            EllipseStatus.Fill = new SolidColorBrush(Colors.Orange);

            string str = "server=" + Application.Current.Properties["ServerName"] + ";" +
                "database=" + Application.Current.Properties["ServerDatabase"] + ";" +
                "UID=" + Application.Current.Properties["ServerUser"] + ";" +
                "password=" + Application.Current.Properties["ServerPassword"] + ";";

            sqlConnection = new SqlConnection(str);

            WriteToConsoleLog("Attempting to connect to server.");

            // Check if you are connected to a network
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                WriteToConsoleLog("Error: You aren't connected to any network.");

            try {
                await sqlConnection.OpenAsync();
            }
            catch (SqlException e) // In case something goes wrong
            {
                switch (e.Number)
                {
                    case 53:
                        WriteToConsoleLog("Error: Couldn't reach the server. The server may be down.");
                        break;
                    case 4060:
                        WriteToConsoleLog("Error: Couldn't find the specified database.");
                        break;
                    case 18456:
                        WriteToConsoleLog("Error: Login failed for specified user.");
                        break;
                    default:
                        WriteToConsoleLog("Error: Couldn't connect to the server.");
                        break;
                }

                EllipseStatus.Fill = new SolidColorBrush(Colors.Red);

                return;
            }

            // In case of success
            EllipseStatus.Fill = new SolidColorBrush(Colors.Green);
            WriteToConsoleLog("Connected successfully to the server!");

        }

        private void FetchMainTable()
        {
            while (true)
            {
                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                {
                    string query = "SELECT * FROM Table_test";
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    dataSet = new DataSet();
                    sqlDataAdapter.Fill(dataSet);
                    this.Dispatcher.Invoke(() => DataGridMain.ItemsSource = dataSet.Tables[0].DefaultView);       
                }


                System.Threading.Thread.Sleep(1000);
            }
        }

        private void WriteToConsoleLog(string str)
        {
            LabelConsole.Content = LabelConsole.Content + str + "\n";
        }

        private void ItemAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Show();
        }

        private void ItemTable_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridMain.IsVisible)
                return;

            DataGridMain.Visibility = Visibility.Visible;
            DataGridLog.Visibility = Visibility.Hidden;
            LabelConsole.Visibility = Visibility.Hidden;
        }

        private void ItemLog_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridLog.IsVisible)
                return;

            DataGridMain.Visibility = Visibility.Hidden;
            DataGridLog.Visibility = Visibility.Visible;
            LabelConsole.Visibility = Visibility.Hidden;
        }

        private void ItemStatus_Click(object sender, RoutedEventArgs e)
        {
            if (LabelConsole.IsVisible)
                return;

            DataGridMain.Visibility = Visibility.Hidden;
            DataGridLog.Visibility = Visibility.Hidden;
            LabelConsole.Visibility = Visibility.Visible;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();

            settingsWindow.ShowDialog();

            if (settingsWindow.isToConnect)
            {
                ConnectToDatabase();
            }
        }

        private async void CallBackgroundTask()
        {
            new Task(() => FetchMainTable()).Start();
        }
    }
}
