using System.Data.SqlClient;
using System.Windows;

namespace TBD
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private const string COLUMN_NOME = "Nome";
        private const string COLUMN_FATURA = "FacturaID";
        private const string COLUMN_CLIENT = "ClienteID";
        private const string COLUMN_MORADA = "Morada";

        private const string COLUMN_SEC_FATURAID = "FacturaID";
        private const string COLUMN_SEC_PRODID = "ProdutoID";
        private const string COLUMN_SEC_DESIGNACAO = "Designacao";
        private const string COLUMN_SEC_PRECO = "Preco";
        private const string COLUMN_SEC_QTD = "Qtd";

        private const string COLUMN_LOG_EVENT_TYPE = "EventType";
        private const string COLUMN_LOG_FATURA_ID_OLD = "FactId_Old";
        private const string COLUMN_LOG_CLIENT_ID_OLD = "ClientID_Old";
        private const string COLUMN_LOG_NOME_OLD = "Nome_Old";
        private const string COLUMN_LOG_MORADA_OLD = "Morada_Old";
        private const string COLUMN_LOG_FATURA_ID = "FactId_New";
        private const string COLUMN_LOG_CLIENT_ID = "ClientID_New";
        private const string COLUMN_LOG_NOME = "Nome_NEW";
        private const string COLUMN_LOG_MORADA = "Morada_New";
        private const string COLUMN_LOG_USER_ID = "UserID";
        private const string COLUMN_LOG_TERMINAL_ID = "TerminalID";
        private const string COLUMN_LOG_TERMINAL_NAME = "TerminalName";
        private const string COLUMN_LOG_START_TIME = "StartTime";
        private const string COLUMN_LOG_END_TIME = "EndTime";

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
            query = " DECLARE @dateBegin DATETIME; ";
            query += " DECLARE @dateEnd DATETIME; ";
            query += " SET @dateBegin = CONVERT( VARCHAR, GETDATE(), 121); ";
            query += " DECLARE @clienteOldID INT;";
            query += " DECLARE @nomeOld nvarchar(30);";
            query += " DECLARE @moradaOld nvarchar(30);";
            query += " SELECT @clienteOldID = " + COLUMN_CLIENT +
                ", @nomeOld = " + COLUMN_NOME + ", @moradaOld = " + COLUMN_MORADA + " FROM " + Config.DEFAULT_TABLENAME +
                " WHERE " + COLUMN_FATURA + " = " + TextBoxFacturaID.Text + ";";
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
            query += " SET @dateEnd = CONVERT( VARCHAR, GETDATE(), 121); ";

            query += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            query += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID +
                ", " + COLUMN_LOG_CLIENT_ID + ", " + COLUMN_LOG_NOME + ", " + COLUMN_LOG_MORADA +
                ", " + COLUMN_LOG_FATURA_ID_OLD + ", " + COLUMN_LOG_CLIENT_ID_OLD + ", " + COLUMN_LOG_NOME_OLD + ", " + COLUMN_LOG_MORADA_OLD + ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            query += "VALUES ('U', " + TextBoxFacturaID.Text + ", @clienteOldID" + ", '" + TextBoxNome.Text + "', @moradaOld" + ", " + TextBoxFacturaID.Text + ", @clienteOldID, @nomeOld, @moradaOld, @dateBegin, @dateEnd);";

            SqlCommand sqlCommand = new SqlCommand(query, sqlConnection);

            sqlCommand.ExecuteNonQuery();

            this.Close();
        }
    }
}
