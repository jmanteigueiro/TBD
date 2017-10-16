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
        SqlConnection sqlConnection;

        DataSet dataSetMain, dataSetLog;

        int refreshTimer = 1000;
        bool useRefreshTimer = false;

        string isolationLevel {
            get {
                return Application.Current.Properties["IsolationLevel"] as string;
            }
            set {
                Application.Current.Properties["IsolationLevel"] = (value as string).ToUpper();
            }
        }

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
            Application.Current.Properties["ServerName"]     = Config.DEFAULT_SERVERNAME;
            Application.Current.Properties["ServerDatabase"] = Config.DEFAULT_SERVERDATABASE;
            Application.Current.Properties["ServerUser"]     = Config.DEFAULT_SERVERUSER;
            Application.Current.Properties["ServerPassword"] = Config.DEFAULT_SERVERPASSWORD;
            Application.Current.Properties["IsolationLevel"] = Config.DEFAULT_ISOLATIONLEVEL;
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

        private void CallBackgroundTask()
        {
            new Task(() => FetchDataTables()).Start();
        }

        private void FetchDataTables()
        {
            while (true)
            {
                if (!useRefreshTimer)
                    continue;

                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                {
                    // Main Table
                    string query = "SELECT * FROM " + Config.DEFAULT_TABLENAME;
                    SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    dataSetMain = new DataSet();

                    try
                    {
                        sqlDataAdapter.Fill(dataSetMain);
                    }
                    catch (Exception e)
                    {
                        sqlConnection.Close();
                        this.Dispatcher.Invoke(() => {
                            WriteToConsoleLog("Error: Main table does not exist. Connection closed.");
                            ItemStatus_Click(null, null);
                            EllipseStatus.Fill = new SolidColorBrush(Colors.Red);
                            });
                        continue;
                    }

                    this.Dispatcher.Invoke(() => DataGridMain.ItemsSource = dataSetMain.Tables[0].DefaultView);

                    // Log Table
                    query = "SELECT * FROM " + Config.DEFAULT_LOGTABLENAME;
                    sqlCommand = new SqlCommand(query, sqlConnection);
                    sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                    dataSetLog = new DataSet();

                    try
                    {
                        sqlDataAdapter.Fill(dataSetLog);
                    }
                    catch (Exception e)
                    {
                        sqlConnection.Close();
                        this.Dispatcher.Invoke(() => {
                            WriteToConsoleLog("Error: Log table does not exist. Connection closed.");
                            ItemStatus_Click(null, null);
                            EllipseStatus.Fill = new SolidColorBrush(Colors.Red);
                        });
                        continue;
                    }

                    this.Dispatcher.Invoke(() => DataGridLog.ItemsSource = dataSetLog.Tables[0].DefaultView);
                }
                System.Threading.Thread.Sleep(refreshTimer);
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

                MenuItemRefresh_Click(null, null);
            }
        }

        private void SliderTimer_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            refreshTimer = Convert.ToInt16(e.NewValue);
            if (LabelMilliseconds != null)
                LabelMilliseconds.Content = refreshTimer.ToString() + " ms";
        }

        private void ComboBoxIsolation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newIsolationLevel = (e.AddedItems[0] as ComboBoxItem).Content as string;
            isolationLevel = newIsolationLevel.ToUpper();
            Console.Out.WriteLine(isolationLevel);
        }

        private void CheckBoxTimer_Checked(object sender, RoutedEventArgs e)
        {
            useRefreshTimer = !useRefreshTimer;
        }

        private void MenuItemRefresh_Click(object sender, RoutedEventArgs e)
        {
            while (sqlConnection.State == ConnectionState.Connecting)
            {
                // Do nothing until it connects
            }

            if (sqlConnection.State == ConnectionState.Closed || sqlConnection.State == ConnectionState.Broken)
                return;

            if (DataGridMain.IsVisible) {
                string query = "SELECT * FROM " + Config.DEFAULT_TABLENAME;
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                dataSetMain = new DataSet();
                sqlDataAdapter.Fill(dataSetMain);
                DataGridMain.ItemsSource = dataSetMain.Tables[0].DefaultView;
            }
            else if (DataGridLog.IsVisible)
            {
                string query = "SELECT * FROM " + Config.DEFAULT_LOGTABLENAME;
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                dataSetLog = new DataSet();
                sqlDataAdapter.Fill(dataSetLog);
                DataGridLog.ItemsSource = dataSetLog.Tables[0].DefaultView;
            }
        }

        private void MenuItemRandom_Click(object sender, RoutedEventArgs e)
        {
            RandomizeWindow randomizeWindow = new RandomizeWindow();

            randomizeWindow.ShowDialog();

            var numberOfActions = randomizeWindow.numberOfActions;

            if (numberOfActions > 0)
                RandomizeActions(numberOfActions);

        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sqlConnection.State == ConnectionState.Closed || sqlConnection.State == ConnectionState.Broken || sqlConnection.State == ConnectionState.Connecting)
                return;

            EditWindow editWindow = new EditWindow(sqlConnection);

            editWindow.Show();
        }

        private void RandomizeActions(int numberOfActions)
        {
            for (int i = 0; i < numberOfActions; numberOfActions++)
            {
                // DO RANDOM TRANSACTIONS
                int latestID = GetLatestIDFromMainTable();
                Console.WriteLine(latestID);
            }
        }

        #region Select Methods

        private DataSet SelectFromMainTable()
        {
            DataSet dataSet;
            string query = "SELECT * FROM " + Config.DEFAULT_TABLENAME;
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }

        private DataSet SelectFromLogTable()
        {
            DataSet dataSet;
            string query = "SELECT * FROM " + Config.DEFAULT_LOGTABLENAME;
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);
            return dataSet;
        }

        private int GetLatestIDFromMainTable()
        {
            DataSet dataSet;
            string query = "SELECT TOP 1 * FROM " + Config.DEFAULT_TABLENAME + " ORDER BY FacturaID DESC";
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);
            int id = Convert.ToInt32(dataSet.Tables[0].Rows[0]["FacturaID"]);
            return id;
        }

        #endregion
    }
}
