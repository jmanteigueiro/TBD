namespace TBD
{
    public class QueryMethods
    {
        private const string COLUMN_NOME = "Nome";
        private const string COLUMN_FATURA = "FacturaID";
        private const string COLUMN_CLIENT = "ClienteID";
        private const string COLUMN_MORADA = "Morada";
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

        public static string GenerateUpdateTransaction(string isolationLevel, int fatura_id, int cliente_id, string nome, string morada,
            int fatura_id_old, int cliente_id_old, string nome_old, string morada_old)
        {
            string tran;
            tran  = "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + "; ";
            tran += "DECLARE @dateBegin DATETIME; ";
            tran += "SET @dateBegin = GetDate(); ";
            tran += "BEGIN TRANSACTION; ";

            tran += "UPDATE " + Config.DEFAULT_TABLENAME + " ";
            tran += "SET " + COLUMN_CLIENT + " = " + cliente_id + ", ";
            tran += "" + COLUMN_NOME + " = '" + nome + "', ";
            tran += "" + COLUMN_MORADA + " = '" + morada + "' ";
            tran += "WHERE " + COLUMN_FATURA + " = " + fatura_id + "; ";

            tran += "COMMIT; ";

            tran += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            tran += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID_OLD + ", " + COLUMN_LOG_CLIENT_ID_OLD +
                ", " + COLUMN_LOG_NOME_OLD + ", " + COLUMN_LOG_MORADA_OLD + ", " + COLUMN_LOG_FATURA_ID +
                ", " + COLUMN_LOG_CLIENT_ID + ", " + COLUMN_LOG_NOME + ", " + COLUMN_LOG_MORADA +
                ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            tran += "VALUES ('U'," + fatura_id_old + ", " + cliente_id_old + ", '" + nome_old + "', '" + morada_old +
                "', " + fatura_id + ", " + cliente_id + ", '" + nome + "', '" + morada + 
                "', @dateBegin" + ", GetDate());";


            return tran;
        }

        public static string GenerateDeleteTransaction(string isolationLevel, int fatura_id, int cliente_id, string nome, string morada)
        {
            string tran;
            tran  = "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + "; ";
            tran += "DECLARE @dateBegin DATETIME; ";
            tran += "SET @dateBegin = GetDate(); ";
            tran += "BEGIN TRANSACTION; ";

            tran += "DELETE FROM " + Config.DEFAULT_TABLENAME + " ";
            tran += "WHERE " + COLUMN_FATURA + " = " + fatura_id;

            tran += " COMMIT; ";

            tran += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            tran += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID_OLD + ", " + COLUMN_LOG_CLIENT_ID_OLD +
                ", " + COLUMN_LOG_NOME_OLD + ", " + COLUMN_LOG_MORADA_OLD + 
                ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            tran += "VALUES ('D'," + fatura_id + ", " + cliente_id + ", '" + nome + "', '" + morada +
                "', @dateBegin" + ", GetDate());";

            return tran;
        }

        public static string GenerateInsertTransaction(string isolationLevel, int fatura_id, int cliente_id, string nome, string morada)
        {
            string tran;
            tran  = "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + "; ";
            tran += "DECLARE @dateBegin DATETIME; ";
            tran += "SET @dateBegin = GetDate(); ";
            tran += "BEGIN TRANSACTION; ";

            tran += "INSERT INTO " + Config.DEFAULT_TABLENAME + "( " + COLUMN_FATURA + ", " + COLUMN_CLIENT + ", " + COLUMN_NOME + ", " + COLUMN_MORADA + ") ";
            tran += "VALUES (" + fatura_id + ", ";
            tran += "" + cliente_id + ", ";
            tran += "'" + nome + "', ";
            tran += "'" + morada + "')";

            tran += " COMMIT; ";

            tran += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            tran += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID +
                ", " + COLUMN_LOG_CLIENT_ID + ", " + COLUMN_LOG_NOME + ", " + COLUMN_LOG_MORADA +
                ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            tran += "VALUES ('I'," + fatura_id + ", " + cliente_id + ", '" + nome + "', '" + morada + "', " + 
                "@dateBegin" + ", GetDate());";

            return tran;
        }
    }
}