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
            query += " BEGIN UPDATE " + Config.DEFAULT_TABLENAME + " SET FacturaID = " + TextBoxFacturaID.Text + ", Nome = '" + TextBoxNome.Text + "' WHERE FacturaID = " + TextBoxFacturaID.Text
                   + "; UPDATE " + Config.DEFAULT_SECONDARYTABLENAME + " SET qtd = '" + TextBoxQtd1.Text + "' WHERE FacturaID = '" + TextBoxFacturaID.Text + "' AND ProdutoID = (select TOP 1 ProdutoID from FactLinha where FacturaID = '" + TextBoxFacturaID.Text + "' order by produtoID asc);"
                   + "; UPDATE " + Config.DEFAULT_SECONDARYTABLENAME + " SET qtd = '" + TextBoxQtd2.Text + "' WHERE FacturaID = '" + TextBoxFacturaID.Text + "' AND ProdutoID = (select TOP 1 ProdutoID from FactLinha where FacturaID = '" + TextBoxFacturaID.Text + "' order by produtoID desc);"
                   + " END;";
            query += " ELSE BEGIN";
            query += " INSERT INTO " + Config.DEFAULT_TABLENAME + " (FacturaID, ClienteID, Nome, Morada) VALUES ('" 
                + TextBoxFacturaID.Text + "', '0', '" + TextBoxNome.Text + "', 'Morada Indisponivel')";
            query += " END ";
            query += "COMMIT ";

            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            sqlCommand.ExecuteNonQuery();

            this.Close();
        }
    }
}
