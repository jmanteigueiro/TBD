using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
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
using TBD.Entities;

namespace TBD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection sqlConnection, sqlConnectionForActions;

        DataSet dataSetMain, dataSetLog;

        int refreshTimer = 1000;
        bool useRefreshTimer = false;
        Random randomizer = new Random();
        int latestNumberOfActions = 0;

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

        private SqlConnection CreateSqlConnection()
        {
            string str = "server=" + Application.Current.Properties["ServerName"] + ";" +
                "database=" + Application.Current.Properties["ServerDatabase"] + ";" +
                "UID=" + Application.Current.Properties["ServerUser"] + ";" +
                "password=" + Application.Current.Properties["ServerPassword"] + ";";

            return new SqlConnection(str);
        }

        async private void ConnectToDatabase()
        {
            EllipseStatus.Fill = new SolidColorBrush(Colors.Orange);

            sqlConnection = CreateSqlConnection();

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
            if (DataGridMain.IsVisible)
            {
                //First fetch of data
                dataSetMain = SelectFromMainTable();
                DataGridMain.ItemsSource = dataSetMain.Tables[0].DefaultView;
                dataSetLog = SelectFromLogTable();
                DataGridLog.ItemsSource = dataSetLog.Tables[0].DefaultView;
            }

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
                {
                    System.Threading.Thread.Sleep(200);
                    continue;
                }

                if (sqlConnection != null && sqlConnection.State == ConnectionState.Open)
                {
                    // Main Tabl
                    string query = "";
                    query += "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + "; ";
                    query += "BEGIN TRAN ";
                    query += "SELECT * FROM " + Config.DEFAULT_TABLENAME + " ";
                    query += "COMMIT; ";
                    // Query com qtdProds 
                    // string query = "SELECT factura.FacturaID, ClienteID, Nome, Morada, SUM(FactLinha.Qtd) as QtdProdutos FROM Factura, FactLinha WHERE FactLinha.FacturaID = Factura.FacturaID GROUP BY Factura.FacturaID, Factura.ClienteID, Factura.Nome, Factura.Morada";

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
                        Console.WriteLine("Exception: " + e.Message);
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
                        Console.WriteLine("Exception: " + e.Message);
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
                dataSetMain = SelectFromMainTable();
                DataGridMain.ItemsSource = dataSetMain.Tables[0].DefaultView;
            }
            else if (DataGridLog.IsVisible)
            {
                dataSetLog = SelectFromLogTable();
                DataGridLog.ItemsSource = dataSetLog.Tables[0].DefaultView;
            }
        }

        private void MenuItemRandom_Click(object sender, RoutedEventArgs e)
        {
            RandomizeWindow randomizeWindow = new RandomizeWindow(latestNumberOfActions);

            randomizeWindow.ShowDialog();

            var numberOfActions = randomizeWindow.numberOfActions;

            if (numberOfActions > 0) {
                latestNumberOfActions = numberOfActions;
                new Task(() => RandomizeActions(numberOfActions)).Start();
            }
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sqlConnection.State == ConnectionState.Closed || sqlConnection.State == ConnectionState.Broken || sqlConnection.State == ConnectionState.Connecting)
                return;

            EditWindow editWindow = new EditWindow(sqlConnection);

            editWindow.Show();
        }

        private void DataGridLog_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd/MM/yyyy HH:mm:ss.fff";

        }

        private void RandomizeActions(int numberOfActions)
        {
            sqlConnectionForActions = CreateSqlConnection();
            sqlConnectionForActions.Open();

            string iLevel = " SET TRANSACTION ISOLATION LEVEL " + isolationLevel + "; ";
            SqlCommand sqlCommand = new SqlCommand(iLevel, sqlConnectionForActions);
            sqlCommand.ExecuteNonQuery();


            for (int i = 0; i < numberOfActions; i++)
            {
                // DO RANDOM TRANSACTIONS
                int latestID = GetLatestIDFromMainTable();

                // Random name
                string[] names = File.ReadAllLines("../../dics/nome_dic.txt");

                // Random address
                string[] addresses = File.ReadAllLines("../../dics/morada_dic.txt");

                // Random designations
                string[] designations = File.ReadAllLines("../../dics/designacao_dic.txt");

                // Random prices
                string[] prices = File.ReadAllLines("../../dics/prod_preco_dic.txt");

                // Randomizer
                int randomNumber = randomizer.Next(1, 10000);

                if (randomNumber <= 3000)
                {
                    //Delete

                    /*
                     * Vai buscar os ids à BD;
                     * Seleciona um aleatoriamente;
                     * Delete;
                     */
                    try {
                        int idToDelete = randomizer.Next(1, latestID);

                        string tran = QueryMethods.GenerateDeleteTransaction(idToDelete);
                        sqlCommand = new SqlCommand(tran, sqlConnectionForActions);
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch(Exception e) {
                        Console.WriteLine("[D] Exception: " + e.Message);
                    }
                }
                else if (randomNumber > 3000 && randomNumber <= 8000)
                {
                    //Update

                    /*
                     * Vai buscar os ids à BD;
                     * Seleciona um aleatoriamente;
                     * Update o nome, com um do dicionário;
                     */
                    try
                    {
                        int idToUpdate = randomizer.Next(1, latestID);

                        //Random Client Name
                        string nome = names[randomizer.Next(names.Length)];
                        //Random Client Address
                        string morada = addresses[randomizer.Next(addresses.Length)];

                        string tran = QueryMethods.GenerateUpdateTransaction(idToUpdate, randomizer.Next(1, 999), nome, morada);
                        sqlCommand = new SqlCommand(tran, sqlConnectionForActions);
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[U] Exception: " + e.Message);
                    }
                }
                else
                {
                    //Insert

                    /*
                     * Vai buscar ao dicionário e insere;
                     */

                    try
                    {
                        //Random Client Name
                        string nome = names[randomizer.Next(names.Length)];
                        //Random Client Address
                        string morada = addresses[randomizer.Next(addresses.Length)];


                        //Random Produtos
                        int qtd_prods = 2; //randomizer.Next(1, 5);
                        string[] prods = new string[qtd_prods];
                        string[] prods_price = new string[qtd_prods];
                        int[] prods_qtd = new int[qtd_prods];
                        int[] prods_id = new int[qtd_prods];
                        for(int j=0; j<qtd_prods; j++)
                        {
                            int random_prod;
                            do
                            {
                                random_prod = randomizer.Next(1, designations.Length);
                            } while (prods_id.Contains(random_prod));
                            prods[j] = designations[random_prod];
                            prods_price[j] = prices[random_prod];
                            prods_qtd[j] = randomizer.Next(1, 6);
                            prods_id[j] = random_prod;
                        }

                        string tran = QueryMethods.GenerateInsertTransaction(randomizer.Next(1, 999), nome, morada, qtd_prods, prods, prods_price, prods_qtd, prods_id);
                        sqlCommand = new SqlCommand(tran, sqlConnectionForActions);
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[I] Exception: " + e.Message);
                    }
                }
            }
        }

        #region Select Methods

        private DataSet SelectFromMainTable()
        {
            DataSet dataSet;
            string query = "SELECT * FROM " + Config.DEFAULT_TABLENAME;
            // Query com qtdProds 
            //string query = "SELECT factura.FacturaID, ClienteID, Nome, Morada, SUM(FactLinha.Qtd) as QtdProdutos FROM Factura, FactLinha WHERE FactLinha.FacturaID = Factura.FacturaID GROUP BY Factura.FacturaID, Factura.ClienteID, Factura.Nome, Factura.Morada";
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
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnectionForActions);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);
            if (dataSet.Tables[0].Rows.Count < 1)
                return 0;
            int id = Convert.ToInt32(dataSet.Tables[0].Rows[0]["FacturaID"]);
            return id;
        }

        private Factura GetFacturaByID(int facturaID)
        {
            DataSet dataSet;
            string query = "SELECT * FROM " + Config.DEFAULT_TABLENAME + " WHERE FacturaID = " + facturaID;
            SqlCommand sqlCommand = new SqlCommand(query, sqlConnectionForActions);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
            dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);

            Factura factura = new Factura();
            factura.FacturaID = Convert.ToInt32(dataSet.Tables[0].Rows[0]["FacturaID"]);
            factura.ClienteID = Convert.ToInt32(dataSet.Tables[0].Rows[0]["ClienteID"]);
            factura.Nome = (dataSet.Tables[0].Rows[0]["Nome"]).ToString();
            factura.Morada = (dataSet.Tables[0].Rows[0]["Morada"]).ToString();

            return factura;
        }

        #endregion
    }
}
