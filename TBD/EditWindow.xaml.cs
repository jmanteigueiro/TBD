using System.Data.SqlClient;
using System.Windows;

namespace TBD
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        const string DEFAULT_TABLENAME      = "Table_Main";
        const string DEFAULT_LOGTABLENAME   = "Table_Log";
        
        SqlConnection sqlConnection;

        public EditWindow()
        {
            InitializeComponent();
        }

        public EditWindow(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
            InitializeComponent();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string isolationLevel = Application.Current.Properties["IsolationLevel"] as string;

            string query = "INSERT INTO " + DEFAULT_TABLENAME + " (id, name) VALUES ('" + TextBoxId.Text + "', '" + TextBoxName.Text + "')";

            string transaction = QueryMethods.CreateTransaction(isolationLevel, query);

            SqlCommand sqlCommand = new SqlCommand(transaction, sqlConnection);

            sqlCommand.ExecuteNonQuery();

            this.Close();
        }
    }
}
