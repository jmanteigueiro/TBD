using System.Data.SqlClient;
using System.Windows;

namespace TBD
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
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

            // TODO

            string query = "";
            query += "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + "; ";
            query += "BEGIN TRAN ";
            query += "IF EXISTS (select * from " + Config.DEFAULT_TABLENAME + " WHERE FacturaID = " + TextBoxFacturaID.Text + " )";
            query += " BEGIN UPDATE " + Config.DEFAULT_TABLENAME + " SET FacturaID = " + TextBoxFacturaID.Text + ", ClienteID = " + TextBoxClienteID.Text
                + ", Nome = '" + TextBoxNome.Text + "', Morada = '" + TextBoxMorada.Text + "' WHERE FacturaID = " + TextBoxFacturaID.Text
                + " END";
            query += " ELSE BEGIN";
            query += " INSERT INTO " + Config.DEFAULT_TABLENAME + " (FacturaID, ClienteID, Nome, Morada) VALUES ('" 
                + TextBoxFacturaID.Text + "', '" + TextBoxClienteID.Text + "', '" + TextBoxNome.Text + "', '" + TextBoxMorada.Text + "')";
            query += " END ";
            query += "COMMIT ";

            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            sqlCommand.ExecuteNonQuery();

            this.Close();
        }
    }
}
